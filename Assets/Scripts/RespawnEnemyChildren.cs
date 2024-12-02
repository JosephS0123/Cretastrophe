using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnEnemyChildren : MonoBehaviour
{

    public void RespawnAll()
    {
        foreach(Transform child in transform)
        {
            child.GetComponent<EnemyControllerAI>().Respawn();
        }
    }
}
