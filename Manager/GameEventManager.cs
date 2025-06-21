using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEventManager : MonoBehaviour
{
    public static GameEventManager instance;

    public HunterEvents hunterEvents;
    public UIEvents uIEvents;
    public BranchEvents branchEvents;
    void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(instance);
        }        
        hunterEvents = new HunterEvents();  
        uIEvents = new UIEvents();
        branchEvents = new BranchEvents();
    }
}
