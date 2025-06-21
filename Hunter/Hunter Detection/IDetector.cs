using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Detector : MonoBehaviour
{
    public HunterDetectionMode detectionMode;
    public Hunter hunter;
    public float viewDistance;
    public float fieldOfView;
    public LayerMask obstacleLayer;
    public LayerMask targetLayer;
    private void OnEnable()
    {
        hunter = GetComponentInParent<Hunter>();
        viewDistance = hunter.viewDistance;
        fieldOfView = hunter.fieldOfView;
    }
    public abstract void ProcessDetection();
    public void ChangeViewDistance(float distance)
    {
        viewDistance = distance;
    }
}
