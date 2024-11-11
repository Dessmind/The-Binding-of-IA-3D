using UnityEngine;

public class Billboard : MonoBehaviour
{
    public Camera mainCamera;

    private void LateUpdate()
    {
        if (mainCamera != null)
        {
            Debug.Log("Billboard Update: Orientando hacia la cámara.");
            transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
                             mainCamera.transform.rotation * Vector3.up);
        }
        else
        {
            Debug.LogWarning("No se ha asignado una cámara en el Billboard");
        }
    }

    private void OnEnable()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera != null)
            {
                Debug.Log("Cámara principal asignada automáticamente en Billboard.");
            }
        }
    }
}
