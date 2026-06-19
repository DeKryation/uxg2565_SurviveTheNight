using UnityEngine;

public class ExplosiveCollectible : MonoBehaviour
{
    public int explosiveAmount = 1;

    [Header("Optional")]
    public GameObject pickupEffect;

    private bool collected = false;

    void OnTriggerEnter(Collider other)
    {
        if (collected) return;

        if (other.CompareTag("Player"))
        {
            ExplosiveInventory inventory = other.GetComponent<ExplosiveInventory>();

            if (inventory != null)
            {
                collected = true;
                inventory.AddExplosive(explosiveAmount);

                if (pickupEffect != null)
                {
                    Instantiate(pickupEffect, transform.position, transform.rotation);
                }
                SoundManager.PlayPickupExplosive();
                Destroy(gameObject);
            }
        }
    }
}
