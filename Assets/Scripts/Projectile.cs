using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 5f;      
    public float direction;

    private Rigidbody2D rb;

    void Start()
    {
        float angleInRadians = direction * Mathf.Deg2Rad;
        Vector2 directionVector = new Vector2(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians));
        rb = GetComponent<Rigidbody2D>();
        
        rb.velocity = directionVector.normalized * speed; 
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, direction));
    }

    /* remove this ?*/
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the collision object is on the "Obstacle" or "Player" layers
        if (collision.gameObject.layer == LayerMask.NameToLayer("Obstacle") ||
            collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            // Destroy the projectile when it collides with something in "Obstacle" or "Player" layer
            Destroy(gameObject);
            if (collision.gameObject.layer == LayerMask.NameToLayer("Player")) {
                Debug.Log("Destroying projectile due to collision");
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the other collider is on the "Obstacle" or "Player" layers
        if (other.gameObject.layer == LayerMask.NameToLayer("Obstacle") ||
            other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                /*TODO: Kill player*/
                Destroy(gameObject);
            }
            else if (other.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
            {
                Destroy(gameObject);
                Debug.Log("Hit an obstacle!");
            }
        }
    }

}
