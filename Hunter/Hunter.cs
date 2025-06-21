using AdiSastrawan.FSM;
using AdiSastrawan.FSMBT;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations.Rigging;

public class Hunter : MonoBehaviour
{
    public BaseState<Hunter> currentState;
    public FindBirdState findBirdState;
    public FollowBirdState followBirdState;
    public ShootBirdState shootBirdState;
    public GoToTrappedBirdState goToTrappedBirdState;
    public SetTrapState setTrapState;
    public HunterBehaviorTree hunterBehaviorTree;
    public HunterDetectionManager hunterDetectionManager;
    public NavMeshAgent agent;
    public Transform currentTarget;
    public bool isTargetVisible;
    public bool usingBehaviorTree;
    public bool usingFSM;
    public Vector3 lastKnownTargetPosition;
    public Animator animator;

    public float viewDistance = 10f;
    public float goToTrappedBirdDistance = 70f;
    public float shootingDistance = 15f;
    public float captureDistance = 1.5f;
    public float fieldOfView = 90f;
    public float waitTime = 1.5f;
    public float setTrapTime = 1.5f;
    public float timeToSetTrapInterval = 4f;

    public Transform firePoint;
    public GameObject bulletPrefab,trapPrefab;
    public float bulletSpeed = 50f;
    public float aimTime = 1.5f;
    public float accuracy = 0.5f;

    public LayerMask obstacleLayer;
    public LayerMask targetLayer;
    public TMP_Text currentStateText;
    public Transform weaponRest;
    public Transform weaponReady;
    public Transform weapon;

    public HunterSoundManager soundManager;
    public Transform aimTarget;
    public Rig aimRig;
    public float iKBlendSpeed = 5f;

    //FSM
    public float fsmCurrentTimeIntervalToSetTrap;
    public float fsmTimeIntervalToSetTrap=15f;
    public Patrol patrolState;
    public Idle idleState;
    public PrepareTrap prepareTrapState;
    public PutTrap putTrapState;
    public Chase chaseState;
    public Wait waitState;
    public Aim aimState;
    public Shoot shootState;
    public Reload reloadState;
    public MoveToTrappedBird moveToTrappedBirdState;
    public CaptureBird captureBirdState;
    void Awake()
    {
        soundManager = GetComponentInChildren<HunterSoundManager>();
        if (usingBehaviorTree) return;
        if (usingFSM)
        {
            patrolState = new Patrol(this);
            idleState = new Idle(this);
            prepareTrapState = new PrepareTrap(this);
            putTrapState = new PutTrap(this);
            chaseState = new Chase(this);
            waitState = new Wait(this);
            aimState = new Aim(this);
            shootState = new Shoot(this);
            reloadState = new Reload(this);
            captureBirdState = new CaptureBird(this);
            moveToTrappedBirdState = new MoveToTrappedBird(this);
            return;
        }
        findBirdState = new FindBirdState(this);
        followBirdState = new FollowBirdState(this);
        shootBirdState = new ShootBirdState(this);
        goToTrappedBirdState = new GoToTrappedBirdState(this);
        setTrapState = new SetTrapState(this);
        currentState = findBirdState;
    }
    void OnEnable()
    {
        GameEventManager.instance.hunterEvents.OnTargetAssign += AssignTarget;
    }
    void OnDisable()
    {
        GameEventManager.instance.hunterEvents.OnTargetAssign -= AssignTarget;
    }
    void Start()
    {
        if (usingBehaviorTree)
        {
            hunterBehaviorTree = new HunterBehaviorTree(this);
            hunterBehaviorTree.SetupTree();
            return;
        }
        if (usingFSM)
        {
            fsmCurrentTimeIntervalToSetTrap = Random.Range(10, fsmTimeIntervalToSetTrap);
            currentState = idleState;
        }
        
        currentState.EnterState();
        
    }
    void Update()
    {
        animator.SetFloat("speed",agent.velocity.magnitude);
        if (usingBehaviorTree) 
        {
            hunterBehaviorTree.behaviourTree.Process();
            return;
        }
        currentState.UpdateState();   
    }
    public void SwitchState(BaseState<Hunter> state)
    {
        if(currentState != null) currentState.ExitState();
        currentState = state;
        currentState.EnterState();
    }
    void AssignTarget(Transform target)
    {
        currentTarget = target;
    }
    public void SetTrap()
    {
        Object.Instantiate(trapPrefab, transform.forward + transform.position, Quaternion.identity);
    }
    public void AimStart(bool activation)
    {
        float targetWeight = activation ? 1f : 0f;
        aimRig.weight = targetWeight;
    }
    public void FSMSetTrapTick()
    {
        fsmCurrentTimeIntervalToSetTrap -= Time.deltaTime * 0.5f;
        if (fsmCurrentTimeIntervalToSetTrap < 0)
        {
            //TO DO Set Trap
            SwitchState(prepareTrapState);
            fsmCurrentTimeIntervalToSetTrap = Random.Range(10, fsmTimeIntervalToSetTrap);
        }
    }
    public bool WaitForAnimation(Animator animator, string animationName)
    {
        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
        return info.IsName(animationName) && animator.IsInTransition(0) && info.normalizedTime >= 0.9f;
    }
}
