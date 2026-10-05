using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("Pause Menu UI")]
    [SerializeField] private GameObject pauseScreen;

    [Header("Clickable Pause Button")]
    [SerializeField] private GameObject pauseButton;

    [Header("Settings UI")]
    [SerializeField] private GameObject settingsScreen;

    [Header("Mouse / Player Control")]
    [Tooltip("Drag the script that controls mouse aiming, player movement, or camera movement here.")]
    [SerializeField] private MonoBehaviour mouseControlScript;

    [Header("Optional Main Menu")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private bool isPaused = false;

    private void Start()
    {
        Time.timeScale = 1f;
        isPaused = false;

        if (pauseScreen != null)
            pauseScreen.SetActive(false);

        if (settingsScreen != null)
            settingsScreen.SetActive(false);

        if (pauseButton != null)
            pauseButton.SetActive(true);

        if (mouseControlScript != null)
            mouseControlScript.enabled = true;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (settingsScreen != null && settingsScreen.activeSelf)
            {
                CloseSettings();
            }
            else
            {
                TogglePause();
            }
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

    public void PauseButtonPressed()
    {
        TogglePause();
    }

    public void PauseGame()
    {
        if (isPaused)
            return;

        isPaused = true;

        if (pauseScreen != null)
            pauseScreen.SetActive(true);

        if (settingsScreen != null)
            settingsScreen.SetActive(false);

        //if (pauseButton != null)
           // pauseButton.SetActive(false);

        if (mouseControlScript != null)
            mouseControlScript.enabled = false;

        Time.timeScale = 0f;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ResumeGame()
    {
        isPaused = false;

        Time.timeScale = 1f;

        if (mouseControlScript != null)
            mouseControlScript.enabled = true;

        if (pauseScreen != null)
            pauseScreen.SetActive(false);

        if (settingsScreen != null)
            settingsScreen.SetActive(false);

        if (pauseButton != null)
            pauseButton.SetActive(true);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void OpenSettings()
    {
        if (!isPaused)
            PauseGame();

        if (pauseScreen != null)
            pauseScreen.SetActive(false);

        if (settingsScreen != null)
            settingsScreen.SetActive(true);

        if (pauseButton != null)
            pauseButton.SetActive(false);
    }

    public void CloseSettings()
    {
        if (settingsScreen != null)
            settingsScreen.SetActive(false);

        if (pauseScreen != null)
            pauseScreen.SetActive(true);

        if (pauseButton != null)
            pauseButton.SetActive(false);
    }

    public void RestartLevel()
    {
        PrepareForSceneChange();

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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
            mouseControlScript.enabled = true;

        if (pauseButton != null)
            pauseButton.SetActive(true);
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }
}