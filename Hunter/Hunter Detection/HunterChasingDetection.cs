using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class HunterChasingDetection : Detector
{
    public LayerMask chaseLayer;
    public override void ProcessDetection()
    {
        Vector3 direction = (hunter.currentTarget.position - transform.position).normalized;
        float facingAngle = Vector3.Angle(transform.forward, direction);
        if (Physics.Raycast(transform.position, direction, out RaycastHit info, viewDistance, chaseLayer) && (facingAngle < fieldOfView))
        {
            if(info.collider.CompareTag("Player") ) 
            { 
                hunter.isTargetVisible = true;
                hunter.lastKnownTargetPosition = hunter.currentTarget.position;
            }
            else
            {
                hunter.isTargetVisible = false;
            }
        }
        else
        {
           hunter.isTargetVisible = false;
        }

    }
}
