using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnChalkChildren : MonoBehaviour
{

    public void RespawnAll()
    {
        foreach(Transform child in transform)
        {
            child.GetComponent<ChalkEater>().Respawn();
        }
    }
}
