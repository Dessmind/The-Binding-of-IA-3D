using UnityEngine;
using UnityEngine.SceneManagement;

public class CanvasController : MonoBehaviour
{
    private void Start()
    {
        // Asegúrate de que el cursor esté visible y desbloqueado al iniciar el menú
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Esta función se asignará al botón en el Canvas
    public void LoadLevelTest()
    {
        // Oculta el cursor y bloquea el control al cargar el nivel
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        SceneManager.LoadScene("LevelTest"); // Cambia a la escena "LevelTest"
    }
}
