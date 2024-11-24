using System;
using Unity.VisualScripting;
using UnityEngine;

/*
    expect a DYNAMIC body type
    and continuous collision detection

    PLEASE make all enemies face left to begin with a rotation about y = 0
        that is an assumption the code makes
*/
public class EnemyControllerAI : MonoBehaviour
{
    /* enemy state management */
    private enum MoveType {straight, freefall, reverse, idle, startJump, midJump, fallOff};
    /*  combatMove is chase and run
        combatStill is sit and shoot
        mobile is just moving around
        immobile is sit and do "nothing"
    */
    public enum StateType {combatMove, combatStill, mobile, immobile};
    public enum PlayerSearchType {colliderSphere, lineOfSight, onContact, passive};
    public enum EnemyBehaviorType {wanderer, patroller, immobile, eraser};
    public enum ViewType {constant, dynamic}; // keep eye at same pos or allow enemy to look up/down at diff point in time
    
    /* Enemy Behavior */
    private MoveType movetype;
    private StateType statetype;
    public StateType defaultStateMobileType = StateType.mobile;
    public StateType defaultStateCombatType = StateType.combatStill;
    public PlayerSearchType searchType;
    public ViewType enemyViewType = ViewType.constant;
    public Projectile.projectileType projectileType = Projectile.projectileType.spike; /*TODO: assign to init func*/
    private ShootProjectiles projectiles;
    public ShootProjectiles.shootingDensity sDensity= ShootProjectiles.shootingDensity.constant;
    public ShootProjectiles.shootingFrequency sFreq = ShootProjectiles.shootingFrequency.constant;
    public ShootProjectiles.shootingType sType = ShootProjectiles.shootingType.semicircleSpread;

    public int projectileCount = 1;
    public float projectileSpeed = 1f;
    public float playerDetectRadius = 3.5f;
    public float enemyFireRate = 3.5f;
    public String prefabName = "Projectile";
    public float viewCone = 45f; // Enemies FOV +- viewcone/2
    public float lookDirectionAngle = 25f; // the point where the enemy is looking 
    private float actualLookDirectionAngle;
    public int aggroFrameTime = 15; // Allow enemy to remain aggroed for N frames
    private int aggroFrameTimeRemaining; // Counter of frames left, reset upon reaggro

    /* Positioning Related Info */
    public LayerMask groundLayer; /* Obstacle layer | May need considerations for other interactions*/
    public LayerMask playerLayer;
    GameObject player;
    Vector3 playerPosition;
    BoxCollider2D playerBoxCollider;
    float playerWidth;
    float playerHeight;

    /* Enemy Movement Attributes */
    public float moveSpeed = 2.5f; // speed is a factor to be able to allow enemies to walk up slopes
    public float jumpForce = 7f;
    public float hopForce = 1f;
    private bool gapAhead = false;
    private bool isWallAhead = false;
    private int stuckCounter = 0;

    /* Enemy Unity stuff*/
    private Rigidbody2D rb;
    private BoxCollider2D enemyCollider;
    private CircleCollider2D playerDetectionCollider;

    /* Useful Enemy attributes */
    private bool isFacingRight; /* TODO: set ALL enemies to face left or right PHYSICALLY */

    private Vector2 lookDirection; /* Should be a simple ((-1 or 1), 0) Vec2 */
    private float enemyWidth; /* collider bounds */
    private float enemyHeight; /* collider bounds */

    /* Stats to prevent getting stuck in pos */
    private Vector3 prevPos;
    private Vector3 prevPrevPos;
    private bool prevLookingRight;
    private bool prevPrevLookingRight;

    /* debug flags 
        0 = no console logs    
        1 = print movetypes (the type of movement enemy should make, e.g. straight, reverse, jump...)
        2 = print movement check gizmos
        3 = 1 & 2
        4 = LOS detection
        10 = everything
    */
    public int debugCode = 0; 
    
    /* or should be an Awake() method? */
    void Start()
    {
        /* Don't want enemy to move if falling down, will begin to move upon contact w/ ground
            Enemy also starts of not in aggro
        */
        movetype = MoveType.freefall;
        statetype = StateType.mobile;

        rb = GetComponent<Rigidbody2D>();
        enemyCollider = GetComponent<BoxCollider2D>(); 
        enemyWidth = enemyCollider.size.x * transform.localScale.x;
        enemyHeight = enemyCollider.size.y * transform.localScale.y;
        SetLookDirection();

        clearOldPositions();
        player = GameObject.Find("Player");
        playerLayer = LayerMask.GetMask("Player");
        playerBoxCollider = player.GetComponent<BoxCollider2D>();
        playerWidth = playerBoxCollider.size.x;
        playerHeight = playerBoxCollider.size.y;

        aggroFrameTimeRemaining = aggroFrameTime;
        actualLookDirectionAngle = lookDirectionAngle;

        projectiles = gameObject.AddComponent<ShootProjectiles>();
        // projectiles.setProjectileCount(projectileCount, "Projectile", projectileSpeed);

        switch (sType) 
        {
            case ShootProjectiles.shootingType.single:
                projectileSpeed = 6f;
                break;
            case ShootProjectiles.shootingType.narrowSpread:
                projectileSpeed = 4f;
                break;
            case ShootProjectiles.shootingType.widespread:
                projectileSpeed = 4.5f;
                break;
            case ShootProjectiles.shootingType.semicircleSpread:
                projectileSpeed = 3.5f;
                break;
            default:
                projectileSpeed = 5f;
                break;
        }


        projectiles.setProjectileEnums(sFreq, sType, projectileType, sDensity);
        projectiles.setProjectileBehavior(enemyFireRate, projectileCount, projectileSpeed, prefabName);

        switch (searchType) {
            case PlayerSearchType.colliderSphere:
                InstantiateCircleCollider(playerDetectRadius, Vector2.zero, true);
                break;
            case PlayerSearchType.lineOfSight:
                /* Do nothing */
                break;
            case PlayerSearchType.onContact:
                break;
            default:
                /* do nothing */
                break;
        }
    }

    // "Resets" position trackers due to transitions from certain enemy states 
    void clearOldPositions()
    {
        prevPos = transform.position - new Vector3(1f, 1f, 0);
        prevPrevPos = prevPos - new Vector3(5f, 5f, 0);
        prevLookingRight = !isFacingRight;
        prevPrevLookingRight = !prevLookingRight;
    }

    // debugging 
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
        if (debugCode == 1) {
            printMovetype();
        }
    }


    void checkState()
    {
        // Check possible movestates first in order to prevent a weird race-like conditions
        TryMove();
        // Updates statetype based on the manner of which the enemy aggroes onto player (if it does)
        playerSearch();

        switch (statetype){
            case StateType.combatMove:
                /* not implemented yet */
                break;
            case StateType.combatStill:
                /* 
                    WIP
                    Enemy is aggroed, stay still and shoot while aggroed til not
                */
                updateCombatStill();
                break;
            case StateType.mobile:
                /* 
                    WIP partially? 
                    Currently passive, check for player activating aggro
                */
                updatePassiveMove();
                break;
            case StateType.immobile:
                /*todo maybe*/
                break;
            default:
                break;
        }
    }

    /* Checks for player update if need be, updates enemy behavior if trigger occurs */
    /* Much pain ensued */
    void playerSearch()
    {
        // Ensure not taking over a "Critical" moment of movement which may cause unwanted enemy behavior
        if (movetype == MoveType.midJump || movetype == MoveType.freefall || movetype == MoveType.startJump || movetype == MoveType.fallOff) {
            aggroFrameTimeRemaining--;
            statetype = StateType.mobile;
            return;
        }

        // select enemy behavior based on how they "react" to "presence" of a player
        switch (searchType)
        {
            // Maybe delete the circlecollider2D in favor for IsWithinRadius() since same thing && more concise (later if i care)
            case PlayerSearchType.colliderSphere:
                break;
            case PlayerSearchType.lineOfSight:
                GetPlayerPosition();

                // If enemy has clear Line of Sight on player w/o total obstruction
                if (IsWithinRadius((Vector2)playerPosition, playerDetectRadius)) {
                    if (isValidLOS()) {
                        statetype = defaultStateCombatType;
                        aggroFrameTimeRemaining = aggroFrameTime;
                    } 
                } 
                // reduce time buffer if frame time since last valid LOS, restore "mobility" if expires
                aggroFrameTimeRemaining--;
                if (aggroFrameTimeRemaining <= 0) {
                    statetype = defaultStateMobileType;
                }
                
                break;
            // Prob wont exist since player dies on contact
            case PlayerSearchType.onContact:
                /* Kill player */
                break;
            case PlayerSearchType.passive:
                /*TODO: maybe consider adding a still mob (like a sitting puppy with tail wagging)
                        that when the player steps/runs on it, it creates a near insta-death punishment trigger
                        questiniong what the player did (for the lols)
                */
                statetype = defaultStateMobileType; // Do as you do w/o aggro, auto aggroes if player enters bubble
                break;
            default:
            /* Do nothing*/
                break;
        }

    }

    /* Perform unaggroed movement */
    void updatePassiveMove()
    {
        // Ensure that it doesn't make hasty decision that it "is" stuck 
        // and reduces the amount of positions to be stored to make checks
        if (statetype != defaultStateCombatType && IsStuck()) {
            if (stuckCounter > 12 ) {
                if (!IsWallAhead()) {
                    if (IsGapAhead() && !IsUnsafeGap()) {
                        movetype = MoveType.straight;
                        stuckCounter = 0;
                    } else {
                        movetype = MoveType.reverse;
                        stuckCounter = 0;
                    }
                }
            }
            /* Breaks enemy movement, unsure if possible to currently implement */
            // if (rollDice(1f) && CanJump()) {
            //     movetype = MoveType.startJump;
            // }
        } else {
            stuckCounter = 0;
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

    /* self-explanatory dontcha think? */
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

    /* Check for obstacle immediately ahead of entity */
    bool IsWallAhead()
    {
        float horizRayLength = 0.1f; 
        float vertRayLength = enemyHeight;
        Vector2 currentPos = transform.position;

        // Calculate offsets for ray start points
        float offsetTopY = enemyHeight / 2; 
        float offsetBottomY = -enemyHeight / 2; 
        float offsetAheadX = enemyWidth / 2f * (isFacingRight ? 1 : -1); // determines if ray starts on left/right side of enemy

        // Calculate ray origins for horizontal probes for walls
        Vector2 topRayOrigin = currentPos + new Vector2(offsetAheadX, offsetTopY);
        Vector2 bottomRayOrigin = currentPos + new Vector2(offsetAheadX, offsetBottomY);
        Vector2 centerRayOrigin = currentPos + new Vector2(offsetAheadX, 0);
        Vector2 verticalRayOrigin = currentPos + new Vector2(lookDirection.x * (enemyWidth / 2 * 1.2f), -enemyHeight/2); // Bottom edge of vertical ray

        RaycastHit2D topHit = Physics2D.Raycast(topRayOrigin, lookDirection, horizRayLength, groundLayer);
        RaycastHit2D bottomHit = Physics2D.Raycast(bottomRayOrigin, lookDirection, horizRayLength, groundLayer);
        RaycastHit2D centerHit = Physics2D.Raycast(centerRayOrigin, lookDirection, horizRayLength, groundLayer);
        RaycastHit2D verticalHit = Physics2D.Raycast(verticalRayOrigin, Vector2.up, vertRayLength, groundLayer);

        // Draw the rays for debugging
        Color rayColor = Color.red; // Color for horizontal rays

        if (debugCode == 2 || debugCode == 3) {
            if (topHit) {
                Debug.DrawRay(topRayOrigin, lookDirection * horizRayLength, rayColor);
            }
            if (centerHit) {
                Debug.DrawRay(centerRayOrigin, lookDirection * horizRayLength, rayColor);
            }
            if (bottomHit) {
                Debug.DrawRay(bottomRayOrigin, lookDirection * horizRayLength, rayColor);
            }
            if (verticalHit) {
                Debug.DrawRay(verticalRayOrigin, Vector2.up * vertRayLength, rayColor); 
                return true;
            }
        }

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
        float vertRayLengthDown = .9f * enemyHeight; 
        float yOffsetBottom = -enemyHeight / 2; // Just above the bottom edge
        float horizRayLength = 0.1f; 
        float xOffset = enemyWidth / 2f * (isFacingRight ? 1 : -1);
        // float vertRayLength = 1.05f * enemyHeight; // was considering using this one for checking if downward slope
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
        Debug.DrawRay(frontRayOrigin, lookDirection * horzRayLength, Color.red);
        return frontRayImmediate.collider == null;
    }

    /* Checks if the enemy can safely fall down the gap ahead */
    bool IsUnsafeGap()
    {
        float vertRayLength = 6 * enemyHeight;
        float xOffset = enemyWidth/2 + 0.15f;

        // Cast the vertical rays downwards
        Vector2 frontRayOrigin = (Vector2)transform.position + new Vector2 (lookDirection.x * xOffset, 0); 
        RaycastHit2D frontHit = Physics2D.Raycast(frontRayOrigin, Vector2.down, vertRayLength, groundLayer);
        Debug.DrawRay(frontRayOrigin, Vector2.down * vertRayLength, Color.red);

        // No safe ground was in contact if null therefore unsafe
        print("is unsafe gap?");
        print(frontHit.collider == null);
        return frontHit.collider == null;
    }

    /* Makes sure the enemy's movement is synced with class vars */
    void SetLookDirection() 
    {
        // Check the rotation of the enemy to set the look direction
        lookDirection = transform.rotation.y > 0f ? Vector2.right : Vector2.left;
        isFacingRight = transform.rotation.y > 0f; /* used to be "< 180f" ; default look direction should be left*/
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
    }


    // RNG, not much use 
    bool rollDice(float successPercentRate)
    {
        return UnityEngine.Random.Range(0f, 1f) < successPercentRate;
    }

    /* checks previous 2 frames + current to see if enemy is "possibly stuck" */
    bool IsStuck() 
    {
        if (prevPrevPos == prevPos && prevPos == transform.position) 
        {
            if (prevPrevLookingRight == prevLookingRight && prevLookingRight == isFacingRight) {
                print("IS STUCK\n\n\n");
                stuckCounter++;
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

    /* Update the enemy look direction, eye direction and sprite model */
    void TurnAround()
    {
        isFacingRight = !isFacingRight;
        lookDirection.x = -lookDirection.x;

        if (isFacingRight) {
            transform.Rotate(0f, -180f, 0f);
            actualLookDirectionAngle = lookDirectionAngle;
        } else { /* NOTE: default look is left */
            transform.Rotate(0f, 180f, 0f);
            actualLookDirectionAngle = 180 - lookDirectionAngle;
        }
    }

    // Detect when something contacts with either enemy hitbox or aggroCollider
    // currently does nothing
    /* TODO: access a class to kill off player */
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the player entered the aggro range
        if (((1 << other.gameObject.layer) & playerLayer) != 0)
        {
            if (playerDetectionCollider != null && other.IsTouching(playerDetectionCollider) && IsGrounded()) {
                /* Set default aggro behavior*/
                Debug.Log("Player entered aggro range!");
                statetype = defaultStateCombatType;
                movetype = MoveType.freefall;
                DoFall();
            } else if (playerDetectionCollider != null && other.IsTouching(playerDetectionCollider)) {
                /* TODO: insert player kill logic here*/
                Debug.Log("Player SHOULD DIE HERE!");
            }
        } 
    }

    // detect when the player exits the aggro range
    // Exit attack mode here and return to normal conditions
    /* No real use atm*/
    private void OnTriggerExit2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & playerLayer) != 0)
        {
            if (playerDetectionCollider != null && !other.IsTouching(playerDetectionCollider)) {
                /* Set default move behavior*/
                Debug.Log("Player left aggro range!");
                statetype = StateType.mobile;
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
        projectiles.FireProjectiles((Vector2)transform.position, lookDirection, (Vector2)playerPosition);
    }

    // nuff sed
    void GetPlayerPosition()
    {
        if (player != null)
        {
            playerPosition = player.transform.position;
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

    // linear alegegra AHHHH!
    bool IsWithinRadius(Vector2 targetPosition, float radius)
    {
        float distance = Vector2.Distance(transform.position, targetPosition);
        return distance <= radius;
    }

    // more linea albegra AGGGGG!!
    // Assumes the player position has been updated prior to call
    bool isValidLOS()
    {
        Vector3 playerPos = player.transform.position;
        bool inverseCorners = actualLookDirectionAngle < 0 && (playerPos.y < transform.position.y - enemyHeight/2f) ? true : false;
        /* generate 3 rays at the furthest visible* corner of the player that is visible from perspective of enemy */
        /* 
            if player is right of enemy -> get bottomRight corner and topLeft corner 
            else if player is left of enemy -> get bottomLeft corner and topRight corner

            IF player is beneath the enemy and enemy has LOS, invert the above
        */

        Vector3 bottomCornerPos = playerPos + (!inverseCorners && isFacingRight ? new Vector3(playerWidth/2f, -playerHeight/2f, 0) : new Vector3(-playerWidth/2, -playerHeight/2, 0));
        Vector3 centerPos = playerPos;
        Vector3 topCornerPos = playerPos + (!inverseCorners && isFacingRight ? new Vector3(-playerWidth/2f, playerHeight/2f, 0) : new Vector3(playerWidth/2, playerHeight/2, 0));

        bool bottomCorner = IsPlayerInLOS(actualLookDirectionAngle, viewCone, bottomCornerPos);
        bool center = IsPlayerInLOS(actualLookDirectionAngle, viewCone, centerPos);
        bool topCorner = IsPlayerInLOS(actualLookDirectionAngle, viewCone, topCornerPos);

        Vector3 directionToPlayer = (playerPosition - transform.position).normalized;
        float angleInRadians = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x);

        bool drawContactRays = (debugCode == 4) || (debugCode == 10)  ? true : false;

        if (bottomCorner && CastRayToTarget(bottomCornerPos)) {
            if (drawContactRays) {
                drawRayToPlayer((Vector2)transform.position, angleInRadians);                
            }
            return true;
        } 
        if (center && CastRayToTarget(centerPos)) {
            if (drawContactRays) {
                drawRayToPlayer((Vector2)transform.position, angleInRadians);                
            }
            return true;
        } 
        if (topCorner && CastRayToTarget(topCornerPos)) {
            if (drawContactRays) {
                drawRayToPlayer((Vector2)transform.position, angleInRadians);                   
            }
            return true;
        }
        return false;
    }

    // Shorthand to draw rays from enemy to player
    void drawRayToPlayer(Vector2 origin, float angleInRad)
    {
        Vector2 direction = new Vector2(Mathf.Cos(angleInRad), Mathf.Sin(angleInRad));

        float rayLength = Vector2.Distance(transform.position, playerPosition);
        Vector2 endPoint = origin + direction * rayLength;

        Debug.DrawLine(origin, endPoint, Color.red); 
    }

    // Checks if the LOS is valid and unobstructed by obstacles
    bool CastRayToTarget(Vector3 targetPoint)
    {
        Vector2 enemyPos = transform.position;

        // Calculate the direction from the current object to the target point
        Vector2 direction = (targetPoint - transform.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(enemyPos, direction, playerDetectRadius, groundLayer | playerLayer);

        // Check if the ray hit something
        if (hit.collider != null)
        {
            // Check if the object hit is on the Obstacle layer
            if (((1 << hit.collider.gameObject.layer) & groundLayer) != 0)
            {
                // Debug.Log("Hit an obstacle: " + hit.collider.gameObject.name);
                // Debug.DrawLine(enemyPos, hit.point, Color.red, 1f); 
                return false; 
            }
            else if (((1 << hit.collider.gameObject.layer) & playerLayer) != 0)
            {
                Debug.Log("Player in LOS: " + hit.collider.gameObject.name);
                // Debug.DrawLine(enemyPos, hit.point, Color.blue, 1f);
                return true; 
            }
        }
        else
        {
            // Debug.Log("Ray did not hit anything.");
        }

        return false; 
    }

    // Call this 3 times, 2 for corners facing enemy and 1 for center
    bool IsPlayerInLOS(float lookDirectionAngle, float viewConeAngle, Vector3 playerPosition)
    {
        float lookDirectionX = Mathf.Cos(lookDirectionAngle * Mathf.Deg2Rad);
        float lookDirectionY = Mathf.Sin(lookDirectionAngle * Mathf.Deg2Rad);
        Vector3 lookDirection = new Vector3(lookDirectionX, lookDirectionY, 0).normalized; 
        Vector3 viewerPosition = transform.position + new Vector3(this.lookDirection.x * enemyWidth/2f, enemyHeight/4f, 0);

        playerPosition.z = 0;
        viewerPosition.z = 0;
        
        // get direction to player
        Vector3 dirToPlayer = (playerPosition - viewerPosition).normalized;

        float dotProduct = Vector3.Dot(lookDirection, dirToPlayer);

        // see if player within bounds of enemy's sight
        float maxDot = Mathf.Cos(viewConeAngle * 0.5f * Mathf.Deg2Rad);

        // Check if the dot product is within the field of view
        return dotProduct >= maxDot;
    }

    // If the method to aggro is using within detection radius
    private void InstantiateCircleCollider(float radius, Vector2 offset, bool isTrigger)
    {
        playerDetectionCollider = gameObject.AddComponent<CircleCollider2D>();
        playerDetectionCollider.radius = radius;
        playerDetectionCollider.isTrigger = isTrigger;
        playerDetectionCollider.offset = offset;
    }
}