using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class collectFlag : MonoBehaviour
{

    public AudioClip drawSound; //Sound effect
    public GameObject completeLevel;
    public GameObject chalkUI;
    public GameObject barUI;
    public LevelComplete levelCompleteUI;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    // This function is called when another collider enters the trigger collider attached to this GameObject
    private void OnTriggerEnter2D(Collider2D collision)
    {
         if (collision.CompareTag("Player"))
         {
            AudioSource.PlayClipAtPoint(drawSound, transform.position);

            // Destroy the item after it's collision triggered
            Destroy(gameObject);
            Debug.Log("Total Diamonds Count ingame: " + GameManager.maxDiamondCount);
            if (levelCompleteUI != null)
            {
                levelCompleteUI.ShowLevelCompleteScreen();  // Update gems collected display
            }
             if (completeLevel != null)
                {
                    completeLevel.SetActive(true);  // Shows the UI
                }
            chalkUI.SetActive(false);
            barUI.SetActive(false);

        }
        
    }
}
