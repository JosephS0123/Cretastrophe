using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EraseController : MonoBehaviour
{
    private Camera _cam;

    void Start()
    {
        _cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 mousePos = _cam.ScreenToWorldPoint(Input.mousePosition);

        transform.position = mousePos;
    }
}
