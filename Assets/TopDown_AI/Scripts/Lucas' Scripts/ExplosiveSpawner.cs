using UnityEngine;

public class ExplosiveSpawner : MonoBehaviour
{
    [Header("Pickup")]
    public GameObject explosivePrefab;

    [Header("Spawn Points")]
    public Transform[] spawnPoints = new Transform[3];

    [Header("Settings")]
    public bool spawnOnStart = true;
    public bool onlySpawnOne = true;

    private GameObject currentExplosive;

    void Start()
    {
        if (spawnOnStart)
        {
            SpawnExplosive();
        }
    }

    public void SpawnExplosive()
    {
        if (explosivePrefab == null)
        {
            Debug.LogWarning("ExplosiveSpawner: No explosive prefab assigned.");
            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("ExplosiveSpawner: No spawn points assigned.");
            return;
        }

        if (onlySpawnOne && currentExplosive != null)
        {
            return;
        }

        Transform chosenSpawnPoint = GetRandomSpawnPoint();

        if (chosenSpawnPoint == null)
        {
            Debug.LogWarning("ExplosiveSpawner: Chosen spawn point is empty.");
            return;
        }

        currentExplosive = Instantiate(
            explosivePrefab,
            chosenSpawnPoint.position,
            chosenSpawnPoint.rotation
        );
    }

    Transform GetRandomSpawnPoint()
    {
        int validCount = 0;

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (spawnPoints[i] != null)
            {
                validCount++;
            }
        }

        if (validCount == 0)
        {
            return null;
        }

        Transform[] validSpawnPoints = new Transform[validCount];
        int index = 0;

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (spawnPoints[i] != null)
            {
                validSpawnPoints[index] = spawnPoints[i];
                index++;
            }
        }

        return validSpawnPoints[Random.Range(0, validSpawnPoints.Length)];
    }
}
