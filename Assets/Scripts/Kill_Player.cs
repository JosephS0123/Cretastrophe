using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Kill_Player : MonoBehaviour
{
    
    private Player playerScript;
    public Transform respawnPoint;

    // Start is called before the first frame update
    void Start()
    {
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
            playerScript.Die();
            //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
