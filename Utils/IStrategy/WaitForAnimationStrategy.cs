using AdiSastrawan.Node;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitForAnimationStrategy : IStrategy
{
    private Animator animator;
    private string animationName;
    private bool started = false;
    Action triggerAnimation = null;
    Action onRunningAction = null;
    Action OnCompleteAnimation = null;

    public WaitForAnimationStrategy(Animator animator,string animationName,Action triggerAnimation,Action onRunningAction,Action completeAnimation)
    {
        this.animator = animator;
        this.animationName = animationName;
        this.triggerAnimation = triggerAnimation;
        this.onRunningAction = onRunningAction;
        OnCompleteAnimation = completeAnimation;
    }    
    public WaitForAnimationStrategy(Animator animator,string animationName,Action triggerAnimation)
    {
        this.animator = animator;
        this.animationName = animationName;
        this.triggerAnimation = triggerAnimation;
    }   
    public WaitForAnimationStrategy(Animator animator,string animationName,Action triggerAnimation,Action onCompleteAnimation)
    {
        this.animator = animator;
        this.animationName = animationName;
        this.triggerAnimation = triggerAnimation;
        this.OnCompleteAnimation = onCompleteAnimation;
    }
    public Status Process()
    {
        if (!started)
        {
            triggerAnimation?.Invoke();
            started = true;
        }
        onRunningAction?.Invoke();
        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
        if (info.IsName(animationName) && animator.IsInTransition(0) && info.normalizedTime >= 0.9f)
        {
            OnCompleteAnimation?.Invoke();
            return Status.Success;
        }

        return Status.Running;
    }

    public void Reset()
    {
        started = false;
    }
}
