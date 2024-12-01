using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraZoneChange : MonoBehaviour
{
    public CinemachineConfiner2D Confiner;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            print("test");
            Confiner.m_BoundingShape2D = gameObject.transform.parent.GetComponent<Collider2D>();
        }
    }
}
