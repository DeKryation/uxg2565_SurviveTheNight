using UnityEngine;

public class KeyCollectible : MonoBehaviour
{
    [Header("Key Settings")]
    public int keyAmount = 1;

    [Header("Optional Pickup Effect")]
    public GameObject pickupEffect;

    private bool collected = false;

    void OnTriggerEnter(Collider other)
    {
        if (collected)
            return;

        KeyInventory inventory =
            other.GetComponentInParent<KeyInventory>();

        if (inventory != null)
        {
            collected = true;

            inventory.AddKey(keyAmount);

            if (pickupEffect != null)
            {
                Instantiate(
                    pickupEffect,
                    transform.position,
                    transform.rotation
                );
            }

            Destroy(gameObject);
        }
    }
}
