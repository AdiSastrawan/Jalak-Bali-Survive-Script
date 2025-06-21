using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIEvents 
{
    public event Action<bool> OnTrappedNotifChange;
    public void TrappedNotifChange(bool isTrapped) => OnTrappedNotifChange?.Invoke(isTrapped);    
    public event Action<float> OnTrappedGaugeChange;
    public void TrappedGaugeChange(float gauge) => OnTrappedGaugeChange?.Invoke(gauge);

    public event Action OnTrappedGaugeSuccess;
    public void TrappedGaugeSuccess()=> OnTrappedGaugeSuccess?.Invoke();

    public event Action<bool> OnBirdCaptured;
    public void BirdCaptured(bool birdCaptured)=>OnBirdCaptured?.Invoke(birdCaptured);
    public event Action<float> OnUpdateStaminaGauge;
    public void UpdateStaminaGauge(float amount) => OnUpdateStaminaGauge.Invoke(amount);
    public event Action<string> OnUpdateCurrentBranch;
    public void UpdateCurrentBranch(string text) => OnUpdateCurrentBranch.Invoke(text);

    public event Action<bool> OnNestCompleted;
    public void NestCompleted(bool nestCompleted) => OnNestCompleted?.Invoke(nestCompleted);

    public event Action<int> OnUpdateBranchInHand;
    public void UpdateBranchInHand(int branchInHand) => OnUpdateBranchInHand?.Invoke(branchInHand);    
    public event Action<string> OnTimeUpdate;
    public void TimeUpdate(string currentTime) => OnTimeUpdate?.Invoke(currentTime);
    public event Action<float> OnGaugeFillAmountUpdate;
    public void GaugeFillAmountUpdate(float amount) => OnGaugeFillAmountUpdate?.Invoke(amount);

    public event Action<bool> OnBirdFly;
    public void BirdFly(bool birdFly) => OnBirdFly?.Invoke(birdFly);
}
