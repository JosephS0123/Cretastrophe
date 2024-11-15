using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsSpikeBall : MonoBehaviour
{
    public SpikeBallSpawner spikeBallSpawner = null;
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "BallDespawn")
        {
            spikeBallSpawner.respawnBall();
            Destroy(gameObject);
        }
    }
}
