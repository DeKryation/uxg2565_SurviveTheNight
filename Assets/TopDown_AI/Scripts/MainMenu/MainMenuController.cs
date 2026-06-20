using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Menu Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject creditsPanel;

    [Header("Gameplay Scene")]
    [SerializeField] private string gameplaySceneName = "GameScene";

    private void Start()
    {
        OpenMainMenu();
    }

    public void StartGame()
    {
        if (string.IsNullOrWhiteSpace(gameplaySceneName))
        {
            Debug.LogError("Gameplay scene name has not been entered.");
            return;
        }

        SceneManager.LoadScene(gameplaySceneName);
    }

    public void OpenSettings()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
        creditsPanel.SetActive(false);
    }

    public void OpenCredits()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);
        creditsPanel.SetActive(true);
    }

    public void OpenMainMenu()
    {
        mainMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);
        creditsPanel.SetActive(false);
    }

    public void QuitGame()
    {
        Debug.Log("Quit button pressed.");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
