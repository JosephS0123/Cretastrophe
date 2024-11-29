using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Igniter : MonoBehaviour
{
    public float fireSpreadRange;
    public LayerMask burnableLayer;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, fireSpreadRange, burnableLayer);
        foreach (Collider2D collider in colliders)
        {
            Burnable _burnable = collider.GetComponent<Burnable>();
            if (_burnable != null)
            {
                _burnable.SetOnFire();
            }
        }
    }
}
