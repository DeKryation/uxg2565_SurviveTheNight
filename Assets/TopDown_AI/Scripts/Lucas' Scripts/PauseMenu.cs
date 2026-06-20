using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("Pause Menu UI")]
    [SerializeField] private GameObject pauseScreen;

    [Header("Mouse / Player Control")]
    [Tooltip("Drag the script that controls mouse aiming, player movement, or camera movement here.")]
    [SerializeField] private MonoBehaviour mouseControlScript;

    [Header("Optional Main Menu")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private bool isPaused = false;

    private void Start()
    {
        // Ensure the game starts normally.
        Time.timeScale = 1f;
        isPaused = false;

        if (pauseScreen != null)
        {
            pauseScreen.SetActive(false);
        }

        if (mouseControlScript != null)
        {
            mouseControlScript.enabled = true;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    public void PauseGame()
    {
        if (isPaused)
        {
            return;
        }

        isPaused = true;

        if (pauseScreen != null)
        {
            pauseScreen.SetActive(true);
        }

        // Stops the character, weapon, or camera from reacting
        // to mouse movement while the pause menu is open.
        if (mouseControlScript != null)
        {
            mouseControlScript.enabled = false;
        }

        // Freeze time-based gameplay.
        Time.timeScale = 0f;

        // Keep the cursor available for pause-menu buttons.
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ResumeGame()
    {
        isPaused = false;

        // Resume time-based gameplay.
        Time.timeScale = 1f;

        if (mouseControlScript != null)
        {
            mouseControlScript.enabled = true;
        }

        if (pauseScreen != null)
        {
            pauseScreen.SetActive(false);
        }

        // Keep this for a top-down game that uses the mouse cursor.
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void RestartLevel()
    {
        PrepareForSceneChange();

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }

    public void ReturnToMainMenu()
    {
        PrepareForSceneChange();

        if (string.IsNullOrWhiteSpace(mainMenuSceneName))
        {
            Debug.LogError("The Main Menu scene name has not been entered.");
            return;
        }

        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void QuitGame()
    {
        PrepareForSceneChange();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void PrepareForSceneChange()
    {
        Time.timeScale = 1f;
        isPaused = false;

        if (mouseControlScript != null)
        {
            mouseControlScript.enabled = true;
        }
    }

    private void OnDestroy()
    {
        // Prevent another scene from remaining frozen.
        Time.timeScale = 1f;
    }
}
