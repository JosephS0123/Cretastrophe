using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Controller2D))]
public class Player : MonoBehaviour
{
    public float gravity = -25;
    float maxVelocityY = 15;
    float fallMultiplier = 1.15f;

    float accelerationTimeAirborne = .15f;
    float accelerationTimeGrounded = .05f;
    float accelerationTimeIcy = .3f;
    float moveSpeed = 6;
    float moveSpeedIce = 10;

    float baseVelocity = 6;
    float holdAcceleration = 25;
    float holdAccelerationFalloff = 35;
    float holdDuration = 0.30f;
    float jumpTimeElapsed;
    float curHoldAcceleration;

    float coyoteTimePre;
    float coyoteTimePost;
    float coyoteDuration = .1f;

    bool coyoteCheckPre = false;
    bool coyoteCheckPost = false;
    bool jumping = false;

    //float jumpVelocity;
    Vector2 velocity;
    float velocityXSmoothing;

    Controller2D controller;

    void Start()
    {
        controller = GetComponent<Controller2D>();

        //gravity = -(2 * jumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        //jumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
        jumpTimeElapsed = 0;
    }

    void Update()
    {
        
        if ((controller.collisions.above && velocity.y > 0) || (controller.collisions.below && velocity.y < 0) && !jumping) 
        {
            if (!controller.collisions.slidingDownMaxSlope)
            {
                velocity.y = 0;
            }
        }
        
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        
        if(controller.collisions.below && !coyoteCheckPost && !jumping)
        {
            coyoteTimePre = 0;
            coyoteCheckPre = true;
        }
        else
        {
            if (coyoteCheckPre)
            {
                if (coyoteTimePre < coyoteDuration)
                {
                    coyoteTimePre += Time.deltaTime;
                }
                else
                {
                    coyoteCheckPre = false;
                }
            }

        }
        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!coyoteCheckPre)
            {
                if (controller.collisions.below)
                {
                    velocity.y = baseVelocity;
                    jumpTimeElapsed = 0;
                    jumping = true;
                    curHoldAcceleration = holdAcceleration;
                    coyoteCheckPost = false;
                }
                else
                {
                    coyoteTimePost = 0;
                    coyoteCheckPost = true;
                }
            }
            else
            {
                velocity.y = baseVelocity;
                jumpTimeElapsed = 0;
                jumping = true;
                curHoldAcceleration = holdAcceleration;
                coyoteCheckPre = false;
            
            }
        }

        if(Input.GetKey(KeyCode.Space))
        {
            if (jumpTimeElapsed < holdDuration && jumping)
            {
                velocity.y += curHoldAcceleration * Time.deltaTime;
                curHoldAcceleration -= holdAccelerationFalloff * Time.deltaTime;
                jumpTimeElapsed += Time.deltaTime;
            }
            else if(coyoteCheckPost)
            {
                if (coyoteTimePost < coyoteDuration)
                {
                    if (controller.collisions.below)
                    {
                        velocity.y = baseVelocity;
                        jumpTimeElapsed = 0;
                        jumping = true;
                        curHoldAcceleration = holdAcceleration;
                        coyoteCheckPost = false;
                    }
                    else
                    {
                        coyoteTimePost += Time.deltaTime;
                    }
                }
                else
                {
                    coyoteCheckPost = false;
                }
            }
            else if(jumpTimeElapsed > holdDuration && jumping)
            {
                jumping = false;
            }
        }

        if(Input.GetKeyUp(KeyCode.Space))
        {
            jumping = false;
        }

        float targetVelocityX = input.x * ((controller.collisions.onIce) ? moveSpeedIce : moveSpeed);
        float smoothTime;
        if (controller.collisions.below)
        {
            smoothTime = (controller.collisions.onIce) ? accelerationTimeIcy : accelerationTimeGrounded;
        }
        else
        {
            smoothTime = accelerationTimeAirborne;
        }
        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, smoothTime);
        if ((controller.collisions.left || controller.collisions.right) && !jumping)
        {
            velocity.x = 0;
        }
        if (velocity.y <= 0)
        {
            velocity.y += gravity * fallMultiplier * Time.deltaTime;
        }
        else if(velocity.y > 0)
        {
            velocity.y += gravity * Time.deltaTime;
        }
        //velocity.y += gravity * Time.deltaTime;
        velocity.y = Mathf.Clamp(velocity.y, -maxVelocityY, maxVelocityY);
        controller.Move(velocity * Time.deltaTime, input);
        //Physics.SyncTransforms();
    }

    // Helper functions for Projectile.cs
    public Vector2 getPlayerVelocity()
    {
        return velocity;
    }

    // Helper functions for Projectile.cs
    public void updatePlayerVelocity(Vector2 velocity)
    {
        this.velocity = velocity;
    }
}
