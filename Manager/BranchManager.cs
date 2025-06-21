using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class BranchManager : MonoBehaviour
{
    public int maxBranchOnMap = 5;
    public int currentBranchOnMap ;

    public float spawnRadius;

    public GameObject branchPrefab;
    public LayerMask obstacleLayer;
    void Start()
    {
        for (int i = 0; i < maxBranchOnMap; i++)
        {
            SpawnBranch();
        }
    }
    void OnEnable()
    {
        GameEventManager.instance.branchEvents.OnBranchPickUp += BranchPickUp;
    }
    void OnDisable()
    {
        GameEventManager.instance.branchEvents.OnBranchPickUp -= BranchPickUp;
    }

    void SpawnBranch()
    {
        Instantiate(branchPrefab,RandomPosition(),Quaternion.identity);
        AddCurrentBranch();
    }
    Vector3 RandomPosition()
    {
        Vector3 spawnPos = new Vector3();
        spawnPos = transform.position + Random.insideUnitSphere * spawnRadius;
        if(Physics.CheckSphere(spawnPos, 1f, obstacleLayer))
        {
            spawnPos = RandomPosition();
        }
        spawnPos.y = 0.1f;
        return spawnPos;
    }
    void AddCurrentBranch()
    {
        currentBranchOnMap++;
    }
    void DecreaseCurrentBranch()
    {
        currentBranchOnMap--;
    }
    void BranchPickUp()
    {
        DecreaseCurrentBranch();
        if(currentBranchOnMap < maxBranchOnMap)
        {
            SpawnBranch();
        }
    }
    void OnDrawGizmosSelected()
    {
        Gizmos.DrawSphere(transform.position, spawnRadius);    
    }
}
