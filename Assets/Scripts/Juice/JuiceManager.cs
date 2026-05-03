using Coffee.UIExtensions;
using UnityEngine;
using DG.Tweening;
using ZenGrid;

public class JuiceManager : MonoBehaviour
{
    public static JuiceManager Instance;
    public Transform canvasTransform;
    
    [Header("References")]
    [SerializeField] Camera mainCamera;
    [SerializeField] UIParticle uiParticle; 
    [SerializeField] GameObject floatingTextPrefab;

    private ParticleSystem _petalParticles;

    private void Awake()
    {
        Instance = this;

        if (uiParticle == null)
        {
            Debug.LogWarning("[JuiceManager] uiParticle is not assigned — visual effects will be skipped.", this);
            return;
        }

        // If the serialized reference points to a Prefab asset (not a scene instance),
        // instantiate it so we can safely move and emit from it at runtime.
        if (!uiParticle.gameObject.scene.IsValid())
        {
            Transform parent = canvasTransform != null ? canvasTransform : transform;
            uiParticle = Instantiate(uiParticle, parent);
            Debug.Log("[JuiceManager] uiParticle was a prefab asset — instantiated into scene.", this);
        }

        if (uiParticle.particles.Count > 0)
        {
            _petalParticles = uiParticle.particles[0];
            var main = _petalParticles.main;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;
        }
        else
        {
            Debug.LogWarning("[JuiceManager] uiParticle has no particle systems — visual effects will be skipped.", this);
        }
    }

    private void OnValidate()
    {
        if (!mainCamera)
            mainCamera = Camera.main;
    }

    // Moves the UIParticle RectTransform to the target position, then emits.
    // Never touch _petalParticles.transform directly — UIParticle owns its hierarchy.
    private void PositionAndEmit(Vector3 worldPosition, Color color, int count)
    {
        if (_petalParticles == null || uiParticle == null) return;

        // Move the UIParticle container to the desired world position
        RectTransform uiRT = uiParticle.GetComponent<RectTransform>();
        if (uiRT != null)
        {
            uiRT.position = worldPosition;
            // Keep Z = 0 in local space so particles aren't clipped behind the canvas
            var lp = uiRT.localPosition;
            uiRT.localPosition = new Vector3(lp.x, lp.y, 0f);
        }

        var main = _petalParticles.main;
        main.startColor = color;
        _petalParticles.Emit(count);
    }

    public void PlayPetals(Vector3 position, Color color)
    {
        PositionAndEmit(position, color, 15);
    }
    
    public void PlayExplosion(Vector3 position, Color color)
    {
        PositionAndEmit(position, color, 60);
        ScreenShake(0.6f, 2f); 
    }

    // --- UI FLOATING TEXT ---

    public void SpawnFloatingText(Vector3 position, string text, Color color, float scale = 1f)
    {
        if (floatingTextPrefab == null || canvasTransform == null) return;

        GameObject textObj = Instantiate(floatingTextPrefab, canvasTransform);
        FloatingText ft = textObj.GetComponent<FloatingText>();
        if (ft != null)
        {
            ft.Setup(text, color, position, 1.0f, 1.5f, scale);
        }
    }

    public void ScreenShake(float duration, float magnitude)
    {
        if (canvasTransform != null)
        {
            canvasTransform.DOComplete();
            canvasTransform.DOShakePosition(duration, magnitude * 8f, 12, 90f, false, true);
        }
    }

    public void PopBlock(RectTransform blockRect)
    {
        if (blockRect == null) return;

        blockRect.DOKill(true);
        blockRect.DOPunchScale(Vector3.one * 0.3f, 0.3f, 1, 0.5f);
    }
}