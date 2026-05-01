using UnityEngine;
using DG.Tweening;
using ZenGrid;

public class JuiceManager : MonoBehaviour
{
    public static JuiceManager Instance;
    
    public Transform canvasTransform;
    [SerializeField] Camera mainCamera;
    [SerializeField] ParticleSystem petalParticles; 
    
    [SerializeField] AudioSource sfxSource;
    [SerializeField] GameObject floatingTextPrefab;

    private void Awake()
    {
        Instance = this;
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
            ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();
            // Because UIParticle handles the Canvas rendering, we can just pass the UI world position directly!
            emitParams.position = position; 
            emitParams.startColor = color;
            
            // Emit directly from the standard Particle System
            petalParticles.Emit(emitParams, 15);
        }
    }
    
    public void PlayExplosion(Vector3 position, Color color)
    {
        if (petalParticles != null)
        {
            ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();
            emitParams.position = position;
            emitParams.startColor = color;
            
            petalParticles.Emit(emitParams, 60);
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

    // --- AUDIO ---

    public void PlaySound(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }
    
    // --- JUICE ---

    public void ScreenShake(float duration, float magnitude)
    {
        if (mainCamera == null) return;

        mainCamera.transform.DOComplete();
        mainCamera.transform.DOShakePosition(duration, magnitude, 10, 90f, false, true);
    }

    public void PopBlock(RectTransform blockRect)
    {
        if (blockRect == null) return;

        blockRect.DOKill(true);
        blockRect.DOPunchScale(Vector3.one * 0.3f, 0.3f, 1, 0.5f);
    }
}