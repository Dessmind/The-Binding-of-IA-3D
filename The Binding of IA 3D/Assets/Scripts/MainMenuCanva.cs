using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    private void Start()
    {
        // Asegúrate de que el cursor esté visible y desbloqueado al iniciar el menú
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Función para el botón de Start
    public void StartGame()
    {
        // Oculta el cursor y bloquea el control al comenzar el juego
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        SceneManager.LoadScene("Controls"); // Cambia a la escena "Controls"
    }

    // Función para el botón de Exit
    public void ExitGame()
    {
        Debug.Log("Exiting game..."); // Este mensaje solo se verá en el editor
        Application.Quit(); // Cierra la aplicación
    }
}
