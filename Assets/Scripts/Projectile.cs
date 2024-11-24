using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 5f;      
    public float direction;

    private Rigidbody2D rb;
    /*
        spike is a break-on-contact projectile affected by gravity
        bullet is projectile that moves along a specified direction, ignoring gravity until despawns/makes contact
    */
    public enum projectileType {spike, bullet, blackhole, boomerang};
    /*
        make sure sticky doesnt destroy on impact
    */
    public enum projectileAttribute {sticky, nonsticky, tracking, fire, ice};
    public projectileType projectileT; // projectileType.spike  is default
    public projectileAttribute projectileA = projectileAttribute.nonsticky;
    private float distanceTilDespawn = 10f;
    private Vector2 startPos;

    /* player tracking stuff */
    GameObject player;
    Vector2 playerPosition;

    private float timer;
    /* blackhole */
    public float lifeTime = 15f; // time in seconds before blackhole despawns
    public float effectiveDistance; // distance that player must be within to be affected by gravity

    /* for boomerang behavior */
    private float timeTilReturn;
    private bool isReturning;

    void Start()
    {
        float angleInRadians = direction * Mathf.Deg2Rad;
        angleInRadians += (angleInRadians > 1.55f) ? -.2f : .2f;
        Vector2 directionVector = new Vector2(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians));
        rb = GetComponent<Rigidbody2D>();
        
        rb.velocity = directionVector.normalized * speed; 
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, direction));
        startPos = (Vector2)transform.position;

        if (projectileA == projectileAttribute.sticky || projectileA == projectileAttribute.tracking) {
            timer = 0;
            lifeTime = 15f; // time in seconds before projectile despawns
        }

        switch (projectileT)
        {
            case projectileType.boomerang:
                timer = 0;
                isReturning = false;
                break;
            case projectileType.blackhole:
                timer = 0;
                lifeTime = 15f; 
                effectiveDistance = 5f;

                player = GameObject.Find("Player");
                if (player == null) {
                    Debug.Log("PLAYER NOT FOUND IN PROJECTILE.CS \nDestroying projectile object!");
                    Debug.Log("PLAYER NOT FOUND IN PROJECTILE.CS \nDestroying projectile object!");
                    Debug.Log("PLAYER NOT FOUND IN PROJECTILE.CS \nDestroying projectile object!");
                    Debug.Log("PLAYER NOT FOUND IN PROJECTILE.CS \nDestroying projectile object!");
                    Debug.Log("PLAYER NOT FOUND IN PROJECTILE.CS \nDestroying projectile object!");
                } else {
                    playerPosition = player.transform.position;
                }
                break;
            default:
                break;
        }
    }

    void Update()
    {
        // bullet goes out of bounds 
        if (Vector2.Distance((Vector2)transform.position, startPos) >= distanceTilDespawn)
        {
            Destroy(gameObject);
        }

        timer = timer += Time.deltaTime;

        switch (projectileT)
        {
            case projectileType.boomerang:

                if (timer >= timeTilReturn) {
                    /* boomerang moves at the same speed */
                    if (isReturning) {
                        Destroy(gameObject);
                    }
                    isReturning = true;
                    timer = 0;
                    /* make boomerang return, acceleration only on the x-movement unless angled? */
                }
                break;
            case projectileType.blackhole:
                playerPosition = player.transform.position;

                /* call vortex() method to suck in player, updating their movement if nearby */

                if (timer >= lifeTime) {
                    Destroy(gameObject);
                }
                break;
            default:
                break;
        }

    }

    /* remove this ?*/
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the collision object is on the "Obstacle" or "Player" layers
        if (collision.gameObject.layer == LayerMask.NameToLayer("Obstacle") ||
            collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            // Destroy the projectile when it collides with something in "Obstacle" or "Player" layer
            if (collision.gameObject.layer == LayerMask.NameToLayer("Player")) {
                Debug.Log("Destroying projectile due to collision with player");
                if (projectileT == projectileType.blackhole) {
                    /* TODO: KILL PLAYER */
                } else {
                    Destroy(gameObject);
                }
            } else { /* Hit an obstacle */
                if (projectileT == projectileType.boomerang) {
                    if (isReturning) {
                        /* boomerang becomes destructible if its returning otherwise indestructable */
                        Destroy(gameObject);
                    }
                    /* return */
                } else {
                    Destroy(gameObject);
                }
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
                // Debug.Log("Hit an obstacle!");
            }
        }
    }

}
