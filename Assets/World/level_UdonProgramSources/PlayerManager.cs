
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;

public class PlayerManager : UdonSharpBehaviour
{
    public Transform spherePosition;

    public LayerMask groundLayers;
    public float baseWalk = 5f, baseJump = 12f, baseGravity = 0.8f, flySpeed = 15f, sphereRadius = 1f, maxGroundedRise = 1f, maxSlope = 45f;

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
        player.SetWalkSpeed(baseWalk);
        //player.SetRunSpeed(baseRun);
        //player.SetStrafeSpeed(baseStrafe);
        player.SetJumpImpulse(baseJump);
        player.SetGravityStrength(baseGravity);
    }

    void Update()
    {
        if (jumpDelayTimer < maxJumpDelay)
        {
            jumpDelayTimer += Time.deltaTime;
        }
    }

    void FixedUpdate()
    {
        groundCheck();
        Jump();
        ApplyMovement();
    }

    void ApplyMovement()
    {
        // horizontal
        moveDir = moveDir.normalized;
        moveVelocity.x = (moveDir * baseWalk).x;
        moveVelocity.z = (moveDir * baseWalk).z;
        
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
        else if (jumpDelayTimer >= maxJumpDelay) // if on ground and jump safety delay is past due
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
        if (jumping && grounded)
        {
            moveVelocity.y = baseJump;
            jumpDelayTimer = 0f;
        }
    }

    void groundCheck()
    {
        bool test = Physics.SphereCast(spherePosition.position, sphereRadius, Vector3.down, out hitInfo, sphereRadius, groundLayers);

        float angle = Vector3.Angle(transform.up, hitInfo.normal);

        if (test)
        { // grounded if not moving up too fast, touching ground, and slope not too high
            if (angle <= maxSlope)
            {
                if (moveVelocity.y <= maxGroundedRise)
                {
                    grounded = false;
                }
            }
        }
        Debug.Log(angle);

        onSlope = hitInfo.normal != Vector3.up;
    }

    public override void InputJump(bool boolValue, UdonInputEventArgs args)
    {
        jumping = boolValue;

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

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(spherePosition.position, sphereRadius);
    }
}
