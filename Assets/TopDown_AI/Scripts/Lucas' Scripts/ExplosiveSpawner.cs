using UnityEngine;

public class ExplosiveSpawner : MonoBehaviour
{
    [Header("Explosive Prefab")]
    public GameObject explosivePrefab;

    [Header("Spawn Points")]
    public Transform[] spawnPoints;

    void Start()
    {
        SpawnAllExplosives();
    }

    void SpawnAllExplosives()
    {
        if (explosivePrefab == null)
        {
            Debug.LogWarning("ExplosiveSpawner: No explosive prefab assigned.");
            return;
        }

        foreach (Transform spawnPoint in spawnPoints)
        {
            if (spawnPoint == null)
                continue;

            Instantiate(
                explosivePrefab,
                spawnPoint.position,
                spawnPoint.rotation
            );
        }
    }
}
