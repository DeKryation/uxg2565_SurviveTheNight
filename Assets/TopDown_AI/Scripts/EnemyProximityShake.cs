using UnityEngine;

public class EnemyProximityShake : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Transform enemy;

    [Header("Shake Distance")]
    public float startShakeDistance = 12f;
    public float maxShakeDistance = 2f;

    [Header("Shake Strength")]
    public float maxShakeStrength = 0.12f;
    public float shakeSpeed = 20f;
    public float smoothingSpeed = 5f;

    [Header("Settings")]
    public bool enableShake = true;

    private Vector3 currentOffset;
    private Vector3 lastOffset;
    private float currentStrength = 0f;

    void LateUpdate()
    {
        // Remove the offset applied during the previous frame.
        transform.localPosition -= lastOffset;
        lastOffset = Vector3.zero;

        if (!enableShake || player == null || enemy == null ||
            Time.timeScale == 0f)
        {
            currentStrength = Mathf.Lerp(
                currentStrength, 0f,
                Time.unscaledDeltaTime * smoothingSpeed
            );

            return;
        }

        // Calculate horizontal distance between player and enemy.
        Vector3 playerPos = player.position;
        Vector3 enemyPos = enemy.position;

        playerPos.y = 0f;
        enemyPos.y = 0f;

        float distance = Vector3.Distance(playerPos, enemyPos);

        // Convert distance into shake intensity (0 to 1).
        float proximity = Mathf.InverseLerp(
            startShakeDistance,
            maxShakeDistance,
            distance
        );

        proximity = Mathf.Clamp01(proximity);

        float targetStrength = proximity * maxShakeStrength;

        // Smooth transitions.
        currentStrength = Mathf.Lerp(
            currentStrength,
            targetStrength,
            Time.deltaTime * smoothingSpeed
        );

        // Smooth random shake using Perlin noise.
        float time = Time.time * shakeSpeed;

        float offsetX =
            (Mathf.PerlinNoise(time, 0f) * 2f - 1f)
            * currentStrength;

        float offsetY =
            (Mathf.PerlinNoise(0f, time) * 2f - 1f)
            * currentStrength;

        currentOffset = new Vector3(offsetX, offsetY, 0f);

        transform.localPosition += currentOffset;
        lastOffset = currentOffset;
    }

    void OnDisable()
    {
        transform.localPosition -= lastOffset;
        lastOffset = Vector3.zero;
    }
}
