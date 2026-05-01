using UnityEngine;
using DG.Tweening;
using ZenGrid;

public class JuiceManager : MonoBehaviour
{
    public static JuiceManager Instance;
    public Transform canvasTransform;
    
    [Header("References")]
    [SerializeField] Camera mainCamera;
    [SerializeField] ParticleSystem petalParticles; 
    [SerializeField] GameObject floatingTextPrefab;

    private void Awake()
    {
        Instance = this;
        if (petalParticles != null)
        {
            var main = petalParticles.main;
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
            if (canvasTransform != null) petalParticles.transform.SetParent(canvasTransform);
            petalParticles.transform.position = position;
            
            // Force local Z to 0 so it's not buried behind the canvas
            var lp = petalParticles.transform.localPosition * -1;
            petalParticles.transform.localPosition = new Vector3(lp.x, lp.y, 0);

            var main = petalParticles.main;
            main.startColor = color;
            petalParticles.Emit(15);
        }
    }
    
    public void PlayExplosion(Vector3 position, Color color)
    {
        if (petalParticles != null)
        {
            if (canvasTransform != null) petalParticles.transform.SetParent(canvasTransform);
            petalParticles.transform.position = position;
            
            var lp = petalParticles.transform.localPosition;
            petalParticles.transform.localPosition = new Vector3(lp.x, lp.y, 0);

            var main = petalParticles.main;
            main.startColor = color;
            petalParticles.Emit(60);
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