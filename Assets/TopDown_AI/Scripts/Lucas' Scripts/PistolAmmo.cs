using System.Collections;
using UnityEngine;
using TMPro;

public class PistolAmmo : MonoBehaviour
{
    [Header("Ammo")]
    public int magazineSize = 6;
    public int currentAmmo = 6;
    public float reloadTime = 2.5f;

    [Header("UI")]
    public TMP_Text ammoText;

    [Header("Text Sizes")]
    public int ammoTextSize = 36;
    public int reloadingTextSize = 22;

    private bool isReloading = false;

    void Start()
    {
        currentAmmo = magazineSize;
        UpdateAmmoUI();
    }

    public bool CanShoot()
    {
        return !isReloading && currentAmmo > 0;
    }

    public bool TryUseBullet()
    {
        if (isReloading)
            return false;

        if (currentAmmo <= 0)
        {
            StartReload();
            return false;
        }

        currentAmmo--;
        UpdateAmmoUI();

        if (currentAmmo <= 0)
            StartReload();

        return true;
    }

    public void StartReload()
    {
        if (!isReloading)
            StartCoroutine(ReloadRoutine());
        SoundManager.PlayReload();
    }

    IEnumerator ReloadRoutine()
    {
        isReloading = true;
        UpdateAmmoUI();

        yield return new WaitForSeconds(reloadTime);

        currentAmmo = magazineSize;
        isReloading = false;
        UpdateAmmoUI();
        SoundManager.PlayReload();
    }

    void UpdateAmmoUI()
    {
        if (ammoText == null)
            return;

        if (isReloading)
        {
            ammoText.text = "<size=" + reloadingTextSize + ">Reloading</size>";
        }
        else
        {
            ammoText.text = "<size=" + ammoTextSize + ">" + currentAmmo + " / " + magazineSize + "</size>";
        }
    }
}
