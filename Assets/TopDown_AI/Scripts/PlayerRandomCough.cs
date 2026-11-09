using UnityEngine;
using System.Collections;

public class PlayerRandomCough : MonoBehaviour
{
    [Header("Random Cough Timing")]
    public float minimumCoughInterval = 30f;
    public float maximumCoughInterval = 60f;

    [Header("Random Scream Timing")]
    public float minimumScreamInterval = 30f;
    public float maximumScreamInterval = 60f;

    [Header("References")]
    public PlayerBehavior playerBehavior;
    public PlayerGrabQTE playerQTE;

    [Header("Settings")]
    public bool disableSoundsDuringQTE = true;

    private Coroutine coughCoroutine;
    private Coroutine screamCoroutine;

    void Start()
    {
        if (playerBehavior == null)
            playerBehavior = GetComponent<PlayerBehavior>();

        if (playerQTE == null)
            playerQTE = GetComponent<PlayerGrabQTE>();

        coughCoroutine = StartCoroutine(CoughRoutine());
        screamCoroutine = StartCoroutine(ScreamRoutine());
    }

    IEnumerator CoughRoutine()
    {
        while (true)
        {
            float nextCoughTime = Random.Range(
                Mathf.Min(minimumCoughInterval, maximumCoughInterval),
                Mathf.Max(minimumCoughInterval, maximumCoughInterval)
            );

            yield return new WaitForSeconds(
                Mathf.Max(0.1f, nextCoughTime)
            );

            if (CanPlayRandomVoice())
            {
                SoundManager.PlayRandomPlayerCough();
            }
        }
    }

    IEnumerator ScreamRoutine()
    {
        while (true)
        {
            float nextScreamTime = Random.Range(
                Mathf.Min(minimumScreamInterval, maximumScreamInterval),
                Mathf.Max(minimumScreamInterval, maximumScreamInterval)
            );

            yield return new WaitForSeconds(
                Mathf.Max(0.1f, nextScreamTime)
            );

            if (CanPlayRandomVoice())
            {
                SoundManager.PlayRandomPlayerScream();
            }
        }
    }

    bool CanPlayRandomVoice()
    {
        // Player must still be alive and controllable.
        if (playerBehavior == null || !playerBehavior.enabled)
            return false;

        // Prevent random voice sounds during QTE grabs.
        if (disableSoundsDuringQTE &&
            playerQTE != null &&
            playerQTE.IsActive)
        {
            return false;
        }

        if (Time.timeScale <= 0f)
            return false;

        return true;
    }

    void OnDisable()
    {
        if (coughCoroutine != null)
        {
            StopCoroutine(coughCoroutine);
            coughCoroutine = null;
        }

        if (screamCoroutine != null)
        {
            StopCoroutine(screamCoroutine);
            screamCoroutine = null;
        }
    }
}