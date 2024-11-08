using UnityEngine;

public class JumpPad : MonoBehaviour
{
    public float jumpPadForce = 15f; // Fuerza de salto que aplicar� la plataforma
    public AudioClip jumpPadSound; // Sonido que se reproducir� al usar la plataforma
    private AudioSource audioSource; // AudioSource para reproducir el sonido
    private bool hasPlayedSound = false; // Controla si el sonido se ha reproducido en esta colisi�n

    private void Start()
    {
        // Agrega un AudioSource al objeto si no lo tiene
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.clip = jumpPadSound;
        audioSource.playOnAwake = false; // Aseg�rate de que el sonido no se reproduzca autom�ticamente
    }

    private void OnTriggerEnter(Collider other)
    {
        // Verifica si el jugador toca la plataforma
        if (other.CompareTag("Player") && !hasPlayedSound)
        {
            // Accede al `PlayerController` del jugador y aplica la fuerza de salto
            PlayerController playerController = other.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.ApplyJumpForce(jumpPadForce);

                // Reproduce el sonido una vez
                if (audioSource != null && jumpPadSound != null)
                {
                    audioSource.PlayOneShot(jumpPadSound);
                }

                hasPlayedSound = true; // Marca el sonido como reproducido
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Restablece el control para permitir reproducir el sonido en la siguiente colisi�n
        if (other.CompareTag("Player"))
        {
            hasPlayedSound = false;
        }
    }
}
