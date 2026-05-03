Shader "ZenGrid/GlassmorphismUI"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        // Shape
        _Roundness ("Roundness", Range(0, 0.5)) = 0.4
        
        // Glass Properties
        _GlassOpacity ("Center Glass Opacity", Range(0, 1)) = 0.15
        _BevelThickness ("Edge Bevel Thickness", Range(0.001, 0.1)) = 0.04
        _BevelHighlight ("Top-Left Highlight", Range(0, 2)) = 1.0
        _BevelShadow ("Bottom-Right Shadow", Range(0, 1)) = 0.3

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

        Stencil { Ref [_Stencil] Comp [_StencilComp] Pass [_StencilOp] ReadMask [_StencilReadMask] WriteMask [_StencilWriteMask] }
        Cull Off Lighting Off ZWrite Off ZTest [unity_GUIZTestMode]
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
            float _GlassOpacity;
            float _BevelThickness;
            float _BevelHighlight;
            float _BevelShadow;

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

                // 1. Base Shape (Slightly shrunk to allow for anti-aliasing on the edges)
                float dist = RoundedRectSDF(centeredUV, float2(0.48, 0.48), _Roundness);
                float edgeSoftness = fwidth(dist);
                float shapeAlpha = smoothstep(edgeSoftness, -edgeSoftness, dist);

                // 2. Base Glass Translucency
                half4 finalColor = IN.color;
                // Keep the center highly transparent so the background shows through
                finalColor.a = _GlassOpacity * shapeAlpha * IN.color.a;

                // 3. Calculate Directional Lighting for the Bevel
                // Top-left is (-0.5, 0.5). This math creates a diagonal gradient.
                float lightDir = -centeredUV.x + centeredUV.y; 

                // Isolate just the inner edge of the shape
                float innerEdge = smoothstep(-_BevelThickness, 0.0, dist);

                // 4. Top-Left Highlight (The bright glass edge)
                float highlightMask = innerEdge * smoothstep(0.0, 0.6, lightDir);
                finalColor.rgb += (half3(1,1,1) * highlightMask * _BevelHighlight);
                // Make the highlighted edge more opaque than the center
                finalColor.a += (highlightMask * shapeAlpha * 0.8); 

                // 5. Bottom-Right Shadow (The dark glass edge)
                float shadowMask = innerEdge * smoothstep(0.0, 0.6, -lightDir);
                finalColor.rgb -= (half3(1,1,1) * shadowMask * _BevelShadow);
                // Make the shadowed edge slightly more opaque
                finalColor.a += (shadowMask * shapeAlpha * 0.5);

                return finalColor;
            }
            ENDHLSL
        }
    }
}