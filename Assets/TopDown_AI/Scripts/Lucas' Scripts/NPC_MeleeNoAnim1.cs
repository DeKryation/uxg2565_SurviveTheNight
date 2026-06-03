using UnityEngine;
using System.Collections;

/// <summary>
/// Enemy NPC that uses the same patrol / roam / inspect movement style as NPC_Enemy,
/// but has no Animator. The visual is a dark green circle by default, and it turns
/// into a square during its melee attack.
///
/// Needs:
/// - NavMeshAgent on the same object or assigned in Inspector
/// - Collider with tag "Enemy"
/// - NPCSensor_Sight_MeleeNoAnim for sight detection
/// - Player object tagged "Player" with PlayerBehavior
/// </summary>
public class NPC_MeleeNoAnim : MonoBehaviour {
    [Header("State")]
    public NPC_EnemyState idleState = NPC_EnemyState.IDLE_ROAMER;

    [Header("Movement")]
    public UnityEngine.AI.NavMeshAgent navMeshAgent;
    public NPC_PatrolNode patrolNode;
    public LayerMask hitTestLayer;
    public float patrolSpeed = 6.0f;
    public float roamSpeed = 7.0f;
    public float inspectSpeed = 16.0f;
    public float reachedDistance = 1.5f;

    [Header("Melee Attack")]
    public Transform weaponPivot;
    public float meleeRange = 2.0f;
    public float attackDelay = 0.2f;
    public float attackCooldown = 0.4f;
    public LayerMask attackLayerMask = ~0;

    [Header("No Animation Shape Visual")]
    public SpriteRenderer shapeRenderer;
    public float shapeSize = 1.2f;
    public Color normalColor = new Color(0.0f, 0.22f, 0.0f, 1.0f); // dark green
    public Color attackColor = new Color(0.0f, 0.22f, 0.0f, 1.0f); // keep dark green, only shape changes
    public bool createShapeIfMissing = true;
    public bool faceCamera = true;

    NPC_EnemyState currentState = NPC_EnemyState.NONE;
    Vector3 targetPos;
    Vector3 startingPos;

    delegate void StateMethod();
    StateMethod _initState;
    StateMethod _updateState;
    StateMethod _endState;

    Misc_Timer idleTimer = new Misc_Timer();
    Misc_Timer idleRotateTimer = new Misc_Timer();
    bool idleWaiting;
    bool idleMoving;

    Misc_Timer inspectTimer = new Misc_Timer();
    Misc_Timer inspectTurnTimer = new Misc_Timer();
    bool inspectWait;

    Misc_Timer attackActionTimer = new Misc_Timer();
    bool actionDone;

    Sprite circleSprite;
    Sprite squareSprite;

    void Start() {
        startingPos = transform.position;

        if (navMeshAgent == null)
            navMeshAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();

        if (weaponPivot == null)
            weaponPivot = transform;

        SetupShapeVisual();
        SetCircleVisual();

        SetState(idleState);
        GameManager.AddToEnemyCount();
    }

    void Update() {
        if (_updateState != null)
            _updateState();
    }

    void LateUpdate() {
        if (faceCamera && shapeRenderer != null && Camera.main != null)
            shapeRenderer.transform.rotation = Camera.main.transform.rotation;
    }

    public void SetState(NPC_EnemyState newState) {
        if (currentState == newState)
            return;

        if (_endState != null)
            _endState();

        switch (newState) {
            case NPC_EnemyState.IDLE_STATIC:
                _initState = StateInit_IdleStatic;
                _updateState = StateUpdate_IdleStatic;
                _endState = StateEnd_IdleStatic;
                break;

            case NPC_EnemyState.IDLE_ROAMER:
                _initState = StateInit_IdleRoamer;
                _updateState = StateUpdate_IdleRoamer;
                _endState = StateEnd_IdleRoamer;
                break;

            case NPC_EnemyState.IDLE_PATROL:
                _initState = StateInit_IdlePatrol;
                _updateState = StateUpdate_IdlePatrol;
                _endState = StateEnd_IdlePatrol;
                break;

            case NPC_EnemyState.INSPECT:
                _initState = StateInit_Inspect;
                _updateState = StateUpdate_Inspect;
                _endState = StateEnd_Inspect;
                break;

            case NPC_EnemyState.ATTACK:
                _initState = StateInit_Attack;
                _updateState = StateUpdate_Attack;
                _endState = StateEnd_Attack;
                break;

            default:
                return;
        }

        currentState = newState;
        _initState();
    }

    // ---------------------------------------------------------
    // SHAPE VISUAL
    // ---------------------------------------------------------
    void SetupShapeVisual() {
        circleSprite = CreateCircleSprite(64);
        squareSprite = CreateSquareSprite(64);

        if (shapeRenderer == null && createShapeIfMissing) {
            GameObject shapeObj = new GameObject("Dark Green Shape Visual");
            shapeObj.transform.SetParent(transform);
            shapeObj.transform.localPosition = Vector3.up * 0.1f;
            shapeObj.transform.localScale = Vector3.one * shapeSize;
            shapeRenderer = shapeObj.AddComponent<SpriteRenderer>();
        }

        if (shapeRenderer != null)
            shapeRenderer.transform.localScale = Vector3.one * shapeSize;
    }

    void SetCircleVisual() {
        if (shapeRenderer == null)
            return;

        shapeRenderer.sprite = circleSprite;
        shapeRenderer.color = normalColor;
    }

    void SetSquareVisual() {
        if (shapeRenderer == null)
            return;

        shapeRenderer.sprite = squareSprite;
        shapeRenderer.color = attackColor;
    }

    Sprite CreateCircleSprite(int size) {
        Texture2D texture = new Texture2D(size, size, TextureFormat.ARGB32, false);
        texture.filterMode = FilterMode.Point;

        float radius = size * 0.45f;
        Vector2 center = new Vector2(size / 2.0f, size / 2.0f);

        for (int y = 0; y < size; y++) {
            for (int x = 0; x < size; x++) {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                texture.SetPixel(x, y, distance <= radius ? Color.white : Color.clear);
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    Sprite CreateSquareSprite(int size) {
        Texture2D texture = new Texture2D(size, size, TextureFormat.ARGB32, false);
        texture.filterMode = FilterMode.Point;

        for (int y = 0; y < size; y++) {
            for (int x = 0; x < size; x++)
                texture.SetPixel(x, y, Color.white);
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    // ---------------------------------------------------------
    // IDLE STATIC
    // ---------------------------------------------------------
    void StateInit_IdleStatic() {
        SetCircleVisual();
        ResumeAgent();
        navMeshAgent.SetDestination(startingPos);
    }

    void StateUpdate_IdleStatic() { }
    void StateEnd_IdleStatic() { }

    // ---------------------------------------------------------
    // IDLE PATROL
    // ---------------------------------------------------------
    void StateInit_IdlePatrol() {
        SetCircleVisual();
        ResumeAgent();
        navMeshAgent.speed = patrolSpeed;

        if (patrolNode != null)
            navMeshAgent.SetDestination(patrolNode.GetPosition());
    }

    void StateUpdate_IdlePatrol() {
        if (patrolNode == null)
            return;

        if (HasReachedMyDestination()) {
            patrolNode = patrolNode.nextNode;
            if (patrolNode != null)
                navMeshAgent.SetDestination(patrolNode.GetPosition());
        }
    }

    void StateEnd_IdlePatrol() { }

    // ---------------------------------------------------------
    // IDLE ROAMER
    // ---------------------------------------------------------
    void StateInit_IdleRoamer() {
        SetCircleVisual();
        ResumeAgent();
        navMeshAgent.speed = roamSpeed;

        idleTimer.StartTimer(Random.Range(2.0f, 4.0f));
        RandomRotate();
        AdvanceIdle();
        idleWaiting = false;
        idleMoving = true;
    }

    void StateUpdate_IdleRoamer() {
        idleTimer.UpdateTimer();

        if (idleMoving) {
            if (HasReachedMyDestination())
                AdvanceIdle();
        }
        else if (idleWaiting) {
            idleRotateTimer.UpdateTimer();
            if (idleRotateTimer.IsFinished()) {
                RandomRotate();
                idleRotateTimer.StartTimer(Random.Range(1.5f, 3.25f));
            }
        }

        if (idleTimer.IsFinished()) {
            if (idleMoving) {
                StopAgent();
                float waitTime = Random.Range(2.5f, 6.5f);
                idleRotateTimer.StartTimer(waitTime / 2.0f);
                idleTimer.StartTimer(waitTime);
            }
            else if (idleWaiting) {
                idleTimer.StartTimer(Random.Range(2.0f, 4.0f));
                AdvanceIdle();
            }

            idleMoving = !idleMoving;
            idleWaiting = !idleMoving;
        }
    }

    void StateEnd_IdleRoamer() { }

    void AdvanceIdle() {
        RaycastHit hit;
        Physics.Raycast(transform.position, transform.forward * 5.0f, out hit, 50.0f, hitTestLayer);

        if (hit.collider == null)
            return;

        if (hit.distance < 3.0f) {
            Vector3 dir = hit.point - transform.position;
            Vector3 reflectedVector = Vector3.Reflect(dir, hit.normal);
            Physics.Raycast(transform.position, reflectedVector, out hit, 50.0f, hitTestLayer);
        }

        if (hit.collider != null) {
            ResumeAgent();
            navMeshAgent.SetDestination(hit.point);
        }
    }

    // ---------------------------------------------------------
    // INSPECT / CHASE TARGET POSITION
    // ---------------------------------------------------------
    void StateInit_Inspect() {
        SetCircleVisual();
        ResumeAgent();
        navMeshAgent.speed = inspectSpeed;
        inspectTimer.StopTimer();
        inspectWait = false;
    }

    void StateUpdate_Inspect() {
        navMeshAgent.SetDestination(targetPos);

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) {
            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

            if (distanceToPlayer <= meleeRange && CanSeePlayer(player.transform)) {
                SetState(NPC_EnemyState.ATTACK);
                return;
            }
        }

        if (HasReachedMyDestination() && !inspectWait) {
            inspectWait = true;
            inspectTimer.StartTimer(2.0f);
            inspectTurnTimer.StartTimer(1.0f);
        }

        if (inspectWait) {
            inspectTimer.UpdateTimer();
            inspectTurnTimer.UpdateTimer();

            if (inspectTurnTimer.IsFinished()) {
                RandomRotate();
                inspectTurnTimer.StartTimer(Random.Range(0.5f, 1.25f));
            }

            if (inspectTimer.IsFinished())
                SetState(idleState);
        }
    }

    void StateEnd_Inspect() { }

    // ---------------------------------------------------------
    // MELEE ATTACK
    // ---------------------------------------------------------
    void StateInit_Attack() {
        StopAgent();
        navMeshAgent.velocity = Vector3.zero;

        SetSquareVisual();

        CancelInvoke("AttackAction");
        Invoke("AttackAction", attackDelay);

        attackActionTimer.StartTimer(attackCooldown);
        actionDone = false;
    }

    void StateUpdate_Attack() {
        FacePlayer();
        attackActionTimer.UpdateTimer();

        if (!actionDone && attackActionTimer.IsFinished()) {
            actionDone = true;
            EndAttack();
        }
    }

    void StateEnd_Attack() {
        SetCircleVisual();
    }

    void EndAttack() {
        SetState(NPC_EnemyState.INSPECT);
    }

    void AttackAction() {
        RaycastHit[] hits = Physics.SphereCastAll(weaponPivot.position, meleeRange, weaponPivot.forward, 0.1f, attackLayerMask);

        foreach (RaycastHit hit in hits) {
            if (hit.collider != null && hit.collider.CompareTag("Player")) {
                PlayerBehavior player = hit.collider.GetComponent<PlayerBehavior>();
                if (player != null)
                    player.DamagePlayer();

                return;
            }
        }
    }

    // ---------------------------------------------------------
    // PUBLIC FUNCTIONS USED BY SENSOR / PLAYER
    // ---------------------------------------------------------
    public void SetAlertPos(Vector3 newPos) {
        if (idleState != NPC_EnemyState.IDLE_STATIC)
            SetTargetPos(newPos);
    }

    public void SetTargetPos(Vector3 newPos) {
        targetPos = newPos;

        if (currentState != NPC_EnemyState.ATTACK)
            SetState(NPC_EnemyState.INSPECT);
    }

    public void Damage() {
        if (navMeshAgent != null)
            navMeshAgent.velocity = Vector3.zero;

        GameManager.AddScore(100);
        GameManager.RemoveEnemy();
        Destroy(gameObject);
    }

    // ---------------------------------------------------------
    // HELPERS
    // ---------------------------------------------------------
    public bool HasReachedMyDestination() {
        if (navMeshAgent == null)
            return false;

        float dist = Vector3.Distance(transform.position, navMeshAgent.destination);
        return dist <= reachedDistance;
    }

    bool CanSeePlayer(Transform player) {
        Vector3 origin = transform.position + Vector3.up * 1.0f;
        Vector3 direction = player.position - origin;
        RaycastHit hit;

        if (Physics.Raycast(origin, direction, out hit, meleeRange + 0.5f, attackLayerMask))
            return hit.collider.CompareTag("Player");

        return false;
    }

    void FacePlayer() {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
            return;

        Vector3 direction = player.transform.position - transform.position;
        direction.y = 0.0f;

        if (direction.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(direction);
    }

    void RandomRotate() {
        float randomAngle = Random.Range(45, 180);
        if (Random.Range(0, 2) == 0)
            randomAngle *= -1;

        transform.Rotate(0, randomAngle, 0);
    }

    void ResumeAgent() {
        if (navMeshAgent == null)
            return;

        navMeshAgent.isStopped = false;
    }

    void StopAgent() {
        if (navMeshAgent == null)
            return;

        navMeshAgent.isStopped = true;
    }
}
