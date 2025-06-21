using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainUIManager : MonoBehaviour
{
    public GameObject capturedUI;
    public GameObject completedUI;
    public GameObject objectiveUI;
    public Image staminaGauge;
    public TMP_Text currrentBranchAmount;
    public TMP_Text currrentBranchInHand;
    public TMP_Text currentTime;
    public TMP_Text currentTimeLose;
    public TMP_Text currentTimeWin;
    public GameObject tutorialFly;
    public GameObject tutorialLand;
    void OnEnable()
    {
        GameEventManager.instance.uIEvents.OnBirdCaptured += ShowCapturedUI;
        GameEventManager.instance.uIEvents.OnUpdateStaminaGauge += UpdateStaminaAmount;
        GameEventManager.instance.uIEvents.OnUpdateCurrentBranch += UpdateBranchAmount;
        GameEventManager.instance.uIEvents.OnNestCompleted += ShowCompletedUI;
        GameEventManager.instance.uIEvents.OnUpdateBranchInHand += UpdateCurrentBranchInHand;
        GameEventManager.instance.uIEvents.OnTimeUpdate+= UpdateCurrentTime;
        GameEventManager.instance.uIEvents.OnBirdFly += TutorialText;
    }
    void OnDisable()
    {
        GameEventManager.instance.uIEvents.OnBirdCaptured -= ShowCapturedUI;
        GameEventManager.instance.uIEvents.OnUpdateStaminaGauge -= UpdateStaminaAmount;
        GameEventManager.instance.uIEvents.OnUpdateCurrentBranch -= UpdateBranchAmount;
        GameEventManager.instance.uIEvents.OnNestCompleted -= ShowCompletedUI;
        GameEventManager.instance.uIEvents.OnUpdateBranchInHand -= UpdateCurrentBranchInHand;
        GameEventManager.instance.uIEvents.OnTimeUpdate -= UpdateCurrentTime;
        GameEventManager.instance.uIEvents.OnBirdFly -= TutorialText;
    }
    void Awake()
    {
        Time.timeScale = 0f;    
    }
   
    void ShowCapturedUI(bool activation)
    {
        AudioManager.instance.PlaySFX("Failed");
        capturedUI.SetActive(activation);
    }
    void UpdateStaminaAmount(float amount)
    {
        staminaGauge.fillAmount = Mathf.Clamp01(amount);
    }

    void ShowCompletedUI(bool activation)
    {
        completedUI.SetActive(activation);
    }
    void UpdateBranchAmount(string text)
    {
        currrentBranchAmount.text = text;
    }
    void UpdateCurrentBranchInHand(int amount)
    {
        currrentBranchInHand.text = amount.ToString();
    }
    void UpdateCurrentTime(string text)
    {
        currentTime.text = text;    
        currentTimeLose.text = text;    
        currentTimeWin.text = text;    
    }
    public void CloseObjectivePanel()
    {
        objectiveUI.SetActive(false);
        AudioManager.instance.PlayButtonSFX();
        Time.timeScale = 1f;
    }
    public void Retry()
    {
        AudioManager.instance.PlayButtonSFX();
        GameManager.instance.MoveToScene("GameplayScene");
    }
    public void ToMainMenu()
    { 
        AudioManager.instance.PlayButtonSFX();
        GameManager.instance.MoveToScene("MainMenuScene");
    }
    public void TutorialText(bool isFlying)
    {
        tutorialFly.SetActive(isFlying);
        tutorialLand.SetActive(!isFlying);
    }
}
