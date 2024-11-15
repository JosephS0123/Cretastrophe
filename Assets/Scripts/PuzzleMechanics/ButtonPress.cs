using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ButtonPress : MonoBehaviour
{
    public GameObject door;
    public AudioClip drawSound; //Sound effect
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.CompareTag("PhysicsObj"))
        {
            Destroy(door);
            Destroy(gameObject);
            AudioSource.PlayClipAtPoint(drawSound, transform.position);

        }
    }
}
