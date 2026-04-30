using UnityEngine;
using UnityEditor;

public class SetupJuiceEditor : EditorWindow
{
    [MenuItem("ZenGrid/Setup Juice")]
    public static void Run()
    {
        ZenGridManager manager = Object.FindAnyObjectByType<ZenGridManager>();
        if (manager == null) return;

        GameObject juiceObj = new GameObject("JuiceManager");
        JuiceManager jm = juiceObj.AddComponent<JuiceManager>();
        
        // Setup Petals Particle System
        GameObject petalsObj = new GameObject("Petals");
        petalsObj.transform.SetParent(juiceObj.transform);
        ParticleSystem ps = petalsObj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.loop = false;
        main.playOnAwake = false;
        main.duration = 1f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(50f, 200f);
        main.startSize = new ParticleSystem.MinMaxCurve(10f, 30f);
        main.startLifetime = new ParticleSystem.MinMaxCurve(1f, 2f);
        main.gravityModifier = 2f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        
        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[]{ new ParticleSystem.Burst(0, 5, 15) });
        
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 20f;
        
        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(Color.white, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 0.7f), new GradientAlphaKey(0.0f, 1.0f) }
        );
        colorOverLifetime.color = grad;

        ParticleSystemRenderer psr = petalsObj.GetComponent<ParticleSystemRenderer>();
        psr.material = new Material(Shader.Find("Sprites/Default"));

        jm.petalParticles = ps;
        manager.petalParticles = ps;

        Debug.Log("Juice Setup Complete");
    }
}