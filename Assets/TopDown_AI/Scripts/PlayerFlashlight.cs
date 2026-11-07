using UnityEngine;

public class PlayerFlashlight : MonoBehaviour
{
    [Header("Flashlight")]
    public Light flashlight;
    public float aimDistance = 7f;

    [Header("Zombie")]
    public SpriteRenderer enemySprite;

    [Header("Zombie Colours")]
    public Color enemyDarkColor = new Color(0.25f, 0.25f, 0.25f, 1f);
    public Color enemyLitColor = Color.white;

    [Header("Wall Blocking")]
    public bool wallsBlockFlashlight = true;
    public LayerMask wallLayers;

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;

        if (enemySprite != null)
            enemySprite.color = enemyDarkColor;
    }

    void LateUpdate()
    {
        if (flashlight == null || mainCamera == null)
            return;

        AimFlashlight();
        CheckEnemyIllumination();
    }

    void AimFlashlight()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        Plane groundPlane = new Plane(Vector3.up, transform.position);

        if (groundPlane.Raycast(ray, out float distance))
        {
            Vector3 mouseWorldPos = ray.GetPoint(distance);

            Vector3 direction = mouseWorldPos - transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude < 0.001f)
                return;

            direction.Normalize();

            Vector3 aimPoint = transform.position + direction * aimDistance;

            flashlight.transform.rotation = Quaternion.LookRotation(
                aimPoint - flashlight.transform.position
            );
        }
    }

    void CheckEnemyIllumination()
    {
        if (enemySprite == null)
            return;

        if (!flashlight.enabled || !flashlight.gameObject.activeInHierarchy)
        {
            enemySprite.color = enemyDarkColor;
            return;
        }

        Vector3 targetPosition = enemySprite.bounds.center;
        Vector3 directionToEnemy =
            targetPosition - flashlight.transform.position;

        float distanceToEnemy = directionToEnemy.magnitude;

        bool inRange = distanceToEnemy <= flashlight.range;

        float angle = Vector3.Angle(
            flashlight.transform.forward,
            directionToEnemy
        );

        bool insideCone = angle <= flashlight.spotAngle * 0.5f;

        bool blocked = false;

        if (wallsBlockFlashlight && distanceToEnemy > 0.001f)
        {
            blocked = Physics.Raycast(
                flashlight.transform.position,
                directionToEnemy.normalized,
                distanceToEnemy,
                wallLayers,
                QueryTriggerInteraction.Ignore
            );
        }

        if (inRange && insideCone && !blocked)
        {
            // Full RGB 255, 255, 255
            enemySprite.color = enemyLitColor;
        }
        else
        {
            enemySprite.color = enemyDarkColor;
        }
    }

    void OnDisable()
    {
        if (enemySprite != null)
            enemySprite.color = enemyDarkColor;
    }
}
