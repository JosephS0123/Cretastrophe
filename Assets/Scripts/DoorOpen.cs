using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorOpen : MonoBehaviour
{
    public int buttonCount;
    private bool[] buttons;

    void Start()
    {
        buttons = new bool[buttonCount];
        Array.Fill(buttons, false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DoorUpdate(int index, bool value)
    {
        buttons[index] = value;
        bool allPressed = true;
        for (int i = 0; i < buttonCount; i++)
        {
            if (!buttons[i]) 
            {
                allPressed = false;
                break; 
            }
        }

        if (allPressed)
        {
            gameObject.SetActive(false);
        }
    }
}
