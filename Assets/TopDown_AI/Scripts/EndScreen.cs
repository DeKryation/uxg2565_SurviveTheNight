using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class EndScreen : MonoBehaviour
{
    [Header("Scene Selection")]
    public string sceneIfNotEnoughExplosives;
    public string sceneIfEnoughExplosives;

    [Header("Explosive Requirement")]
    public int explosivesRequired = 4;

    [Header("Transition Sound")]
    public bool playTransitionSound = true;
    public float delayBeforeLoad = 0.5f;

    private bool isLoading = false;

    void OnTriggerEnter(Collider user)
    {
        if (isLoading)
            return;

        if (user.CompareTag("Player"))
        {
            StartCoroutine(LoadSceneWithSound(user));
        }
    }

    IEnumerator LoadSceneWithSound(Collider playerCollider)
    {
        isLoading = true;

        // Find the player's explosive inventory.
        ExplosiveInventory inventory =
            playerCollider.GetComponentInParent<ExplosiveInventory>();

        int explosivesCollected = 0;

        if (inventory != null)
        {
            explosivesCollected = inventory.explosivesCollected;
        }
        else
        {
            Debug.LogWarning(
                "Trigger_LoadScene: ExplosiveInventory not found!"
            );
        }

        // Choose which scene to load.
        string targetScene;

        if (explosivesCollected >= explosivesRequired)
        {
            targetScene = sceneIfEnoughExplosives;

            Debug.Log(
                "Enough explosives collected! Loading: " +
                targetScene
            );
        }
        else
        {
            targetScene = sceneIfNotEnoughExplosives;

            Debug.Log(
                "Not enough explosives! Loading: " +
                targetScene
            );
        }

        // Play your existing transition sound.
        if (playTransitionSound)
        {
            SoundManager.PlaySceneTransition();
        }

        // Wait before changing scenes.
        yield return new WaitForSeconds(delayBeforeLoad);

        SceneManager.LoadScene(targetScene);
    }
}
