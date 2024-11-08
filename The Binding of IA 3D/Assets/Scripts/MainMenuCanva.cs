using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    private void Start()
    {
        // Aseg�rate de que el cursor est� visible y desbloqueado al iniciar el men�
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Funci�n para el bot�n de Start
    public void StartGame()
    {
        // Oculta el cursor y bloquea el control al comenzar el juego
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        SceneManager.LoadScene("Controls"); // Cambia a la escena "Controls"
    }

    // Funci�n para el bot�n de Exit
    public void ExitGame()
    {
        Debug.Log("Exiting game..."); // Este mensaje solo se ver� en el editor
        Application.Quit(); // Cierra la aplicaci�n
    }
}
