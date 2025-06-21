using AdiSastrawan.Node;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionStrategy : IStrategy
{
    private Action action;
    public ActionStrategy(Action action)
    {
        this.action = action; 
    }
    public Status Process()
    {
        action.Invoke();
        return Status.Success;
    }

    public void Reset()
    {
        
    }
}
