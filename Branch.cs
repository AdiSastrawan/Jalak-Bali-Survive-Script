using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Branch : MonoBehaviour
{
    Outline outline;
    void Awake()
    {
        outline = GetComponentInChildren<Outline>();
        outline.enabled = false;
    }
    public void TriggerEnter(Collider other)
    {
        Debug.Log(other.tag);
        if (other.CompareTag("Player"))
        {
            outline.enabled = true;
        }
    }    
    public void TriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            outline.enabled = false;
        }
    }
    public void PickMeUp()
    {
        GameEventManager.instance.branchEvents.BranchPickUp();
        Destroy(gameObject);
    }
}
