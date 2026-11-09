using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SceneInstructionsPopup : MonoBehaviour
{
    [Header("UI References")]
    public GameObject instructionsPanel;
    public Button closeButton;

    [Header("Settings")]
    public bool pauseGameWhileOpen = true;
    public bool unlockCursorWhileOpen = true;

    private bool isOpen = false;
    private float previousTimeScale = 1f;

    void Start()
    {
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(CloseInstructions);
            closeButton.onClick.AddListener(CloseInstructions);
        }

        OpenInstructions();
    }

    public void OpenInstructions()
    {
        if (isOpen)
            return;

        previousTimeScale = Time.timeScale;
        isOpen = true;

        if (instructionsPanel != null)
            instructionsPanel.SetActive(true);

        if (pauseGameWhileOpen)
            Time.timeScale = 0f;

        if (unlockCursorWhileOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // Clear any previously selected UI.
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    public void CloseInstructions()
    {
        if (!isOpen)
            return;

        isOpen = false;

        if (instructionsPanel != null)
            instructionsPanel.SetActive(false);

        if (pauseGameWhileOpen)
            Time.timeScale = previousTimeScale;

        if (unlockCursorWhileOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void OnDestroy()
    {
        if (closeButton != null)
            closeButton.onClick.RemoveListener(CloseInstructions);

        // Prevent the scene remaining paused if destroyed.
        if (isOpen && pauseGameWhileOpen)
            Time.timeScale = previousTimeScale;
    }
}
