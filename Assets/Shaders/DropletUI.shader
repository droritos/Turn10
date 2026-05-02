Shader "ZenGrid/DropletUI"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        // Shape
        _Roundness ("Roundness", Range(0, 0.5)) = 0.4
        
        // Volume & Depth
        _InnerShadowStrength ("Edge Shadow Strength", Range(0, 1)) = 0.6
        _RimLightIntensity ("Bottom Rim Glow", Range(0, 2)) = 1.0
        
        // Glossy Highlight
        _HighlightY ("Highlight Height", Range(0, 0.5)) = 0.35
        _HighlightWidth ("Highlight Width", Range(0.1, 1)) = 0.6
        _HighlightHeight ("Highlight Thickness", Range(0.01, 0.5)) = 0.15
        _HighlightIntensity ("Highlight Brightness", Range(0, 1)) = 0.8
        _HighlightSoftness ("Highlight Softness", Range(0.01, 0.5)) = 0.02

        // Required by Unity UI
        [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInspector] _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil 
        { 
            Ref [_Stencil] 
            Comp [_StencilComp] 
            Pass [_StencilOp] 
            ReadMask [_StencilReadMask] 
            WriteMask [_StencilWriteMask] 
        }
        
        Cull Off 
        Lighting Off 
        ZWrite Off 
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                half4 color     : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            half4 _Color;
            float _Roundness;
            float _InnerShadowStrength;
            float _RimLightIntensity;
            float _HighlightY;
            float _HighlightWidth;
            float _HighlightHeight;
            float _HighlightIntensity;
            float _HighlightSoftness;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = TransformObjectToHClip(IN.vertex.xyz);
                OUT.color = IN.color * _Color; 
                OUT.texcoord = IN.texcoord;
                return OUT;
            }

            float RoundedRectSDF(float2 p, float2 extents, float radius)
            {
                float2 d = abs(p) - extents + radius;
                return min(max(d.x, d.y), 0.0) + length(max(d, 0.0)) - radius;
            }

            half4 frag(v2f IN) : SV_Target
            {
                float2 centeredUV = IN.texcoord - 0.5;

                // 1. Base Shape (Squircle)
                float dist = RoundedRectSDF(centeredUV, float2(0.5, 0.5), _Roundness);
                float edgeSoftness = fwidth(dist);
                float shapeAlpha = smoothstep(edgeSoftness, -edgeSoftness, dist);

                // 2. Liquid Volume (Darken edges)
                float edgeGradient = smoothstep(0.0, -_Roundness - 0.2, dist);
                half4 baseColor = IN.color;
                // Preserve color saturation by multiplying by a tinted dark value rather than pure black
                baseColor.rgb = lerp(baseColor.rgb, baseColor.rgb * 0.35, edgeGradient * _InnerShadowStrength);

                // 3. Bottom Rim Light (Internal Refraction)
                float bottomMask = smoothstep(0.1, -0.4, centeredUV.y); 
                float rimGlow = smoothstep(0.0, -_Roundness, dist) * bottomMask;
                
                // FIX: Multiply by the shape's color (IN.color.rgb) instead of white (1,1,1)
                // This keeps the glow rich and saturated!
                baseColor.rgb += (IN.color.rgb * rimGlow * _RimLightIntensity);

                // 4. Sharp Glass Highlight (Top Reflection)
                float2 hlUV = centeredUV;
                hlUV.y -= _HighlightY; 
                hlUV.x /= _HighlightWidth;  
                hlUV.y /= _HighlightHeight; 
                
                float hlDist = length(hlUV);
                // Keep this pure white, as it's the reflection of the room/lights
                float highlight = smoothstep(0.5 + _HighlightSoftness, 0.5, hlDist) * _HighlightIntensity;

                // 5. Combine
                half4 finalColor = baseColor;
                finalColor.rgb += (half3(1,1,1) * highlight); 
                finalColor.a = shapeAlpha * IN.color.a;

                return finalColor;
            }
            ENDHLSL
        }
    }
}