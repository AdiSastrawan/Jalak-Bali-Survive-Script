using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HunterSoundManager : MonoBehaviour
{
    public SoundClip[] soundClips;
    public AudioSource hunterAudioSource;
    public void PlaySFX(string name)
    {
        hunterAudioSource.clip = FindSoundClip(name);
        hunterAudioSource.Play();
    }
    private AudioClip FindSoundClip(string name)
    {
        AudioClip audio = null;
        foreach (var clip in soundClips)
        {
            if (clip.audioName == name) audio = clip.clip;
        }
        return audio;
    }
}
