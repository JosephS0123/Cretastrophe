using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{

    private Player playerScript;
    public AudioClip checkpointSound; //Sound effect
    Collider2D coll;

    // Start is called before the first frame update
    void Start()
    {
        coll = GetComponent<Collider2D>();
        playerScript = FindObjectOfType<Player>();   
    }

    // Update is called once per frame
    void Update()
    {
        
    }

     private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            AudioSource.PlayClipAtPoint(checkpointSound, transform.position);
            playerScript.updateCheckpoint(transform.position);
            coll.enabled = false;
            //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
