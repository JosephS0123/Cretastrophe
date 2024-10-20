using System.Collections;
using System.Collections.Generic;
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

        if (Input.GetKeyDown(KeyCode.Space) && controller.collisions.below)
        {
            velocity.y = baseVelocity;
            jumpTimeElapsed = 0;
            jumping = true;
            curHoldAcceleration = holdAcceleration;
        }

        if(Input.GetKey(KeyCode.Space) && jumpTimeElapsed < holdDuration && jumping)
        {
            velocity.y += curHoldAcceleration * Time.deltaTime;
            curHoldAcceleration -= holdAccelerationFalloff * Time.deltaTime;
            jumpTimeElapsed += Time.deltaTime;
        }

        if(Input.GetKeyUp(KeyCode.Space))
        {
            jumping = false;
        }

        float targetVelocityX = input.x * moveSpeed;
        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below)?accelerationTimeGrounded:accelerationTimeAirborne);
        if (controller.collisions.left || controller.collisions.right)
        {
            velocity.x = 0;
        }
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
        //Physics.SyncTransforms();
    }
}
