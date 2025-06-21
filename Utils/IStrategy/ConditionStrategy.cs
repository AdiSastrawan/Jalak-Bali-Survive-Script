using AdiSastrawan.Node;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionStrategy : IStrategy
{
    Func<bool> condition;
    public ConditionStrategy(Func<bool> condition)
    {
        this.condition = condition;
    }
    public Status Process()
    {
        return condition.Invoke() ? Status.Success : Status.Failed;
    }

    public void Reset()
    {
        
    }

}
