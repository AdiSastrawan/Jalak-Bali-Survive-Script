using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HunterEvents
{
    public event Action<Transform> OnSeeingTarget;
    public void SeeingTarget(Transform target) => OnSeeingTarget.Invoke(target);
    public event Func<Transform> OnCheckTarget;
    public Transform CheckTarget() => OnCheckTarget.Invoke();
    public event Action<bool,Transform> OnSetTargetTrap;
    public void SetTargetTrap(bool isTrapped,Transform target = null)=> OnSetTargetTrap?.Invoke(isTrapped,target);
    public event Func<bool> OnCheckTargetTrapped;
    public bool CheckTargetTrapped() => OnCheckTargetTrapped.Invoke();

    public event Action<Transform> OnTargetAssign;
    public void TargetAssign(Transform target) => OnTargetAssign?.Invoke(target);
}
