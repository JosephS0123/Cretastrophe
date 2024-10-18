using UnityEngine;

public class collect_key : MonoBehaviour
{

     public AudioClip drawSound; //Sound effect

    // This function is called when another collider enters the trigger collider attached to this GameObject
    private void OnTriggerEnter2D(Collider2D collision)
    {
        AudioSource.PlayClipAtPoint(drawSound, transform.position);

        // Destroy the item after it's collision triggered
        Destroy(gameObject);

        //Add to key counter
        GameManager.addKey();

        
    }
}