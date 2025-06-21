using AdiSastrawan.Node;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SocialPlatforms.Impl;

public class HunterBehaviorTree
{
    public BehaviourTree behaviourTree;
    public Hunter hunter;
    Vector3 currentTargetPosition = Vector3.zero;
    bool isPatroling;
    bool isIdling;
    float currentIdlingTime;
    float currentTimeToSetTrap;

    float waitTime = 0;
    Vector3 groundLastKnownPosition;
    public HunterBehaviorTree(Hunter hunter)
    {
        this.hunter = hunter;
    }
    public void SetupTree()
    {
        currentIdlingTime = 0.5f;
        hunter.lastKnownTargetPosition = Vector3.zero;
        behaviourTree = new BehaviourTree("Hunter Behavior");
        currentTimeToSetTrap = hunter.timeToSetTrapInterval;
        Selector root = new Selector("root");
        //find Bird
        Sequence findBirdSequence = new Sequence("find bird");
        Sequence patrolSequence = new Sequence("Patrol Seqeunce");
        Sequence idleSequence = new Sequence("Idle Seqeunce");
        Leaf changeToSearchingMode = new Leaf("Change mode to Searching", new ActionStrategy(() =>
        {
            hunter.hunterDetectionManager.SwitchDetectionMode(HunterDetectionMode.Searching);
            hunter.currentTarget = null;
        }));
        Leaf checkIsIdling = new Leaf("CheckIsIdling", new ConditionStrategy(() => !isPatroling));
        Leaf checkIsPatroling = new Leaf("Check is Patroling", new ConditionStrategy(() => !isIdling));
        Leaf patrol = new Leaf("Patrol", new ActionStrategy(() =>
         {

             if (Vector3.Distance(currentTargetPosition, hunter.transform.position) < 0.1f && currentTargetPosition != Vector3.zero)
             {
                 isIdling = true;
                 isPatroling = false;
                 currentTargetPosition = Vector3.zero;
             }
             else if (currentTargetPosition == Vector3.zero)
             {
                 currentTargetPosition = RandomPatrolPosition(hunter.transform.position, 25f, LayerMask.NameToLayer("Ground"));
                 hunter.agent.SetDestination(currentTargetPosition);
             }
         }));
        Leaf Idling = new Leaf("Idling", new ActionStrategy(() =>
        {
            if (isIdling)
            {
                currentIdlingTime -= Time.deltaTime * 2;
                if (currentIdlingTime < 0)
                {
                    isIdling = false;
                    isPatroling = true;
                    currentIdlingTime = Random.Range(hunter.waitTime, 2);
                }
            }
        }));
        Leaf checkIsBirdTrapped = new Leaf("CheckIsBirdTrapped", new ConditionStrategy(() =>
        {
            if (hunter.currentTarget != null)
            {
                bool isBirdTrapped = Vector3.Distance(hunter.currentTarget.position, hunter.transform.position) < 70f && GameEventManager.instance.hunterEvents.CheckTargetTrapped();
                return isBirdTrapped;
            }
            return false;
        }));
        Leaf timeToSetTrap = new Leaf("Time To SeT Trap", new ConditionStrategy(() =>
        {
            currentTimeToSetTrap -= Time.deltaTime;
            if(currentTimeToSetTrap < 0)
            {
                currentTimeToSetTrap = hunter.timeToSetTrapInterval;
                return false;
            }
            return true;
        }));
        Selector patrolSelector = new Selector("Patrol selector");
        patrolSequence.AddChild(checkIsPatroling);
        patrolSequence.AddChild(patrol);
        idleSequence.AddChild(checkIsIdling);
        idleSequence.AddChild(Idling);
        idleSequence.AddChild(timeToSetTrap);
        patrolSelector.AddChild(patrolSequence);
        patrolSelector.AddChild(idleSequence);
        findBirdSequence.AddChild(changeToSearchingMode);
        findBirdSequence.AddChild(patrolSelector);
        //Follow Bird
        Sequence followBirdSeqeunce = new Sequence("Follow Bird Seqeunce");
        Leaf checkStillVisible = new Leaf("CheckStillVisible", new ConditionStrategy(() => {
            if (!hunter.isTargetVisible)
            {
                isPatroling = false;
                isIdling = true;
                currentTargetPosition = Vector3.zero;
            }
            return hunter.isTargetVisible;
        }));
        Leaf checkIsSeenBird = new Leaf("Check Is Seen Bird", new ConditionStrategy(() =>
        {
            Transform target = hunter.currentTarget;
            return target != null;
        }));
        Leaf changeToChasingMode = new Leaf("Change mode to Chasing", new ActionStrategy(() =>
        { 
            hunter.hunterDetectionManager.UpdateDetectionInvoke(0.05f);
            hunter.hunterDetectionManager.SwitchDetectionMode(HunterDetectionMode.Chasing);
        }));
        Leaf follow = new Leaf("Follow", new ActionStrategy(() =>
        {
            waitTime = hunter.waitTime;
            if (hunter.currentTarget != null)
            {
                groundLastKnownPosition = hunter.lastKnownTargetPosition;
                groundLastKnownPosition.y = 0;
                hunter.agent.SetDestination(groundLastKnownPosition);
            }
            waitTime = hunter.waitTime;
        }));

        Sequence waitSequence = new Sequence("Wait seqeunce");
        Leaf waitToSwitch = new Leaf("Wait", new ActionStrategy(() =>
        {
            waitTime -= Time.deltaTime;
        }));
        Leaf checkIsWaiting = new Leaf("Check Is Waiting", new ConditionStrategy(() => waitTime > 0));

        waitSequence.AddChild(waitToSwitch);
        waitSequence.AddChild(checkIsWaiting);
        //Shoot Bird
        Sequence shootBirdSequence = new Sequence("shoot Bird Seqeunce");
        Leaf checkOnShootingRange = new Leaf("Check On Shooting Range", new ConditionStrategy(() =>
        {
            bool isNearShootingRange = Vector3.Distance(hunter.currentTarget.position, hunter.transform.position) < hunter.shootingDistance;
            return isNearShootingRange;
        }));
        Leaf updateShootAction = new Leaf("Update shoot action", new ActionStrategy(() =>
        {
            hunter.agent.ResetPath();
            hunter.agent.isStopped = true;
        }));
        Leaf aim = new Leaf("Aim", new WaitingTimeStrategy(1.5f, () => {
            Vector3 directionToTarget = (hunter.lastKnownTargetPosition - hunter.firePoint.position).normalized;
            hunter.transform.rotation = Quaternion.Slerp(hunter.transform.rotation, Quaternion.LookRotation(directionToTarget), Time.deltaTime * 2f);
        }, () =>
        {
            hunter.animator.SetBool("isAiming", true);
            hunter.hunterDetectionManager.SwitchDetectionMode(HunterDetectionMode.Chasing, 50f);
        },() => hunter.animator.SetBool("isAiming", false)));
        Leaf shoot = new Leaf("Shoot", new WaitForAnimationStrategy(hunter.animator, "Firing Rifle", () =>
        {
            hunter.animator.SetTrigger("firing");
            Shoot();
        }));
        Leaf reload = new Leaf("Reload", new WaitForAnimationStrategy(hunter.animator, "Reloading", () => { }, () => { }, () => hunter.hunterDetectionManager.SwitchDetectionMode(HunterDetectionMode.Chasing, 25f)));
        shootBirdSequence.AddChild(checkOnShootingRange);
        shootBirdSequence.AddChild(updateShootAction);
        shootBirdSequence.AddChild(aim);
        shootBirdSequence.AddChild(shoot);
        shootBirdSequence.AddChild(reload);

        Sequence seenBirdSeqeunce = new Sequence("Seen bird seqeunce");
        Selector seenBirdDecision = new Selector("Seen bird Decision");

        Sequence birdStillVisibleSeqeunce = new Sequence("Bird Still Visible Seqeunce");

        Selector selectShootingOrFollow = new Selector("Select Shooting or Follow Selector");

        selectShootingOrFollow.AddChild(shootBirdSequence);
        selectShootingOrFollow.AddChild(follow);

        birdStillVisibleSeqeunce.AddChild(checkStillVisible);
        birdStillVisibleSeqeunce.AddChild(selectShootingOrFollow);

        seenBirdDecision.AddChild(birdStillVisibleSeqeunce);
        seenBirdDecision.AddChild(waitSequence);

        //Go to Trapped Bird
        Sequence goToTrappedBirdSequence = new Sequence("Go to trapped bird");
        Leaf checkBirdStillTrapped = new Leaf("Check Bird Still Trapped", new ConditionStrategy(() =>
        {
            hunter.agent.isStopped = false;
            hunter.viewDistance = 25;
            hunter.hunterDetectionManager.UpdateDetectionInvoke();
            if (hunter.currentTarget == null) return false;
            bool checkBirdStillTrapped = Vector3.Distance(hunter.currentTarget.position, hunter.transform.position) < hunter.goToTrappedBirdDistance && GameEventManager.instance.hunterEvents.CheckTargetTrapped();
            return checkBirdStillTrapped;
        }));
        Leaf goToBird = new Leaf("Go to bird", new ActionStrategy(() =>
        {
            if (hunter.currentTarget != null)
            {
                Vector3 currentTargetPos = hunter.currentTarget.transform.position;
                currentTargetPos.y = 0;
                hunter.agent.SetDestination(currentTargetPos);
            }
        }));
        Leaf checkIsNearTrappedBird = new Leaf("Check Is Near Trapped Bird", new ConditionStrategy(() =>
        {
            bool isNearTrappedBird = Vector3.Distance(hunter.currentTarget.position, hunter.transform.position) < hunter.captureDistance;
            return isNearTrappedBird;
        }));
        Leaf captureBird = new Leaf("Capture Bird", new ActionStrategy(() =>
        {
            ScoreManager.instance.EndGame(true);
            GameEventManager.instance.uIEvents.BirdCaptured(true);
        }));
        Sequence captureSequence = new Sequence("Capture Seqeunce");
        Selector capturingSelector = new Selector("Capturing Selector");
        captureSequence.AddChild(checkIsNearTrappedBird);
        captureSequence.AddChild(captureBird);
        capturingSelector.AddChild(captureSequence);
        capturingSelector.AddChild(goToBird);

        goToTrappedBirdSequence.AddChild(checkBirdStillTrapped);
        goToTrappedBirdSequence.AddChild(capturingSelector);


        // Set Trap
        Sequence setTrapSeqeunce = new Sequence("Set Trap Seqeunce");
        Leaf prepareTrap = new Leaf("Preparing Trap", new WaitForAnimationStrategy(hunter.animator, "Set Trap", () =>
        {
            hunter.weapon.SetParent(hunter.weaponRest);
            hunter.weapon.localPosition = hunter.weaponRest.localPosition;
            hunter.weapon.rotation = hunter.weaponRest.rotation;
            hunter.animator.SetTrigger("setTrap");
        }));
        Leaf setTrap = new Leaf("Set Trap", new ActionStrategy(() =>
        {
            hunter.SetTrap();
            hunter.weapon.SetParent(hunter.weaponReady);
            hunter.weapon.localPosition = hunter.weaponReady.localPosition;
            hunter.weapon.rotation = hunter.weaponReady.rotation;
        }));

        setTrapSeqeunce.AddChild(prepareTrap);
        setTrapSeqeunce.AddChild(setTrap);

        seenBirdSeqeunce.AddChild(checkIsSeenBird);
        seenBirdSeqeunce.AddChild(changeToChasingMode);
        seenBirdSeqeunce.AddChild(seenBirdDecision);

        root.AddChild(goToTrappedBirdSequence);
        root.AddChild(seenBirdSeqeunce);
        root.AddChild(findBirdSequence);
        root.AddChild(setTrapSeqeunce);
        behaviourTree.AddChild(root);
        
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
    void Shoot()
    {
        Vector3 directionToTarget = (hunter.lastKnownTargetPosition - hunter.firePoint.position).normalized;
        Vector3 finalDirection = (directionToTarget).normalized;

        GameObject bullet = GameObject.Instantiate(hunter.bulletPrefab, hunter.firePoint.position, Quaternion.LookRotation(finalDirection));
    }
}
