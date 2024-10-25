using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Controller2D))]
public class Player : MonoBehaviour
{
    //public float jumpHeight = 4;
    //public float timeToJumpApex = .4f;
    
    public float gravity = -25;

    float accelerationTimeAirborne = .1f;
    float accelerationTimeGrounded = .05f;
    float moveSpeed = 6;

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
        
        if (controller.collisions.above || controller.collisions.below) 
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

        float targetVelocityX = input.x * moveSpeed;
        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below)?accelerationTimeGrounded:accelerationTimeAirborne);
        if (controller.collisions.left || controller.collisions.right && !jumping)
        {
            velocity.x = 0;
        }
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
        //Physics.SyncTransforms();
    }
}
