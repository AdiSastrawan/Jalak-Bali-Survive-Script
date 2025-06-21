using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HunterSearching : Detector
{

    public override void ProcessDetection()
    {
        Collider[] targets = Physics.OverlapSphere(transform.position, viewDistance, targetLayer);
        Transform prevTarget = null;
        float shortestDistance = Mathf.Infinity;
        foreach (Collider target in targets)
        {
            Vector3 targetDirection = (target.transform.position - transform.position).normalized;
            float facingAngle = Vector3.Angle(transform.forward, targetDirection);
            float distanceBetween = Vector3.Distance(transform.position, target.transform.position);
            if (facingAngle < fieldOfView / 2)
            {
                if (Physics.Raycast(transform.position, targetDirection, distanceBetween, obstacleLayer)) continue;
                if (shortestDistance > distanceBetween)
                {
                    shortestDistance = distanceBetween;
                    prevTarget = target.transform.parent;
                }
            }
        }
        if(prevTarget != null) hunter.currentTarget = prevTarget;
    }
}
