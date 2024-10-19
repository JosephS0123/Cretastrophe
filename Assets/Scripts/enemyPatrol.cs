using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class EnemyTest : MonoBehaviour
{
    public float rayDistGround;
    public float rayDistFront;
    public float speed;
    private bool moveRight = true; //Enemy starts out by moving right.
    public Transform Detector; //This will detect if there is ground in front of the enemy or not.
    public Collider2D enemyCollider;  // Reference to the enemy's collider
    public Collider2D playerDetect; //Ref to killPlayer collider

    void Start()
    {
        enemyCollider = GetComponent<Collider2D>();
        playerDetect = GetComponent<Collider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector2.right * speed * Time.deltaTime);

        // Raycast to check for the ground below the detector
        RaycastHit2D groundCheck = Physics2D.Raycast(Detector.position, Vector2.down, rayDistGround);

        // Raycast to check for obstacles in front of the detector
        RaycastHit2D frontCheck;
        if (moveRight)
        {
            frontCheck = Physics2D.Raycast(Detector.position, Vector2.right, rayDistFront);
            Debug.DrawRay(Detector.position, Vector2.right * rayDistFront, Color.red);  // Visualize the front ray when moving right
        }
        else
        {
            frontCheck = Physics2D.Raycast(Detector.position, Vector2.left, rayDistFront);
            Debug.DrawRay(Detector.position, Vector2.left * rayDistFront, Color.red);  // Visualize the front ray when moving left
        }

        // Visualize the ground ray
        Debug.DrawRay(Detector.position, Vector2.down * rayDistGround, Color.green);

        // If the frontCheck detects something, ensure it's not the enemy's own collider
        if (!groundCheck.collider || (frontCheck.collider && frontCheck.collider != enemyCollider && frontCheck.collider != playerDetect))
        {

            // Check if the frontCheck hits player
            if (frontCheck.collider && frontCheck.collider.CompareTag("Player"))
            {
            // KILL muahaha
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }

            if (moveRight)
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

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("work pls");
        if(other.gameObject.CompareTag("Player"))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

}
