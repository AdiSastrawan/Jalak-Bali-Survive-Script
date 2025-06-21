using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject); // Destroy the duplicate
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
        
    }

    public void MoveToScene(string scene)
    {
        SceneManager.LoadScene(scene);
    }

    public void ExitGame()
    {
        Application.Quit();
    }

}
