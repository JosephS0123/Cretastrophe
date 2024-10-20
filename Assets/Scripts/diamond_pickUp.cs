using UnityEngine;

public class diamond_pickUp : MonoBehaviour
{

    void Start()
    {
        GameManager.totalDiamonds();
    }

     public AudioClip drawSound; //Sound effect

    // This function is called when another collider enters the trigger collider attached to this GameObject
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            AudioSource.PlayClipAtPoint(drawSound, transform.position);

            // Destroy the item after it's collision triggered
            Destroy(gameObject);

            //Add to diamond counter
            GameManager.AddDiamond();

        }
    }
}