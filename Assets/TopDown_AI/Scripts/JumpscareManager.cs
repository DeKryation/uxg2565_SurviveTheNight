using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class JumpscareManager : MonoBehaviour
{
    [Serializable]
    public class JumpscareEntry
    {
        public Sprite image;
        public AudioClip sound;
    }

    [Header("Jumpscare UI")]
    public GameObject jumpscarePanel;
    public Image jumpscareImage;

    [Header("Progressive Jumpscare Pairings")]
    public JumpscareEntry[] jumpscares;

    [Header("Jumpscare Settings")]
    public float jumpscareDuration = 1f;
    public bool stayOnLastJumpscare = true;

    [Header("Audio Settings")]
    [Range(0f, 1f)]
    public float jumpscareVolume = 1f;

    private AudioSource audioSource;

    private bool isPlaying = false;

    // Index of the next catch jumpscare.
    private int currentJumpscareIndex = 0;

    // Index used during the most recent catch.
    private int lastPlayedJumpscareIndex = 0;

    // Death jumpscare request, queued if needed.
    private bool deathJumpscareQueued = false;
    private Action queuedDeathCallback;

    public bool IsPlaying => isPlaying;
    public int CurrentJumpscareIndex => currentJumpscareIndex;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f;

        if (jumpscarePanel != null)
            jumpscarePanel.SetActive(false);
    }

    // Called whenever the zombie catches the player.
    public bool PlayCurrentJumpscare(
        Action onFinished = null,
        bool advanceAfterPlay = true)
    {
        if (!CanPlayJumpscare() || isPlaying)
            return false;

        int index = Mathf.Clamp(
            currentJumpscareIndex,
            0,
            jumpscares.Length - 1
        );

        lastPlayedJumpscareIndex = index;

        StartCoroutine(
            PlayCatchRoutine(
                index,
                onFinished,
                advanceAfterPlay
            )
        );

        return true;
    }

    IEnumerator PlayCatchRoutine(
        int index,
        Action onFinished,
        bool advanceAfterPlay)
    {
        isPlaying = true;

        ShowJumpscare(index);

        yield return new WaitForSecondsRealtime(
            Mathf.Max(0.1f, jumpscareDuration)
        );

        HideJumpscare();

        if (advanceAfterPlay)
            AdvanceJumpscareStage();

        // If QTE failed during the first jumpscare,
        // play the death jumpscare before game over.
        if (deathJumpscareQueued)
        {
            deathJumpscareQueued = false;

            Action callback = queuedDeathCallback;
            queuedDeathCallback = null;

            yield return PlayDeathRoutine(callback);
        }
        else
        {
            isPlaying = false;
        }

        onFinished?.Invoke();
    }

    // Called when the player fails the entire QTE.
    public bool PlayDeathJumpscare(Action onFinished)
    {
        if (!CanPlayJumpscare())
            return false;

        // If catch jumpscare is still playing,
        // queue the death jumpscare instead.
        if (isPlaying)
        {
            if (deathJumpscareQueued)
                return false;

            deathJumpscareQueued = true;
            queuedDeathCallback = onFinished;

            return true;
        }

        StartCoroutine(
            PlayDeathRoutine(onFinished)
        );

        return true;
    }

    IEnumerator PlayDeathRoutine(Action onFinished)
    {
        isPlaying = true;

        // Replay the same jumpscare as the last catch.
        int index = Mathf.Clamp(
            lastPlayedJumpscareIndex,
            0,
            jumpscares.Length - 1
        );

        ShowJumpscare(index);

        yield return new WaitForSecondsRealtime(
            Mathf.Max(0.1f, jumpscareDuration)
        );

        HideJumpscare();

        isPlaying = false;

        // Trigger the existing death sequence afterward.
        onFinished?.Invoke();
    }

    void ShowJumpscare(int index)
    {
        JumpscareEntry selected = jumpscares[index];

        jumpscareImage.sprite = selected.image;
        jumpscareImage.enabled = selected.image != null;

        jumpscarePanel.SetActive(true);

        if (audioSource != null)
        {
            audioSource.Stop();

            if (selected.sound != null)
            {
                audioSource.PlayOneShot(
                    selected.sound,
                    jumpscareVolume
                );
            }
        }
    }

    void HideJumpscare()
    {
        if (jumpscarePanel != null)
            jumpscarePanel.SetActive(false);
    }

    bool CanPlayJumpscare()
    {
        return jumpscarePanel != null &&
               jumpscareImage != null &&
               jumpscares != null &&
               jumpscares.Length > 0;
    }

    // Move to the next jumpscare after each catch.
    public void AdvanceJumpscareStage()
    {
        if (jumpscares == null || jumpscares.Length == 0)
            return;

        if (currentJumpscareIndex < jumpscares.Length - 1)
        {
            currentJumpscareIndex++;
        }
        else if (!stayOnLastJumpscare)
        {
            currentJumpscareIndex = 0;
        }
    }

    // Use when starting a completely new run.
    public void ResetJumpscareSequence()
    {
        currentJumpscareIndex = 0;
        lastPlayedJumpscareIndex = 0;

        deathJumpscareQueued = false;
        queuedDeathCallback = null;
    }

    public void SetJumpscareStage(int index)
    {
        if (jumpscares == null || jumpscares.Length == 0)
            return;

        currentJumpscareIndex = Mathf.Clamp(
            index,
            0,
            jumpscares.Length - 1
        );
    }
}
