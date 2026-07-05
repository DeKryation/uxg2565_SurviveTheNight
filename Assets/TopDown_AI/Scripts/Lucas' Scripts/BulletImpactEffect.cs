using UnityEngine;

public class BulletImpactEffect : MonoBehaviour
{
    [Header("Lifetime")]
    public float destroyAfterSeconds = 0.5f;

    void Start()
    {
        Destroy(gameObject, destroyAfterSeconds);
    }
}
