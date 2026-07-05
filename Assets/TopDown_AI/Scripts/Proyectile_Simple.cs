using UnityEngine;
using System.Collections;

public class Proyectile_Simple : MonoBehaviour
{
    public enum CollisionTarget { PLAYER, ENEMIES }
    public CollisionTarget collisionTarget;

    public float lifeTime = 3.0f;

    [Tooltip("This keeps the old project feel: movement amount per frame, not per second.")]
    public float speed = 1.5f;

    [Tooltip("Use a small spherecast so fast bullets do not slip through thin walls.")]
    public float bulletRadius = 0.08f;

    [Header("Impact VFX")]
    public GameObject bulletImpactPrefab;
    public float impactOffset = 0.05f;

    bool destroyed = false;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        if (destroyed) return;

        Vector3 startPos = transform.position;
        Vector3 move = transform.forward * speed;
        Vector3 endPos = startPos + move;

        RaycastHit hit;
        bool didHit;

        if (bulletRadius > 0f)
        {
            didHit = Physics.SphereCast(
                startPos,
                bulletRadius,
                transform.forward,
                out hit,
                move.magnitude,
                ~0,
                QueryTriggerInteraction.Ignore
            );
        }
        else
        {
            didHit = Physics.Raycast(
                startPos,
                transform.forward,
                out hit,
                move.magnitude,
                ~0,
                QueryTriggerInteraction.Ignore
            );
        }

        if (didHit)
        {
            transform.position = hit.point;
            SpawnImpactEffect(hit.point, hit.normal);
            HandleHit(hit.collider);
            return;
        }

        transform.position = endPos;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (destroyed) return;

        if (collision.contacts.Length > 0)
        {
            ContactPoint contact = collision.contacts[0];
            SpawnImpactEffect(contact.point, contact.normal);
        }
        else
        {
            SpawnImpactEffect(transform.position, -transform.forward);
        }

        HandleHit(collision.collider);
    }

    void OnTriggerEnter(Collider other)
    {
        if (destroyed) return;

        // Triggers do not give contact points, so use the bullet position.
        SpawnImpactEffect(transform.position, -transform.forward);

        HandleHit(other);
    }

    void HandleHit(Collider hitCollider)
    {
        if (hitCollider == null) return;

        if (collisionTarget == CollisionTarget.PLAYER && hitCollider.CompareTag("Player"))
        {
            PlayerBehavior player = hitCollider.GetComponent<PlayerBehavior>();
            if (player != null)
            {
                player.DamagePlayer();
            }

            DestroyProyectile();
            return;
        }

        if (collisionTarget == CollisionTarget.ENEMIES && hitCollider.CompareTag("Enemy"))
        {
            NPC_Enemy enemy = hitCollider.GetComponent<NPC_Enemy>();
            if (enemy != null)
            {
                enemy.Damage();
            }

            NPC_MeleeNoAnim meleeEnemy = hitCollider.GetComponent<NPC_MeleeNoAnim>();
            if (meleeEnemy != null)
            {
                meleeEnemy.Damage();
            }

            DestroyProyectile();
            return;
        }

        // Walls, ProBuilder walls, props, old map colliders, etc. all stop bullets.
        DestroyProyectile();
    }

    void SpawnImpactEffect(Vector3 hitPoint, Vector3 hitNormal)
    {
        if (bulletImpactPrefab == null)
            return;

        Vector3 spawnPosition = hitPoint + hitNormal * impactOffset;
        Quaternion spawnRotation = Quaternion.LookRotation(hitNormal);

        Instantiate(bulletImpactPrefab, spawnPosition, spawnRotation);
    }

    void DestroyProyectile()
    {
        destroyed = true;
        Destroy(gameObject);
    }
}



