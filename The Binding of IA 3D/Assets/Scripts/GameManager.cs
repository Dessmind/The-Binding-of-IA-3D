using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GameObject playerPrefab;
    public Transform playerSpawnPoint;

    public GameObject cameraPrefab; // Prefab de la c�mara
    private GameObject currentCamera;

    public Image healthBarFill; // Imagen de la barra de salud
    public Image energyBarFill; // Imagen de la barra de energ�a

    public GameObject HUD; // Referencia al HUD

    private GameObject currentPlayer;

    private void Start()
    {
        ReloadLightingSettings();
        SpawnPlayer();
        SpawnCamera();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartScene();
        }

        // Detecta si se presionan las teclas Esc o M para cambiar al men� principal
        // Input.GetKeyDown(KeyCode.Escape) ||
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.M))
        {
            GoToMainMenu();
        }
    }

    public void SpawnPlayer()
    {
        currentPlayer = Instantiate(playerPrefab, playerSpawnPoint.position, playerSpawnPoint.rotation);

        // Asigna el HUD al PlayerController
        PlayerController playerController = currentPlayer.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.healthBarFill = healthBarFill;
            playerController.energyBarFill = energyBarFill;
        }
    }

    public void SpawnCamera()
    {
        // Si ya existe una c�mara, la destruye para evitar duplicados
        if (currentCamera != null)
        {
            Destroy(currentCamera);
        }

        // Instancia la c�mara y sigue al jugador
        currentCamera = Instantiate(cameraPrefab);

        // Vincula la c�mara al jugador si hay un script de seguimiento de c�mara
        CameraFollow cameraFollow = currentCamera.GetComponent<CameraFollow>();
        if (cameraFollow != null && currentPlayer != null)
        {
            cameraFollow.player = currentPlayer.transform;
        }
    }

    public void HideHUD()
    {
        if (HUD != null)
        {
            HUD.SetActive(false); // Oculta el HUD cuando el jugador muere
        }
    }

    public void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void GoToMainMenu()
    {
        // Cambia a la escena del men� principal
        SceneManager.LoadScene("MainMenu");
    }

    private void ReloadLightingSettings()
    {
        DynamicGI.UpdateEnvironment();
    }
}
