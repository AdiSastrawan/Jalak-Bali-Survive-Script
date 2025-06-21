using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUIManager : MonoBehaviour
{
    //TMP
    public Image musicImage;
    public Sprite musicOnSprite;
    public Sprite musicOffSprite;

    void Start()
    {
        UpdateMusicUI(AudioManager.instance.IsPlayingMusic());
    }

    public void ToggleMusicUI()
    {
        PlayButtonSfx();
        AudioManager.instance.ToggleMusic();
        UpdateMusicUI(AudioManager.instance.IsPlayingMusic());
    }
    void UpdateMusicUI(bool isPlayingMusic)
    {
        musicImage.sprite = isPlayingMusic ? musicOnSprite : musicOffSprite;
    }
    public void StartButton()
    {
        PlayButtonSfx();
        GameManager.instance.MoveToScene("GameplayScene");
    }    
    public void QuitButton()
    {
        PlayButtonSfx();
        GameManager.instance.ExitGame();
    }
    public void PlayButtonSfx()
    {
        AudioManager.instance.PlayButtonSFX();
    }
}
