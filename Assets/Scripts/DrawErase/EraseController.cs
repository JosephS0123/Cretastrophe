using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EraseController : MonoBehaviour
{
    private Camera _cam;
    private Rigidbody2D _rb;

    void Start()
    {
        _cam = Camera.main;
        _rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 mousePos = _cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 nextPos = transform.position;
        
        while(nextPos != mousePos)
        {
            nextPos = Vector2.MoveTowards(nextPos, mousePos, Time.deltaTime);
            transform.position = nextPos;
        }
        //transform.position = mousePos;
    }

    /*void FixedUpdate()
    {
        Vector2 mousePos = _cam.ScreenToWorldPoint(Input.mousePosition);
        
        _rb.position = mousePos;
    }*/
}
