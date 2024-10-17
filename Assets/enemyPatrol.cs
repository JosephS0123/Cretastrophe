using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTest : MonoBehaviour
{
    public float rayDistGround;
    
    public float rayDistFront;
    public float speed;
    private bool moveRight = true; //Enemy starts out by moving right.
    public Transform Detector; //This will detect if there is ground in front of the enemy or not.

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector2.right * speed * Time.deltaTime);
        RaycastHit2D groundCheck = Physics2D.Raycast(Detector.position, Vector2.down, rayDistGround);
        RaycastHit2D frontCheck;
        
        if (moveRight)
        {
            frontCheck = Physics2D.Raycast(Detector.position, Vector2.right, rayDistFront);
        }
        else
        {
            frontCheck = Physics2D.Raycast(Detector.position, Vector2.left, rayDistFront);
        }
    
        if(!groundCheck.collider || frontCheck.collider)
        {
            if(moveRight)
            {
                transform.eulerAngles = new Vector3(0, -180, 0);
                moveRight = false;
            }
            else
            {
                transform.eulerAngles = new Vector3(0, 0, 0);
                moveRight = true;
            }
        }
    }
}
