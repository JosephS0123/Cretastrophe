using System.Collections.Generic;
using UnityEngine;

public class ButtonPressRelockInverse : MonoBehaviour
{
    public DoorOpenRelockInverse door; // Reference to the door script
    public Sprite pressedSprite; // Sprite when the button is pressed
    public Sprite unpressedSprite; // Sprite when the button is unpressed

    private SpriteRenderer _renderer;
    private List<GameObject> collidingList;

    private void Start()
    {
        _renderer = GetComponent<SpriteRenderer>();
        collidingList = new List<GameObject>();

        // Start the door as open (inactive)
        door.SetDoorState(false); // Door is open by default, meaning inactive with no collider and semi-transparent
        _renderer.sprite = unpressedSprite; // Button starts unpressed
    }

    private void Update()
    {
        // Clean up any null references in the colliding list
        collidingList.RemoveAll(item => item == null);

        if (collidingList.Count > 0)
        {
            // An object is pressing the button, so close the door
            door.SetDoorState(true); // Door is now closed (active)
            _renderer.sprite = pressedSprite; // Button is pressed
        }
        else
        {
            // No object on the button, door stays open
            door.SetDoorState(false); // Door is open (inactive)
            _renderer.sprite = unpressedSprite; // Button is unpressed
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (IsValidObject(collision.gameObject))
        {
            collidingList.Add(collision.gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (IsValidObject(collision.gameObject))
        {
            collidingList.Remove(collision.gameObject);
        }
    }

    private bool IsValidObject(GameObject obj)
    {
        // Check if the object is a valid trigger (Player, PhysicsObj, or any specific component)
        return obj.CompareTag("Player") || obj.CompareTag("PhysicsObj") || obj.GetComponent<ChalkEater>() != null;
    }
}
