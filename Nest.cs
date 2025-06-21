using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nest : MonoBehaviour
{
    Outline outline;
    Player player;
    int currentAmount = 0;
    void Start()
    {
        outline = GetComponentInChildren<Outline>(); 
    }
    void Update()
    {
        if (player != null)
        {
            if (Input.GetMouseButtonDown(0))
            {
                StoreBranch(player.ClearBranchInHand());
            }
        }
    }
    public void StoreBranch(int amount)
    {
        AudioManager.instance.PlaySFX("Nest");
        currentAmount += amount;
        ScoreManager.instance.UpdateScore(currentAmount);
    }
    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            player = other.GetComponentInParent<Player>();
            if(player.amountBranchInHand > 0)
            outline.OutlineColor = Color.green;
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (player != null)
        {
            if (player.amountBranchInHand < 1)
                outline.OutlineColor = Color.white;
        }
    }
    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            player = null;
            outline.OutlineColor = Color.white;
        }
    }
}
