using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 5f;      
    public Vector2 direction = new Vector2(0,0);      

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        rb.velocity = direction * speed; 
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
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
