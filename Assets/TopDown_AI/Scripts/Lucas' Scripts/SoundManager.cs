using UnityEngine;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;
    public AudioSource footstepSource;

    [Header("Music")]
    public AudioClip mainMenuMusic;
    public AudioClip levelMusic;
    public string mainMenuSceneName = "MainMenu";

    [Header("SFX Clips")]
    public AudioClip playerShootSFX;
    public AudioClip playerKnifeSFX;
    public AudioClip playerHitSFX;
    public AudioClip playerDeathSFX;
    public AudioClip enemyDeathSFX;
    public AudioClip pickupExplosiveSFX;
    public AudioClip reloadSFX;
    public AudioClip buttonClickSFX;
    public AudioClip enemyGunshotSFX;
    public AudioClip buttonHoverSFX;
    public AudioClip footstepSFX;
    public AudioClip sceneTransitionSFX;
    public AudioClip[] enemyDeathSFXVariations;

    [Header("Overall Volume")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float musicVolume = 0.6f;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    [Header("Individual SFX Volume")]
    [Range(0f, 1f)] public float playerShootVolume = 1f;
    [Range(0f, 1f)] public float playerKnifeVolume = 1f;
    [Range(0f, 1f)] public float playerHitVolume = 1f;
    [Range(0f, 1f)] public float playerDeathVolume = 1f;
    [Range(0f, 1f)] public float enemyDeathVolume = 1f;
    [Range(0f, 1f)] public float pickupExplosiveVolume = 1f;
    [Range(0f, 1f)] public float reloadVolume = 1f;
    [Range(0f, 1f)] public float buttonClickVolume = 1f;
    [Range(0f, 1f)] public float enemyGunshotVolume = 1f;
    [Range(0f, 1f)] public float buttonHoverVolume = 1f;
    [Range(0f, 1f)] public float footstepVolume = 1f;
    [Range(0f, 1f)] public float sceneTransitionVolume = 1f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SetupAudioSources();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        PlayMusicForCurrentScene();
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayMusicForCurrentScene();
    }

    void PlayMusicForCurrentScene()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;

        if (currentSceneName == mainMenuSceneName)
        {
            PlayMusic(mainMenuMusic);
        }
        else
        {
            PlayMusic(levelMusic);
        }
    }

    void SetupAudioSources()
    {
        if (musicSource == null)
        {
            GameObject musicObject = new GameObject("Music Source");
            musicObject.transform.SetParent(transform);
            musicSource = musicObject.AddComponent<AudioSource>();
        }

        if (sfxSource == null)
        {
            GameObject sfxObject = new GameObject("SFX Source");
            sfxObject.transform.SetParent(transform);
            sfxSource = sfxObject.AddComponent<AudioSource>();
        }

        if (footstepSource == null)
        {
            GameObject footstepObject = new GameObject("Footstep Source");
            footstepObject.transform.SetParent(transform);
            footstepSource = footstepObject.AddComponent<AudioSource>();
        }

        musicSource.loop = true;
        musicSource.playOnAwake = false;

        sfxSource.loop = false;
        sfxSource.playOnAwake = false;

        footstepSource.loop = true;
        footstepSource.playOnAwake = false;

        ApplyVolume();
    }

    public void PlayMusic(AudioClip musicClip)
    {
        if (musicClip == null || musicSource == null)
            return;

        if (musicSource.clip == musicClip && musicSource.isPlaying)
            return;

        musicSource.clip = musicClip;
        musicSource.Play();
        ApplyVolume();
    }

    public void StopMusic()
    {
        if (musicSource != null)
            musicSource.Stop();
    }

    public void PlaySFX(AudioClip clip)
    {
        PlaySFX(clip, 1f);
    }

    public void PlaySFX(AudioClip clip, float individualVolume)
    {
        if (clip == null || sfxSource == null)
            return;

        float finalVolume = masterVolume * sfxVolume * Mathf.Clamp01(individualVolume);
        sfxSource.PlayOneShot(clip, finalVolume);
    }

    public void PlaySFXAtPosition(AudioClip clip, Vector3 position)
    {
        PlaySFXAtPosition(clip, position, 1f);
    }

    public void PlaySFXAtPosition(AudioClip clip, Vector3 position, float individualVolume)
    {
        if (clip == null)
            return;

        float finalVolume = masterVolume * sfxVolume * Mathf.Clamp01(individualVolume);
        AudioSource.PlayClipAtPoint(clip, position, finalVolume);
    }

    public void ApplyVolume()
    {
        if (musicSource != null)
            musicSource.volume = masterVolume * musicVolume;

        if (sfxSource != null)
            sfxSource.volume = masterVolume * sfxVolume;

        if (footstepSource != null)
            footstepSource.volume = masterVolume * sfxVolume * footstepVolume;
    }

    public void SetMasterVolume(float value)
    {
        masterVolume = Mathf.Clamp01(value);
        ApplyVolume();
    }

    public void SetMusicVolume(float value)
    {
        musicVolume = Mathf.Clamp01(value);
        ApplyVolume();
    }

    public void SetSFXVolume(float value)
    {
        sfxVolume = Mathf.Clamp01(value);
        ApplyVolume();
    }

    public static void PlayPlayerShoot()
    {
        if (Instance != null)
            Instance.PlaySFX(Instance.playerShootSFX, Instance.playerShootVolume);
    }

    public static void PlayPlayerKnife()
    {
        if (Instance != null)
            Instance.PlaySFX(Instance.playerKnifeSFX, Instance.playerKnifeVolume);
    }

    public static void PlayPlayerHit()
    {
        if (Instance != null)
            Instance.PlaySFX(Instance.playerHitSFX, Instance.playerHitVolume);
    }

    public static void PlayPlayerDeath()
    {
        if (Instance != null)
            Instance.PlaySFX(Instance.playerDeathSFX, Instance.playerDeathVolume);
    }

    public static void PlayEnemyDeath()
    {
        if (Instance == null)
            return;

        // Always play the default enemy death sound.
        Instance.PlaySFX(Instance.enemyDeathSFX, Instance.enemyDeathVolume);

        // Also play 1 random optional variation, if any are assigned.
        if (Instance.enemyDeathSFXVariations != null && Instance.enemyDeathSFXVariations.Length > 0)
        {
            AudioClip chosenVariation = Instance.enemyDeathSFXVariations[
                Random.Range(0, Instance.enemyDeathSFXVariations.Length)
            ];

            if (chosenVariation != null)
            {
                Instance.PlaySFX(chosenVariation, Instance.enemyDeathVolume);
            }
        }
    }

    public static void PlayPickupExplosive()
    {
        if (Instance != null)
            Instance.PlaySFX(Instance.pickupExplosiveSFX, Instance.pickupExplosiveVolume);
    }

    public static void PlayReload()
    {
        if (Instance != null)
            Instance.PlaySFX(Instance.reloadSFX, Instance.reloadVolume);
    }

    public static void PlayEnemyGunshot()
    {
        if (Instance != null)
            Instance.PlaySFX(Instance.enemyGunshotSFX, Instance.enemyGunshotVolume);
    }

    public static void PlayButtonClick()
    {
        if (Instance != null)
            Instance.PlaySFX(Instance.buttonClickSFX, Instance.buttonClickVolume);
    }
    public static void PlayButtonHover()
    {
        if (Instance != null)
            Instance.PlaySFX(Instance.buttonHoverSFX, Instance.buttonHoverVolume);
    }
    public static void PlaySceneTransition()
    {
        if (Instance != null)
            Instance.PlaySFX(Instance.sceneTransitionSFX, Instance.sceneTransitionVolume);
    }
    public static void StartFootsteps()
    {
        if (Instance == null) return;
        if (Instance.footstepSFX == null) return;
        if (Instance.footstepSource == null) return;

        if (!Instance.footstepSource.isPlaying)
        {
            Instance.footstepSource.clip = Instance.footstepSFX;
            Instance.footstepSource.loop = true;
            Instance.footstepSource.volume = Instance.masterVolume * Instance.sfxVolume * Instance.footstepVolume;
            Instance.footstepSource.Play();
        }
    }
    public static void StopFootsteps()
    {
        if (Instance == null) return;
        if (Instance.footstepSource == null) return;

        if (Instance.footstepSource.isPlaying)
        {
            Instance.footstepSource.Stop();
        }
    }
}
