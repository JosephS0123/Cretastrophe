using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Completion_Menu : MonoBehaviour
{

    public GameObject drawManager;


     public void Home()
    {
        SceneManager.LoadScene("Main Menu");
        Time.timeScale = 1; //Unpause game 
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Time.timeScale = 1; //Unpause game
    }

    public void Next()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
