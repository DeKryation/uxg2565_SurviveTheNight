using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(BoxCollider))]
public class AudioZone : MonoBehaviour
{
    [Header("Zone Enter SFX Variations")]
    public AudioClip[] zoneEnterSFXVariations;

    [Range(0f, 1f)]
    public float zoneEnterVolume = 1f;

    [Header("Zone Enter SFX Cooldown")]
    [Min(0f)]
    public float enterSFXCooldown = 15f;

    [Header("Footstep SFX Variations")]
    public AudioClip[] zoneFootstepSFXVariations;

    [Range(0f, 2f)]
    public float zoneFootstepVolume = 1f;

    [Header("Looping Ambient SFX")]
    public bool enableLoopingSFX = true;

    public AudioClip[] loopingSFXVariations;

    [Range(0f, 1f)]
    public float loopingSFXVolume = 0.8f;

    [Header("Looping SFX Fade")]
    [Min(0f)]
    public float fadeInDuration = 1.5f;

    [Min(0f)]
    public float fadeOutDuration = 3f;

    [Header("3D Audio Distance")]
    [Tooltip("Optional: Where the ambient sound comes from.")]
    public Transform soundOrigin;

    public float minDistance = 2f;
    public float maxDistance = 20f;

    [Header("Settings")]
    public bool playEnterSound = true;
    public bool changeFootsteps = true;

    private readonly HashSet<Collider> playerColliders =
        new HashSet<Collider>();

    private AudioClip selectedFootstepClip;
    private AudioSource loopingAudioSource;
    private Coroutine fadeCoroutine;

    private float nextAllowedEnterSFXTime = 0f;
    private float currentFadeVolume = 0f;

    private BoxCollider zoneCollider;

    void Awake()
    {
        zoneCollider = GetComponent<BoxCollider>();
        zoneCollider.isTrigger = true;

        SetupLoopingAudio();
    }

    void Start()
    {
        UpdateAudioPosition();
    }

    void Update()
    {
        // Follow the assigned sound origin.
        UpdateAudioPosition();

        // Update volume to respect SoundManager settings.
        UpdateLoopingVolume();
    }

    void SetupLoopingAudio()
    {
        GameObject audioObject =
            new GameObject("Zone Looping Audio");

        audioObject.transform.SetParent(transform);

        loopingAudioSource =
            audioObject.AddComponent<AudioSource>();

        loopingAudioSource.playOnAwake = false;
        loopingAudioSource.loop = true;

        // Fully spatial 3D sound.
        loopingAudioSource.spatialBlend = 1f;

        loopingAudioSource.rolloffMode =
            AudioRolloffMode.Linear;

        loopingAudioSource.minDistance =
            Mathf.Max(0.01f, minDistance);

        loopingAudioSource.maxDistance =
            Mathf.Max(
                loopingAudioSource.minDistance + 0.01f,
                maxDistance
            );

        loopingAudioSource.dopplerLevel = 0f;

        loopingAudioSource.volume = 0f;
    }

    void OnTriggerEnter(Collider other)
    {
        PlayerBehavior player =
            other.GetComponentInParent<PlayerBehavior>();

        if (player == null)
            return;

        if (!playerColliders.Add(other))
            return;

        // Only activate on the first player collider.
        if (playerColliders.Count > 1)
            return;

        // ----------------------------
        // ZONE ENTER SFX
        // ----------------------------

        if (playEnterSound &&
            Time.time >= nextAllowedEnterSFXTime)
        {
            AudioClip enterClip =
                GetRandomClip(zoneEnterSFXVariations);

            if (enterClip != null &&
                SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySFX(
                    enterClip,
                    zoneEnterVolume
                );

                nextAllowedEnterSFXTime =
                    Time.time + enterSFXCooldown;
            }
        }

        // ----------------------------
        // FOOTSTEP OVERRIDE
        // ----------------------------

        if (changeFootsteps)
        {
            selectedFootstepClip =
                GetRandomClip(zoneFootstepSFXVariations);

            if (selectedFootstepClip != null)
            {
                SoundManager.EnterFootstepZone(
                    GetInstanceID(),
                    selectedFootstepClip,
                    zoneFootstepVolume
                );
            }
        }

        // ----------------------------
        // LOOPING AMBIENT SFX
        // ----------------------------

        if (enableLoopingSFX)
        {
            StartLoopingSFX();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!playerColliders.Remove(other))
            return;

        if (playerColliders.Count > 0)
            return;

        LeaveZone();
    }

    void LeaveZone()
    {
        // Restore previous/default footsteps.
        SoundManager.ExitFootstepZone(GetInstanceID());

        selectedFootstepClip = null;

        // Gradually fade out the ambient sound.
        StopLoopingSFX();
    }

    // ====================================
    // LOOPING AUDIO
    // ====================================

    void StartLoopingSFX()
    {
        if (loopingAudioSource == null)
            return;

        // Cancel any ongoing fade out.
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }

        // If the sound is already playing,
        // continue the same clip.
        if (!loopingAudioSource.isPlaying)
        {
            AudioClip selectedClip =
                GetRandomClip(loopingSFXVariations);

            if (selectedClip == null)
                return;

            loopingAudioSource.clip = selectedClip;
            loopingAudioSource.Play();
        }

        fadeCoroutine = StartCoroutine(
            FadeLoopingAudio(1f, fadeInDuration, false)
        );
    }

    void StopLoopingSFX()
    {
        if (loopingAudioSource == null ||
            !loopingAudioSource.isPlaying)
            return;

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }

        fadeCoroutine = StartCoroutine(
            FadeLoopingAudio(0f, fadeOutDuration, true)
        );
    }

    IEnumerator FadeLoopingAudio(
        float target,
        float duration,
        bool stopWhenFinished)
    {
        float startVolume = currentFadeVolume;
        float elapsed = 0f;

        if (duration <= 0f)
        {
            currentFadeVolume = target;
            UpdateLoopingVolume();
        }
        else
        {
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;

                float t = Mathf.Clamp01(
                    elapsed / duration
                );

                // Smooth fade curve.
                t = Mathf.SmoothStep(0f, 1f, t);

                currentFadeVolume = Mathf.Lerp(
                    startVolume,
                    target,
                    t
                );

                UpdateLoopingVolume();

                yield return null;
            }

            currentFadeVolume = target;
            UpdateLoopingVolume();
        }

        if (stopWhenFinished &&
            loopingAudioSource != null)
        {
            loopingAudioSource.Stop();
        }

        fadeCoroutine = null;
    }

    void UpdateLoopingVolume()
    {
        if (loopingAudioSource == null)
            return;

        float masterVolume = 1f;
        float sfxVolume = 1f;

        if (SoundManager.Instance != null)
        {
            masterVolume =
                SoundManager.Instance.masterVolume;

            sfxVolume =
                SoundManager.Instance.sfxVolume;
        }

        loopingAudioSource.volume =
            Mathf.Clamp01(
                loopingSFXVolume *
                currentFadeVolume *
                masterVolume *
                sfxVolume
            );

        // Keep the distance settings adjustable.
        loopingAudioSource.minDistance =
            Mathf.Max(0.01f, minDistance);

        loopingAudioSource.maxDistance =
            Mathf.Max(
                loopingAudioSource.minDistance + 0.01f,
                maxDistance
            );
    }

    void UpdateAudioPosition()
    {
        if (loopingAudioSource == null)
            return;

        if (soundOrigin != null)
        {
            loopingAudioSource.transform.position =
                soundOrigin.position;
        }
        else
        {
            // Use the centre of the trigger zone.
            loopingAudioSource.transform.position =
                zoneCollider.bounds.center;
        }
    }

    // ====================================
    // CLEANUP
    // ====================================

    void OnDisable()
    {
        if (playerColliders.Count > 0)
        {
            SoundManager.ExitFootstepZone(GetInstanceID());
            playerColliders.Clear();
        }

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }

        if (loopingAudioSource != null)
        {
            loopingAudioSource.Stop();
            loopingAudioSource.volume = 0f;
        }

        currentFadeVolume = 0f;
        selectedFootstepClip = null;
    }

    // ====================================
    // RANDOM AUDIO SELECTION
    // ====================================

    AudioClip GetRandomClip(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0)
            return null;

        List<AudioClip> validClips =
            new List<AudioClip>();

        foreach (AudioClip clip in clips)
        {
            if (clip != null)
                validClips.Add(clip);
        }

        if (validClips.Count == 0)
            return null;

        return validClips[
            Random.Range(0, validClips.Count)
        ];
    }
}