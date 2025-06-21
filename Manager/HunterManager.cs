using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.UI.Image;
using UnityEngine.AI;

public class HunterManager : MonoBehaviour
{
    [SerializeField] GameObject hunterPrefab;
    [SerializeField] Transform hunterTarget;
    [SerializeField] Transform[] spawnerPos;
    public bool isTargetTrapped;
    public int amountToSpawn = 5;
    public LayerMask groundLayer;
    void OnEnable()
    {
        GameEventManager.instance.hunterEvents.OnSeeingTarget += SetHunterTarget;
        GameEventManager.instance.hunterEvents.OnCheckTarget += GetHunterTarget;
        GameEventManager.instance.hunterEvents.OnSetTargetTrap += SetIsTargetTrapped;
        GameEventManager.instance.hunterEvents.OnCheckTargetTrapped += CheckIsTargetTrapped;
    }
    void OnDisable()
    {
        GameEventManager.instance.hunterEvents.OnSeeingTarget -= SetHunterTarget;
        GameEventManager.instance.hunterEvents.OnCheckTarget -= GetHunterTarget;
        GameEventManager.instance.hunterEvents.OnSetTargetTrap -= SetIsTargetTrapped;
        GameEventManager.instance.hunterEvents.OnCheckTargetTrapped -= CheckIsTargetTrapped;
    }
    private void Start()
    {
        for (int i = 0; i < amountToSpawn; i++)
        {
            Vector3 spawnOrigin = spawnerPos[Random.Range(0, spawnerPos.Length)].position;
            Vector3 spawnLocation = SpawnLocation(spawnOrigin);
            Debug.Log(spawnLocation);
            Instantiate(hunterPrefab, spawnLocation, Quaternion.identity);
        }
    }
    void SetHunterTarget(Transform target)
    {
        if (hunterTarget != target && target != null) hunterTarget = target;
    }
    Transform GetHunterTarget() => hunterTarget;
    void SetIsTargetTrapped(bool isTrapped, Transform target = null)
    {
        isTargetTrapped = isTrapped;
        if (isTrapped)
        {
            hunterTarget = target;
            GameEventManager.instance.hunterEvents.TargetAssign(hunterTarget);
        }
    }
    bool CheckIsTargetTrapped()
    {
        return isTargetTrapped;
    }

    Vector3 SpawnLocation(Vector3 originPos)
    {
        Vector3 randomSphere = (Random.insideUnitSphere) * 40;
        randomSphere += originPos;
        Vector3 targetPosition = randomSphere;
        targetPosition.y = 0;
        Collider[] collider = Physics.OverlapSphere(targetPosition, 1f);
        bool isPositionExist = true;
        foreach (Collider col in collider)
        {
            if (!col.CompareTag("Ground"))
            {
                Debug.Log(col.gameObject);
                isPositionExist = false;
                break;
            }
        }
        if (!isPositionExist || targetPosition.z > 100 || targetPosition.z < -100 || targetPosition.x > 100 || targetPosition.x < -100) targetPosition = SpawnLocation(originPos);
        return targetPosition;
    }
}
