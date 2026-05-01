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
    [SerializeField] UIParticle petalParticles; 
    [SerializeField] GameObject floatingTextPrefab;

    private void Awake()
    {
        Instance = this;
        if (petalParticles != null)
        {
            var main = petalParticles.particles[0].main;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;
        }
    }

    private void OnValidate()
    {
        if(!mainCamera)
            mainCamera = Camera.main;
    }

    public void PlayPetals(Vector3 position, Color color)
    {
        if (petalParticles != null)
        {
            RectTransform ptRect = petalParticles.rectTransform;
            if (ptRect != null)
            {
                // Most robust UI position matching: Use the parent of the particles as the reference
                RectTransform parentRect = ptRect.parent as RectTransform;
                if (parentRect != null)
                {
                    Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(mainCamera, position);
                    if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screenPoint, mainCamera, out var localPoint))
                    {
                        ptRect.anchoredPosition = localPoint;
                        ptRect.localPosition = new Vector3(ptRect.localPosition.x, ptRect.localPosition.y, 0);
                    }
                }
                else
                {
                    // Fallback to world position if hierarchy is weird
                    petalParticles.transform.position = position;
                }
            }

            var main = petalParticles.particles[0].main;
            main.startColor = color;
            petalParticles.particles[0].Emit(15);
        }
    }
    
    public void PlayExplosion(Vector3 position, Color color)
    {
        if (petalParticles != null)
        {
            RectTransform ptRect = petalParticles.rectTransform;
            if (ptRect != null)
            {
                RectTransform parentRect = ptRect.parent as RectTransform;
                if (parentRect != null)
                {
                    Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(mainCamera, position);
                    if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screenPoint, mainCamera, out var localPoint))
                    {
                        ptRect.anchoredPosition = localPoint;
                        ptRect.localPosition = new Vector3(ptRect.localPosition.x, ptRect.localPosition.y, 0);
                    }
                }
                else
                {
                    petalParticles.transform.position = position;
                }
            }

            var main = petalParticles.particles[0].main;
            main.startColor = color;
            petalParticles.particles[0].Emit(60);
        }
        
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
        // Cancel camera shake as requested, only shake the UI
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