using UnityEngine;
using UnityEngine.UI;

public class AudioSettingsUI : MonoBehaviour
{
    private const string MusicVolumeKey = "MusicVolume";
    private const string SFXVolumeKey = "SFXVolume";

    [Header("Sliders")]
    public Slider musicSlider;
    public Slider sfxSlider;

    private void Start()
    {
        SetupSliders();
        LoadSliders();
    }

    private void OnEnable()
    {
        SetupSliders();
        LoadSliders();
    }

    private void SetupSliders()
    {
        musicSlider.minValue = 0f;
        musicSlider.maxValue = 1f;
        musicSlider.wholeNumbers = false;

        sfxSlider.minValue = 0f;
        sfxSlider.maxValue = 1f;
        sfxSlider.wholeNumbers = false;

        musicSlider.onValueChanged.RemoveAllListeners();
        sfxSlider.onValueChanged.RemoveAllListeners();

        musicSlider.onValueChanged.AddListener(ChangeMusicVolume);
        sfxSlider.onValueChanged.AddListener(ChangeSFXVolume);
    }

    private void LoadSliders()
    {
        if (SoundManager.Instance == null)
            return;

        musicSlider.value = SoundManager.Instance.musicVolume;
        sfxSlider.value = SoundManager.Instance.sfxVolume;
    }

    public void ChangeMusicVolume(float value)
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.SetMusicVolume(value);
    }

    public void ChangeSFXVolume(float value)
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.SetSFXVolume(value);
    }

    public void ConfirmSettings()
    {
        if (SoundManager.Instance == null)
            return;

        PlayerPrefs.SetFloat(MusicVolumeKey, SoundManager.Instance.musicVolume);
        PlayerPrefs.SetFloat(SFXVolumeKey, SoundManager.Instance.sfxVolume);
        PlayerPrefs.Save();

        SoundManager.PlayButtonClick();
    }
}
