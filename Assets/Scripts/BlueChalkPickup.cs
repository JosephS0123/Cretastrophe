using UnityEngine;

public class NewChalk : MonoBehaviour
{

     //public GameObject chalkUI; //UI Ref
     public AudioClip drawSound; //Sound effect
     public DrawManager drawManager; //DrawManager ref
     public GameObject chalkBar;

    // This function is called when another collider enters the trigger collider attached to this GameObject
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            AudioSource.PlayClipAtPoint(drawSound, transform.position);

            //if (chalkUI != null)
            //{
            //    chalkUI.SetActive(true);  // Shows the chalk UI
            //}

            if (drawManager != null)
            {
                drawManager.canDrawBlue = true; //Enables drawing
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