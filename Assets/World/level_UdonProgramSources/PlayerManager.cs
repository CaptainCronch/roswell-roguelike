
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;
using UnityEngine.UI;

public class PlayerManager : UdonSharpBehaviour
{
    public Transform spherePosition;
    public Text debugText;

    public LayerMask groundLayers;
    public float baseSpeed = 5f, baseJump = 12f, baseGravity = 0.8f, flySpeed = 15f, sphereRadius = 1f, maxGroundedRise = 1f, maxSlope = 45f, baseAcceleration = 0.2f, airAcceleration = 0f;
    private float currentSpeed = 0f, currentAcceleration = 0f, maxSpeed;

    public VRCPlayerApi player;

    public float maxDelay = 0.2f, maxJumpDelay = 0.2f, maxCoyote = 0.2f, maxBuffer = 0.2f;
    private float delayTimer = 0f, jumpDelayTimer = 0f, coyoteTimer = 0f, bufferTimer = 0f;

    private bool grounded = false, onSlope = false, jumping = false;

    private Vector3 moveDir = new Vector3();
    private Vector3 moveVelocity = new Vector3();
    private RaycastHit hitInfo;

    void Start()
    {
        player = Networking.LocalPlayer;
        player.SetWalkSpeed(0);
        player.SetRunSpeed(0);
        player.SetStrafeSpeed(0);
        player.SetJumpImpulse(0);
        player.SetGravityStrength(0);
    }

    void Update()
    {
        if (jumpDelayTimer < maxJumpDelay)
        {
            jumpDelayTimer += Time.deltaTime;
        }
        if (coyoteTimer < maxCoyote)
        {
            coyoteTimer += Time.deltaTime;
        }
        if (bufferTimer < maxBuffer)
        {
            bufferTimer += Time.deltaTime;
        }

        debugText.text = "jumpDelayTimer: " + jumpDelayTimer.ToString();

        if (onSlope && jumpDelayTimer >= maxJumpDelay)
        {
            player.SetVelocity(Vector3.ProjectOnPlane(moveVelocity, hitInfo.normal));
        }
        else
        {
            player.SetVelocity(moveVelocity);
        }
    }

    void FixedUpdate()
    {
        debugText.text = "";
        groundCheck();
        Jump();
        ApplyMovement();
    }

    void ApplyMovement()
    {
        // horizontal
        // if (moveDir.magnitude > 1f)
        // {
        //     moveDir = moveDir.normalized;
        // }

        // if (grounded)
        // {
        //     currentAcceleration = baseAcceleration;
        // }
        // else
        // {
        //     currentAcceleration = airAcceleration;
        // }

        // maxSpeed = baseSpeed * moveDir.magnitude;

        // if (currentSpeed < maxSpeed)
        // {
        //     currentSpeed += currentAcceleration;
        // }
        // else if (currentSpeed > maxSpeed)
        // {
        //     currentSpeed -= currentAcceleration;
        // }
        float targetSpeed = moveDir.magnitude * baseSpeed;


        moveVelocity.x = (moveDir * baseSpeed).x;
        moveVelocity.z = (moveDir * baseSpeed).z;
        
        float playerRotation = Mathf.Deg2Rad * (player.GetRotation().eulerAngles.y);

        moveVelocity = new Vector3( // rotates movement towards player direction
            (Mathf.Cos(playerRotation) * moveVelocity.x) + (Mathf.Sin(playerRotation) * moveVelocity.z),
            moveVelocity.y,
            (-Mathf.Sin(playerRotation) * moveVelocity.x) + (Mathf.Cos(playerRotation) * moveVelocity.z)
        );

        //vertical
        if (!grounded)
        {
            moveVelocity.y -= baseGravity;
        }
        else if (jumpDelayTimer >= maxJumpDelay && moveVelocity.y < 0f) // if on ground and jump safety delay is past due and moving down
        {
            moveVelocity.y = 0f;
        }

        if (onSlope && jumpDelayTimer >= maxJumpDelay)
        {
            player.SetVelocity(Vector3.ProjectOnPlane(moveVelocity, hitInfo.normal));
        }
        else
        {
            player.SetVelocity(moveVelocity);
        }
    }

    void Jump()
    {
        if (bufferTimer <= maxBuffer && coyoteTimer <= maxCoyote)
        {
            moveVelocity.y = baseJump;
            jumpDelayTimer = 0f;
            bufferTimer = maxBuffer+1;
            coyoteTimer = maxCoyote+1;
        }
    }

    void groundCheck()
    {
        bool test = Physics.SphereCast(spherePosition.position, sphereRadius, Vector3.down, out hitInfo, sphereRadius, groundLayers);

        float angle = Vector3.Angle(transform.up, hitInfo.normal);

        if (test && (angle <= maxSlope) && (moveVelocity.y <= maxGroundedRise))
        { // grounded if not moving up too fast, touching ground, and slope not too high
            coyoteTimer = 0f;
            grounded = true;
        }
        else{
            grounded = false;
        }

        onSlope = hitInfo.normal != Vector3.up;
    }

    public override void InputJump(bool boolValue, UdonInputEventArgs args)
    {
        jumping = boolValue;

        if (jumping)
        {
            bufferTimer = 0f;
        }

        //flying = boolValue;
    }

    public override void InputMoveHorizontal(float floatValue, UdonInputEventArgs args)
    {
        moveDir.x = floatValue;
    }

    public override void InputMoveVertical(float floatValue, UdonInputEventArgs args)
    {
        moveDir.z = floatValue;
    }

    // void OnDrawGizmos()
    // {
    //     if (grounded)
    //     {
    //         Gizmos.color = Color.green;
    //         Debug.Log("green");
    //     }
    //     else
    //     {
    //         Gizmos.color = Color.red;
    //         Debug.Log("red");
    //     }
        
    //     Gizmos.DrawSphere(spherePosition.position - new Vector3(0, sphereRadius, 0), sphereRadius);
    // }
}
