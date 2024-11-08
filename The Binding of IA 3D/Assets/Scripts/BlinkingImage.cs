using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BlinkingImage : MonoBehaviour
{
    public Image targetImage; // La imagen que queremos hacer parpadear
    public float blinkSpeed = 1f; // Velocidad del parpadeo
    public float minAlpha = 0f; // Valor mínimo de alfa
    public float maxAlpha = 1f; // Valor máximo de alfa

    private bool isBlinking = true;

    private void Start()
    {
        if (targetImage == null)
        {
            Debug.LogError("No Image assigned to BlinkingImage script.");
            return;
        }
        StartBlinking();
    }

    private void OnEnable()
    {
        StartBlinking();
    }

    private void OnDisable()
    {
        StopBlinking();
    }

    private void StartBlinking()
    {
        isBlinking = true;
        StartCoroutine(Blink());
    }

    private void StopBlinking()
    {
        isBlinking = false;
    }

    private IEnumerator Blink()
    {
        while (isBlinking)
        {
            // Alterna entre minAlpha y maxAlpha en el canal alfa para hacer parpadear la imagen
            float alpha = Mathf.Lerp(minAlpha, maxAlpha, Mathf.PingPong(Time.time * blinkSpeed, 1f));

            // Actualiza la transparencia de la imagen
            Color color = targetImage.color;
            color.a = alpha;
            targetImage.color = color;

            yield return null;
        }
    }
}
