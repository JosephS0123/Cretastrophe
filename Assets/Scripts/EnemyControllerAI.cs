using Unity.VisualScripting;
using UnityEngine;
// using Pathfinding;

/*
expect a DYNAMIC body type
and continuous collision detection
*/
public class EnemyWander : MonoBehaviour
{

    private enum MoveType {straight, freefall, reverse, idle, startJump, midJump, fallOff};
    private enum StateType {combatMove, combatStill, passiveMove};
    private enum PlayerSearchType {colliderSphere, lineOfSight, onContact};
    MoveType movetype = MoveType.freefall;
    StateType statetype = StateType.passiveMove;
    public LayerMask playerLayer;
    private ShootProjectiles projectiles;
    GameObject player;
    Vector3 playerPosition;
    public float moveSpeed = 2.5f; // speed is a factor to be able to allow enemies to walk up slopes
    public float jumpForce = 7f;
    public float hopForce = 1f;
    public LayerMask groundLayer; /* Obstacle layer | May need considerations for other interactions*/

    private Rigidbody2D rb;
    private BoxCollider2D enemyCollider;
    private CircleCollider2D playerDetectionCollider;

    private bool isFacingRight;

    private Vector2 lookDirection; /* Should be a simple ((-1 or 1), 0) Vec2 */
    private float enemyWidth; /* collider bounds */
    private float enemyHeight; /* collider bounds */
    private bool gapAhead = false;

    private Vector3 prevPos;
    private Vector3 prevPrevPos;
    private bool prevLookingRight;
    private bool prevPrevLookingRight;
    private bool isWallAhead = false;
    
    /* or should be an Awake() method? */
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        enemyCollider = GetComponent<BoxCollider2D>(); 
        playerDetectionCollider = GetComponent<CircleCollider2D>(); 
        SetLookDirection();
        enemyWidth = enemyCollider.size.x * transform.localScale.x;;
        enemyHeight = enemyCollider.size.y * transform.localScale.y;;
        clearOldPositions();
        player = GameObject.Find("Player");
        playerLayer = LayerMask.GetMask("Player");
        projectiles = gameObject.AddComponent<ShootProjectiles>();
        projectiles.setProjectileCount(5, "Projectile");
    }

    void clearOldPositions()
    {
        prevPos = transform.position - new Vector3(1f, 1f, 0);
        prevPrevPos = prevPos - new Vector3(5f, 5f, 0);
        prevLookingRight = !isFacingRight;
        prevPrevLookingRight = prevLookingRight;
    }



    void printMovetype()
    {
        switch (movetype)
        {
            case MoveType.straight:
                print("straight");
                break;
            case MoveType.reverse:
                print("reverse");
                break;
            case MoveType.startJump:
                print("startJump");
                break;
            case MoveType.midJump:
                print("midJump");
                break;
            case MoveType.fallOff:
                print("fallOff");
                break;
            case MoveType.freefall:
                print("freefall");
                break;
            default:
                print("default");
                /* shouldnt happen*/
                break;
        }
    }

    /*currently updates 50 FPS per editor default*/
    public void FixedUpdate()
    {
       checkState();
    }


    void checkState()
    {
        switch (statetype){
            case StateType.combatMove:
                /* not implemented yet */
                break;
            case StateType.combatStill:
                /* WIP */
                updateCombatStill();
                break;
            case StateType.passiveMove:
                /* WIP partially? mostly done */
                updatePassiveMove();
                break;
            default:
                break;
        }
    }

    void updatePassiveMove()
    {
        TryMove(); 
        if (IsStuck()) {
            movetype = MoveType.reverse;
        }

        // debug current movetype
        // printMovetype();

        // decide on the movement to take based on previous action
        switch (movetype)
        {
            case MoveType.straight:
                MoveForward();
                break;
            case MoveType.reverse:
                TurnAround();
                break;
            case MoveType.startJump:
                DoJump();
                movetype = MoveType.midJump;
                break;
            case MoveType.midJump:
                MoveForward();
                break;
            case MoveType.fallOff:
                MoveForward();
                break;
            case MoveType.freefall:
                DoFall();
                break;
            default:
                /* shouldnt happen*/
                break;
        }

        prevPrevLookingRight = prevLookingRight;
        prevPrevPos = prevPos;

        prevLookingRight = isFacingRight;
        prevPos = transform.position;
    }

    void TryMove()
    {
        /* walls and gaps cant happen at the same place, wall takes priority
            then slopes, then gaps */
        bool onGround = IsGrounded();

        /* sudden contact with ground mid jump cancels forward motion*/
        if (!onGround) {
            if (movetype == MoveType.midJump) {
                movetype = MoveType.midJump;
            } else {
                movetype = MoveType.freefall;
            }
        } else {
            if (IsWallAhead())
            {
                if (IsSlope()){
                    movetype = MoveType.straight;
                } else {
                    if (CanJump()) {
                        if (rollDice(.8f)) {
                            movetype = MoveType.startJump;
                            return;
                        }
                    }
                    /* implied else */ 
                    movetype = MoveType.reverse;
                }
            }
            else if (gapAhead)
            {
                if (IsUnsafeGap()) {
                    if (CanJump()) {
                        // if (rollDice(.8f)){
                        movetype = MoveType.startJump;
                        return;
                        // }
                    }
                    /* Implied else */ 
                    movetype = MoveType.reverse;
                    gapAhead = IsGapAhead();
                } else {
                    movetype = MoveType.fallOff;
                    gapAhead = false;
                }
            } else {
                gapAhead = IsGapAhead();
                movetype = MoveType.straight;
            }
        }
        
        
    }

    /* Check for obstacle ahead of entity */
    bool IsWallAhead()
    {
        float horizRayLength = 0.1f; 
        float vertRayLength = enemyHeight; // Length of the vertical ray + 5% error

        // Calculate offsets for ray start points
        float yOffsetTop = enemyHeight / 2; // Just below the top edge
        float yOffsetBottom = -enemyHeight / 2; // Just above the bottom edge
        float xOffset = enemyWidth / 2f * (isFacingRight ? 1 : -1);

        // Calculate ray origins based on the enemy's position and facing direction
        Vector2 topRayOrigin = (Vector2)transform.position + new Vector2(xOffset, yOffsetTop);
        Vector2 bottomRayOrigin = (Vector2)transform.position + new Vector2(xOffset, yOffsetBottom);
        Vector2 centerRayOrigin = (Vector2)transform.position + new Vector2(xOffset, 0);
        Vector2 verticalRayOrigin = (Vector2)transform.position + new Vector2(lookDirection.x * (enemyWidth / 2 * 1.2f), -enemyHeight/2); // Bottom edge of vertical ray

        RaycastHit2D topHit = Physics2D.Raycast(topRayOrigin, lookDirection, horizRayLength, groundLayer);
        RaycastHit2D bottomHit = Physics2D.Raycast(bottomRayOrigin, lookDirection, horizRayLength, groundLayer);
        RaycastHit2D centerHit = Physics2D.Raycast(centerRayOrigin, lookDirection, horizRayLength, groundLayer);
        
        // Cast vertical ray from center to top
        RaycastHit2D verticalHit = Physics2D.Raycast(verticalRayOrigin, Vector2.up, vertRayLength, groundLayer);

        // Draw the rays for debugging
        Color rayColor = Color.red; // Color for horizontal rays
        // if (topHit) {
            Debug.DrawRay(topRayOrigin, lookDirection * horizRayLength, rayColor);
        // }
        // if (centerHit) {
            Debug.DrawRay(centerRayOrigin, lookDirection * horizRayLength, rayColor);
        // }
        // if (bottomHit) {
            Debug.DrawRay(bottomRayOrigin, lookDirection * horizRayLength, rayColor);
        // }
        // if (verticalHit) {
            Debug.DrawRay(verticalRayOrigin, Vector2.up * vertRayLength, Color.red); // Debug for vertical ray
            // return true;
        // }

        // Count how many raycasts hit something
        int hitCount = 0;
        if (topHit.collider != null) hitCount++;
        if (bottomHit.collider != null) hitCount++;
        if (centerHit.collider != null) hitCount++;
        if (verticalHit.collider != null) {
            return isWallAhead;
        }

        // Check if at least two raycasts hit something
        return hitCount >= 2;
    }


    /* Might need an overhaul or not*/
    /* If its not a wall ahead then yea, prob a slope, there might be missing edge case considerations
        though*/
    bool IsSlope()
    {
        /* .8 / .85f may be a stretch to try */
        float vertRayLengthDown = .9f * enemyHeight; // Length of the vertical ray + 5% error
        float yOffsetBottom = -enemyHeight / 2; // Just above the bottom edge
        float horizRayLength = 0.1f; 
        float xOffset = enemyWidth / 2f * (isFacingRight ? 1 : -1);
        // float vertRayLength = 1.05f * enemyHeight; // Length of the vertical ray + 5% error
        Vector2 bottomRayOrigin = (Vector2)transform.position + new Vector2(xOffset, yOffsetBottom);
        RaycastHit2D bottomHit = Physics2D.Raycast(bottomRayOrigin, lookDirection, horizRayLength, groundLayer);
        Debug.DrawRay(bottomRayOrigin, lookDirection * horizRayLength, Color.cyan);

        // Check for an increasing slope
        Vector2 verticalRayOrigin = (Vector2)transform.position + new Vector2(lookDirection.x * (enemyWidth / 2 * 1.2f), enemyHeight / 2);
        // Vector2 verticalRayOrigin2 = (Vector2)transform.position + new Vector2(lookDirection.x * (enemyWidth / 2 * 1.01f), 0); // Bottom edge of vertical ray

        // Cast vertical ray from top to bottom
        RaycastHit2D verticalHitUpSlope = Physics2D.Raycast(verticalRayOrigin, Vector2.down, vertRayLengthDown, groundLayer);

        // Draw the rays for debugging
        Color rayColor = Color.green; // Color for horizontal rays
        // if (verticalHitUpSlope) {
            Debug.DrawRay(verticalRayOrigin, Vector2.down * vertRayLengthDown, rayColor); // Debug for vertical ray
            // return true;
        // }

        print("slope ahead?");
        print(verticalHitUpSlope.collider == null);
        return verticalHitUpSlope.collider == null;
    }

    /* Checks a little ahead of the enemy to see if there is a gap wide enough for it to fall in */
    bool IsGapAhead()
    {
        float horzRayLength = enemyWidth;
        float yOffset = enemyHeight;

        // a horizontal raycast, width of enemy
        Vector2 frontRayOrigin = (Vector2)transform.position + new Vector2 (lookDirection.x * enemyWidth/2 + .03f, -yOffset); 
        RaycastHit2D frontRayImmediate = Physics2D.Raycast(frontRayOrigin, lookDirection, horzRayLength, groundLayer);
        // RaycastHit2D frontRayAhead = Physics2D.Raycast((Vector2)transform.position + new Vector2 (3 * enemyWidth/2, -yOffset), lookDirection, horzRayLength, groundLayer);

        Debug.DrawRay(frontRayOrigin, lookDirection * horzRayLength, Color.red);
        // Debug.DrawRay((Vector2)transform.position + new Vector2 (5 * dir * enemyWidth/2, -yOffset), -lookDirection * horzRayLength, Color.green);

        // Return true if ray did not hit anything (gap detected)
        // print(frontRayImmediate.collider == null);
        // print("is gap ahead?");
        // print(frontRayImmediate.collider == null);
        return frontRayImmediate.collider == null;
    }

    /* Checks if the enemy can safely fall down the gap ahead */
    bool IsUnsafeGap()
    {
        float vertRayLength = 6 * enemyHeight;

        float xOffset = enemyWidth/2 + 0.15f;

        Vector2 frontRayOrigin = (Vector2)transform.position + new Vector2 (lookDirection.x * xOffset, 0); 

        // Cast the vertical rays downwards
        RaycastHit2D frontHit = Physics2D.Raycast(frontRayOrigin, Vector2.down, vertRayLength, groundLayer);

        // Draw the rays for debugging
        // if (!frontHit) {
            Debug.DrawRay(frontRayOrigin, Vector2.down * vertRayLength, Color.red);
        // }

        // Return true if both rays hit nothing (indicating a safe gap)
        print("is unsafe gap?");
        print(frontHit.collider == null);
        return frontHit.collider == null;
    }

    /* Makes sure the enemy's movement is synced with class vars */
    void SetLookDirection() 
    {
        // Check the rotation of the enemy to set the look direction
        lookDirection = transform.rotation.x < 180f ? Vector2.right : Vector2.left;
        isFacingRight = transform.rotation.x < 180f;
    }

    /* Uses some dynamic raycasting to predict valid jump spots, if none then fail to jump */
    /* physics 1 moment qq */
    bool CanJump()
    {
        float effectiveGravity = rb.gravityScale * Physics2D.gravity.y;
        float maxHeight = jumpForce * jumpForce / (2f * Mathf.Abs(effectiveGravity));
        float enemyHeightTemp = 0;

        // Cast vertical ray to simulate the max height enemy can make straight up
        Vector2 vertRayOrigin = (Vector2)transform.position + new Vector2(lookDirection.x * (enemyWidth / 2), -enemyHeight / 2);
        RaycastHit2D vertHit = Physics2D.Raycast(vertRayOrigin, Vector2.up, maxHeight, groundLayer);

        // If the vertical ray hit something, calculate the new max achievable height
        if (vertHit.collider != null)
        {
            maxHeight = vertHit.point.y - (transform.position.y + enemyHeight / 2);
            enemyHeightTemp = enemyHeight;
        }

        // Update relevant vars based on maxHeight
        float timeToMaxHeight = Mathf.Sqrt(2 * Mathf.Abs(maxHeight) / Mathf.Abs(effectiveGravity));
        float totalJumpTime = 2 * timeToMaxHeight; // Time to ascend and descend
        float horizontalDistance = moveSpeed * totalJumpTime;

        // Calculate ray origins
        // Should be at the apex of the jump - if the jump was cut short then subtract by enemyHeight to accomodate height diff
        Vector2 horizRayOrigin = (Vector2)transform.position + new Vector2(lookDirection.x * (enemyWidth / 2), maxHeight - (enemyHeightTemp / 2)); // apex position
        // Check if it can land (assuming that there is a wall past the apex)
        Vector2 downRayOriginMid = horizRayOrigin + lookDirection * (horizontalDistance / 2);
        // otherwise if it can land a clean jump to the end, do so
        Vector2 downRayOriginEnd = horizRayOrigin + lookDirection * horizontalDistance;

        // Cast the horizontal ray (is path clear to land around apex)
        RaycastHit2D horizHit = Physics2D.Raycast(horizRayOrigin, lookDirection, horizontalDistance, groundLayer);

        // Check downward rays | allows jump to land further down if need be
        float downRayLength = maxHeight + 5 * enemyHeight; // Length of downward rays
        RaycastHit2D downHitMid = Physics2D.Raycast(downRayOriginMid, Vector2.down, downRayLength, groundLayer);
        RaycastHit2D downHitEnd = Physics2D.Raycast(downRayOriginEnd, Vector2.down, downRayLength, groundLayer);

        // Draw rays for debugging
        Debug.DrawRay(vertRayOrigin, Vector2.up * maxHeight, Color.red);
        Debug.DrawRay(horizRayOrigin, lookDirection * horizontalDistance, Color.blue); // Horizontal ray
        Debug.DrawRay(downRayOriginMid, Vector2.down * downRayLength, Color.red); // Mid downward ray
        Debug.DrawRay(downRayOriginEnd, Vector2.down * downRayLength, Color.red); // End downward ray

        // Check conditions for jumping
        if (horizHit.collider == null) {
            if (downHitMid.collider != null || downHitEnd.collider != null) {
                return true;
            }
        }
        return false;
    }


    /* Checks if the enemy is currently on OR VERY near the ground */
    bool IsGrounded()
    {
        float rayOffsetY = enemyHeight/2 + .02f;

        Vector2 bottomRayOrigin = (Vector2)transform.position + new Vector2 (-lookDirection.x * enemyWidth/2 + .03f, -rayOffsetY); 
        RaycastHit2D bottomRayImmediate = Physics2D.Raycast(bottomRayOrigin, lookDirection, enemyWidth, groundLayer);
        Debug.DrawRay(bottomRayOrigin, lookDirection * enemyWidth, Color.green);
        
        return bottomRayImmediate.collider != null;

        // Calculate positions for three rays: left edge, center, and right edge
        // Vector2 leftRayOrigin = (Vector2)transform.position + Vector2.down * rayOffsetY - lookDirection * enemyWidth/2;
        // Vector2 centerRayOrigin = (Vector2)transform.position + Vector2.down * rayOffsetY;
        // Vector2 rightRayOrigin = (Vector2)transform.position + Vector2.down * rayOffsetY + lookDirection * enemyWidth/2;

        // Did the rays hit anything
        // bool leftHit = Physics2D.Raycast(leftRayOrigin, Vector2.down, rayLength, groundLayer);
        // bool centerHit = Physics2D.Raycast(centerRayOrigin, Vector2.down, rayLength, groundLayer);
        // bool rightHit = Physics2D.Raycast(rightRayOrigin, Vector2.down, rayLength, groundLayer);

        // Debug.DrawRay(leftRayOrigin, Vector2.down * rayLength, Color.green);
        // Debug.DrawRay(centerRayOrigin, Vector2.down * rayLength, Color.green);
        // Debug.DrawRay(rightRayOrigin, Vector2.down * rayLength, Color.green);  
        // Return true if any ray hits the ground (grounded)
        // return leftHit || centerHit || rightHit;
    }


    bool rollDice(float successPercentRate)
    {
        return Random.Range(0f, 1f) < successPercentRate;
    }

    /* WIP not sure if useful */
    bool IsStuck() 
    {
        // in same pos past 2 frames & currently
        if (prevPrevPos == prevPos && prevPos == transform.position) 
        {
            if (prevPrevLookingRight == prevLookingRight && prevLookingRight== isFacingRight) {
                print("IS STUCK\n\n\n");
                return true;
            }
        }
        return false;
    }

    void MoveForward()
    {
        rb.velocity = new Vector2(lookDirection.x * moveSpeed, rb.velocity.y);
    }

    /* Prevent enemy from moving horizontally, let gravity do the work */
    void DoFall() {
        rb.velocity = new Vector2(0, rb.velocity.y);
    }

    /* Update the enemy look direction and sprite model */
    void TurnAround()
    {
        isFacingRight = !isFacingRight;
        lookDirection.x = -lookDirection.x; 

        if (transform.rotation.x <= 180.0f) {
            transform.Rotate(0f, -180f, 0f);
        } else {
            transform.Rotate(0f, 180f, 0f);
        }
    }

    // Detect when something contacts with either enemy hitbox or aggroCollider
    // Enter some attack mode here
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the player entered the aggro range
        if (((1 << other.gameObject.layer) & playerLayer) != 0)
        {
            if (other.IsTouching(playerDetectionCollider) && IsGrounded()) {
                /* Set default aggro behavior*/
                Debug.Log("Player entered aggro range!");
                statetype = StateType.combatStill;
                movetype = MoveType.freefall;
                DoFall();
            } else if (other.IsTouching(playerDetectionCollider)) {
                /* TODO: insert player kill logic here*/
                Debug.Log("Player SHOULD DIE HERE!");
            }
        } 
    }

    // Optionally, detect when the player exits the aggro range
    // Exit attack mode here and return to normal conditions
    private void OnTriggerExit2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & playerLayer) != 0)
        {
            if (!other.IsTouching(playerDetectionCollider)) {
                /* Set default move behavior*/
                Debug.Log("Player left aggro range!");
                statetype = StateType.passiveMove;
                movetype = MoveType.freefall;
                DoFall();
            }
        } 
    }

    // Preserve current x velocity and add upward force, y velocity naturally decreases due to big G
    void DoJump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
    }

    /* For enemy of type sit and shoot */
    void updateCombatStill()
    {
        GetPlayerPosition();

        /* update enemy look direction when trying to shoot */
        if (playerPosition.x - transform.position.x > 0 && !isFacingRight){
            TurnAround();
        } else if (playerPosition.x - transform.position.x < 0 && isFacingRight) {
            TurnAround();
        }

        clearOldPositions();
        projectiles.FireProjectiles((Vector2)transform.position + new Vector2 (enemyWidth/2f * lookDirection.x, 0), lookDirection);
    }
    void GetPlayerPosition()
    {
        if (player != null)
        {
            playerPosition = player.transform.position;
            // Debug.Log("Player Position: " + playerPosition);
        }
        else
        {
            Debug.LogWarning("Player not found! Player Model is not currently Loaded!");
            Debug.LogWarning("Player not found! Player Model is not currently Loaded!");
            Debug.LogWarning("Player not found! Player Model is not currently Loaded!");
            Debug.LogWarning("Player not found! Player Model is not currently Loaded!");
            Debug.LogWarning("Player not found! Player Model is not currently Loaded!");
            Debug.LogWarning("Player not found! Player Model is not currently Loaded!");
        }
    }
}