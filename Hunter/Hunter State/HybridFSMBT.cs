using AdiSastrawan.Node;
using UnityEngine.AI;
using UnityEngine;

namespace AdiSastrawan.FSMBT
{
    public class FindBirdState : BaseState<Hunter>
    {
        BehaviourTree behaviourTree;
        Vector3 currentTargetPosition = Vector3.zero;
        bool isPatroling;
        bool isIdling;
        float currentIdlingTime;
        float currentTimeToSetTrap;
        public FindBirdState(Hunter obj) : base(obj)
        {
        }

        public override void EnterState()
        {
            obj.currentStateText.text = "Current State : Find Bird";
            currentIdlingTime = 0.5f;
            obj.hunterDetectionManager.SwitchDetectionMode(HunterDetectionMode.Searching);
            obj.lastKnownTargetPosition = Vector3.zero;
            obj.currentTarget = null;
            currentTimeToSetTrap = Random.Range(3, obj.timeToSetTrapInterval);
            isIdling = true;
            SetupTree();
        }

        public override void ExitState()
        {
            isPatroling = false;
            currentTargetPosition = Vector3.zero;
        }

        public override void UpdateState()
        {
            if(currentTimeToSetTrap < 0)
            {
                obj.SwitchState(obj.setTrapState);
                return;
            }
            if (obj.currentTarget != null)
            {
                bool isBirdTrapped = Vector3.Distance(obj.currentTarget.position, obj.transform.position) < obj.goToTrappedBirdDistance && GameEventManager.instance.hunterEvents.CheckTargetTrapped();
                if (isBirdTrapped)
                {
                    obj.SwitchState(obj.goToTrappedBirdState);
                    return;
                }
                if(Vector3.Distance(obj.currentTarget.position, obj.transform.position) < obj.shootingDistance)
                {
                    obj.SwitchState(obj.shootBirdState);
                    return;
                }
                obj.SwitchState(obj.followBirdState);
                return;
            }

            behaviourTree.Process();
        }

        void SetupTree()
        {
            behaviourTree = new BehaviourTree("FindBird");
            Selector selectAction = new Selector("Select Action");
            Sequence checkIdling = new Sequence("Check Idling");
            Sequence checkPatroling = new Sequence("Check Patroling");

            Sequence setTrapSeqeunce = new Sequence("Set Trap Seqeunce");
            Leaf checkTimeToSetTrap = new Leaf("CheckTimeToSetTrap", new ConditionStrategy(() =>
            {
                currentTimeToSetTrap -= Time.deltaTime;
                return currentTimeToSetTrap < 0;
            }));
            Leaf patrol = new Leaf("Patrol", new ActionStrategy(
            () =>
            {
                if (Vector3.Distance(currentTargetPosition, obj.transform.position) < 0.1f && currentTargetPosition != Vector3.zero)
                {
                    isIdling = true;
                    isPatroling = false;
                    currentTargetPosition = Vector3.zero;
                }
                else if (currentTargetPosition == Vector3.zero)
                {
                    currentTargetPosition = RandomPatrolPosition(obj.transform.position, 25f, LayerMask.NameToLayer("Ground"));
                    obj.agent.SetDestination(currentTargetPosition);
                }
            }));
            Leaf checkIsPatroling = new Leaf("Check is Patroling", new ConditionStrategy(() => !isIdling));
            checkPatroling.AddChild(checkIsPatroling);
            checkPatroling.AddChild(patrol);
            Leaf Idling = new Leaf("Idling", new ActionStrategy(() =>
            {
                if (isIdling)
                {
                    currentIdlingTime -= Time.deltaTime * 2;
                    if (currentIdlingTime < 0)
                    {
                        isIdling = false;
                        isPatroling = true;
                        currentIdlingTime = Random.Range(obj.waitTime, 2);
                    }
                }
            }));
            Leaf checkIsIdling = new Leaf("CheckIsIdling", new ConditionStrategy(() => !isPatroling));
        
            Selector selectShootOrFollowSelector = new Selector("select Shoot Or Follow Selector");
            Selector checkNeedToSwitchStateSelector = new Selector("Check Need To Switch State");

            Sequence shootingSeqeunce = new Sequence("Shooting Seqeunce");
            Sequence goToTrappedBirdSeqeunce = new Sequence("GoToTrappedBird Seqeunce");

            Selector selectSwitchStateOrFindBird = new Selector("Switch State Or Find Bird");
            checkIdling.AddChild(checkIsIdling);
            checkIdling.AddChild(Idling);
            selectAction.AddChild(checkTimeToSetTrap);
            selectAction.AddChild(checkIdling);
            selectAction.AddChild(checkPatroling);
            behaviourTree.AddChild(selectAction);
        }

        Vector3 RandomPatrolPosition(Vector3 origin, float distance, int layermask)
        {
            Vector3 randomSphere = Random.insideUnitSphere * distance;
            randomSphere += origin;
            NavMeshHit navHit;
            NavMesh.SamplePosition(randomSphere, out navHit, distance, layermask);
            Vector3 targetPosition = navHit.position;
            targetPosition.y = 0;
            return targetPosition;
        }
    }
    public class FollowBirdState : BaseState<Hunter>
    {
        BehaviourTree behaviourTree;
        float waitTime = 3;
        Vector3 groundLastKnownPosition;
        string stateText = "Current State : Follow Bird";
        public FollowBirdState(Hunter obj) : base(obj)
        {
        }

        public override void EnterState()
        {
            obj.currentStateText.text = stateText;
            waitTime = obj.waitTime;
            obj.hunterDetectionManager.SwitchDetectionMode(HunterDetectionMode.Chasing);
            SetupTree();
        }

        public override void ExitState()
        {
        }

        public override void UpdateState()
        {
            if (obj.currentTarget != null)
            {
                bool isBirdTrapped = Vector3.Distance(obj.currentTarget.position, obj.transform.position) < obj.goToTrappedBirdDistance && GameEventManager.instance.hunterEvents.CheckTargetTrapped();
                if (isBirdTrapped)
                {
                    obj.SwitchState(obj.goToTrappedBirdState);
                    return;
                }
                if (obj.isTargetVisible&&Vector3.Distance(obj.currentTarget.position, obj.transform.position) < obj.shootingDistance)
                {
                    obj.SwitchState(obj.shootBirdState);
                    return;
                }
                if(waitTime < 0)
                {
                    obj.SwitchState(obj.findBirdState);
                    return;
                }
            }
            behaviourTree.Process();
        }

        void SetupTree()
        {
            behaviourTree = new BehaviourTree("FollowBird");
            Selector selectAction = new Selector("Select Action");

            Leaf checkStillVisible = new Leaf("CheckStillVisible", new ConditionStrategy(() => obj.isTargetVisible));
            Leaf follow = new Leaf("Follow", new ActionStrategy(() =>
            {
                if (obj.currentTarget != null)
                {
                    groundLastKnownPosition = obj.lastKnownTargetPosition;
                    groundLastKnownPosition.y = 0;
                    obj.agent.SetDestination(groundLastKnownPosition);
                }
                waitTime = obj.waitTime;
            }));
            Sequence followSequence = new Sequence("Follow Seqeunce");
            followSequence.AddChild(checkStillVisible);
            followSequence.AddChild(follow);
            Leaf checkWaitCountdown = new Leaf("Wait", new ConditionStrategy(() =>
            {
                waitTime -= Time.deltaTime;
                return waitTime < 0;
            }));

            selectAction.AddChild(followSequence);
            selectAction.AddChild(checkWaitCountdown);
            behaviourTree.AddChild(selectAction);
        }

    }
    public class GoToTrappedBirdState : BaseState<Hunter>
    {
        BehaviourTree behaviourTree;
        public GoToTrappedBirdState(Hunter obj) : base(obj)
        {
        }

        public override void EnterState()
        {
            obj.currentStateText.text = "Current State : Go to Trapped Bird Bird";
            obj.agent.stoppingDistance = 0.2f;
            SetupTree();
        }

        public override void ExitState()
        {
            obj.agent.stoppingDistance = 0f;
        }

        public override void UpdateState()
        {
            if (obj.currentTarget != null && !GameEventManager.instance.hunterEvents.CheckTargetTrapped()) { 
                if (!obj.isTargetVisible)
                {
                    obj.SwitchState(obj.findBirdState);
                    return;
                }
                if (Vector3.Distance(obj.currentTarget.position, obj.transform.position) < obj.shootingDistance)
                {
                    obj.SwitchState(obj.shootBirdState);
                    return;
                }
                else
                {
                    obj.SwitchState(obj.followBirdState);
                    return;
                }
            }
             
            behaviourTree.Process();
        }

        void SetupTree()
        {
            behaviourTree = new BehaviourTree("Go To Trapped Bird");
            Leaf checkBirdStillTrapped = new Leaf("Check Bird Still Trapped", new ConditionStrategy(() =>
            {
                return GameEventManager.instance.hunterEvents.CheckTargetTrapped();
            }));
            Leaf goToBird = new Leaf("Go to bird", new ActionStrategy(() =>
            {
                if (obj.currentTarget != null)
                {
                    Vector3 currentTargetPos = obj.currentTarget.transform.position;
                    currentTargetPos.y = 0;
                    obj.agent.SetDestination(currentTargetPos);
                }
            }));
            Leaf checkIsNearTrappedBird = new Leaf("Check Is Near Trapped Bird", new ConditionStrategy(() =>
            {
                if (obj.currentTarget == null) return false;
                return Vector3.Distance(obj.currentTarget.position, obj.transform.position) < obj.captureDistance;
            }));
            Sequence captureSequence = new Sequence("Capture Seqeunce");
            Leaf captureBird = new Leaf("Capture Bird", new ActionStrategy(() =>
            {
                if (!ScoreManager.instance.isGameEnd)
                {
                    GameEventManager.instance.uIEvents.BirdCaptured(true);
                }
                ScoreManager.instance.EndGame(true);
            }));
            Selector capturingSelector = new Selector("Capturing Selector");

            captureSequence.AddChild(checkIsNearTrappedBird);
            captureSequence.AddChild(captureBird);
            capturingSelector.AddChild(captureSequence);
            capturingSelector.AddChild(goToBird);
            behaviourTree.AddChild(checkBirdStillTrapped);
            behaviourTree.AddChild(capturingSelector);
        }
    }
    public class SetTrapState : BaseState<Hunter>
    {
        BehaviourTree behaviourTree;

        public SetTrapState(Hunter obj) : base(obj)
        {
        }

        public override void EnterState()
        {
            obj.currentStateText.text = "Current State : Set Trap";
            SetupTree();
        }

        public override void ExitState()
        {
        }

        public override void UpdateState()
        {
            behaviourTree.Process();
        }
        void SetupTree()
        {
            behaviourTree = new BehaviourTree("Set Trap State");
            Leaf prepareTrap = new Leaf("Preparing Trap", new WaitForAnimationStrategy(obj.animator, "Set Trap", () =>
            {
                obj.weapon.SetParent(obj.weaponRest);
                obj.weapon.localPosition = obj.weaponRest.localPosition;
                obj.weapon.rotation = obj.weaponRest.rotation;
                obj.animator.SetTrigger("setTrap");
            }));
            Leaf setTrap = new Leaf("Set Trap", new ActionStrategy(() =>
            {
                obj.SetTrap();
                obj.weapon.SetParent(obj.weaponReady);
                obj.weapon.localPosition = obj.weaponReady.localPosition;
                obj.weapon.rotation = obj.weaponReady.rotation;
            }));
            Leaf switchState = new Leaf("Switch to Find Bird", new ActionStrategy(() =>
            {
                obj.SwitchState(obj.findBirdState);
            }));
            behaviourTree.AddChild(prepareTrap);
            behaviourTree.AddChild(setTrap);
            behaviourTree.AddChild(switchState);
        }
    }
    public class ShootBirdState : BaseState<Hunter>
    {
        BehaviourTree behaviourTree;
        bool isAiming;
        public ShootBirdState(Hunter obj) : base(obj)
        {
        }

        public override void EnterState()
        {
            obj.currentStateText.text = "Current State : Shoot Bird";
            SetupTree();
            obj.hunterDetectionManager.UpdateDetectionInvoke(0.05f);
            isAiming = true;
            obj.agent.ResetPath();
            obj.agent.isStopped = true;
        }

        public override void ExitState()
        {
            obj.agent.isStopped = false;
            obj.viewDistance = 25;
            obj.hunterDetectionManager.UpdateDetectionInvoke();

        }

        public override void UpdateState()
        {
            if (obj.currentTarget != null && !isAiming)
            {
                bool isBirdTrapped = Vector3.Distance(obj.currentTarget.position, obj.transform.position) < obj.goToTrappedBirdDistance && GameEventManager.instance.hunterEvents.CheckTargetTrapped();
                if (isBirdTrapped)
                {
                    obj.SwitchState(obj.goToTrappedBirdState);
                    return;
                }
                if (obj.isTargetVisible && Vector3.Distance(obj.currentTarget.position, obj.transform.position) < obj.shootingDistance)
                {
                    obj.SwitchState(obj.shootBirdState);
                    return;
                }
                if (!obj.isTargetVisible)
                {
                    obj.SwitchState(obj.findBirdState);
                    return;
                }
                float dist = Vector3.Distance(obj.currentTarget.position, obj.transform.position);
                bool checkIsNearby = dist < obj.viewDistance && dist > obj.shootingDistance;
                if (checkIsNearby)
                {
                    obj.SwitchState(obj.followBirdState);
                    return;
                }
            }

            behaviourTree.Process();
        }

        void SetupTree()
        {
            behaviourTree = new BehaviourTree("ShootBird");
            UntilFail root = new UntilFail("root");
            Sequence shootingSeqeunce = new Sequence("ShootingSeqeunce");
            Leaf aim = new Leaf("Aim", new WaitingTimeStrategy(1.5f, () =>
            {
                Vector3 directionToTarget = (obj.lastKnownTargetPosition - obj.firePoint.position).normalized;
                obj.transform.rotation = Quaternion.Slerp(obj.transform.rotation, Quaternion.LookRotation(directionToTarget), Time.deltaTime * 3f);
                obj.aimTarget.position = Vector3.MoveTowards(obj.aimTarget.position, obj.lastKnownTargetPosition, Time.deltaTime * 10f);

            }, () =>
            {
                obj.AimStart(true);
                obj.animator.SetBool("isAiming", true);
                obj.hunterDetectionManager.SwitchDetectionMode(HunterDetectionMode.Chasing, 50f);
            }
            ,
            () =>
            {
                obj.animator.SetBool("isAiming", false);

            }));
            Leaf shoot = new Leaf("Shoot", new WaitForAnimationStrategy(obj.animator, "Firing Rifle", () =>
            {
                obj.animator.SetTrigger("firing");
                obj.soundManager.PlaySFX("Shoot");

                Shoot();
            }, () => { }));
            Leaf reload = new Leaf("Reload", new WaitForAnimationStrategy(obj.animator, "Reloading", () => { }, () => { }, () =>
            {
                obj.hunterDetectionManager.SwitchDetectionMode(HunterDetectionMode.Chasing, 25f);
                obj.AimStart(false);
                isAiming = false;
            }));

            shootingSeqeunce.AddChild(aim);
            shootingSeqeunce.AddChild(shoot);
            shootingSeqeunce.AddChild(reload);
            behaviourTree.AddChild(shootingSeqeunce);
        }
        void Shoot()
        {
            Vector3 directionToTarget = (obj.lastKnownTargetPosition - obj.firePoint.position).normalized;
            Vector3 finalDirection = (directionToTarget).normalized;
            GameObject bullet = GameObject.Instantiate(obj.bulletPrefab, obj.firePoint.position, Quaternion.LookRotation(finalDirection));
        }
    }
}