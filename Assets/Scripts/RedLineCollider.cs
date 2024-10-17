using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedLineCollider : MonoBehaviour
{
    [SerializeField] private LineRenderer _renderer;
    [SerializeField] private EdgeCollider2D _collider;
    [SerializeField] private GameObject _line;
    void Start()
    {

    }


    void Update()
    {

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Destroy the item after it's collision triggered
        
        if(collision.gameObject.tag == "Eraser") 
        {
            Destroy(_line);
        }

        
    }
}
