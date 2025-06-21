using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEvent : MonoBehaviour
{
    public AudioSource footStepSource;
    public AudioSource metalSource;
    public void SetTrap()
    {
        GetComponentInParent<Hunter>().SetTrap();
    }
    public void PlayTrapSound()
    {
        metalSource.Play();
    }
    public void StopTrapSound()
    {
        metalSource.Stop();
    }
    public void PlaySoundFootStepRight()
    {
        footStepSource.pitch = 1f;
        footStepSource.Play();
    }    
    public void PlaySoundFootStepLeft()
    {
        footStepSource.pitch = 1.25f;
        footStepSource.Play();
    }
}
