using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("UI")]
    public GameObject pauseScreen;

    private bool isPaused = false;

    void Start()
    {
        ResumeGame();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
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
    }

    public void PauseGame()
    {
        isPaused = true;

        if (pauseScreen != null)
        {
            pauseScreen.SetActive(true);
        }

        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        isPaused = false;

        if (pauseScreen != null)
        {
            pauseScreen.SetActive(false);
        }

        Time.timeScale = 1f;
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }
}
