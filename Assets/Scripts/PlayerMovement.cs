using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private CharacterController cc;

    private float speed = 9.0f;         // XZ movement speed
    private float rotationSpeed = 720f; // rotation sensitivity

    private float gravity = -9.81f;     // default gravity (this will change)
    private float yVelocity = 0f;       // current y Velocity
    private float yVelocityWhenGrounded = -4f;  // this ensures cc.isGrounded will work 

    private float jumpHeight = 3.0f;    // the height of our jump in units
    private float jumpTime = 0.5f;      // the time of our jump in seconds
    private float initialJumpVelocity;  // upward velocity for jumping (precalculated)

    private float jumpsAvailable = 0;
    private float jumpsMax = 2;

    [SerializeField] private Animator anim; // model's animator component

    [SerializeField] private GameObject model;          // a reference to the model (inside the Player gameObject)
    private float rotateToFaceMovementSpeed = 10f;       // the speed to rotate our model towards the movement vector

    [SerializeField] private Camera cam;                // a reference to the main camera
    private float rotateToFaceAwayFromCameraSpeed = 10f; // the speed to rotate our Player to align with the camera view.


    private void Start()
    {
        // calculate gravity & initial jump velocity required for our jump
        float timeToApex = jumpTime / 2.0f;
        gravity = (-2 * jumpHeight) / Mathf.Pow(timeToApex, 2);
        initialJumpVelocity = Mathf.Sqrt(jumpHeight * -2 * gravity);
    }

    void Update()
    {
        // determine XZ movement vector
        float horizInput = Input.GetAxis("Horizontal");
        float vertInput = Input.GetAxis("Vertical");
        Vector3 movement = new Vector3(horizInput, 0, vertInput);

        // ensure diagonal movement doesn't exceed horiz/vert movement speed
        movement = Vector3.ClampMagnitude(movement, 1.0f);

        // set animator's velocity parameter based on XZ movement
        anim.SetFloat("velocity", movement.magnitude);

        // convert from local to global coordinates
        movement = transform.TransformDirection(movement);
        

        // Rotate model to face movement direction (if movement exists)
        if (movement.magnitude > 0)
        {
            RotateModelToFaceMovement(movement);
            RotatePlayerToFaceAwayFromCamera();
        }

        movement *= speed;

        // calculate yVelocity and add it to the player's movement vector
        yVelocity += gravity * Time.deltaTime;

        // Check if we are falling for animation purposes
        if (yVelocity < -25)
        {
            anim.SetBool("isFalling", true);
        }

        // if we are on the ground and we were falling
        if( cc.isGrounded && yVelocity < 0.0)
        {
            yVelocity = yVelocityWhenGrounded;
            jumpsAvailable = jumpsMax;
            anim.SetBool("isFalling", false);
        }

        // give upward y Velocity if we jumped
        if(Input.GetButtonDown("Jump") && jumpsAvailable > 0)
        {
            anim.SetTrigger("jump"); // play jump animation
            yVelocity = initialJumpVelocity;
            jumpsAvailable--;
        }

        // tell animator if we're grounded
        anim.SetBool("isGrounded", cc.isGrounded);

        movement.y = yVelocity;

        movement *= Time.deltaTime; // make all movement processor independent

        // move the player  (using the character controller)
        cc.Move(movement);

        // rotate the player (commented out because now uses the function below
        //Vector3 rotation = Vector3.up * rotationSpeed * Time.deltaTime * Input.GetAxis("Mouse X");
        //transform.Rotate(rotation);

        // convert from local to global coordinates
        //movement = transform.TransformDirection(movement); // Can't do this when using camera with direction of movement

        
    }


    // Set the rotation of the model to match the direction of the movement vector
    private void RotateModelToFaceMovement(Vector3 moveDirection)
    {
        Debug.Log("here");
        // Determine the rotation needed to face the direction of movement(only XZ movement -ignore Y)
        Quaternion newRotation = Quaternion.LookRotation(new Vector3(moveDirection.x, 0f, moveDirection.z));

        // set the model's rotation
        //model.transform.rotation = newRotation;

        // replace the above line with this one to enable smoothing
        model.transform.rotation = Quaternion.Slerp(model.transform.rotation, newRotation, rotateToFaceMovementSpeed * Time.deltaTime);
    }

    // set the player's Y rotation (yaw) to be aligned with the camera's Y rotation
    private void RotatePlayerToFaceAwayFromCamera()
    {
        // isolate the camera's Y rotation
        Quaternion camRotation = Quaternion.Euler(0, cam.transform.rotation.eulerAngles.y, 0);

        // set the player's rotation
        //transform.rotation = camRotation;
        
        // replace the above line with this one to enable smoothing
        transform.rotation = Quaternion.Slerp(transform.rotation, camRotation, rotateToFaceAwayFromCameraSpeed * Time.deltaTime);
    }

}
