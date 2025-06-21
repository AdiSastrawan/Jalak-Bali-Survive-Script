using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TriggerChild : MonoBehaviour
{
    public Branch parent;
    void OnTriggerEnter(Collider other)
    {
        parent.TriggerEnter(other);
    }
    void OnTriggerExit(Collider other)
    {
        parent.TriggerExit(other);
    }
}
