using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemyControllerScript : MonoBehaviour
{
    public GameObject enemyPrefab;
    // maybe create 3D list where we store "sectors" containing a list of enemies that reside in that sector
    public List<Enemy> enemyList;

    private Transform playerPos;

    // Start is called before the first frame update
    void Start()
    {
        playerPos = GameObject.FindGameObjectWithTag("Player").transform;

        // dynamically search for all objects in the scene that have the Enemy script attached, and it returns an array of those objects
        enemyList = new List<Enemy>(FindObjectsOfType<Enemy>());

    }

    // Update is called once per frame
    // maybe do a fixed update to cap fps
    void Update()
    {
        foreach (var enemy in enemyList)
        {
            enemy.UpdateState(player);  // Pass the player reference to each enemy
        }
    
    }
}
