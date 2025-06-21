using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    public GameObject trappedNotify;
    public Image trappedGauge;
    [Range(0f, 1f)] 
    public float decreaseSpeedMult = 0.1f;
    private void OnEnable()
    {
        GameEventManager.instance.uIEvents.OnTrappedNotifChange += SetTrappedNotify;
        GameEventManager.instance.uIEvents.OnTrappedGaugeChange += SetTrappedGauge;
    }
    private void OnDisable()
    {
        GameEventManager.instance.uIEvents.OnTrappedNotifChange -= SetTrappedNotify;
        GameEventManager.instance.uIEvents.OnTrappedGaugeChange -= SetTrappedGauge;
    }

    private void Update()
    {
        if (trappedNotify.activeInHierarchy)
        {
            trappedGauge.fillAmount = Mathf.Clamp01(trappedGauge.fillAmount -Time.deltaTime * decreaseSpeedMult);
            if (trappedGauge.fillAmount > 0.95) 
            {
                trappedGauge.fillAmount = 0;
                GameEventManager.instance.uIEvents.TrappedGaugeSuccess(); 
            }
        }
    }
    void SetTrappedGauge(float value)
    {
        trappedGauge.fillAmount += 1/value;
    }
    void SetTrappedNotify(bool isTrapped)
    {
        trappedNotify.SetActive(isTrapped);
    }

}
