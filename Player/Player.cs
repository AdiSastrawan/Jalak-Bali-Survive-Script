using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private Rigidbody rb;
    public Animator animator;
    public bool flying;
    public bool grounded;
    public int maxAmountBranchInHand = 5;
    public int amountBranchInHand = 0;
    public float pickUpRadius=1f;
    public LayerMask collectableLayer;
    public float gaugeFillerAmount = 10f;
    public bool isTrapped { get; private set; }
    private void OnEnable()
    {
        GameEventManager.instance.uIEvents.OnTrappedGaugeSuccess += QTESuccess;
        GameEventManager.instance.uIEvents.OnGaugeFillAmountUpdate += AddGaugeFillAmount;
    }
    private void OnDisable()
    {
        GameEventManager.instance.uIEvents.OnTrappedGaugeSuccess -= QTESuccess;
        GameEventManager.instance.uIEvents.OnGaugeFillAmountUpdate -= AddGaugeFillAmount;
    }
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    private void Update()
    {
        if (ScoreManager.instance.isGameEnd) return;
        if (isTrapped)
        {
            if (Input.GetMouseButtonDown(0))
            {
                GameEventManager.instance.uIEvents.TrappedGaugeChange(gaugeFillerAmount);
            }
        }
        else if (Input.GetMouseButtonDown(0))
        {
            Collider[] colliders =Physics.OverlapSphere(transform.position, pickUpRadius,collectableLayer);
            foreach (Collider collider in colliders)
            {
                if (collider.CompareTag("Branch"))
                {
                    if(amountBranchInHand < maxAmountBranchInHand)
                    {
                       collider.GetComponentInParent<Branch>().PickMeUp();
                       UpdateAmountBranchInHand();
                    }
                    break;
                }
            }
        }
           
        
    }
    void QTESuccess()
    {
        SetIsTrapped(false);
    }
    public void SetIsTrapped(bool isTrapped)
    {
        this.isTrapped = isTrapped;
        flying = false;
        rb.velocity = Vector3.zero;
        if (isTrapped) AudioManager.instance.PlaySFX("Hit");
        GameEventManager.instance.hunterEvents.SetTargetTrap(isTrapped,transform);
        GameEventManager.instance.uIEvents.TrappedNotifChange(isTrapped);
    }
    void UpdateAmountBranchInHand()
    {
        AudioManager.instance.PlaySFX("Pick Up Branch");
        amountBranchInHand++;
        GameEventManager.instance.uIEvents.UpdateBranchInHand(amountBranchInHand);
    }
    public int ClearBranchInHand()
    {
        int currentBranch = amountBranchInHand;
        amountBranchInHand = 0;
        GameEventManager.instance.uIEvents.UpdateBranchInHand(amountBranchInHand);
        return currentBranch;
    }
    void AddGaugeFillAmount(float amount)
    {
        gaugeFillerAmount += amount;
    }
}
