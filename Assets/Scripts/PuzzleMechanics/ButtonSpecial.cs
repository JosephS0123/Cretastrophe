using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonSpecial : MonoBehaviour
{
    public DoorOpenRelock door;
    public Volcano volcano;
    //public AudioClip drawSound; //Sound effect
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
        if (collision.CompareTag("Player") || collision.CompareTag("PhysicsObj") || collision.CompareTag("BlueLine"))
        {
            door.ButtonPressed(doorIndex - 1);

            if (!volcano.isActive) { volcano.activate(true); }
            //AudioSource.PlayClipAtPoint(drawSound, transform.position);
            _renderer.sprite = pressedSprite;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        
        _renderer.sprite = unpressedSprite;
    }

    public void respawn()
    {
        door.ButtonReleased(doorIndex - 1);
        volcano.activate(false);
    }
}
