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
        // Espera un peque�o retraso antes de asignar la c�mara, permitiendo que se instancie en la escena
        yield return new WaitForSeconds(0.1f); // Ajusta el tiempo seg�n sea necesario
        mainCamera = Camera.main;

        if (mainCamera == null)
        {
            Debug.LogWarning("No se encontr� una c�mara principal en la escena para el Billboard.");
        }
        else
        {
            Debug.Log("C�mara principal asignada autom�ticamente en Billboard.");
        }
    }

    private void LateUpdate()
    {
        if (mainCamera != null)
        {
            Debug.Log("Billboard Update: Orientando hacia la c�mara.");
            transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
                             mainCamera.transform.rotation * Vector3.up);
        }
    }
}
