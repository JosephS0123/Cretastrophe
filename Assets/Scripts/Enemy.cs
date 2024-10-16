using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// contains the AI for the enemy
public class Enemy : MonoBehaviour
{
    public enum State 
    {
        Idle,
        Chase,
        Attack,
        Patrol,
        Wander
    }

    public enum EnemyType
    {
        Bouncy,     /* like a slime that jumps as it moves */
        Eraser,     /* subset of chaser, but is always on chase mode */
        Chaser,     /* an enemy that moves towards a target */
        Static,     /* an enemy that doesn't move, may attack from a distance */
        Barreler    /* an enemy that moves along 1 direction until it collides with something */
    // TODO: add more enemy classifications
    }

    private Transform playerPos;
    private Rigidbody2D enemyModel;
    RaycastHit2D groundRay; // checks for valid footing ahead of enemy, if NULL or depth > threshold, try jump else turn around

    private boolean isLookingLeft = true;
    private boolean isOnSlope = false;
    private boolean isGrounded = true;
    private float theta; // a radian angle to where the enemy is moving (cosine)

    private State curState = State.Idle;
    private State defaultState; // either patrol OR wander, so that IDLE knows what to transition to 
    private boolean doChangeState = false;
    private boolean isIdle = true;

    private float aggroRadius;

    private int time = 0;
    private int FPS = 60, numSecToUpdate = 10;
    private int timeToChangeStates = FPS * numSecToUpdate; 

    // Update is called once per frame
    // consider maybe fixedUpdate
    void UpdateState()
    {
        // shorthand to change from idle to some movement behavior and v.v. after some period of time
        if (time %= timeToChangeStates == 0) {
            isIdle = !isIdle;
            if (isIdle)
            {
                curState = State.defaultState;
            } 
            else 
            {
                curState = State.Idle;
            }
        }

        switch (currentState)
        {
            case State.Idle:
                doIdle();
                break;
            case State.Chase:
                doChase();
                break;
            case State.Attack:
                doAttack();
                break;
            case State.Patrol:
                doPatrol();
                break;
            case State.Wander:
                doWander();
                break;
            default:
                // error occured?
                break;
        }
        time += 1;
    }

    void doIdle()
    {
        // do nothing?
    }

    void doChase()
    {
        // TODO: Implement chase script HERE
    }

    void doAttack()
    {
        //TODO: figure out logic
        // Idea: depends on the enemy type
        switch (EnemyType)
        {
            case State.Bouncy:
            // TODO: Implement attack logic for respective enemy type
                break;
            case State.Eraser:
            // TODO: Implement attack logic for respective enemy type
                break;
            case State.Chaser:
            // TODO: Implement attack logic for respective enemy type
                break;
            case State.Static:
            // TODO: Implement attack logic for respective enemy type
                break;
            case State.Barreler:
            // TODO: Implement attack logic for respective enemy type
                break;
            default:
            // ERROR: Undefined enemy
                break;
        }
    }

    void doPatrol()
    {
        // TODO: Implement Patrol Script HERE
    }

    void doWander()
    {
        /* Psuedocode */
        /* check direction OR incorporate boolean checks midcode and work from there */
        /* 1- Check if player is in aggro radius */
            /* If true, call doChase() and end conditional branch here
                If false, continue moving as normal

                MAKE SURE THIS IS NESTED otherwise may cause execution errors
            */

            /* 2- Check if it can move ahead
                    // not guaranteed to happen
                    Do %probability% for jump action in current direction 
                        if can jump, set num = RNG
                            if RNG > minNum then Jump


                    // the following below are conflicting branches
                    // may lead to weird behavior maybe have to swap orders or add other conditionals

                    Check for wall/obstacle ahead using horizontal raycast?
                        if so check if you can jump over it
                            if true jump and cotinue moving forward
                        if not turn around, (change direction)

                    Check for valid ground ahead
                        -use the verticalRaycast to check if there is valid ground ahead (~~0.01 units or so ahead)
                            >threshold is the maximum depth that a dip can be if the character is allowed to just fall down
                            >float distance = Mathf.Abs(raycast.point.y - transform.position.y);
                        
                        if (verticalRaycast != NULL) OR (distance < threshold)
                            continue walking ahead and fall down the ledge
                        else 
                            check if you can jump
                                jump if able
                                else turn around
              */
            /*  */
            /*  */
            /*  */  
    }
}

