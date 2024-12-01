using System.Collections;
using System.Collections.Generic;
using CartoonFX;
using UnityEngine;

public class PhysicsSpikeBall : MonoBehaviour
{
    public SpikeBallSpawner spikeBallSpawner = null;
    public bool canBeErased;
    public GameObject effect;
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "BallDespawn" || (collision.gameObject.tag == "Eraser" && canBeErased))
        {
            spikeBallSpawner.respawnBall();
            Destroy(gameObject);
            if(collision.gameObject.tag == "Eraser")
            {
                GameObject _effect = Instantiate(effect, transform.position, Quaternion.identity);
                _effect.transform.localScale = new Vector2(0.5f, 0.5f);
            }
        }
    }
}
