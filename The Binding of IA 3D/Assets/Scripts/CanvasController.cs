using UnityEngine;
using UnityEngine.SceneManagement;

public class CanvasController : MonoBehaviour
{
    private void Start()
    {
        // Aseg�rate de que el cursor est� visible y desbloqueado al iniciar el men�
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Esta funci�n se asignar� al bot�n en el Canvas
    public void LoadLevelTest()
    {
        // Oculta el cursor y bloquea el control al cargar el nivel
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        SceneManager.LoadScene("LevelTest"); // Cambia a la escena "LevelTest"
    }
}
