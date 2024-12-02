using UnityEngine;

public class RedChalkPickUp : MonoBehaviour
{

     public GameObject chalkUI; //UI Ref
     public AudioClip drawSound; //Sound effect
     public DrawManager drawManager; //DrawManager ref
     public GameObject drawManagerObj; //DrawManager ref
     public GameObject chalkBar;
     public chalkSelector_NEW chalkSelect; //ChalkSelector Script ref

    // This function is called when another collider enters the trigger collider attached to this GameObject
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            AudioSource.PlayClipAtPoint(drawSound, transform.position);
            //New line here:
            chalkSelect.SetSelectedChalkIndex(1);

            if (chalkUI != null)
            {
                drawManagerObj.SetActive(true); //Enables drawing
                chalkUI.SetActive(true);  // Shows the chalk UI
                drawManager.hasRed = true; //chalk ui test
            }

            if (drawManager != null)
            {
                //drawManager.SetActive(true); //Enables drawing
                drawManager.canDrawRed = true; //Enables drawing
            }

            if (chalkBar != null)
            {
                chalkBar.SetActive(true);
            }
            // Destroy the item after it's collision triggered
            Destroy(gameObject);
        }
    }
}