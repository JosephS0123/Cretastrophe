using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class ButtonPress : MonoBehaviour
{
    public DoorOpen door;
    public AudioClip drawSound; //Sound effect
    public Sprite pressedSprite;
    public Sprite unpressedSprite;
    public int doorIndex;
    private SpriteRenderer _renderer;

    List<GameObject> collidingList;

    public int numInTrigger = 0;
    void Start()
    {
        _renderer = gameObject.GetComponent<SpriteRenderer>();
        collidingList = new List<GameObject>();
    }

    void Update()
    {
        GameObject[] collidingArray = collidingList.ToArray();
        foreach(GameObject colliding in collidingArray)
        {
            if(colliding == null)
            {
                if (collidingList.Remove(colliding.gameObject)) { numInTrigger--; }
            }
        }

        if (collidingList.Count == 0)
        {
            door.DoorUpdate(doorIndex - 1, false);
            _renderer.sprite = unpressedSprite;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.CompareTag("PhysicsObj") || collision.CompareTag("BlueLine") || collision.gameObject.GetComponent<ChalkEater>() != null)
        {
            collidingList.Add(collision.gameObject);

            if (collidingList.Count == 1)
            {
                door.DoorUpdate(doorIndex - 1, true);
                //AudioSource.PlayClipAtPoint(drawSound, transform.position);
                _renderer.sprite = pressedSprite;
            }
            numInTrigger++;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.CompareTag("PhysicsObj") || collision.CompareTag("BlueLine") || collision.gameObject.GetComponent<ChalkEater>() != null)
        {
            if(collidingList.Remove(collision.gameObject)) { numInTrigger--; }
            
            if (collidingList.Count == 0)
            {
                door.DoorUpdate(doorIndex - 1, false);
                _renderer.sprite = unpressedSprite;
            }
        }
    }

    public void changeNum(int num)
    {
        numInTrigger += num;
    }
}

