using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{

    [SerializeField] GameObject pauseMenu;
    public GameObject drawManager;

    public void Pause()
    {
        pauseMenu.SetActive(true);
        Time.timeScale = 0; // Pause the game
        drawManager.SetActive(false); //Disables drawing
    }

    public void Home()
    {
        SceneManager.LoadScene("Main Menu");
        Time.timeScale = 1; //Unpause game 
    }

    public void Play()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1; //Unpause game
        drawManager.SetActive(true); //Enables drawing
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Time.timeScale = 1; //Unpause game
    }

}
