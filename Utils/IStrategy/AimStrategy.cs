using AdiSastrawan.Node;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitingTimeStrategy : IStrategy
{
    float time;
    float currentTime;
    bool toggledStartingAction;
    Action action = null;
    Action actionFinish = null;
    Action startingAction = null;
    public WaitingTimeStrategy(float time) 
    {
        this.time = time;
        currentTime = time;
    }
    public WaitingTimeStrategy(float time,Action action) 
    {
        this.time = time;
        currentTime = time;
        this.action = action;
    }     
    public WaitingTimeStrategy(float time,Action action,Action startingAction) 
    {
        this.time = time;
        currentTime = time;
        this.action = action;
        this.startingAction = startingAction;
    }
    public WaitingTimeStrategy(float time, Action action, Action startingAction,Action actionFinish) 
    {
        this.time = time;
        currentTime = time;
        this.action = action;
        this.startingAction = startingAction;
        this.actionFinish = actionFinish;
    }
    public Status Process()
    {
        if (!toggledStartingAction)
        {
            startingAction?.Invoke();
            toggledStartingAction = true;
        }
        action?.Invoke();
        currentTime -= Time.deltaTime; 
        if(currentTime < 0)
        {
            actionFinish?.Invoke(); 
            currentTime = time;
            return Status.Success;
        }
        return Status.Running;
    }

    public void Reset()
    {
        toggledStartingAction =false;
    }
}
