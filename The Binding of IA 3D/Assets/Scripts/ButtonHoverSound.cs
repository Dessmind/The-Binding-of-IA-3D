using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(AudioSource))]
public class ButtonHoverSound : MonoBehaviour, IPointerEnterHandler
{
    public AudioClip hoverSound; // Sonido que se reproducirá al pasar el mouse
    [Range(0f, 1f)] public float volume = 0.5f; // Volumen del sonido

    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();

        if (hoverSound != null)
        {
            audioSource.clip = hoverSound;
            audioSource.playOnAwake = false;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverSound != null && !audioSource.isPlaying)
        {
            audioSource.PlayOneShot(hoverSound, volume);
        }
    }
}
