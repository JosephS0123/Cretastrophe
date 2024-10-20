using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikeBallSpawner : MonoBehaviour
{
    public PhysicsSpikeBall spikeBall;
    private PhysicsSpikeBall spikeBallInstance;
    void Start()
    {
        Vector3 spawnPos = transform.position;
        
        spikeBallInstance = Instantiate(spikeBall, transform.position, Quaternion.identity, this.transform);
        spikeBallInstance.spikeBallSpawner = this;
    }

    void Update()
    {
        
        
    }

    public void respawnBall()
    {
        StartCoroutine(respawnBallIEnum());
    }

    private IEnumerator respawnBallIEnum()
    {
        yield return new WaitForSeconds(3);
        spikeBallInstance = Instantiate(spikeBall, transform.position, Quaternion.identity, this.transform);
        spikeBallInstance.spikeBallSpawner = this;
    }
}
