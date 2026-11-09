using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Main Menu Music Fade In")]
    public bool enableMainMenuFadeIn = true;
    public float mainMenuFadeDuration = 3f;

    private Coroutine musicFadeCoroutine;
    private float musicFadeMultiplier = 1f;
    private float sceneMusicVolumeMultiplier = 1f;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;
    public AudioSource footstepSource;

    [Header("Music")]
    public AudioClip mainMenuMusic;
    public AudioClip levelMusic;
    public string mainMenuSceneName = "MainMenu";
    [System.Serializable]
    public class SceneMusicEntry
    {
        public string sceneName;
        public AudioClip musicClip;

        [Header("Music Settings")]
        [Range(0f, 1f)]
        public float volume = 1f;

        [Tooltip("Should this scene's music fade in?")]
        public bool fadeIn = false;
    }

    [Header("Scene-Specific Background Music")]
    public SceneMusicEntry[] sceneMusicOverrides;

    [Header("SFX Clips")]
    public AudioClip playerShootSFX;
    public AudioClip playerKnifeSFX;
    public AudioClip playerHitSFX;
    public AudioClip[] playerHitSFXVariations;
    public AudioClip playerDeathSFX;
    public AudioClip enemyDeathSFX;
    public AudioClip[] pickupExplosiveSFXVariations;
    public AudioClip monsterTriggeredSFX;
    public AudioClip reloadSFX;
    public AudioClip buttonClickSFX;
    public AudioClip enemyGunshotSFX;
    public AudioClip buttonHoverSFX;
    public AudioClip footstepSFX;
    public AudioClip sceneTransitionSFX;
    public AudioClip[] enemyDeathSFXVariations;
    public AudioClip playerHuffPuffSFX;
    public AudioClip[] playerCoughSFX;
    public AudioClip[] playerQTEShoutSFX;
    public AudioClip pickupKeySFX;
    public AudioClip[] playerRandomScreamSFX;

    [Header("Tension SFX")]
    public AudioClip tensionLowSFX;
    public AudioClip tensionMediumSFX;
    public AudioClip tensionHighSFX;

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
    [Range(0f, 1f)] public float monsterTriggeredVolume = 1f;
    [Range(0f, 1f)] public float reloadVolume = 1f;
    [Range(0f, 1f)] public float buttonClickVolume = 1f;
    [Range(0f, 1f)] public float enemyGunshotVolume = 1f;
    [Range(0f, 1f)] public float buttonHoverVolume = 1f;
    [Range(0f, 1f)] public float footstepVolume = 1f;
    [Range(0f, 1f)] public float sceneTransitionVolume = 1f;
    [Range(0f, 1f)] public float tensionLowVolume = 1f;
    [Range(0f, 1f)] public float tensionMediumVolume = 1f;
    [Range(0f, 1f)] public float tensionHighVolume = 1f;
    [Range(0f, 1f)] public float playerHuffPuffVolume = 1f;
    [Range(0f, 1f)] public float playerCoughVolume = 0.8f;
    [Range(0f, 1f)] public float playerQTEShoutVolume = 1f;
    [Range(0f, 1f)] public float pickupKeyVolume = 1f;
    [Range(0f, 1f)] public float playerRandomScreamVolume = 0.8f;

    [Header("Low Health Latin Chant")]
    public AudioClip latinChantSFX;

    [Range(0f, 1f)]
    public float latinChantVolume = 0.6f;

    private AudioSource latinChantSource;

    [Header("Audio Zone Footsteps")]
    private List<FootstepZoneData> activeFootstepZones =
    new List<FootstepZoneData>();

    [Header("Third Hit Looping SFX")]
    public AudioClip thirdHitSFX1;
    public AudioClip thirdHitSFX2;

    [Range(0f, 1f)]
    public float thirdHitVolume1 = 0.7f;

    [Range(0f, 1f)]
    public float thirdHitVolume2 = 0.7f;

    [Min(0f)]
    public float thirdHitSecondSoundDelay = 3f;

    private AudioSource thirdHitSource1;
    private AudioSource thirdHitSource2;

    private Coroutine thirdHitDelayCoroutine;
    private bool thirdHitLoopsActive = false;

    [Header("QTE Success / Miss SFX")]
    public AudioClip qteSuccessSFX;
    public AudioClip[] qteMissSFXVariations;

    [Range(0f, 1f)]
    public float qteSuccessVolume = 0.8f;

    [Range(0f, 1f)]
    public float qteMissVolume = 0.8f;

    private class FootstepZoneData
    {
        public int zoneID;
        public AudioClip clip;
        public float volume;

        public FootstepZoneData(
            int id,
            AudioClip footstepClip,
            float footstepVolume)
        {
            zoneID = id;
            clip = footstepClip;
            volume = footstepVolume;
        }
    }

    private float currentZoneFootstepVolume = 1f;

    public static void EnterFootstepZone(
    int zoneID,
    AudioClip footstepClip,
    float volume = 1f)
    {
        if (Instance == null || footstepClip == null)
            return;

        // Remove this zone if already registered.
        Instance.RemoveFootstepZone(zoneID);

        // Most recently entered zone takes priority.
        Instance.activeFootstepZones.Add(
            new FootstepZoneData(
                zoneID,
                footstepClip,
                volume
            )
        );

        Instance.RefreshFootstepAudio();
    }

    public static void ExitFootstepZone(int zoneID)
    {
        if (Instance == null)
            return;

        Instance.RemoveFootstepZone(zoneID);
        Instance.RefreshFootstepAudio();
    }

    private void RemoveFootstepZone(int zoneID)
    {
        activeFootstepZones.RemoveAll(
            zone => zone.zoneID == zoneID
        );
    }

    private void RefreshFootstepAudio()
    {
        if (footstepSource == null)
            return;

        AudioClip selectedClip = footstepSFX;
        currentZoneFootstepVolume = 1f;

        // Use the most recently entered active zone.
        if (activeFootstepZones.Count > 0)
        {
            FootstepZoneData currentZone =
                activeFootstepZones[
                    activeFootstepZones.Count - 1
                ];

            selectedClip = currentZone.clip;

            currentZoneFootstepVolume =
                currentZone.volume;
        }

        bool wasPlaying = footstepSource.isPlaying;

        // Only restart the audio if the clip changes.
        if (footstepSource.clip != selectedClip)
        {
            footstepSource.Stop();
            footstepSource.clip = selectedClip;

            if (wasPlaying && selectedClip != null)
            {
                footstepSource.loop = true;
                footstepSource.Play();
            }
        }

        ApplyVolume();
    }

    private AudioClip GetCurrentFootstepClip()
    {
        if (activeFootstepZones.Count > 0)
        {
            return activeFootstepZones[
                activeFootstepZones.Count - 1
            ].clip;
        }

        return footstepSFX;
    }

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
        string currentSceneName =
            SceneManager.GetActiveScene().name;

        // Main Menu music
        if (currentSceneName == mainMenuSceneName)
        {
            sceneMusicVolumeMultiplier = 1f;

            PlayMusic(
                mainMenuMusic,
                enableMainMenuFadeIn
            );

            ApplyVolume();
            return;
        }

        // Scene-specific BGM overrides
        if (sceneMusicOverrides != null)
        {
            foreach (SceneMusicEntry entry in sceneMusicOverrides)
            {
                if (entry == null)
                    continue;

                if (currentSceneName == entry.sceneName &&
                    entry.musicClip != null)
                {
                    sceneMusicVolumeMultiplier =
                        Mathf.Clamp01(entry.volume);

                    PlayMusic(
                        entry.musicClip,
                        entry.fadeIn
                    );

                    ApplyVolume();
                    return;
                }
            }
        }

        // Default level music
        sceneMusicVolumeMultiplier = 1f;

        PlayMusic(levelMusic, false);
        ApplyVolume();
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

        if (latinChantSource == null)
        {
            GameObject chantObject = new GameObject("Latin Chant Source");
            chantObject.transform.SetParent(transform);

            latinChantSource = chantObject.AddComponent<AudioSource>();
        }

        latinChantSource.playOnAwake = false;
        latinChantSource.loop = true;
        latinChantSource.spatialBlend = 0f;
        latinChantSource.clip = latinChantSFX;

        musicSource.loop = true;
        musicSource.playOnAwake = false;

        sfxSource.loop = false;
        sfxSource.playOnAwake = false;

        footstepSource.loop = true;
        footstepSource.playOnAwake = false;

        ApplyVolume();
    }

    public void PlayMusic(AudioClip musicClip, bool fadeIn = false)
    {
        if (musicClip == null || musicSource == null)
            return;

        // Don't restart the same music unnecessarily.
        if (musicSource.clip == musicClip && musicSource.isPlaying)
            return;

        // Cancel any previous fade.
        if (musicFadeCoroutine != null)
        {
            StopCoroutine(musicFadeCoroutine);
            musicFadeCoroutine = null;
        }

        musicSource.Stop();
        musicSource.clip = musicClip;

        if (fadeIn && mainMenuFadeDuration > 0f)
        {
            musicFadeMultiplier = 0f;
            ApplyVolume();

            musicSource.Play();

            musicFadeCoroutine =
                StartCoroutine(FadeInMusic());
        }
        else
        {
            musicFadeMultiplier = 1f;
            ApplyVolume();

            musicSource.Play();
        }
    }

    private IEnumerator FadeInMusic()
    {
        float elapsed = 0f;

        while (elapsed < mainMenuFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(
                elapsed / mainMenuFadeDuration
            );

            // Smooth, gradual fade.
            musicFadeMultiplier =
                Mathf.SmoothStep(0f, 1f, progress);

            ApplyVolume();

            yield return null;
        }

        musicFadeMultiplier = 1f;
        ApplyVolume();

        musicFadeCoroutine = null;
    }

    public static void PlayTensionLow()
    {
        if (Instance != null)
            Instance.PlaySFX(Instance.tensionLowSFX, Instance.tensionLowVolume);
    }

    public static void PlayTensionMedium()
    {
        if (Instance != null)
            Instance.PlaySFX(Instance.tensionMediumSFX, Instance.tensionMediumVolume);
    }

    public static void PlayTensionHigh()
    {
        if (Instance != null)
            Instance.PlaySFX(Instance.tensionHighSFX, Instance.tensionHighVolume);
    }

    public void StopMusic()
    {
        if (musicFadeCoroutine != null)
        {
            StopCoroutine(musicFadeCoroutine);
            musicFadeCoroutine = null;
        }

        musicFadeMultiplier = 1f;

        if (musicSource != null)
            musicSource.Stop();

        ApplyVolume();
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
        {
            musicSource.volume =
                masterVolume *
                musicVolume *
                sceneMusicVolumeMultiplier *
                musicFadeMultiplier;
        }

        if (sfxSource != null)
        {
            sfxSource.volume =
                masterVolume * sfxVolume;
        }

        if (footstepSource != null)
        {
            footstepSource.volume =
                masterVolume *
                sfxVolume *
                footstepVolume *
                currentZoneFootstepVolume;
        }

        if (latinChantSource != null)
        {
            latinChantSource.volume =
                masterVolume *
                sfxVolume *
                latinChantVolume;
        }
        if (thirdHitSource1 != null)
        {
            thirdHitSource1.volume =
                masterVolume *
                sfxVolume *
                thirdHitVolume1;
        }

        if (thirdHitSource2 != null)
        {
            thirdHitSource2.volume =
                masterVolume *
                sfxVolume *
                thirdHitVolume2;
        }
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
        if (Instance == null)
            return;

        // Randomly select one hurt variation.
        if (Instance.playerHitSFXVariations != null &&
            Instance.playerHitSFXVariations.Length > 0)
        {
            System.Collections.Generic.List<AudioClip> validClips =
                new System.Collections.Generic.List<AudioClip>();

            foreach (AudioClip clip in Instance.playerHitSFXVariations)
            {
                if (clip != null)
                    validClips.Add(clip);
            }

            if (validClips.Count > 0)
            {
                AudioClip selectedHurt = validClips[
                    Random.Range(0, validClips.Count)
                ];

                Instance.PlaySFX(
                    selectedHurt,
                    Instance.playerHitVolume
                );

                return;
            }
        }

        // Fallback if no variations are assigned.
        Instance.PlaySFX(
            Instance.playerHitSFX,
            Instance.playerHitVolume
        );
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
        if (Instance == null)
            return;

        if (Instance.pickupExplosiveSFXVariations == null ||
            Instance.pickupExplosiveSFXVariations.Length == 0)
            return;

        // Collect only assigned audio clips.
        System.Collections.Generic.List<AudioClip> validClips =
            new System.Collections.Generic.List<AudioClip>();

        foreach (AudioClip clip in Instance.pickupExplosiveSFXVariations)
        {
            if (clip != null)
                validClips.Add(clip);
        }

        if (validClips.Count == 0)
            return;

        // Play one randomly selected explosive pickup sound.
        AudioClip selectedClip = validClips[
            Random.Range(0, validClips.Count)
        ];

        Instance.PlaySFX(
            selectedClip,
            Instance.pickupExplosiveVolume
        );
    }

    public static void PlayMonsterTriggered()
    {
        if (Instance == null)
            return;

        Instance.PlaySFX(
            Instance.monsterTriggeredSFX,
            Instance.monsterTriggeredVolume
        );
    }

    public static void PlayMonsterTriggeredDelayed()
    {
        if (Instance == null)
            return;

        Instance.StartCoroutine(
            Instance.MonsterTriggeredDelayRoutine()
        );
    }

    private System.Collections.IEnumerator MonsterTriggeredDelayRoutine()
    {
        yield return new WaitForSeconds(1f);

        PlayMonsterTriggered();
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
        if (Instance == null)
            return;

        if (Instance.footstepSource == null)
            return;

        AudioClip selectedClip =
            Instance.GetCurrentFootstepClip();

        if (selectedClip == null)
            return;

        // Update the clip if the active zone has changed.
        if (Instance.footstepSource.clip != selectedClip)
        {
            Instance.footstepSource.Stop();
            Instance.footstepSource.clip = selectedClip;
        }

        if (!Instance.footstepSource.isPlaying)
        {
            Instance.footstepSource.loop = true;
            Instance.ApplyVolume();
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
    public static void PlayPlayerHuffPuff()
    {
        if (Instance != null)
            Instance.PlaySFX(Instance.playerHuffPuffSFX, Instance.playerHuffPuffVolume);
    }

    public static void PlayRandomPlayerCough()
    {
        if (Instance == null)
            return;

        if (Instance.playerCoughSFX == null ||
            Instance.playerCoughSFX.Length == 0)
            return;

        // Pick one random cough from the available clips.
        AudioClip selectedCough = Instance.playerCoughSFX[
            Random.Range(0, Instance.playerCoughSFX.Length)
        ];

        if (selectedCough != null)
        {
            Instance.PlaySFX(
                selectedCough,
                Instance.playerCoughVolume
            );
        }
    }
    public static void PlayRandomPlayerQTEShout()
    {
        if (Instance == null)
            return;

        if (Instance.playerQTEShoutSFX == null ||
            Instance.playerQTEShoutSFX.Length == 0)
            return;

        // Pick a random shouting variation.
        AudioClip selectedShout =
            Instance.playerQTEShoutSFX[
                Random.Range(
                    0,
                    Instance.playerQTEShoutSFX.Length
                )
            ];

        if (selectedShout != null)
        {
            Instance.PlaySFX(
                selectedShout,
                Instance.playerQTEShoutVolume
            );
        }
    }

    public static void StartLatinChant()
    {
        if (Instance == null)
            return;

        if (Instance.latinChantSource == null ||
            Instance.latinChantSFX == null)
            return;

        // Don't restart the chant if it's already playing.
        if (Instance.latinChantSource.isPlaying)
            return;

        Instance.latinChantSource.clip = Instance.latinChantSFX;
        Instance.latinChantSource.loop = true;

        Instance.ApplyVolume();
        Instance.latinChantSource.Play();
    }
    public static void StopLatinChant()
    {
        if (Instance == null ||
            Instance.latinChantSource == null)
            return;

        Instance.latinChantSource.Stop();
    }

    public static void PlayPickupKey()
    {
        if (Instance == null)
            return;

        Instance.PlaySFX(
            Instance.pickupKeySFX,
            Instance.pickupKeyVolume
        );
    }
    private void SetupThirdHitAudio()
    {
        if (thirdHitSource1 == null)
        {
            GameObject obj1 = new GameObject("Third Hit SFX 1");
            obj1.transform.SetParent(transform);

            thirdHitSource1 = obj1.AddComponent<AudioSource>();
        }

        if (thirdHitSource2 == null)
        {
            GameObject obj2 = new GameObject("Third Hit SFX 2");
            obj2.transform.SetParent(transform);

            thirdHitSource2 = obj2.AddComponent<AudioSource>();
        }

        thirdHitSource1.playOnAwake = false;
        thirdHitSource1.loop = true;
        thirdHitSource1.spatialBlend = 0f;

        thirdHitSource2.playOnAwake = false;
        thirdHitSource2.loop = true;
        thirdHitSource2.spatialBlend = 0f;

        thirdHitSource1.clip = thirdHitSFX1;
        thirdHitSource2.clip = thirdHitSFX2;

        ApplyVolume();
    }

    public static void StartThirdHitLoops()
    {
        if (Instance == null)
            return;

        Instance.StartThirdHitLoopsInternal();
    }

    private void StartThirdHitLoopsInternal()
    {
        // Prevent restarting the loops on subsequent damage.
        if (thirdHitLoopsActive)
            return;

        thirdHitLoopsActive = true;

        if (thirdHitSource1 == null || thirdHitSource2 == null)
            SetupThirdHitAudio();

        thirdHitSource1.clip = thirdHitSFX1;
        thirdHitSource2.clip = thirdHitSFX2;

        ApplyVolume();

        // First SFX plays immediately.
        if (thirdHitSFX1 != null)
            thirdHitSource1.Play();

        // Second SFX begins after the specified delay.
        thirdHitDelayCoroutine = StartCoroutine(
            StartSecondThirdHitSFX()
        );
    }

    private System.Collections.IEnumerator StartSecondThirdHitSFX()
    {
        yield return new WaitForSeconds(thirdHitSecondSoundDelay);

        if (thirdHitLoopsActive &&
            thirdHitSource2 != null &&
            thirdHitSFX2 != null)
        {
            thirdHitSource2.Play();
        }

        thirdHitDelayCoroutine = null;
    }

    public static void StopThirdHitLoops()
    {
        if (Instance == null)
            return;

        Instance.StopThirdHitLoopsInternal();
    }

    private void StopThirdHitLoopsInternal()
    {
        thirdHitLoopsActive = false;

        // Cancel the delayed SFX if it hasn't started yet.
        if (thirdHitDelayCoroutine != null)
        {
            StopCoroutine(thirdHitDelayCoroutine);
            thirdHitDelayCoroutine = null;
        }

        if (thirdHitSource1 != null)
            thirdHitSource1.Stop();

        if (thirdHitSource2 != null)
            thirdHitSource2.Stop();
    }

    public static void PlayRandomPlayerScream()
    {
        if (Instance == null)
            return;

        if (Instance.playerRandomScreamSFX == null ||
            Instance.playerRandomScreamSFX.Length == 0)
            return;

        // Gather assigned scream variations.
        System.Collections.Generic.List<AudioClip> validClips =
            new System.Collections.Generic.List<AudioClip>();

        foreach (AudioClip clip in Instance.playerRandomScreamSFX)
        {
            if (clip != null)
                validClips.Add(clip);
        }

        if (validClips.Count == 0)
            return;

        // Randomly choose one scream.
        AudioClip selectedScream = validClips[
            Random.Range(0, validClips.Count)
        ];

        Instance.PlaySFX(
            selectedScream,
            Instance.playerRandomScreamVolume
        );
    }
    public static void PlayQTESuccess()
    {
        if (Instance == null)
            return;

        Instance.PlaySFX(
            Instance.qteSuccessSFX,
            Instance.qteSuccessVolume
        );
    }

    public static void PlayQTEMiss()
    {
        if (Instance == null)
            return;

        if (Instance.qteMissSFXVariations == null ||
            Instance.qteMissSFXVariations.Length == 0)
            return;

        // Collect assigned audio clips.
        System.Collections.Generic.List<AudioClip> validClips =
            new System.Collections.Generic.List<AudioClip>();

        foreach (AudioClip clip in Instance.qteMissSFXVariations)
        {
            if (clip != null)
                validClips.Add(clip);
        }

        if (validClips.Count == 0)
            return;

        // Randomly select one of the miss sounds.
        AudioClip selectedClip = validClips[
            Random.Range(0, validClips.Count)
        ];

        Instance.PlaySFX(
            selectedClip,
            Instance.qteMissVolume
        );
    }
}
