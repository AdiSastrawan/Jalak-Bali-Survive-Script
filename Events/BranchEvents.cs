using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BranchEvents 
{
    public event Action OnBranchPickUp;
    public void BranchPickUp()=>OnBranchPickUp?.Invoke();
}
