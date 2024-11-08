using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;
    public float distance = 5.0f;
    public float height = 2.0f;
    public float mouseSensitivity = 100f;

    public float horizontalOffset = 1.5f; // Ajusta este valor en el inspector
    public float maxYUp = 25f;
    public float maxYDown = -40f;

    private float xRotation = 0f;
    private float yRotation = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Application.targetFrameRate = 144;

        Vector3 angles = transform.eulerAngles;
        xRotation = angles.x;
        yRotation = angles.y;
    }

    void LateUpdate()
    {
        // Verifica si el jugador aún existe antes de intentar seguirlo
        if (player == null)
        {
            return; // Sal de la función si el jugador ha sido destruido
        }

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.unscaledDeltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.unscaledDeltaTime;

        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, maxYDown, maxYUp);

        Quaternion rotation = Quaternion.Euler(xRotation, yRotation, 0f);

        Vector3 offset = new Vector3(horizontalOffset, height, -distance);
        Vector3 desiredPosition = player.position + rotation * offset;

        RaycastHit hit;
        if (Physics.Linecast(player.position + Vector3.up * height, desiredPosition, out hit))
        {
            transform.position = hit.point;
        }
        else
        {
            transform.position = desiredPosition;
        }

        transform.LookAt(player.position + Vector3.up * height);
    }
}
