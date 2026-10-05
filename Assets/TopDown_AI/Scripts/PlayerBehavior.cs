using UnityEngine;
using System.Collections;

public enum PlayerWeaponType { KNIFE, PISTOL, NULL }

public class PlayerBehavior : MonoBehaviour
{
    Rigidbody myRigidBody;

    public float moveSpeed = 10.0f;
    public Transform hitTestPivot, gunPivot;
    public GameObject mousePointer, proyectilePrefab;
    public Animator animator;

    int hashSpeed;
    float attackTime = 0.4f;

    PlayerWeaponType currentWeapon = PlayerWeaponType.NULL;

    Misc_Timer attackTimer = new Misc_Timer();

    public MuzzleFlashLight muzzleFlash;

    void Awake()
    {

    }

    void Start()
    {
        // Player starts with no weapon.
        SetWeapon(PlayerWeaponType.NULL);

        myRigidBody = GetComponent<Rigidbody>();
        hashSpeed = Animator.StringToHash("Speed");
        attackTimer.StartTimer(0.1f);

        // Cursor.visible = false;
    }

    void Update()
    {
        bool isMoving =
            Mathf.Abs(Input.GetAxis("Horizontal")) > 0.1f ||
            Mathf.Abs(Input.GetAxis("Vertical")) > 0.1f;

        if (isMoving)
        {
            SoundManager.StartFootsteps();
        }
        else
        {
            SoundManager.StopFootsteps();
        }

        animator.SetFloat(hashSpeed, myRigidBody.linearVelocity.magnitude);

        float inputHorizontal = Input.GetAxis("Horizontal");
        float inputVertical = Input.GetAxis("Vertical");

        Vector3 newVelocity = new Vector3(
            inputVertical * moveSpeed,
            0.0f,
            inputHorizontal * -moveSpeed
        );

        myRigidBody.linearVelocity = newVelocity;

        // Attacking disabled.
        // No shooting.
        // No knifing.

        // Weapon switching disabled.
        // Alpha1 and Alpha2 no longer change weapons.

        attackTimer.UpdateTimer();
        UpdateAim();
    }

    public void DamagePlayer()
    {
        PlayerHealth health = GetComponent<PlayerHealth>();

        if (health != null)
        {
            health.TakeDamage(1);
            return;
        }

        DiePlayer();
    }

    public void DiePlayer()
    {
        SoundManager.PlayPlayerDeath();

        animator.SetBool("Dead", true);
        animator.transform.parent = null;

        this.enabled = false;

        myRigidBody.isKinematic = true;

        GameManager.RegisterPlayerDeath();

        gameObject.GetComponent<Collider>().enabled = false;

        GameCamera.ToggleShake(0.3f);
    }

    void UpdateAim()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.y = transform.position.y;

        mousePointer.transform.position = mousePos;

        float deltaY = mousePos.z - transform.position.z;
        float deltaX = mousePos.x - transform.position.x;

        float angleInDegrees = Mathf.Atan2(deltaY, deltaX) * 180 / Mathf.PI;

        transform.eulerAngles = new Vector3(0, -angleInDegrees, 0);
    }

    public void Attack()
    {
        // Attacking disabled for this prototype.
        return;
    }

    void AlertEnemies()
    {
        RaycastHit[] hits = Physics.SphereCastAll(hitTestPivot.position, 20.0f, hitTestPivot.up);

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider != null && hit.collider.tag == "Enemy")
            {
                hit.collider.GetComponent<NPC_Enemy>().SetAlertPos(transform.position);
            }
        }
    }

    public void DoHitTest()
    {
        // Knife hit test disabled.
        return;
    }

    void AttackOver()
    {
        animator.SetBool("Attack", false);
    }

    void SetWeapon(PlayerWeaponType weaponType)
    {
        currentWeapon = PlayerWeaponType.NULL;

        animator.SetInteger("WeaponType", 0);

        GameManager.SelectWeapon(PlayerWeaponType.NULL);
    }
}
