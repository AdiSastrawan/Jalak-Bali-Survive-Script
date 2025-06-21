using AdiSastrawan.Node;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IStrategy 
{
    public Status Process();
    public void Reset();
}
