using UnityEngine;

public class RotateObjectButton : MonoBehaviour
{
    [Header("Button Settings")]
    public Color hitColor = Color.red;
    public AudioClip hitSound;
    public float colorChangeDuration = 0.5f;

    [Header("Rotation Settings")]
    public Transform targetObject;         // Objeto a rotar
    public float rotationAngleY = 90f;     // �ngulo de rotaci�n en Y
    public float rotationDuration = 5f;    // Duraci�n de la rotaci�n en segundos

    private Renderer buttonRenderer;
    private Color originalColor;
    private AudioSource audioSource;
    private bool hasBeenPressed = false;    // Controla que solo se pueda presionar una vez
    private bool isRotating = false;        // Indica si el objeto est� rotando
    private Quaternion initialRotation;     // Rotaci�n inicial del objeto
    private Quaternion targetRotation;      // Rotaci�n objetivo del objeto
    private float rotationStartTime;        // Tiempo de inicio de la rotaci�n

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
            StartRotation();  // Inicia la rotaci�n
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
            Debug.LogWarning("El AudioClip hitSound no est� asignado en " + gameObject.name);
        }
        else if (audioSource == null)
        {
            Debug.LogError("AudioSource no est� asignado en " + gameObject.name);
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

            // Lerp entre la rotaci�n inicial y la rotaci�n objetivo
            targetObject.rotation = Quaternion.Slerp(initialRotation, targetRotation, t);

            if (t >= 1f)
            {
                isRotating = false; // Finaliza la rotaci�n

                // Forzar actualizaci�n del transform y collider despu�s de la rotaci�n
                targetObject.hasChanged = true;
            }
        }
    }
}
