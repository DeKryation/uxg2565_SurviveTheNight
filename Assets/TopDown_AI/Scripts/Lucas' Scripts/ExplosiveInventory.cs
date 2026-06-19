using UnityEngine;
using TMPro;

public class ExplosiveInventory : MonoBehaviour
{
    [Header("Explosives")]
    public int explosivesCollected = 0;
    public int explosivesNeeded = 5;

    [Header("UI")]
    public TMP_Text explosiveText;

    private static int savedExplosivesCollected = 0;

    void Start()
    {
        explosivesCollected = savedExplosivesCollected;
        UpdateExplosiveUI();
    }

    public void AddExplosive(int amount = 1)
    {
        explosivesCollected += amount;
        explosivesCollected = Mathf.Clamp(explosivesCollected, 0, explosivesNeeded);

        savedExplosivesCollected = explosivesCollected;

        UpdateExplosiveUI();
    }

    public bool HasEnoughExplosives()
    {
        return explosivesCollected >= explosivesNeeded;
    }

    public static void ResetSavedExplosives()
    {
        savedExplosivesCollected = 0;
    }

    void UpdateExplosiveUI()
    {
        if (explosiveText != null)
        {
            explosiveText.text = explosivesCollected + " / " + explosivesNeeded + "\nCollected";
        }
    }
}
