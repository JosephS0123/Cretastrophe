using UnityEngine;

public class PickUpBaseChalk : MonoBehaviour
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
            chalkSelect.SetSelectedChalkIndex(0);

            if (chalkUI != null)
            {
                chalkUI.SetActive(true);  // Shows the chalk UI
            }

            if (drawManager != null)
            {
                drawManagerObj.SetActive(true); //Enables drawing
                drawManager.canDrawWhite = true; //Enables drawing
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