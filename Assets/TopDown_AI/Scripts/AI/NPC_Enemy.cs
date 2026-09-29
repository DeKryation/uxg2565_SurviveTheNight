using UnityEngine;
using System.Collections;
public enum NPC_EnemyState{IDLE_STATIC,IDLE_ROAMER,IDLE_PATROL,INSPECT,ATTACK,FLEE,FIND_WEAPON,KNOCKED_OUT,DEAD,NONE}
public enum NPC_WeaponType{KNIFE,RIFLE, SHOTGUN }
public class NPC_Enemy : MonoBehaviour
{
	public float inspectTimeout; //Once the npc reaches the destination, how much time unitl in goes back.
	public UnityEngine.AI.NavMeshAgent navMeshAgent;
	public Animator npcAnimator;

	public GameObject proyectilePrefab;
	delegate void InitState();
	delegate void UpdateState();
	delegate void EndState();
	InitState _initState;
	InitState _updateState;
	InitState _endState;
	public NPC_WeaponType weaponType = NPC_WeaponType.KNIFE;
	public NPC_EnemyState idleState = NPC_EnemyState.IDLE_ROAMER;
	NPC_EnemyState currentState = NPC_EnemyState.NONE;
	Vector3 targetPos, startingPos;
	public LayerMask hitTestLayer;
	float weaponRange;
	public Transform weaponPivot;
	float weaponActionTime, weaponTime;
	int hashSpeed;
	public NPC_PatrolNode patrolNode;

	[Header("Tension Director Settings")]
	public float lowMoveSpeed = 7f;
	public float mediumMoveSpeed = 7f;
	public float highMoveSpeed = 11f;

	public float lowRoamDistance = 12f;
	public float mediumRoamDistance = 25f;
	public float highRoamDistance = 25f;

	[Header("Flee After Hitting Player")]
	public float fleeSpeed = 14f;
	public float fleeDistance = 12f;
	public float fleeTime = 2.5f;

	private Misc_Timer fleeTimer = new Misc_Timer();
	private Transform playerToFleeFrom;

	// Use this for initialization

	void Start()
	{
		startingPos = transform.position;
		hashSpeed = Animator.StringToHash("Speed");
		SetWeapon(weaponType);
		SetState(idleState);
		GameManager.AddToEnemyCount();
	}
	void SetWeapon(NPC_WeaponType newWeapon)
	{
		npcAnimator.SetTrigger("WeaponChange");
		npcAnimator.SetInteger("WeaponType", (int)weaponType);
		switch (weaponType)
		{
			case NPC_WeaponType.KNIFE:
				weaponRange = 1.0f;
				weaponActionTime = 0.2f;
				weaponTime = 0.4f;
				break;
			case NPC_WeaponType.RIFLE:
				weaponRange = 20.0f;
				weaponActionTime = 0.025f;
				weaponTime = 0.05f;
				break;
			case NPC_WeaponType.SHOTGUN:
				weaponRange = 20.0f;
				weaponActionTime = 0.35f;
				weaponTime = 0.75f;
				break;
		}
	}
	// Update is called once per frame
	void Update()
	{
		_updateState();

		npcAnimator.SetFloat(hashSpeed, navMeshAgent.velocity.magnitude);
	}
	public void SetState(NPC_EnemyState newState)
	{
		if (currentState != newState)
		{
			if (_endState != null)
				_endState();
			switch (newState)
			{
				case NPC_EnemyState.IDLE_STATIC: _initState = StateInit_IdleStatic; _updateState = StateUpdate_IdleStatic; _endState = StateEnd_IdleStatic; break;
				case NPC_EnemyState.IDLE_ROAMER: _initState = StateInit_IdleRoamer; _updateState = StateUpdate_IdleRoamer; _endState = StateEnd_IdleRoamer; break;
				case NPC_EnemyState.IDLE_PATROL: _initState = StateInit_IdlePatrol; _updateState = StateUpdate_IdlePatrol; _endState = StateEnd_IdlePatrol; break;
				case NPC_EnemyState.INSPECT: _initState = StateInit_Inspect; _updateState = StateUpdate_Inspect; _endState = StateEnd_Inspect; break;
				case NPC_EnemyState.ATTACK: _initState = StateInit_Attack; _updateState = StateUpdate_Attack; _endState = StateEnd_Attack; break;
				case NPC_EnemyState.FLEE: _initState = StateInit_Flee; _updateState = StateUpdate_Flee; _endState = StateEnd_Flee; break;

			}
			_initState();
			currentState = newState;
		}
	}

	void UpdateSensors()
	{

	}

	///////////////////////////////////////////////////////// STATE: IDLE STATIC


	void StateInit_IdleStatic()
	{
		navMeshAgent.SetDestination(startingPos);
		navMeshAgent.Resume();
	}
	void StateUpdate_IdleStatic()
	{


	}
	void StateEnd_IdleStatic()
	{
	}
	///////////////////////////////////////////////////////// STATE: IDLE PATROL


	void StateInit_IdlePatrol()
	{
		navMeshAgent.speed = 6.0f;
		navMeshAgent.SetDestination(patrolNode.GetPosition());
	}
	void StateUpdate_IdlePatrol()
	{
		if (HasReachedMyDestination())
		{
			patrolNode = patrolNode.nextNode;
			navMeshAgent.SetDestination(patrolNode.GetPosition());
		}

	}
	void StateEnd_IdlePatrol()
	{
	}

	///////////////////////////////////////////////////////// STATE: IDLE ROAMER


	Misc_Timer idleTimer = new Misc_Timer();
	Misc_Timer idleRotateTimer = new Misc_Timer();
	bool idleWaiting, idleMoving;
	void StateInit_IdleRoamer()
	{
		navMeshAgent.speed = GetMoveSpeedByTension();

		idleTimer.StartTimer(Random.Range(2.0f, 4.0f));
		RandomRotate();
		AdvanceIdle();
		idleWaiting = false;
		idleMoving = true;

	}
	void StateUpdate_IdleRoamer()
	{

		idleTimer.UpdateTimer();

		if (idleMoving)
		{
			if (HasReachedMyDestination())
			{
				AdvanceIdle();

			}
		}
		else if (idleWaiting)
		{
			idleRotateTimer.UpdateTimer();
			if (idleRotateTimer.IsFinished())
			{
				RandomRotate();
				idleRotateTimer.StartTimer(Random.Range(1.5f, 3.25f));
			}

		}
		if (idleTimer.IsFinished())
		{
			if (idleMoving)
			{
				navMeshAgent.Stop();
				float waitTime = Random.Range(2.5f, 6.5f);
				float randomTurnTime = waitTime / 2.0f;
				idleRotateTimer.StartTimer(randomTurnTime);
				idleTimer.StartTimer(waitTime);


			}
			else if (idleWaiting)
			{
				idleTimer.StartTimer(Random.Range(2.0f, 4.0f));

				AdvanceIdle();
			}

			idleMoving = !idleMoving;
			idleWaiting = !idleMoving;

		}

	}
	void StateEnd_IdleRoamer()
	{
	}


	void RayDebug()
	{
		RaycastHit hit = new RaycastHit();
		Physics.Raycast(transform.position, transform.forward * 5.0f, out hit, 50.0f, hitTestLayer);

		Debug.DrawLine(transform.position, hit.point, Color.red);
		Vector3 dir = hit.point - transform.position;
		Vector3 reflectedVector = Vector3.Reflect(dir, hit.normal);
		Debug.DrawRay(hit.point, reflectedVector * 5.0f, Color.green);
	}

	void AdvanceIdle()
	{

		RaycastHit hit = new RaycastHit();
		Physics.Raycast(transform.position, transform.forward * 5.0f, out hit, GetRoamDistanceByTension(), hitTestLayer);
		//Debug.DrawRay (transform.position, transform.forward, Color.red);

		if (hit.distance < 3.0f)
		{
			Vector3 dir = hit.point - transform.position;
			Vector3 reflectedVector = Vector3.Reflect(dir, hit.normal);
			Physics.Raycast(transform.position, reflectedVector, out hit, GetRoamDistanceByTension(), hitTestLayer);
		}

		navMeshAgent.Resume();
		navMeshAgent.SetDestination(hit.point);


	}
	///////////////////////////////////////////////////////// STATE: INSPECT
	Misc_Timer inspectTimer = new Misc_Timer();
	Misc_Timer inspectTurnTimer = new Misc_Timer();
	bool inspectWait;
	void StateInit_Inspect()
	{
		navMeshAgent.speed = GetMoveSpeedByTension() + 4f;
		navMeshAgent.Resume();
		inspectTimer.StopTimer();
		inspectWait = false;
	}
	void StateUpdate_Inspect()
	{


		if (HasReachedMyDestination() && !inspectWait)
		{
			inspectWait = true;
			inspectTimer.StartTimer(2.0f);
			inspectTurnTimer.StartTimer(1.0f);
		}
		navMeshAgent.SetDestination(targetPos);
		RaycastHit hit = new RaycastHit();
		Physics.Raycast(transform.position, transform.forward, out hit, weaponRange, hitTestLayer);

		if (hit.collider != null && hit.collider.tag == "Player")
		{
			SetState(NPC_EnemyState.ATTACK);
		}
		if (inspectWait)
		{
			inspectTimer.UpdateTimer();
			inspectTurnTimer.UpdateTimer();
			if (inspectTurnTimer.IsFinished())
			{
				RandomRotate();
				inspectTurnTimer.StartTimer(Random.Range(0.5f, 1.25f));
			}
			if (inspectTimer.IsFinished())
				SetState(idleState);
		}
	}
	void StateEnd_Inspect()
	{
	}

	///////////////////////////////////////////////////////// STATE: ATTACK
	Misc_Timer attackActionTimer = new Misc_Timer();
	bool actionDone;
	void StateInit_Attack()
	{
		navMeshAgent.Stop();
		navMeshAgent.velocity = Vector3.zero;
		npcAnimator.SetBool("Attack", true);
		CancelInvoke("AttackAction");
		Invoke("AttackAction", weaponActionTime);
		attackActionTimer.StartTimer(weaponTime);

		actionDone = false;
	}
	void StateUpdate_Attack()
	{
		attackActionTimer.UpdateTimer();
		if (!actionDone && attackActionTimer.IsFinished())
		{
			EndAttack();

			actionDone = true;
		}
	}
	void StateEnd_Attack()
	{
		npcAnimator.SetBool("Attack", false);
	}
	void EndAttack()
	{
        if (currentState == NPC_EnemyState.FLEE)
            return;

        SetState(NPC_EnemyState.INSPECT);
    }
	void AttackAction()
	{
		switch (weaponType)
		{
			case NPC_WeaponType.KNIFE:
				RaycastHit[] hits = Physics.SphereCastAll(weaponPivot.position, 2.0f, weaponPivot.forward);
				SoundManager.PlayPlayerKnife();

				foreach (RaycastHit hit in hits)
				{
                    if (hit.collider != null && hit.collider.CompareTag("Player"))
                    {
                        hit.collider.GetComponent<PlayerBehavior>().DamagePlayer();
                        TensionDirector.ResetTension();
                        playerToFleeFrom = hit.collider.transform;
                        SetState(NPC_EnemyState.FLEE);

                        return;
                    }
                }
				break;
			case NPC_WeaponType.RIFLE:
				GameObject bullet = GameObject.Instantiate(proyectilePrefab, weaponPivot.position, weaponPivot.rotation) as GameObject;
				bullet.transform.Rotate(0, Random.Range(-7.5f, 7.5f), 0);
				SoundManager.PlayEnemyGunshot();

				break;
			case NPC_WeaponType.SHOTGUN:
				for (int i = 0; i < 5; i++)
				{
					GameObject birdshot = GameObject.Instantiate(proyectilePrefab, weaponPivot.position, weaponPivot.rotation) as GameObject;
					birdshot.transform.Rotate(0, Random.Range(-15, 15), 0);
					SoundManager.PlayEnemyGunshot();
				}
				break;
		}
	}
	////////////////////////// MISC FUNCTIONS //////////////////////////

	void RandomRotate()
	{
		float randomAngle = Random.Range(45, 180);
		float randomSign = Random.Range(0, 2);
		if (randomSign == 0)
			randomAngle *= -1;

		transform.Rotate(0, randomAngle, 0);
	}
	/*float randomMoveInnerRadius=0.5f, randomMoveOuterRadius=10.0f;
	private Vector3 GetRandomPoint(){	
		Vector3 newPos;
		//do{
			newPos=Random.insideUnitSphere * randomMoveOuterRadius;
		//}while(newPos.x <randomMoveInnerRadius && newPos.y<randomMoveInnerRadius);
		Vector3 finalPos = transform.position + newPos;

		return finalPos;
	}*/
	public bool HasReachedMyDestination()
	{
		float dist = Vector3.Distance(transform.position, navMeshAgent.destination);
		if (dist <= 1.5f)
		{
			return true;
		}

		return false;
	}
	////////////////////////// PUBLIC FUNCTIONS //////////////////////////
	public void SetAlertPos(Vector3 newPos)
	{
		if (idleState != NPC_EnemyState.IDLE_STATIC)
		{
			SetTargetPos(newPos);
		}
	}
	public void SetTargetPos(Vector3 newPos)
	{
        if (currentState == NPC_EnemyState.ATTACK || currentState == NPC_EnemyState.FLEE)
            return;

        targetPos = newPos;
        SetState(NPC_EnemyState.INSPECT);
    }
	public void Damage()
	{
		navMeshAgent.velocity = Vector3.zero;
		//navMeshAgent.Stop ();
		npcAnimator.SetBool("Dead", true);
		GameManager.AddScore(100);
		npcAnimator.transform.parent = null;
		Vector3 pos = npcAnimator.transform.position;
		pos.y = 0.2f;
		npcAnimator.transform.position = pos;
		GameManager.RemoveEnemy();
		SoundManager.PlayEnemyDeath();
		Destroy(gameObject);
	}

	float GetMoveSpeedByTension()
	{
		TensionLevel tension = TensionDirector.GetTensionLevel();

		if (tension == TensionLevel.HIGH)
			return highMoveSpeed;

		if (tension == TensionLevel.MEDIUM)
			return mediumMoveSpeed;

		return lowMoveSpeed;
	}

	float GetRoamDistanceByTension()
	{
		TensionLevel tension = TensionDirector.GetTensionLevel();

		if (tension == TensionLevel.HIGH)
			return highRoamDistance;

		if (tension == TensionLevel.MEDIUM)
			return mediumRoamDistance;

		return lowRoamDistance;
	}

    void StateInit_Flee()
    {
        CancelInvoke("AttackAction");

        npcAnimator.SetBool("Attack", false);

        navMeshAgent.speed = fleeSpeed;
        navMeshAgent.Resume();

        fleeTimer.StartTimer(fleeTime);

        if (playerToFleeFrom == null)
        {
            SetState(idleState);
            return;
        }

        Vector3 fleeDirection = transform.position - playerToFleeFrom.position;

        // If enemy is too close/on top of player, use opposite of enemy forward.
        if (fleeDirection.magnitude < 0.1f)
        {
            fleeDirection = -transform.forward;
        }

        fleeDirection.Normalize();

        Vector3 fleeTarget = transform.position + fleeDirection * fleeDistance;

        UnityEngine.AI.NavMeshHit navHit;

        if (UnityEngine.AI.NavMesh.SamplePosition(fleeTarget, out navHit, fleeDistance, UnityEngine.AI.NavMesh.AllAreas))
        {
            navMeshAgent.SetDestination(navHit.position);
        }
        else
        {
            navMeshAgent.SetDestination(transform.position + fleeDirection * 4f);
        }
    }

    void StateUpdate_Flee()
    {
        fleeTimer.UpdateTimer();

        // Keep running away while flee timer is active.
        if (playerToFleeFrom != null)
        {
            Vector3 fleeDirection = transform.position - playerToFleeFrom.position;

            if (fleeDirection.magnitude < 0.1f)
            {
                fleeDirection = -transform.forward;
            }

            fleeDirection.Normalize();

            Vector3 fleeTarget = transform.position + fleeDirection * fleeDistance;

            UnityEngine.AI.NavMeshHit navHit;

            if (UnityEngine.AI.NavMesh.SamplePosition(fleeTarget, out navHit, fleeDistance, UnityEngine.AI.NavMesh.AllAreas))
            {
                navMeshAgent.SetDestination(navHit.position);
            }
        }

        if (fleeTimer.IsFinished())
        {
            playerToFleeFrom = null;
            SetState(idleState);
        }
    }

    void StateEnd_Flee()
    {
        playerToFleeFrom = null;
    }

}
