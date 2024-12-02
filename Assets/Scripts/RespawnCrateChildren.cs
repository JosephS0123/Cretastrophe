using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnCrateChildren : MonoBehaviour
{

    public void RespawnAll()
    {
        foreach(Transform child in transform)
        {
            child.GetComponent<Burnable>().Respawn();
        }
    }
}
