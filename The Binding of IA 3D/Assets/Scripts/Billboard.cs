using UnityEngine;
using System.Collections;

public class Billboard : MonoBehaviour
{
    private Camera mainCamera;

    private void Start()
    {
        StartCoroutine(WaitAndAssignCamera());
    }

    private IEnumerator WaitAndAssignCamera()
    {
        // Espera un pequeño retraso antes de asignar la cámara, permitiendo que se instancie en la escena
        yield return new WaitForSeconds(0.1f); // Ajusta el tiempo según sea necesario
        mainCamera = Camera.main;

        if (mainCamera == null)
        {
            Debug.LogWarning("No se encontró una cámara principal en la escena para el Billboard.");
        }
        else
        {
            Debug.Log("Cámara principal asignada automáticamente en Billboard.");
        }
    }

    private void LateUpdate()
    {
        if (mainCamera != null)
        {
            Debug.Log("Billboard Update: Orientando hacia la cámara.");
            transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
                             mainCamera.transform.rotation * Vector3.up);
        }
    }
}
