using UnityEngine;

public class AudioSettingsLoader : MonoBehaviour
{
    private const string MusicVolumeKey = "MusicVolume";
    private const string SFXVolumeKey = "SFXVolume";

    private void Start()
    {
        Invoke(nameof(LoadSavedAudioSettings), 0.1f);
    }

    private void LoadSavedAudioSettings()
    {
        if (SoundManager.Instance == null)
            return;

        float musicValue = PlayerPrefs.GetFloat(
            MusicVolumeKey,
            SoundManager.Instance.musicVolume
        );

        float sfxValue = PlayerPrefs.GetFloat(
            SFXVolumeKey,
            SoundManager.Instance.sfxVolume
        );

        SoundManager.Instance.SetMusicVolume(musicValue);
        SoundManager.Instance.SetSFXVolume(sfxValue);
    }
}
