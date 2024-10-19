using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class door_Lock : MonoBehaviour
{

    private Collider2D doorCollider;  // Ref to the door's collider
    public AudioClip soundEffect; //Sound effect

    // Start is called before the first frame update
    void Start()
    {
        doorCollider = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Check if the player has at least one key
            if (GameManager.keyCount >= 1)
            {
                // Unlock the door 
                doorCollider.enabled = false;
                this.gameObject.SetActive(false);
                AudioSource.PlayClipAtPoint(soundEffect, transform.position);

                //Remove key
                GameManager.removeKey();

                Debug.Log("Door unlocked!");
            }
            else
            {
                Debug.Log("You need a key to unlock this door.");
            }
        }
    }
}

