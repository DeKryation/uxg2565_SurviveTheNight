using UnityEngine;
using TMPro;

public class ExplosiveInventory : MonoBehaviour
{
    [Header("Explosives")]
    public int explosivesCollected = 0;
    public int explosivesNeeded = 5;

    [Header("UI")]
    public TMP_Text explosiveText;

    void Start()
    {
        UpdateExplosiveUI();
    }

    public void AddExplosive(int amount = 1)
    {
        explosivesCollected += amount;
        explosivesCollected = Mathf.Clamp(explosivesCollected, 0, explosivesNeeded);

        UpdateExplosiveUI();
    }

    public bool HasEnoughExplosives()
    {
        return explosivesCollected >= explosivesNeeded;
    }

    void UpdateExplosiveUI()
    {
        if (explosiveText != null)
        {
            explosiveText.text = explosivesCollected + " / " + explosivesNeeded + " Explosives Collected";
        }
    }
}
