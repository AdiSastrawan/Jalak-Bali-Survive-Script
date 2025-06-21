using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    public AudioSource musicSource;
    public AudioSource sfxSource;
    public SoundClip[] soundClips;
    bool musicPlaying = false;
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
        sfxSource.ignoreListenerPause = true;
        musicSource.ignoreListenerPause = true;
    }

    public void PlayBGM(string name,bool isLooping = true,float volume = 1f)
    {
        musicSource.clip = FindSoundClip(name);
        musicSource.loop = isLooping;
        musicSource.volume = volume;
        musicSource.Play();
    }
    public void StopBGM()
    {
        musicSource.Stop();
    }
    public void ChangeMusicVolume(float volume=1f)
    {
        musicSource.volume = volume;
    }
    public void PlaySFX(string name,bool isLooping = false,float volume = 1f)
    {
        sfxSource.loop = isLooping;
        sfxSource.volume = volume;
        sfxSource.PlayOneShot(FindSoundClip(name));
    }
    public void PlayButtonSFX()
    {
        sfxSource.clip = FindSoundClip("Button");
        sfxSource.Play();
    }
    private AudioClip FindSoundClip(string name)
    {
        AudioClip audio = null;
        foreach (var clip in soundClips)
        {
            if (clip.audioName == name)audio = clip.clip;
        }
        return audio;
    }
    public void ToggleMusic()
    {
        musicPlaying = !musicPlaying;
        if (musicPlaying)
        {
            PlayBGM("BGM", true, 0.5f);
        }
        else
        {
            StopBGM();
        }
    }
    public bool IsPlayingMusic() => musicPlaying;
}
