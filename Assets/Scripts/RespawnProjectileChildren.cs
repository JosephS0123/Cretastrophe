using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnProjectileChildren : MonoBehaviour
{

    public void RespawnAll()
    {
        foreach(Transform child in transform)
        {
            child.GetComponent<Projectile>().Respawn();
        }
    }
}
