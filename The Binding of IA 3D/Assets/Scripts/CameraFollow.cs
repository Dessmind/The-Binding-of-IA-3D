// CameraFollow.cs
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player; // Referencia al jugador

    public float distance = 5.0f; // Distancia desde el jugador
    public float height = 2.0f; // Altura sobre el jugador
    public float mouseSensitivity = 100f;

    private float xRotation = 0f; // Ángulo vertical
    private float yRotation = 0f; // Ángulo horizontal

    void Start()
    {
        // Bloquear el cursor al centro de la pantalla y ocultarlo
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Inicializar ángulos de rotación
        Vector3 angles = transform.eulerAngles;
        xRotation = angles.x;
        yRotation = angles.y;
    }

    void Update()
    {
        // Obtener movimientos del ratón
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Ajustar ángulos de rotación
        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -20f, 80f); // Limitar el ángulo vertical

        // Calcular la posición de la cámara
        Quaternion rotation = Quaternion.Euler(xRotation, yRotation, 0f);
        Vector3 offset = rotation * new Vector3(0, 0, -distance);
        Vector3 targetPosition = player.position + Vector3.up * height + offset;

        // Actualizar posición y rotación de la cámara
        transform.position = targetPosition;
        transform.LookAt(player.position + Vector3.up * height);
    }
}
