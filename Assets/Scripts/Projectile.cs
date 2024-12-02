using UnityEngine;
using UnityEngine.SceneManagement; 

public class Projectile : MonoBehaviour
{
    Vector3 spawnPos;
    public float speed = 5f;      
    public float direction;

    private Rigidbody2D rb;
    /*
        spike is a break-on-contact projectile affected by gravity
        bullet is projectile that moves along a specified direction, ignoring gravity until despawns/makes contact
    */
    public enum projectileType { spike, bullet, blackhole, boomerang};
    /*
        make sure sticky doesnt destroy on impact
    */
    public enum projectileAttribute { sticky, nonsticky, combustible};
    public enum projectileElement { normal, fire, ice };
    public enum projectileMobility { immobile, mobile};
    public projectileType projectileT; // projectileType.spike  is default
    public projectileAttribute projectileA = projectileAttribute.nonsticky; // What most normal projectiles are
    public projectileElement pElement = projectileElement.normal;
    public Igniter iScript;
    private float distanceTilDespawn = 20f;
    private Vector2 startPos;

    /* player tracking stuff */
    GameObject player;
    private Player playerScript;
    public projectileMobility pMobility = projectileMobility.mobile;

    private float timer = 0;
    /* blackhole */
    public float projectileLifetime = 15f; // time in seconds before blackhole despawns
    public float pullRadius = 3f; // distance that player must be within to be affected by gravity
    public float maxPullStrength = 3f;
    private Controller2D playerController;
    public float pullAccelerationTime = 10f;
    private float pullTimer;
    private bool killingPlayer = false;
    private int timeTilDeath;

    /* for boomerang behavior */
    private float timeTilReturn;
    private bool isReturning;
    private bool runTimer = false;

    /* For shrinking player in blakc hoel*/
    private int tick = 5;
    public bool canShrinkPlayer = false;
    public LayerMask playerLayer;
    public LayerMask groundLayer;
    public LayerMask chalkLayer;

    /* changin sprites */
    private SpriteRenderer spriteRenderer;
    public Sprite defaultSprite;
    public Sprite onFireSprite;

    void Start()
    {
        timer = 0;
        pullTimer = 5f;
        killingPlayer = false;
        runTimer = false;
        timeTilDeath = 60;
        tick = 5;

        spawnPos = transform.position;
        Transform projectileTransform = transform.Find("projectile");
        spriteRenderer = projectileTransform.GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = defaultSprite;

        if (pMobility == projectileMobility.immobile) 
        {
            rb = GetComponent<Rigidbody2D>();

            rb.velocity = Vector2.zero;
            rb.gravityScale = 0;
            startPos = (Vector2)transform.position;

            switch (projectileT)
            {
                case projectileType.blackhole:
                    player = GameObject.Find("Player");
                    playerController = player.GetComponent<Controller2D>();
                    playerScript = player.GetComponent<Player>();
                    break;
                default:
                    break;
            }
        } 
        else 
        {
            float angleInRadians = direction * Mathf.Deg2Rad;
            // angleInRadians += (angleInRadians > 1.55f) ? -.2f : .2f;
            Vector2 directionVector = new Vector2(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians));
            rb = GetComponent<Rigidbody2D>();
            
            rb.velocity = directionVector.normalized * speed; 
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, direction));
            startPos = (Vector2)transform.position;

            setProjectileLifetime();

            switch (projectileT)
            {
                case projectileType.boomerang:
                    isReturning = false;
                    break;
                case projectileType.blackhole:
                    projectileLifetime = 15f; 

                    player = GameObject.Find("Player");
                    
                    playerController = player.GetComponent<Controller2D>();
                    playerScript = player.GetComponent<Player>();
                    
                    break;
                case projectileType.bullet:
                    rb.gravityScale = 0;
                    break;
                default:
                    break;
            }
        }

        if (pElement == projectileElement.fire) {
            IgniteProjectile();
        } 
    }

    void IgniteProjectile()
    {
        gameObject.AddComponent<Igniter>(); // Dynamically add the Igniter component
        iScript = gameObject.GetComponent<Igniter>();

        if (iScript != null)
        {
            iScript.burnableLayer = 1 << LayerMask.NameToLayer("Obstacle");
            iScript.fireSpreadRange = 0.5f;

            spriteRenderer.sprite = onFireSprite;

        }
    }

    void FixedUpdate()
    {
        // bullet goes out of bounds 
        if (pMobility == projectileMobility.mobile && Vector2.Distance((Vector2)transform.position, startPos) >= distanceTilDespawn)
        {
            Destroy(gameObject);
        }

        if (runTimer && (pMobility != projectileMobility.immobile)) {
            timer += Time.deltaTime;
            if (timer >= projectileLifetime) {
                Destroy(gameObject);
            }
        }

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
                if ((pMobility == projectileMobility.mobile) && !runTimer) {
                    doRunTimer();
                }

                // give like a bit over a second in the black hole before killing player
                if (killingPlayer) {
                    if (timeTilDeath <= 0) {
                        killPlayer();
                    }
                }

                doBlackHole();

                if (pMobility == projectileMobility.mobile && timer >= projectileLifetime) {
                    Destroy(gameObject);
                }
                break;
            default:
                break;
        }

    }

    void doBlackHole()
{
    Vector2 playerPosition = player.transform.position;
    Vector2 blackHolePosition = transform.position;
    float distance = Vector2.Distance(playerPosition, blackHolePosition);

    // If the player is within range, start pulling
    if (distance <= pullRadius)
    {
        if (distance <= .5f)
        {
            timeTilDeath--;
            if (canShrinkPlayer && timeTilDeath / 10 % 6 == tick)
            {
                tick--;
                Shrink();
            }
        }
        Vector2 directionToBlackHole = (blackHolePosition - playerPosition).normalized;

        float pullStrength = Mathf.Clamp01(1 - (distance / pullRadius)) * maxPullStrength;

        // Gradually increase pull strength over time (simulate acceleration with timer)
        pullTimer += Time.deltaTime;
        float currentPullStrength = Mathf.Min(pullStrength * (pullTimer / pullAccelerationTime), pullStrength);

        // Apply the gravitational pull (directly modify the velocity here)
        ApplyGravitationalPull(directionToBlackHole, currentPullStrength);
    }
    else
    {
        // If the player is out of range, reset the pull timer
        pullTimer = 0f;
    }
}

    void ApplyGravitationalPull(Vector2 direction, float strength)
{
    // Calculate the pull velocity directly without considering the player's current velocity
    Vector2 pullVelocity = direction * strength;

    // Update the player's movement by directly applying the calculated pull velocity
    // The second parameter (Vector2.zero) can be used for other movement input, we ignore it here
    playerController.Move(pullVelocity * Time.deltaTime, Vector2.zero);
}

    
    void killPlayer()
    {
        player = GameObject.Find("Player");
        playerScript = player.GetComponent<Player>();
        playerScript.Die();
    }


    void OnTriggerEnter2D(Collider2D other)
    {
        bool destroyObject = true;
        // Check if the other collider is on the "Obstacle" or "Player" layers
        if (other.gameObject.layer == LayerMask.NameToLayer("Obstacle") ||
            other.gameObject.layer == LayerMask.NameToLayer("Player") ||
            other.gameObject.layer == LayerMask.NameToLayer("Chalk"))
        {
            /* when projectile hits player*/
            if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                if (projectileT == projectileType.blackhole) {
                    killingPlayer = true;
                } else {
                    killPlayer();
                }
            }
            else
            {
                if (projectileA == projectileAttribute.sticky) {
                    if (!runTimer) {
                        doRunTimer();
                    }
                    
                    rb.velocity = Vector3.zero;
                    rb.gravityScale = 0;
                    return;
                } else if (projectileA == projectileAttribute.combustible) {
                    if (other.gameObject.CompareTag("Red")) { /* when hitting red chalk */
                        IgniteProjectile();
                        destroyObject = false;
                    }
                }

                if (projectileT == projectileType.boomerang) {
                    if (isReturning) {
                        /* boomerang becomes destructible if its returning otherwise indestructable */
                        Destroy(gameObject);
                    }
                    /* return */
                } else if (projectileT == projectileType.blackhole) {
                    rb.velocity = Vector2.zero;
                } else {
                    if (destroyObject){
                        Destroy(gameObject);
                    }
                }
            }
        }
    }


    private void doRunTimer()
    {
        runTimer = true;
        timer = 0;
    }

    private void setProjectileLifetime()
    {
        switch (projectileT)
        {
            case projectileType.blackhole:
                doRunTimer();
                projectileLifetime = 15f;
                break;
            case projectileType.boomerang:
                doRunTimer();
                projectileLifetime = 8f;
                break;
            default:
                break;
        }

        switch (projectileA) 
        {
            case projectileAttribute.sticky:
                projectileLifetime = 3.5f;
                break;
            default:
                break;
        }
    }

    private float shrinkFactor = 1f;
    public void Shrink()
    {
        shrinkFactor /= 1.4f;
        // Get the current local scale of the object
        Vector3 currentScale = player.transform.localScale;

        // Shrink the object by multiplying its scale by the shrink factor
        player.transform.localScale = currentScale * shrinkFactor;
    }

    public void Respawn()
    {
        transform.position = spawnPos;
        Start();
        

    }

}
