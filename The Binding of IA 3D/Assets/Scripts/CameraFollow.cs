// CameraFollow.cs
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player; // Referencia al jugador

    public float distance = 5.0f; // Distancia desde el jugador
    public float height = 2.0f; // Altura sobre el jugador
    public float mouseSensitivity = 100f;

    private float xRotation = 0f; // �ngulo vertical
    private float yRotation = 0f; // �ngulo horizontal

    void Start()
    {
        // Bloquear el cursor al centro de la pantalla y ocultarlo
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Inicializar �ngulos de rotaci�n
        Vector3 angles = transform.eulerAngles;
        xRotation = angles.x;
        yRotation = angles.y;
    }

    void Update()
    {
        // Obtener movimientos del rat�n
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Ajustar �ngulos de rotaci�n
        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -20f, 80f); // Limitar el �ngulo vertical

        // Calcular la posici�n de la c�mara
        Quaternion rotation = Quaternion.Euler(xRotation, yRotation, 0f);
        Vector3 offset = rotation * new Vector3(0, 0, -distance);
        Vector3 targetPosition = player.position + Vector3.up * height + offset;

        // Actualizar posici�n y rotaci�n de la c�mara
        transform.position = targetPosition;
        transform.LookAt(player.position + Vector3.up * height);
    }
}
