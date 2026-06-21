using UnityEngine;
using System.Collections;

public class MuzzleFlashLight : MonoBehaviour
{
    [Header("Light")]
    public Light muzzleLight;

    [Header("Flash Settings")]
    public float flashIntensity = 4f;
    public float flashDuration = 0.05f;

    private Coroutine flashRoutine;

    void Awake()
    {
        if (muzzleLight == null)
        {
            muzzleLight = GetComponent<Light>();
        }

        if (muzzleLight != null)
        {
            muzzleLight.intensity = 0f;
        }
    }

    public void Flash()
    {
        if (muzzleLight == null) return;

        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
        }

        flashRoutine = StartCoroutine(FlashRoutine());
    }

    IEnumerator FlashRoutine()
    {
        muzzleLight.intensity = flashIntensity;

        yield return new WaitForSeconds(flashDuration);

        muzzleLight.intensity = 0f;
    }
}
