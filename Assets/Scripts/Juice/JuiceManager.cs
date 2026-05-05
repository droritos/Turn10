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

        if (!uiParticle.gameObject.scene.IsValid())
        {
            Transform parent = canvasTransform != null ? canvasTransform : transform;
            uiParticle = Instantiate(uiParticle, parent);
        }

        if (uiParticle.particles.Count > 0)
        {
            _petalParticles = uiParticle.particles[0];
            var main = _petalParticles.main;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;
        }
    }

    private void OnValidate()
    {
        if (!mainCamera)
            mainCamera = Camera.main;
    }

    private void PositionAndEmit(Vector3 worldPosition, Color color, int count)
    {
        if (_petalParticles == null || uiParticle == null) return;

        RectTransform uiRT = uiParticle.GetComponent<RectTransform>();
        if (uiRT != null)
        {
            uiRT.position = worldPosition;
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
            // Kill safely without forcing it to completion (which causes violent snapping)
            canvasTransform.DOKill();
            
            // Reset to center just in case a previous shake left it slightly off-center
            canvasTransform.localPosition = Vector3.zero; 
            
            canvasTransform.DOShakePosition(duration, magnitude * 8f, 12, 90f, false, true);
        }
    }

    public void PopBlock(RectTransform blockRect)
    {
        if (blockRect == null) return;

        // Kill only the scale tween safely without fast-forwarding other animations
        blockRect.DOKill(false);
        
        // Ensure scale is reset to 1 before we pop, so multiple fast pops don't compound and grow huge
        blockRect.localScale = Vector3.one;

        // A manual sequence is much more stable than DOPunchScale for rapid interruptions
        Sequence popSeq = DOTween.Sequence();
        popSeq.Append(blockRect.DOScale(Vector3.one * 1.3f, 0.15f).SetEase(Ease.OutQuad));
        popSeq.Append(blockRect.DOScale(Vector3.one, 0.15f).SetEase(Ease.InQuad));
        
        // Bind the sequence to the block so it gets destroyed safely if the block is deleted
        popSeq.SetTarget(blockRect); 
    }
}