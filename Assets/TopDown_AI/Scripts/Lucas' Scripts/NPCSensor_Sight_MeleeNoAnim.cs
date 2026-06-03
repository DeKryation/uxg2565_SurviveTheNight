using UnityEngine;
using System.Collections;

/// <summary>
/// Sight sensor for NPC_MeleeNoAnim.
/// It works like NPCSensor_Sight, but references NPC_MeleeNoAnim instead of NPC_Enemy.
/// </summary>
public class NPCSensor_Sight_MeleeNoAnim : MonoBehaviour {
    const float SIGHT_DIRECT_ANGLE = 120.0f;
    const float SIGHT_MAX_DISTANCE = 20.0f;

    public NPC_MeleeNoAnim npcBase;
    public LayerMask hitTestMask;
    public Material material;
    public Color idleColor = Color.green;
    public Color attackColor = Color.red;

    float height = 2.0f;

    void Start() {
        if (npcBase == null)
            npcBase = GetComponent<NPC_MeleeNoAnim>();

        if (material != null)
            material.SetColor("_Color", idleColor);
    }

    void Update() {
        GetTargetInSight();
    }

    void GetTargetInSight() {
        Collider[] overlappedObjects = Physics.OverlapSphere(transform.position, SIGHT_MAX_DISTANCE);
        bool playerSeen = false;

        for (int i = 0; i < overlappedObjects.Length; i++) {
            if (!overlappedObjects[i].CompareTag("Player"))
                continue;

            Vector3 direction = overlappedObjects[i].transform.position - transform.position;
            float objAngle = Vector3.Angle(direction, transform.forward);

            if (objAngle < SIGHT_DIRECT_ANGLE && TargetInSight(overlappedObjects[i].transform, SIGHT_MAX_DISTANCE)) {
                if (npcBase != null)
                    npcBase.SetTargetPos(overlappedObjects[i].transform.position);

                playerSeen = true;
                break;
            }
        }

        if (material != null)
            material.SetColor("_Color", playerSeen ? attackColor : idleColor);
    }

    bool TargetInSight(Transform target, float distance) {
        Vector3 sightPosition = transform.position;
        sightPosition.y += height;

        RaycastHit hit;
        Vector3 dir = target.position - sightPosition;
        Physics.Raycast(sightPosition, dir, out hit, distance, hitTestMask);

        return hit.collider != null && target.gameObject == hit.collider.gameObject;
    }
}
