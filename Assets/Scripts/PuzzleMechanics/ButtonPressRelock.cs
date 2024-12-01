using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ButtonPressRelock : MonoBehaviour
{
    public DoorOpenRelock door;
    public AudioClip drawSound; //Sound effect
    public Sprite pressedSprite;
    public Sprite unpressedSprite;
    public int doorIndex;
    private SpriteRenderer _renderer;
    void Start()
    {
        _renderer = gameObject.GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.CompareTag("PhysicsObj")|| collision.gameObject.GetComponent<ChalkEater>() != null)
        {
            door.DoorUpdate(doorIndex-1, true);
            //AudioSource.PlayClipAtPoint(drawSound, transform.position);
            _renderer.sprite = pressedSprite;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        door.DoorUpdate(doorIndex-1, false);
        _renderer.sprite = unpressedSprite;
    }
}

