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
            Leaf switchStateToSetTrap = new Leaf("Switch State To SeT Trap", new ActionStrategy(() =>
            {
                obj.SwitchState(obj.setTrapState);
            }));

            setTrapSeqeunce.AddChild(checkTimeToSetTrap);
            setTrapSeqeunce.AddChild(switchStateToSetTrap);
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
            Leaf checkIsSeeBird = new Leaf("CheckIsSeeBird", new ConditionStrategy(() =>
            {
                Transform target = obj.currentTarget;
                return target;
            }));
            Leaf checkIsBirdTrapped = new Leaf("CheckIsBirdTrapped", new ConditionStrategy(() =>
            {
                bool isBirdTrapped = false;
                if (obj.currentTarget != null)
                {
                    isBirdTrapped = Vector3.Distance(obj.currentTarget.position, obj.transform.position) < obj.goToTrappedBirdDistance && GameEventManager.instance.hunterEvents.CheckTargetTrapped();
                }
                return isBirdTrapped;
            }));
            Leaf checkIsNearShootingRange = new Leaf("Check Is Near Shooting Range", new ConditionStrategy(() =>
            {
                return Vector3.Distance(obj.currentTarget.position, obj.transform.position) < obj.shootingDistance;
            }));

            Selector selectShootOrFollowSelector = new Selector("select Shoot Or Follow Selector");
            Leaf switchStateToShootBird = new Leaf("Switch State To Shoot Bird State", new ActionStrategy(() => obj.SwitchState(obj.shootBirdState)));
            Leaf switchStateToFollowBird = new Leaf("Switch State To Follow Bird State", new ActionStrategy(() => obj.SwitchState(obj.followBirdState)));
            Leaf switchStateToGoToTrappedbBird = new Leaf("Switch State To Go To Trapped Bird State", new ActionStrategy(() => obj.SwitchState(obj.goToTrappedBirdState)));
            Selector checkNeedToSwitchStateSelector = new Selector("Check Need To Switch State");
            Sequence checkSwitchToShootOrFollowSeqeunce = new Sequence("Check Switch To Shoot Or Follow");

            Sequence shootingSeqeunce = new Sequence("Shooting Seqeunce");
            Sequence goToTrappedBirdSeqeunce = new Sequence("GoToTrappedBird Seqeunce");

            Selector selectSwitchStateOrFindBird = new Selector("Switch State Or Find Bird");

            goToTrappedBirdSeqeunce.AddChild(checkIsBirdTrapped);
            goToTrappedBirdSeqeunce.AddChild(switchStateToGoToTrappedbBird);

            shootingSeqeunce.AddChild(checkIsNearShootingRange);
            shootingSeqeunce.AddChild(switchStateToShootBird);

            selectShootOrFollowSelector.AddChild(shootingSeqeunce);
            selectShootOrFollowSelector.AddChild(switchStateToFollowBird);

            checkSwitchToShootOrFollowSeqeunce.AddChild(checkIsSeeBird);
            checkSwitchToShootOrFollowSeqeunce.AddChild(selectShootOrFollowSelector);

            checkNeedToSwitchStateSelector.AddChild(goToTrappedBirdSeqeunce);
            checkNeedToSwitchStateSelector.AddChild(checkSwitchToShootOrFollowSeqeunce);
            checkNeedToSwitchStateSelector.AddChild(setTrapSeqeunce);

            checkIdling.AddChild(checkIsIdling);
            checkIdling.AddChild(Idling);
            selectAction.AddChild(checkIdling);
            selectAction.AddChild(checkPatroling);
            selectSwitchStateOrFindBird.AddChild(checkNeedToSwitchStateSelector);
            selectSwitchStateOrFindBird.AddChild(selectAction);
            behaviourTree.AddChild(selectSwitchStateOrFindBird);
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
            behaviourTree.Process();
        }

        void SetupTree()
        {
            behaviourTree = new BehaviourTree("FollowBird");
            UntilFail root = new UntilFail("root");
            Selector selectAction = new Selector("Select Action");
            Sequence sequenceFollowBird = new Sequence("SequenceFollow");
            Sequence toShootStateSequence = new Sequence("To Shoot State Sequence");

            Leaf checkIsNearShootingRange = new Leaf("Check Is Near Shooting Range", new ConditionStrategy(() => Vector3.Distance(obj.currentTarget.position, obj.transform.position) < obj.shootingDistance));
            Leaf switchStateToShootState = new Leaf("Switch State To Shoot State", new ActionStrategy(() => obj.SwitchState(obj.shootBirdState)));

            toShootStateSequence.AddChild(checkIsNearShootingRange);
            toShootStateSequence.AddChild(switchStateToShootState);

            Sequence toGoToTrappedBirdStateSequence = new Sequence("Go To Trapped Bird State");

            Leaf checkIsBirdTrapped = new Leaf("Check Is Bird Trapped", new ConditionStrategy(() =>
              GameEventManager.instance.hunterEvents.CheckTargetTrapped() && Vector3.Distance(obj.currentTarget.position, obj.transform.position) < obj.goToTrappedBirdDistance
            ));
            Leaf switchStateToGoToTrappedBirdState = new Leaf("Switch State To Go To Trapped Bird State", new ActionStrategy(() => obj.SwitchState(obj.goToTrappedBirdState)));

            toGoToTrappedBirdStateSequence.AddChild(checkIsBirdTrapped);
            toGoToTrappedBirdStateSequence.AddChild(switchStateToGoToTrappedBirdState);

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

            Sequence waitSeqeunce = new Sequence("Wait Seqeunce");
            Leaf checkWaitCountdown = new Leaf("Wait", new ConditionStrategy(() =>
            {
                waitTime -= Time.deltaTime;
                return waitTime < 0;
            }));
            Leaf switchStateToFindBirdState = new Leaf("Switch State To Find Bird", new ActionStrategy(() => obj.SwitchState(obj.findBirdState)));

            waitSeqeunce.AddChild(checkWaitCountdown);
            waitSeqeunce.AddChild(switchStateToFindBirdState);

            Selector selectShootOrFollowSelector = new Selector("Select Shoot Or Follow");

            selectShootOrFollowSelector.AddChild(toShootStateSequence);
            selectShootOrFollowSelector.AddChild(follow);

            sequenceFollowBird.AddChild(checkStillVisible);
            sequenceFollowBird.AddChild(selectShootOrFollowSelector);
            selectAction.AddChild(toGoToTrappedBirdStateSequence);
            selectAction.AddChild(sequenceFollowBird);
            selectAction.AddChild(waitSeqeunce);
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
            behaviourTree.Process();
        }

        void SetupTree()
        {
            behaviourTree = new BehaviourTree("Go To Trapped Bird");
            Leaf checkBirdStillTrapped = new Leaf("Check Bird Still Trapped", new ConditionStrategy(() =>
            {
                bool checkBirdStillTrapped = GameEventManager.instance.hunterEvents.CheckTargetTrapped();
                return checkBirdStillTrapped;
            }));
            Leaf checkIsStillVisible = new Leaf("Check is Bird Still Visible", new ConditionStrategy(() => obj.isTargetVisible));
            Leaf checkIsNearShootingRange = new Leaf("Check Is Near Shooting Range", new ConditionStrategy(() =>
            {
                return Vector3.Distance(obj.currentTarget.position, obj.transform.position) < obj.shootingDistance;
            }));
            Leaf switchStateToShootBird = new Leaf("Switch State To Shoot Bird State", new ActionStrategy(() => obj.SwitchState(obj.shootBirdState)));
            Leaf switchStateToFollowBird = new Leaf("Switch State To Follow Bird State", new ActionStrategy(() => obj.SwitchState(obj.followBirdState)));
            Leaf switchStateToFindBird = new Leaf("Switch State To Find Bird State", new ActionStrategy(() => obj.SwitchState(obj.findBirdState)));
            Selector checkNeedToSwitchStateSelector = new Selector("Check Need To Switch State");
            Sequence checkSwitchToShootOrFollowSeqeunce = new Sequence("Check Switch To Shoot Or Follow");
            Selector selectShootOrFollowSelector = new Selector("select Shoot Or Follow Selector");
            Sequence switchToShootingSeqeunce = new Sequence("shooting Seqeunce");
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
                bool isNearTrappedBird = Vector3.Distance(obj.currentTarget.position, obj.transform.position) < obj.captureDistance;
                return isNearTrappedBird;
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

            switchToShootingSeqeunce.AddChild(checkIsNearShootingRange);
            switchToShootingSeqeunce.AddChild(switchStateToShootBird);

            selectShootOrFollowSelector.AddChild(switchToShootingSeqeunce);
            selectShootOrFollowSelector.AddChild(switchStateToFollowBird);

            checkSwitchToShootOrFollowSeqeunce.AddChild(checkIsStillVisible);
            checkSwitchToShootOrFollowSeqeunce.AddChild(selectShootOrFollowSelector);

            checkNeedToSwitchStateSelector.AddChild(checkBirdStillTrapped);
            checkNeedToSwitchStateSelector.AddChild(checkSwitchToShootOrFollowSeqeunce);
            checkNeedToSwitchStateSelector.AddChild(switchStateToFindBird);

            captureSequence.AddChild(checkIsNearTrappedBird);
            captureSequence.AddChild(captureBird);
            capturingSelector.AddChild(captureSequence);
            capturingSelector.AddChild(goToBird);
            behaviourTree.AddChild(checkNeedToSwitchStateSelector);
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
        public ShootBirdState(Hunter obj) : base(obj)
        {
        }

        public override void EnterState()
        {
            obj.currentStateText.text = "Current State : Shoot Bird";
            SetupTree();
            obj.hunterDetectionManager.UpdateDetectionInvoke(0.05f);
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
            }));
            Selector needToSwitchStateSelector = new Selector("Need To Switch State Selector");

            Sequence toSwitchGoToTrappedBirdSeqeuence = new Sequence("To Switch Go To Trapped Bird");
            Leaf checkIsTrapped = new Leaf("CheckIsTrapped", new ConditionStrategy(() =>
                Vector3.Distance(obj.currentTarget.position, obj.transform.position) < obj.goToTrappedBirdDistance && GameEventManager.instance.hunterEvents.CheckTargetTrapped()
            ));
            Leaf switchStateToGoToTrappedBird = new Leaf("Switch State To Go To Trapped Bird", new ActionStrategy(() => obj.SwitchState(obj.goToTrappedBirdState)));

            toSwitchGoToTrappedBirdSeqeuence.AddChild(checkIsTrapped);
            toSwitchGoToTrappedBirdSeqeuence.AddChild(switchStateToGoToTrappedBird);

            Sequence toSwitchFollowBird = new Sequence("To Switch Follow Bird");

            Leaf checkIsStillNearby = new Leaf("Check is Still Nearby", new ConditionStrategy(() =>
            {
                float dist = Vector3.Distance(obj.currentTarget.position, obj.transform.position);
                bool checkIsNearby = dist < obj.viewDistance && dist > obj.shootingDistance;
                return checkIsNearby;
            }));
            Leaf switchStateToFollowBirdState = new Leaf("Switch State To Follow Bird State", new ActionStrategy(() => obj.SwitchState(obj.followBirdState)));

            toSwitchFollowBird.AddChild(checkIsStillNearby);
            toSwitchFollowBird.AddChild(switchStateToFollowBirdState);

            Sequence toSwitchFindBird = new Sequence("To Switch Find Bird");
            Leaf checkIsNotVisible = new Leaf("CheckIsNotVisible", new ConditionStrategy(() => !obj.isTargetVisible));
            Leaf switchStateToFindBirdState = new Leaf("Switch To Find Bird", new ActionStrategy(() => obj.SwitchState(obj.findBirdState)));

            toSwitchFindBird.AddChild(checkIsNotVisible);
            toSwitchFindBird.AddChild(switchStateToFindBirdState);

            shootingSeqeunce.AddChild(aim);
            shootingSeqeunce.AddChild(shoot);
            shootingSeqeunce.AddChild(reload);
            needToSwitchStateSelector.AddChild(toSwitchGoToTrappedBirdSeqeuence);
            needToSwitchStateSelector.AddChild(toSwitchFollowBird);
            needToSwitchStateSelector.AddChild(toSwitchFindBird);
            shootingSeqeunce.AddChild(needToSwitchStateSelector);
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