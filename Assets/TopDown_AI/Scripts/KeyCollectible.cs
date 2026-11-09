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

            // Add the key to the player's inventory.
            inventory.AddKey(keyAmount);

            // Play the key pickup sound.
            SoundManager.PlayPickupKey();

            // Optional pickup VFX.
            if (pickupEffect != null)
            {
                Instantiate(
                    pickupEffect,
                    transform.position,
                    transform.rotation
                );
            }

            // Remove the collected key.
            Destroy(gameObject);
        }
    }
}
