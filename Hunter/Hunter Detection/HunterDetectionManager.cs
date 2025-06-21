using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HunterDetectionManager : MonoBehaviour
{
    public float detectionInterval=0.125f;
    public float currentDetectionInterval;
    public GameObject detectionContainer;
    public HunterDetectionMode currentDetectionMode;
    Dictionary<HunterDetectionMode, Detector> detectors;
    void Start()
    {
        Dictionary<HunterDetectionMode, Detector> tempDict = new Dictionary<HunterDetectionMode, Detector>();
        foreach (Transform child in detectionContainer.transform)
        {
            if (child.TryGetComponent(out Detector detector))
            {
                tempDict.Add(detector.detectionMode, detector);
            }
        }
        detectors = tempDict;
        currentDetectionMode = HunterDetectionMode.Searching;
        currentDetectionInterval = detectionInterval;
        StartCoroutine(DetectionRoutine());
    }
    public void SwitchDetectionMode(HunterDetectionMode mode)
    {
       currentDetectionMode = mode;
    }    
    public void SwitchDetectionMode(HunterDetectionMode mode,float viewDistance = 15f)
    {
       currentDetectionMode = mode;
        detectors[currentDetectionMode].ChangeViewDistance(viewDistance);
    }
    IEnumerator DetectionRoutine()
    {
        while (true)
        {
            Detect();
            yield return new WaitForSeconds(currentDetectionInterval);
        }
    }

    public void UpdateDetectionInvoke()
    {
        currentDetectionInterval = detectionInterval;
        StopCoroutine(DetectionRoutine()); // Stop old coroutine
        StartCoroutine(DetectionRoutine());
    }

    public void UpdateDetectionInvoke(float currentInterval)
    {
        currentDetectionInterval = currentInterval;
        StopCoroutine(DetectionRoutine()); // Stop old coroutine
        StartCoroutine(DetectionRoutine());
    }    
    private void Detect()
    {
        if (detectors.ContainsKey(currentDetectionMode))
        {
            detectors[currentDetectionMode].ProcessDetection();
        }
    }
    private void OnDrawGizmos()
    {
        // Draw the vision radius
        if (detectors == null) return;
        if (!detectors.ContainsKey(currentDetectionMode)) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectors[currentDetectionMode].viewDistance);

        // Draw the FOV cone
        Vector3 leftBoundary = DirectionFromAngle(-detectors[currentDetectionMode].fieldOfView / 2);
        Vector3 rightBoundary = DirectionFromAngle(detectors[currentDetectionMode].fieldOfView / 2);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary * detectors[currentDetectionMode].viewDistance);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary * detectors[currentDetectionMode].viewDistance);

        // Draw the target (if detected)
        if (detectors[currentDetectionMode].hunter.currentTarget != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, detectors[currentDetectionMode].hunter.currentTarget.position);
        }
    }

    private Vector3 DirectionFromAngle(float angleInDegrees)
    {
        float radian = (angleInDegrees + transform.eulerAngles.y) * Mathf.Deg2Rad;
        return new Vector3(Mathf.Sin(radian), 0, Mathf.Cos(radian));
    }
}

public enum HunterDetectionMode
{
    Searching,
    Chasing
}