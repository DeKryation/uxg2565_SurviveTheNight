using UnityEngine;
using UnityEngine.UI;

public class SettingsTabs : MonoBehaviour
{
    [Header("Panels")]
    public GameObject audioSettingsPanel;
    public GameObject controlsSettingsPanel;

    [Header("Tab Button Images")]
    public Image audioButtonImage;
    public Image controlsButtonImage;

    [Header("Audio Button Sprites")]
    public Sprite audioNormalSprite;
    public Sprite audioSelectedSprite;

    [Header("Controls Button Sprites")]
    public Sprite controlsNormalSprite;
    public Sprite controlsSelectedSprite;

    private void Start()
    {
        ShowAudioTab();
    }

    public void ShowAudioTab()
    {
        audioSettingsPanel.SetActive(true);
        controlsSettingsPanel.SetActive(false);

        audioButtonImage.sprite = audioSelectedSprite;
        controlsButtonImage.sprite = controlsNormalSprite;
    }

    public void ShowControlsTab()
    {
        audioSettingsPanel.SetActive(false);
        controlsSettingsPanel.SetActive(true);

        audioButtonImage.sprite = audioNormalSprite;
        controlsButtonImage.sprite = controlsSelectedSprite;
    }
}
