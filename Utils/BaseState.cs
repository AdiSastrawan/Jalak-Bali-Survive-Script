using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseState<T>
{
    public T obj;
    public BaseState(T obj)
    {
        this.obj = obj; 
    }

    public abstract void EnterState();
    public abstract void UpdateState();
    public abstract void ExitState();
}
