using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NPCSensor_Sight : NPCSensor_Base
{
    [Header("Zombie Sight Settings")]
    [Tooltip("Maximum distance at which the zombie can see the player.")]
    public float sightRange = 30f;

    [Tooltip("Half-angle of the detection cone. 60 means 120 degrees total.")]
    [Range(0f, 180f)]
    public float sightAngle = 120f;

    [Tooltip("Height of the sight ray above the zombie's position.")]
    public float height = 2f;

    [Header("Chase Memory")]
    [Tooltip("How many seconds the zombie remembers the player after losing sight.")]
    public float chaseMemoryDuration = 10f;

    [Tooltip("How frequently the zombie updates its last known target.")]
    public float memoryUpdateInterval = 0.5f;

    [Header("Sight Raycast")]
    public LayerMask hitTestMask;

    [Header("Sight Colors")]
    public Color idleColor = Color.green;
    public Color alertedColor = Color.yellow;
    public Color attackColor = Color.red;

    [Header("Visual Material")]
    public Material material;

    private const float SIGHT_MIN_DISTANCE = 0.2f;

    private Vector3 lastKnownPlayerPosition;
    private float lastTargetTime = float.NegativeInfinity;
    private float nextMemoryUpdateTime = 0f;

    private bool targetSpotted = false;
    private bool wasCapturing = false;

    private PlayerGrabQTE playerQTE;

    protected override void StartSensor()
    {
        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            playerQTE = playerObject.GetComponent<PlayerGrabQTE>();
        }

        SetSightColor(idleColor);
    }

    protected override void UpdateSensor()
    {
        // Do not update pursuit while the player is in a QTE.
        if (playerQTE != null && playerQTE.IsActive)
        {
            wasCapturing = true;
            ClearChaseMemory();
            return;
        }

        // When the capture finishes, discard the old target.
        // The zombie's FLEE state should control its next movement.
        if (wasCapturing)
        {
            wasCapturing = false;
            ClearChaseMemory();
        }

        GetTargetInSight();
    }

    void GetTargetInSight()
    {
        bool playerVisible = false;

        Collider[] overlappingObjects = Physics.OverlapSphere(
            transform.position,
            sightRange
        );

        foreach (Collider targetCollider in overlappingObjects)
        {
            if (!targetCollider.CompareTag("Player"))
                continue;

            Vector3 direction =
                targetCollider.transform.position - transform.position;

            // Use horizontal direction for the field-of-view check.
            direction.y = 0f;

            Vector3 forward = transform.forward;
            forward.y = 0f;

            if (direction.sqrMagnitude < 0.0001f)
                continue;

            float objAngle = Vector3.Angle(direction, forward);

            if (objAngle <= sightAngle &&
                TargetInSight(targetCollider.transform, sightRange))
            {
                playerVisible = true;

                lastKnownPlayerPosition =
                    targetCollider.transform.position;

                lastTargetTime = Time.time;
                targetSpotted = true;

                npcBase.SetTargetPos(lastKnownPlayerPosition);

                SetSightColor(attackColor);
                break;
            }
        }

        if (playerVisible)
            return;

        // The player has disappeared from view.
        // Keep pursuing their last known location.
        if (targetSpotted &&
            Time.time - lastTargetTime < chaseMemoryDuration)
        {
            if (Time.time >= nextMemoryUpdateTime)
            {
                npcBase.SetTargetPos(lastKnownPlayerPosition);

                nextMemoryUpdateTime =
                    Time.time + Mathf.Max(0.05f, memoryUpdateInterval);
            }

            SetSightColor(alertedColor);
            return;
        }

        // Chase memory has expired.
        ClearChaseMemory();
        SetSightColor(idleColor);
    }

    bool TargetInSight(Transform target, float distance)
    {
        Vector3 sightPosition = transform.position;
        sightPosition.y += height;

        Vector3 direction = target.position - sightPosition;

        if (direction.sqrMagnitude < 0.0001f)
            return true;

        RaycastHit hit;

        if (Physics.Raycast(
            sightPosition,
            direction.normalized,
            out hit,
            distance,
            hitTestMask,
            QueryTriggerInteraction.Ignore))
        {
            return hit.collider != null &&
                   hit.collider.transform == target;
        }

        return false;
    }

    void ClearChaseMemory()
    {
        targetSpotted = false;
        lastTargetTime = float.NegativeInfinity;
        nextMemoryUpdateTime = 0f;
    }

    void SetSightColor(Color newColor)
    {
        if (material != null)
            material.SetColor("_Color", newColor);
    }

    // Optional: Call this if another system needs to reset sight.
    public void ResetSightMemory()
    {
        ClearChaseMemory();
        SetSightColor(idleColor);
    }

    // -----------------------------------------------------
    // FIELD OF VIEW VISUALISATION
    // Kept for compatibility with your original script.
    // -----------------------------------------------------

    private int quality = 100;
    private Mesh mesh;

    private float FoVHeight = 0.1f;

    void InitFoV()
    {
        mesh = new Mesh();

        mesh.vertices = new Vector3[4 * quality];
        mesh.triangles = new int[6 * quality];

        transform.Rotate(0f, -18f, 0f);
    }

    void UpdateFoV()
    {
        if (mesh == null)
            InitFoV();

        float angleLookAt = GetEnemyAngle();

        float angleStart = angleLookAt - sightAngle;
        float angleEnd = angleLookAt + sightAngle;

        float angleDelta = (angleEnd - angleStart) / quality;

        Vector3[] vertices = new Vector3[4 * quality];
        int[] triangles = new int[6 * quality];

        Vector3 fovPos = transform.position;
        fovPos.y += FoVHeight;

        for (int i = 0; i < quality; i++)
        {
            float angleCurr = angleStart + angleDelta * i;
            float angleNext = angleCurr + angleDelta;

            Vector3 sphereCurr = new Vector3(
                Mathf.Sin(Mathf.Deg2Rad * angleCurr),
                0f,
                Mathf.Cos(Mathf.Deg2Rad * angleCurr)
            );

            Vector3 sphereNext = new Vector3(
                Mathf.Sin(Mathf.Deg2Rad * angleNext),
                0f,
                Mathf.Cos(Mathf.Deg2Rad * angleNext)
            );

            Vector3 posCurrMin =
                fovPos + sphereCurr * SIGHT_MIN_DISTANCE;

            Vector3 posCurrMax =
                fovPos + sphereCurr * sightRange;

            Vector3 posNextMin =
                fovPos + sphereNext * SIGHT_MIN_DISTANCE;

            Vector3 posNextMax =
                fovPos + sphereNext * sightRange;

            RaycastHit currHit;
            RaycastHit nextHit;

            if (Physics.Raycast(
                posCurrMin,
                sphereCurr,
                out currHit,
                sightRange - SIGHT_MIN_DISTANCE,
                hitTestMask))
            {
                posCurrMax = currHit.point;
            }

            if (Physics.Raycast(
                posNextMin,
                sphereNext,
                out nextHit,
                sightRange - SIGHT_MIN_DISTANCE,
                hitTestMask))
            {
                posNextMax = nextHit.point;
            }

            int a = 4 * i;
            int b = a + 1;
            int c = a + 2;
            int d = a + 3;

            vertices[a] = posCurrMin;
            vertices[b] = posCurrMax;
            vertices[c] = posNextMax;
            vertices[d] = posNextMin;

            triangles[6 * i] = a;
            triangles[6 * i + 1] = b;
            triangles[6 * i + 2] = c;

            triangles[6 * i + 3] = c;
            triangles[6 * i + 4] = d;
            triangles[6 * i + 5] = a;
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        if (material != null)
        {
            Graphics.DrawMesh(
                mesh,
                Vector3.zero,
                Quaternion.identity,
                material,
                0
            );
        }
    }

    float GetEnemyAngle()
    {
        return 90f - Mathf.Rad2Deg *
            Mathf.Atan2(transform.forward.z, transform.forward.x);
    }
}
