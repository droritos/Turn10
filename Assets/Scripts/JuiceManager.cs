using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class JuiceManager : MonoBehaviour
{
    public static JuiceManager Instance;
    public ParticleSystem petalParticles;
    
    private void Awake()
    {
        Instance = this;
    }
    
    public AudioSource sfxSource; // Hook up in inspector later
    public GameObject floatingTextPrefab;
    public Transform canvasTransform;

    public void PlayPetals(Vector3 position, Color color)
    {
        if (petalParticles != null)
        {
            petalParticles.transform.position = position;
            var main = petalParticles.main;
            main.startColor = color;
            petalParticles.Play();
        }
    }
    
    public void PlayExplosion(Vector3 position, Color color)
    {
        // A much bigger, juicier particle burst for the Lotus explosion
        if (petalParticles != null)
        {
            petalParticles.transform.position = position;
            var main = petalParticles.main;
            main.startColor = color;
            var emission = petalParticles.emission;
            
            // Burst many more particles
            emission.SetBurst(0, new ParticleSystem.Burst(0.0f, 60));
            petalParticles.Play();
            
            // Reset to normal burst later
            StartCoroutine(ResetBurst(emission));
        }
        
        ScreenShake(0.6f, 30f); // Massive shake
    }

    private IEnumerator ResetBurst(ParticleSystem.EmissionModule emission)
    {
        yield return new WaitForSeconds(0.5f);
        emission.SetBurst(0, new ParticleSystem.Burst(0.0f, 15)); // Default burst count
    }

    public void SpawnFloatingText(Vector3 position, string text, Color color)
    {
        if (floatingTextPrefab == null || canvasTransform == null) return;

        GameObject textObj = Instantiate(floatingTextPrefab, canvasTransform);
        FloatingText ft = textObj.GetComponent<FloatingText>();
        if (ft != null)
        {
            ft.Setup(text, color, position);
        }
    }

    public void PlaySound(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }
    
    public void ScreenShake(float duration, float magnitude)
    {
        StartCoroutine(DoShake(duration, magnitude));
    }
    
    private IEnumerator DoShake(float duration, float magnitude)
    {
        Vector3 originalPos = Camera.main.transform.localPosition;
        float elapsed = 0.0f;
        
        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            
            Camera.main.transform.localPosition = new Vector3(x, y, originalPos.z);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        Camera.main.transform.localPosition = originalPos;
    }

    public void PopBlock(RectTransform blockRect)
    {
        StartCoroutine(DoPop(blockRect));
    }

    private IEnumerator DoPop(RectTransform rt)
    {
        float duration = 0.15f;
        float elapsed = 0;
        
        // Scale up
        while (elapsed < duration)
        {
            rt.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 1.3f, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        elapsed = 0;
        // Scale down
        while (elapsed < duration)
        {
            rt.localScale = Vector3.Lerp(Vector3.one * 1.3f, Vector3.one, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        rt.localScale = Vector3.one;
    }
}