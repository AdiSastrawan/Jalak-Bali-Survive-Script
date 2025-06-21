using AdiSastrawan.Node;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunUntilSuccessStrategy : IStrategy
{
    Func<bool> condition;
    public RunUntilSuccessStrategy(Func<bool> condition)
    {
        this.condition = condition;
    }
    public Status Process()
    {
        return condition.Invoke() ? Status.Success : Status.Running;
    }

    public void Reset()
    {
    }
}
