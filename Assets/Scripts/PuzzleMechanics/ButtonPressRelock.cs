using System.Collections.Generic;
using UnityEngine;

public class ButtonPressRelock : MonoBehaviour
{
    public DoorOpenRelock door;
    public AudioClip drawSound; // Sound effect
    public Sprite pressedSprite;
    public Sprite unpressedSprite;
    public int doorIndex;
    private SpriteRenderer _renderer;

    private bool isPressed = false;

    private void Start()
    {
        _renderer = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.CompareTag("PhysicsObj") || collision.GetComponent<ChalkEater>() != null)
        {
            if (!isPressed)
            {
                isPressed = true;
                door.ButtonPressed(doorIndex);
                _renderer.sprite = pressedSprite;
                //AudioSource.PlayClipAtPoint(drawSound, transform.position);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.CompareTag("PhysicsObj") || collision.GetComponent<ChalkEater>() != null)
        {
            if (isPressed)
            {
                isPressed = false;
                door.ButtonReleased(doorIndex);
                _renderer.sprite = unpressedSprite;
            }
        }
    }
}
