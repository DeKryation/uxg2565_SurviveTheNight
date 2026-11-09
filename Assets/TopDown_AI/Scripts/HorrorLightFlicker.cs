using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Light))]
public class HorrorLightFlicker : MonoBehaviour
{
    [Header("Flicker Settings")]
    public bool enableFlicker = true;

    [Tooltip("Minimum time between flickers.")]
    public float minFlickerInterval = 0.1f;

    [Tooltip("Maximum time between flickers.")]
    public float maxFlickerInterval = 2f;

    [Header("Light Intensity")]
    public float normalIntensity = 3f;
    public float minimumIntensity = 0.2f;

    [Header("Random Blackouts")]
    public bool allowBlackouts = true;

    [Range(0f, 1f)]
    public float blackoutChance = 0.2f;

    public float minBlackoutDuration = 0.1f;
    public float maxBlackoutDuration = 0.8f;

    [Header("Flicker Speed")]
    public float minFlickerDuration = 0.03f;
    public float maxFlickerDuration = 0.15f;

    private Light targetLight;
    private Coroutine flickerCoroutine;

    void Awake()
    {
        targetLight = GetComponent<Light>();
    }

    void OnEnable()
    {
        if (targetLight == null)
            targetLight = GetComponent<Light>();

        flickerCoroutine = StartCoroutine(FlickerRoutine());
    }

    void OnDisable()
    {
        if (flickerCoroutine != null)
        {
            StopCoroutine(flickerCoroutine);
            flickerCoroutine = null;
        }

        if (targetLight != null)
            targetLight.intensity = normalIntensity;
    }

    IEnumerator FlickerRoutine()
    {
        while (true)
        {
            if (!enableFlicker)
            {
                targetLight.intensity = normalIntensity;
                yield return null;
                continue;
            }

            // Light remains normal for a random interval.
            targetLight.intensity = normalIntensity;

            float waitTime = Random.Range(
                minFlickerInterval,
                maxFlickerInterval
            );

            yield return new WaitForSeconds(waitTime);

            // Occasionally turn completely off.
            if (allowBlackouts &&
                Random.value < blackoutChance)
            {
                targetLight.intensity = 0f;

                float blackoutDuration = Random.Range(
                    minBlackoutDuration,
                    maxBlackoutDuration
                );

                yield return new WaitForSeconds(
                    blackoutDuration
                );
            }
            else
            {
                // Briefly dim the light.
                targetLight.intensity = Random.Range(
                    minimumIntensity,
                    normalIntensity * 0.6f
                );

                float flickerDuration = Random.Range(
                    minFlickerDuration,
                    maxFlickerDuration
                );

                yield return new WaitForSeconds(
                    flickerDuration
                );
            }

            // Restore the light.
            targetLight.intensity = normalIntensity;
        }
    }
}
