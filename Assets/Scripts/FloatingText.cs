using UnityEngine;
using TMPro;
using DG.Tweening;

public class FloatingText : MonoBehaviour
{
    public TextMeshProUGUI textMesh;

    public void Setup(string text, Color color, Vector3 startPos, float floatDistance = 150f, float duration = 1.5f)
    {
        transform.position = startPos;
        textMesh.text = text;
        textMesh.color = color;
        
        // Ensure scale is normal
        transform.localScale = Vector3.zero;

        // Sequence for popping in, floating up, and fading out
        Sequence seq = DOTween.Sequence();
        
        // Pop in
        seq.Append(transform.DOScale(Vector3.one * 1.5f, 0.2f).SetEase(Ease.OutBack));
        seq.Append(transform.DOScale(Vector3.one, 0.1f));
        
        // Float up and fade
        seq.Join(transform.DOMoveY(startPos.y + floatDistance, duration).SetEase(Ease.OutCubic));
        seq.Join(textMesh.DOFade(0, duration).SetEase(Ease.InExpo));
        
        // Destroy when done
        seq.OnComplete(() => {
            Destroy(gameObject);
        });
    }
}
