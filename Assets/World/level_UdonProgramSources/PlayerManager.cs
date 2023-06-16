
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
    public Collider col;

    public LayerMask groundLayers;
    public float baseSpeed = 5f, baseJump = 12f, baseGravity = 0.8f, sphereRadius = 1f, maxGroundedRise = 1f;
    public float maxSlope = 45f, baseAcceleration = 0.2f, maxAirSpeed = 10f, airStrafeForce = 2f;
    private float currentAcceleration = 0f;

    public VRCPlayerApi player;

    public float maxDelay = 0.2f, maxJumpDelay = 0.2f, maxCoyote = 0.2f, maxBuffer = 0.2f;
    private float delayTimer = 0f, jumpDelayTimer = 0f, coyoteTimer = 0f, bufferTimer = 0f;

    private bool grounded = false, onSlope = false, jumping = false;

    private Vector2 moveDir = new Vector2();
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

        debugText.text = Mathf.Floor(new Vector3(moveVelocity.x, 0, moveVelocity.z).magnitude).ToString() + " m/s";

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
        Vector3 targetVelocity = Vector3.zero;

        Debug.Log(currentAcceleration);

        if (moveDir.magnitude > 1)
        {
            moveDir.Normalize();
        }

        targetVelocity.x = (moveDir * baseSpeed).x;
        targetVelocity.z = (moveDir * baseSpeed).y;
        
        float playerRotation = Mathf.Deg2Rad * (player.GetRotation().eulerAngles.y);

        targetVelocity = new Vector3( // rotates movement towards player direction
            (Mathf.Cos(playerRotation) * targetVelocity.x) + (Mathf.Sin(playerRotation) * targetVelocity.z),
            moveVelocity.y,
            (-Mathf.Sin(playerRotation) * targetVelocity.x) + (Mathf.Cos(playerRotation) * targetVelocity.z)
        );

        if (grounded && jumpDelayTimer >= maxJumpDelay)
        {
            // Smoothly interpolate the player's velocity towards the target velocity
            moveVelocity.x = Mathf.Lerp(moveVelocity.x, targetVelocity.x, baseAcceleration * Time.deltaTime);
            moveVelocity.z = Mathf.Lerp(moveVelocity.z, targetVelocity.z, baseAcceleration * Time.deltaTime);
        }
        else
        {
            AirMovement(targetVelocity);
        }

        // vertical
        if (!grounded)
        {
            moveVelocity.y -= baseGravity;
        }
        else if (jumpDelayTimer >= maxJumpDelay && moveVelocity.y < 0f) // if on ground and jump safety delay is past due and moving down
        {
            moveVelocity.y = 0f;
        }

        // apply
        if (onSlope && jumpDelayTimer >= maxJumpDelay)
        {
            player.SetVelocity(Vector3.ProjectOnPlane(moveVelocity, hitInfo.normal));
        }
        else
        {
            player.SetVelocity(moveVelocity);
        }
    }

    void AirMovement(Vector3 vector3)
    {
        Vector3 horVelocity = new Vector3(moveVelocity.x, 0, moveVelocity.z);
        // project the velocity onto the movevector
        Vector3 projVel = Vector3.Project(horVelocity, vector3);

        // check if the movevector is moving towards or away from the projected velocity
        bool isAway = Vector3.Dot(vector3, projVel) <= 0f;

        // only apply force if moving away from velocity or velocity is below MaxAirSpeed
        if (projVel.magnitude < maxAirSpeed || isAway)
        {
            // calculate the ideal movement force
            Vector3 vc = vector3.normalized * airStrafeForce;

            // cap it if it would accelerate beyond MaxAirSpeed directly.
            if (!isAway)
            {
                vc = Vector3.ClampMagnitude(vc, maxAirSpeed - projVel.magnitude);
            }
            else
            {
                vc = Vector3.ClampMagnitude(vc, maxAirSpeed + projVel.magnitude);
            }

            // Apply the force
            moveVelocity.z += vc.z;
            moveVelocity.x += vc.x;
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

    public void Explode(Vector3 position, float distance, float strength, float radius)
    {
        Vector3 offset = new Vector3(0, 0, 0);
        if (distance > radius) {return;}
        Vector3 direction = (col.transform.position - position).normalized;
        moveVelocity += Mathf.InverseLerp(radius, 0f, distance) * strength * direction;
        jumpDelayTimer = 0f;
    }

    public override void InputJump(bool boolValue, UdonInputEventArgs args)
    {
        jumping = boolValue;

        if (jumping)
        {
            bufferTimer = 0f;
        }
    }

    public override void InputMoveHorizontal(float floatValue, UdonInputEventArgs args)
    {
        moveDir.x = floatValue;
    }

    public override void InputMoveVertical(float floatValue, UdonInputEventArgs args)
    {
        moveDir.y = floatValue;
    }
}
