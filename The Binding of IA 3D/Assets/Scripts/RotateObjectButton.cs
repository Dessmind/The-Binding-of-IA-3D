using UnityEngine;

public class RotateObjectButton : MonoBehaviour
{
    [Header("Button Settings")]
    public Color hitColor = Color.red;
    public AudioClip hitSound;
    public float colorChangeDuration = 0.5f;

    [Header("Rotation Settings")]
    public Transform targetObject;         // Objeto a rotar
    public float rotationAngleY = 90f;     // Ángulo de rotación en Y
    public float rotationDuration = 5f;    // Duración de la rotación en segundos

    private Renderer buttonRenderer;
    private Color originalColor;
    private AudioSource audioSource;
    private bool hasBeenPressed = false;    // Controla que solo se pueda presionar una vez
    private bool isRotating = false;        // Indica si el objeto está rotando
    private Quaternion initialRotation;     // Rotación inicial del objeto
    private Quaternion targetRotation;      // Rotación objetivo del objeto
    private float rotationStartTime;        // Tiempo de inicio de la rotación

    private void Start()
    {
        buttonRenderer = GetComponent<Renderer>();
        originalColor = buttonRenderer.material.color;

        // Verificamos y asignamos el AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Projectile") && !hasBeenPressed && !isRotating)
        {
            hasBeenPressed = true;  // Marcar como presionado para que solo funcione una vez
            ChangeColor();
            PlaySound();
            StartRotation();  // Inicia la rotación
            Destroy(other.gameObject);
        }
    }

    private void ChangeColor()
    {
        buttonRenderer.material.color = hitColor;
        Invoke("ResetColor", colorChangeDuration);
    }

    private void ResetColor()
    {
        buttonRenderer.material.color = originalColor;
    }

    private void PlaySound()
    {
        if (audioSource != null && hitSound != null)
        {
            audioSource.PlayOneShot(hitSound);
        }
        else if (hitSound == null)
        {
            Debug.LogWarning("El AudioClip hitSound no está asignado en " + gameObject.name);
        }
        else if (audioSource == null)
        {
            Debug.LogError("AudioSource no está asignado en " + gameObject.name);
        }
    }

    private void StartRotation()
    {
        if (targetObject != null)
        {
            initialRotation = targetObject.rotation;
            targetRotation = Quaternion.Euler(initialRotation.eulerAngles.x, initialRotation.eulerAngles.y + rotationAngleY, initialRotation.eulerAngles.z);
            rotationStartTime = Time.time;
            isRotating = true;
        }
        else
        {
            Debug.LogWarning("No se ha asignado un objeto de destino para rotar.");
        }
    }

    private void Update()
    {
        if (isRotating)
        {
            float elapsed = Time.time - rotationStartTime;
            float t = Mathf.Clamp01(elapsed / rotationDuration);

            // Lerp entre la rotación inicial y la rotación objetivo
            targetObject.rotation = Quaternion.Slerp(initialRotation, targetRotation, t);

            if (t >= 1f)
            {
                isRotating = false; // Finaliza la rotación

                // Forzar actualización del transform y collider después de la rotación
                targetObject.hasChanged = true;
            }
        }
    }
}
