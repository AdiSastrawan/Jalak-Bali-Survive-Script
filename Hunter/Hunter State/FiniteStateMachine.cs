using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace AdiSastrawan.FSM
{
    public class Patrol : BaseState<Hunter>
    {
        Vector3 currentTargetPosition = Vector3.zero;
        float currentIdlingTime;
        public Patrol(Hunter obj) : base(obj)
        {
        }

        public override void EnterState()
        {
            obj.currentStateText.text = "Current State : Patrol";
            currentIdlingTime = 0.5f;
            obj.hunterDetectionManager.SwitchDetectionMode(HunterDetectionMode.Searching);
            obj.lastKnownTargetPosition = Vector3.zero;
            obj.currentTarget = null;
        }

        public override void ExitState()
        {
            currentTargetPosition = Vector3.zero;
        }

        public override void UpdateState()
        {
            if (Vector3.Distance(currentTargetPosition, obj.transform.position) < 0.1f && currentTargetPosition != Vector3.zero)
            {
                currentTargetPosition = Vector3.zero;
                obj.SwitchState(obj.idleState);
            }
            else if (currentTargetPosition == Vector3.zero)
            {
                currentTargetPosition = RandomPatrolPosition(obj.transform.position, 25f, LayerMask.NameToLayer("Ground"));
                obj.agent.SetDestination(currentTargetPosition);
            }

            if (obj.currentTarget != null)
            {
                if (Vector3.Distance(obj.currentTarget.position, obj.transform.position) < obj.goToTrappedBirdDistance && GameEventManager.instance.hunterEvents.CheckTargetTrapped())
                {
                    obj.SwitchState(obj.moveToTrappedBirdState);
                    return;
                }
                if(Vector3.Distance(obj.currentTarget.position, obj.transform.position) < obj.shootingDistance)
                {
                    obj.SwitchState(obj.aimState);
                    return;
                }
                obj.SwitchState(obj.chaseState);
            }
            obj.FSMSetTrapTick();
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
    public class Idle : BaseState<Hunter>
    {
        float currentIdlingTime;
        public Idle(Hunter obj) : base(obj)
        {
        }

        public override void EnterState()
        {
            obj.currentStateText.text = "Current State : Idle";
            currentIdlingTime = Random.Range(obj.waitTime, 2);
            obj.hunterDetectionManager.SwitchDetectionMode(HunterDetectionMode.Searching);
            obj.lastKnownTargetPosition = Vector3.zero;
            obj.currentTarget = null;
        }

        public override void ExitState()
        {

        }

        public override void UpdateState()
        {
            currentIdlingTime -= Time.deltaTime*0.5f;
            if (currentIdlingTime < 0)
            {
                obj.SwitchState(obj.patrolState);
            }
            if (obj.currentTarget != null)
            {
                if (Vector3.Distance(obj.currentTarget.position, obj.transform.position) < obj.goToTrappedBirdDistance && GameEventManager.instance.hunterEvents.CheckTargetTrapped())
                {
                    obj.SwitchState(obj.moveToTrappedBirdState);
                    return;
                }
                if (Vector3.Distance(obj.currentTarget.position, obj.transform.position) < obj.shootingDistance)
                {
                    obj.SwitchState(obj.aimState);
                    return;
                }
                obj.SwitchState(obj.chaseState);
            }
            obj.FSMSetTrapTick();
        }
    }
    public class PrepareTrap : BaseState<Hunter>
    {
        public PrepareTrap(Hunter obj) : base(obj)
        {
        }

        public override void EnterState()
        {
            obj.weapon.SetParent(obj.weaponRest);
            obj.weapon.localPosition = obj.weaponRest.localPosition;
            obj.weapon.rotation = obj.weaponRest.rotation;
            obj.animator.SetTrigger("setTrap");
            obj.currentStateText.text = "Current State : Prepare Trap";
        }

        public override void ExitState()
        {
        }

        public override void UpdateState()
        {
            if(obj.WaitForAnimation(obj.animator, "Set Trap"))
            {
                obj.SwitchState(obj.putTrapState);
            }
        }
    }
    public class PutTrap : BaseState<Hunter>
    {
        public PutTrap(Hunter obj) : base(obj)
        {
        }

        public override void EnterState()
        {
            obj.SetTrap();

        }

        public override void ExitState()
        {
            obj.weapon.SetParent(obj.weaponReady);
            obj.weapon.localPosition = obj.weaponReady.localPosition;
            obj.weapon.rotation = obj.weaponReady.rotation;
        }

        public override void UpdateState()
        {
            obj.SwitchState(obj.idleState);
        }
    }
    public class Chase : BaseState<Hunter>
    {
        Vector3 groundLastKnownPosition;
        public Chase(Hunter obj) : base(obj)
        {
        }

        public override void EnterState()
        {
            obj.currentStateText.text = "Current State : Chase";
            obj.hunterDetectionManager.SwitchDetectionMode(HunterDetectionMode.Chasing);
        }

        public override void ExitState()
        {
        }

        public override void UpdateState()
        {

            if(!obj.isTargetVisible)
            {
                obj.SwitchState(obj.waitState);
                return;
            }
            if (obj.currentTarget != null )
            {
                if (Vector3.Distance(obj.currentTarget.position, obj.transform.position) < obj.goToTrappedBirdDistance && GameEventManager.instance.hunterEvents.CheckTargetTrapped())
                {
                    obj.SwitchState(obj.moveToTrappedBirdState);
                    return;
                }
                if (obj.isTargetVisible && Vector3.Distance(obj.currentTarget.position, obj.transform.position) < obj.shootingDistance)
                {
                    obj.SwitchState(obj.aimState);
                    return;
                }
                groundLastKnownPosition = obj.lastKnownTargetPosition;
                groundLastKnownPosition.y = 0;
                obj.agent.SetDestination(groundLastKnownPosition);
                
            }
        }
    }
    public class Wait : BaseState<Hunter>
    {
        public Wait(Hunter obj) : base(obj)
        {
            
        }
        float waitTime;
        public override void EnterState()
        {
            obj.currentStateText.text = "Current State : Wait";
            waitTime = obj.waitTime;
        }

        public override void ExitState()
        {
        }

        public override void UpdateState()
        {
            waitTime -= Time.deltaTime * 0.5f;
            if( waitTime < 0)
            {
                obj.SwitchState(obj.idleState);
            }
            if (obj.currentTarget != null && Vector3.Distance(obj.currentTarget.position, obj.transform.position) < obj.goToTrappedBirdDistance && GameEventManager.instance.hunterEvents.CheckTargetTrapped())
            {
                obj.SwitchState(obj.moveToTrappedBirdState);
                return;
            }
            if (obj.isTargetVisible)
            {
                if (obj.isTargetVisible && Vector3.Distance(obj.currentTarget.position, obj.transform.position) < obj.shootingDistance)
                {
                    obj.SwitchState(obj.aimState);
                    return;
                }
                obj.SwitchState(obj.chaseState);
            }
        }
    }
    public class Aim : BaseState<Hunter>
    {
        float aimingTime = 1.5f;
        float currentAimingTime;
        public Aim(Hunter obj) : base(obj)
        {
        }

        public override void EnterState()
        {
            obj.currentStateText.text = "Current State : Aim";
            obj.hunterDetectionManager.UpdateDetectionInvoke(0.05f);
            obj.agent.ResetPath();
            obj.AimStart(true);
            obj.animator.SetBool("isAiming", true);
            obj.hunterDetectionManager.SwitchDetectionMode(HunterDetectionMode.Chasing, 50f);
            currentAimingTime = aimingTime;
            obj.agent.isStopped = true;
        }

        public override void ExitState()
        {
        }

        public override void UpdateState()
        {
            currentAimingTime -= Time.deltaTime;
            Vector3 directionToTarget = (obj.lastKnownTargetPosition - obj.firePoint.position).normalized;
            obj.transform.rotation = Quaternion.Slerp(obj.transform.rotation, Quaternion.LookRotation(directionToTarget), Time.deltaTime * 3f);
            obj.aimTarget.position = Vector3.MoveTowards(obj.aimTarget.position, obj.lastKnownTargetPosition, Time.deltaTime * 10f);
            if (currentAimingTime < 0)
            {
                obj.animator.SetBool("isAiming", false);
                obj.SwitchState(obj.shootState);
            }
        }
    }
    public class Shoot : BaseState<Hunter>
    {
        public Shoot(Hunter obj) : base(obj)
        {
        }

        public override void EnterState()
        {
            obj.animator.SetTrigger("firing");
            obj.soundManager.PlaySFX("Shoot");
            ShootBullet();
        }

        public override void ExitState()
        {
        }

        public override void UpdateState()
        {
            if(obj.WaitForAnimation(obj.animator, "Firing Rifle"))
            {
                obj.SwitchState(obj.reloadState);
            }
        }
        void ShootBullet()
        {
            Vector3 directionToTarget = (obj.lastKnownTargetPosition - obj.firePoint.position).normalized;
            Vector3 finalDirection = (directionToTarget).normalized;
            GameObject bullet = GameObject.Instantiate(obj.bulletPrefab, obj.firePoint.position, Quaternion.LookRotation(finalDirection));
        }
    }
    public class Reload : BaseState<Hunter>
    {
        public Reload(Hunter obj) : base(obj)
        {
        }

        public override void EnterState()
        {

        }

        public override void ExitState()
        {
            obj.agent.isStopped = false;
            obj.hunterDetectionManager.SwitchDetectionMode(HunterDetectionMode.Chasing, 25f);
            obj.AimStart(false);
        }

        public override void UpdateState()
        {
            if (obj.WaitForAnimation(obj.animator, "Reloading"))
            {
                if (Vector3.Distance(obj.currentTarget.position, obj.transform.position) < obj.goToTrappedBirdDistance && GameEventManager.instance.hunterEvents.CheckTargetTrapped())
                {
                    obj.SwitchState(obj.moveToTrappedBirdState);
                    return;
                }
                float dist = Vector3.Distance(obj.currentTarget.position, obj.transform.position);
                bool checkIsNearby = dist < obj.viewDistance && dist > obj.shootingDistance;
                if (checkIsNearby)
                {
                    obj.SwitchState(obj.chaseState);
                    return;
                }
                if (!obj.isTargetVisible)
                {
                    obj.SwitchState(obj.idleState);
                    return ;
                }
                obj.SwitchState(obj.aimState);
            }
        }
    }
    public class MoveToTrappedBird : BaseState<Hunter>
    {
        public MoveToTrappedBird(Hunter obj) : base(obj)
        {
        }

        public override void EnterState()
        {
            obj.currentStateText.text = "Current State :Move to Trapped Bird";
            obj.agent.stoppingDistance = 0.2f;
        }

        public override void ExitState()
        {
            obj.agent.stoppingDistance = 0;
        }

        public override void UpdateState()
        {
            if (obj.currentTarget != null)
            {
                if (!GameEventManager.instance.hunterEvents.CheckTargetTrapped())
                {
                    if (obj.isTargetVisible)
                    {
                        if(Vector3.Distance(obj.currentTarget.position, obj.transform.position) < obj.shootingDistance)
                        {
                            obj.SwitchState(obj.aimState);
                            return;
                        }
                        obj.SwitchState(obj.chaseState);
                        return;
                    }
                    obj.SwitchState(obj.idleState);
                    return;
                }
                bool isNearTrappedBird = Vector3.Distance(obj.currentTarget.position, obj.transform.position) < obj.captureDistance;
                if (isNearTrappedBird)
                {
                    obj.SwitchState(obj.captureBirdState);
                    return;
                }
                Vector3 currentTargetPos = obj.currentTarget.transform.position;
                currentTargetPos.y = 0;
                obj.agent.SetDestination(currentTargetPos);
            }
        }
    }
    public class CaptureBird : BaseState<Hunter>
    {
        public CaptureBird(Hunter obj) : base(obj)
        {
        }

        public override void EnterState()
        {
            obj.currentStateText.text = "Current State :Move to Trapped Bird";
            if (!ScoreManager.instance.isGameEnd)
            {
                GameEventManager.instance.uIEvents.BirdCaptured(true);
            }
            ScoreManager.instance.EndGame(true);
        }

        public override void ExitState()
        {
        }

        public override void UpdateState()
        {
        }
    }
}
