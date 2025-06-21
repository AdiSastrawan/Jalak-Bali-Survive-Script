using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance;
    public int maxBranch = 10;

    public int minutes=0;
    public int seconds=0;

    public bool isGameEnd;
    void Awake()
    {
        if(instance == null)
        instance = this;
        else Destroy(instance);
    }
    void Start()
    {
        GameEventManager.instance.uIEvents.UpdateCurrentBranch($"{0} / {maxBranch}");
        StartCoroutine(nameof(TickSecond));    
    }

    public void UpdateScore(int current)
    {
        if(current >= maxBranch)
        {
            GameEventManager.instance.uIEvents.NestCompleted(true);
            if (!isGameEnd)
            {
                AudioManager.instance.PlaySFX("Success", false, 0.75f);
            }
            EndGame(true);
            return;
        }
        GameEventManager.instance.uIEvents.UpdateCurrentBranch($"{current} / {maxBranch}");
    }
    public void EndGame(bool activation)
    {
        Time.timeScale = 0;
        isGameEnd = activation;
    }
    IEnumerator TickSecond()
    {
        while (!isGameEnd)
        {
            yield return new WaitForSeconds(1);
            seconds++;
            minutes= seconds/60;
            GameEventManager.instance.uIEvents.TimeUpdate(formatTime());
            GameEventManager.instance.uIEvents.GaugeFillAmountUpdate(0.085f);
        }
    }

    string formatTime()
    {
        return $"{(minutes < 10 ? "0" + minutes.ToString() : minutes)} : {(seconds % 60 < 10 ? "0" + seconds % 60 : seconds % 60)}";
    }
}
