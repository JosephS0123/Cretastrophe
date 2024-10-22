using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class draw_MSG : MonoBehaviour
{
     //public AudioClip drawSound; //Sound effect
     public GameObject MessageUI;

     private void OnTriggerEnter2D(Collider2D collision)
     {
         if (collision.CompareTag("Player"))
         {
            //AudioSource.PlayClipAtPoint(drawSound, transform.position);

            if(MessageUI != null)
            {
                MessageUI.SetActive(true);  // Shows the UI
            }
         }
     }

     public void Close()
     {
         MessageUI.SetActive(false);  // Closes the UI
     }
    
}
