using UnityEngine;

/*
expect a DYNAMIC body type
and continuous collision detection
*/
public class EnemyWander : MonoBehaviour
{

    private enum MoveType {straight, freefall, reverse, idle, startJump, midJump};
    MoveType move;
    public float moveSpeed = 2f;
    public float jumpForce = 10f;
    public LayerMask groundLayer; /* Obstacle layer | May need considerations for other interactions*/

    private Rigidbody2D rb;
    private BoxCollider2D enemyCollider;
    private bool isFacingRight;

    private Vector2 lookDirection; /* Should be a simple ((-1||1), 0) Vec2 */
    private float enemyWidth; /* collider bounds */
    private float enemyHeight; /* collider bounds */
    private bool gapAhead = false;
    
    /* or should be an Awake() method? */
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        enemyCollider = GetComponent<BoxCollider2D>(); 
        updateLookDirection();
        enemyWidth = enemyCollider.size.x * transform.localScale.x;;
        enemyHeight = enemyCollider.size.y * transform.localScale.y;;
    }

    /*currently updates 50 FPS per editor default*/
    public void FixedUpdate()
    {
        TryMove(); //updates move enum

        // decide on the movement to take based on previous action
        switch (move)
        {
            case MoveType.straight:
                print("1");
                MoveForward();
                break;
            case MoveType.reverse:
                print("2");
                TurnAround();
                break;
            case MoveType.freefall:
                print("3");
                DoFall();
                break;
            case MoveType.midJump:
                print("5");
                MoveForward();
                break;
            case MoveType.startJump:
                print("6");
                DoJump();
                break;
            default:
                print("4");
                /* shouldnt happen*/
                break;
        }

    }

    /* Might need an overhaul or not*/
    /* If its not a wall ahead then yea, prob a slope, there might be missing edge case considerations
        though*/
    bool IsSlope()
    {
        return !IsWallAhead(true);
    }

    /* Checks a little ahead of the enemy to see if there is a gap wide enough for it to fall in */
    bool IsGapAhead()
    {
        float horzRayLength = enemyWidth;
        float yOffset = -enemyHeight / 2 - enemyHeight * .05f;
        // a horizontal raycast, width of enemy
        RaycastHit2D leftHit = Physics2D.Raycast((Vector2)transform.position + new Vector2 (0, yOffset), lookDirection, horzRayLength, groundLayer);

        // uncomment for debugging on event update interactions
        // if (!leftHit) {
            Debug.DrawRay((Vector2)transform.position + new Vector2 (0, yOffset), lookDirection * horzRayLength, Color.magenta);
        // }
        // if (!rightHit) {
            // Debug.DrawRay(rightRayOrigin, Vector2.down * horzRayLength, Color.magenta);
        // }

        // Return true if ray did not hit anything (gap detected)
        return leftHit.collider == null;
    }

    /* Makes sure the enemy's movement is synced with class vars */
    void updateLookDirection() 
    {
        // Check the rotation of the enemy to set the look direction
        lookDirection = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        isFacingRight = transform.localScale.x > 0;
    }

    /* Uses some dynamic raycasting to predict valid jump spots, if none then fail to jump */
    /* physics 1 moment qq */
    bool CanJump()
    {
        float effectiveGravity = rb.gravityScale * Physics2D.gravity.y;
        float maxHeight = (jumpForce * jumpForce) / (2 * Mathf.Abs(effectiveGravity));
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
        Debug.DrawRay(downRayOriginMid, Vector2.down * downRayLength, Color.green); // Mid downward ray
        Debug.DrawRay(downRayOriginEnd, Vector2.down * downRayLength, Color.green); // End downward ray

        // Check conditions for jumping
        if (horizHit.collider == null) {
            if (downHitMid.collider != null || downHitEnd.collider != null) {
                return true;
            }
        }
        return false;
    }

    /* Check if the enemy can move past/on the obstacle ahead */
    bool IsWallAhead(bool checkSlope)
    {
        float horizRayLength = 0.1f; 
        float vertRayLength = enemyHeight/2 + .1f; // Length of the vertical ray

        // Calculate offsets for ray start points
        float yOffsetTop = enemyHeight / 2; // Just below the top edge
        float yOffsetBottom = -enemyHeight / 2; // Just above the bottom edge
        float xOffset = enemyWidth / 2f * (isFacingRight ? 1 : -1);

        // Calculate ray origins based on the enemy's position and facing direction
        Vector2 topRayOrigin = (Vector2)transform.position + new Vector2(xOffset, yOffsetTop);
        Vector2 bottomRayOrigin = (Vector2)transform.position + new Vector2(xOffset, yOffsetBottom);
        Vector2 centerRayOrigin = (Vector2)transform.position + new Vector2(xOffset, 0);
        Vector2 verticalRayOrigin = (Vector2)transform.position + new Vector2(lookDirection.x * (enemyWidth / 2 + horizRayLength), -.2f * enemyHeight); // Bottom edge of vertical ray

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
            Debug.DrawRay(verticalRayOrigin, Vector2.up * vertRayLength, Color.green); // Debug for vertical ray
            // return true;
        // }

        // Count how many raycasts hit something
        int hitCount = 0;
        if (topHit.collider != null) hitCount++;
        if (bottomHit.collider != null) hitCount++;
        if (centerHit.collider != null) hitCount++;
        if (verticalHit.collider != null) {
            return true;
        }

        // Check if at least two raycasts hit something
        return hitCount >= 2;
    }

    /* Checks if the enemy is currently on OR VERY near the ground */
    /* Use 3 vertical small rays */
    bool IsGrounded()
    {
        float rayLength = 0.15f;
        float rayOffsetY = enemyHeight/2;

        // Calculate positions for three rays: left edge, center, and right edge
        Vector2 leftRayOrigin = (Vector2)transform.position + Vector2.down * rayOffsetY - lookDirection * enemyWidth/2;
        Vector2 centerRayOrigin = (Vector2)transform.position + Vector2.down * rayOffsetY;
        Vector2 rightRayOrigin = (Vector2)transform.position + Vector2.down * rayOffsetY + lookDirection * enemyWidth/2;

        // Did the rays hit anything
        bool leftHit = Physics2D.Raycast(leftRayOrigin, Vector2.down, rayLength, groundLayer);
        bool centerHit = Physics2D.Raycast(centerRayOrigin, Vector2.down, rayLength, groundLayer);
        bool rightHit = Physics2D.Raycast(rightRayOrigin, Vector2.down, rayLength, groundLayer);

        // Draw rays for debugging
        if (leftHit) {
            Debug.DrawRay(leftRayOrigin, Vector2.down * rayLength, Color.green);
        }
        if (centerHit) {
            Debug.DrawRay(centerRayOrigin, Vector2.down * rayLength, Color.green);
        }
        if (rightHit) {
            Debug.DrawRay(rightRayOrigin, Vector2.down * rayLength, Color.green);  
        } 

        // Return true if any ray hits the ground (grounded)
        return leftHit || centerHit || rightHit;
    }

    void TryMove()
    {
        /* walls and gaps cant happen at the same place, wall takes priority
            then slopes, then gaps, 
            
            ??then?? different platform/jump considerations*/
            // add the !isJumping later
        bool onGround = IsGrounded();

        /* Check if airborne*/
        if (!onGround){
            /* When just falling down*/
            if (move != MoveType.midJump && move !=  MoveType.startJump) {
                move = MoveType.freefall;
                gapAhead = false;
            } else { /* Currently in midjump*/
                move = MoveType.midJump;
            }
        }  else {
            if (IsWallAhead(false))
            {
                if (!IsSlope()){
                    if (CanJump()) {
                        move = MoveType.startJump;
                    } else {
                        move = MoveType.reverse;
                    }
                } else {
                    move = MoveType.straight;
                }
            }
            else if (gapAhead)
            {
                if (!IsSafeGap()) {
                    if (CanJump()) {
                        move = MoveType.startJump;
                    } else {
                        move = MoveType.reverse;
                    }
                    gapAhead = false;
                } else {
                    move = MoveType.straight;
                }
            } else {
                gapAhead = IsGapAhead();
                move = MoveType.straight;
            }
        }
        
    }

    /*enemy may get stuck? haven't seen it happen in a while*/
    void MoveForward()
    {
        rb.velocity = new Vector2(lookDirection.x * moveSpeed, rb.velocity.y);
    }

    void DoFall() {
        rb.velocity = new Vector2(moveSpeed * .05f, rb.velocity.y);
    }

    /* Update the enemy look direction and model */
    void TurnAround()
    {
        isFacingRight = !isFacingRight;
        lookDirection.x = -lookDirection.x; // Flip direction
        transform.Rotate(0f, 180f, 0f); // Rotate the enemy
    }

    /* Checks if the enemy can safely fall down the gap ahead */
    bool IsSafeGap()
    {
        float vertRayLength = 5 * enemyHeight;
        float rayOffset = 0.1f; // Offset in front of the enemy

        float xOffset = enemyWidth / 2 + rayOffset;

        Vector2 frontRayOrigin = (Vector2)transform.position + lookDirection * xOffset - new Vector2 (0, enemyHeight/2); 
        Vector2 centerRayOrigin = (Vector2)transform.position - new Vector2 (0, enemyHeight/2); 

        // Cast the vertical rays downwards
        RaycastHit2D frontHit = Physics2D.Raycast(frontRayOrigin, Vector2.down, vertRayLength, groundLayer);
        RaycastHit2D centerHit = Physics2D.Raycast(centerRayOrigin, Vector2.down, vertRayLength, groundLayer);

        // Draw the rays for debugging
        // if (!frontHit) {
            Debug.DrawRay(frontRayOrigin, Vector2.down * vertRayLength, Color.blue);
        // }
        // if (!centerHit) {
            Debug.DrawRay(centerRayOrigin, Vector2.down * vertRayLength, Color.blue);
        // }

        // Return true if both rays hit nothing (indicating a safe gap)
        return frontHit.collider != null || centerHit.collider != null;
    }

    // Preserve current x velocity and add upward force
    void DoJump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
    }


}