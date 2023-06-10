
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;

public class PlayerManager : UdonSharpBehaviour
{
    public Transform spherePosition;

    public LayerMask groundLayers;
    public float baseWalk = 5f, baseJump = 12f, baseGravity = 0.8f, flySpeed = 15f, sphereRadius = 1f;

    public VRCPlayerApi player;

    public float maxFly = 3f, maxDelay = 0.2f;

    private float flyTimer = 0f, delayTimer = 0f;
    private bool flying = false;
    private bool canFly = false;
    private bool grounded = false;

    private Vector2 moveDir = new Vector2();
    private Vector3 moveVelocity = new Vector3();

    void Start()
    {
        flyTimer = maxFly;
        player = Networking.LocalPlayer;
        player.SetWalkSpeed(baseWalk);
        //player.SetRunSpeed(baseRun);
        //player.SetStrafeSpeed(baseStrafe);
        player.SetJumpImpulse(baseJump);
        player.SetGravityStrength(baseGravity);
    }

    void Update()
    {
        if (!flying && flyTimer < maxFly)
        {
            flyTimer += Time.deltaTime;
        }
        else if (flying && flyTimer > 0)
        {
            Fly();
        }

        Vector3 debugOffset = new Vector3(0, 2.5f, 0);
    }

    void FixedUpdate()
    {
        groundCheck();
        ApplyMovement();
    }

    void ApplyMovement()
    {
        // horizontal
        moveVelocity.x = (moveDir.normalized * baseWalk).x;
        moveVelocity.z = (moveDir.normalized * baseWalk).y;
        float playerRotation = Mathf.Deg2Rad * (player.GetRotation().eulerAngles.y);

        moveVelocity = new Vector3( // rotates movement towards player direction
            (Mathf.Cos(playerRotation) * moveVelocity.x) + (Mathf.Sin(playerRotation) * moveVelocity.z),
            0,
            (-Mathf.Sin(playerRotation) * moveVelocity.x) + (Mathf.Cos(playerRotation) * moveVelocity.z)
        );

        //vertical
        moveVelocity.y -= baseGravity;

        player.SetVelocity(moveVelocity);
    }

    void Jump()
    {
        moveVelocity.y = baseJump;
    }

    void groundCheck()
    {
        grounded = Physics.CheckSphere(spherePosition.position, sphereRadius, groundLayers);
    }

    void Fly()
    {
        //flyTimer -= Time.deltaTime * 2;
        //player.SetVelocity(new Vector3(0, flySpeed, 0));
    }

    public override void InputJump(bool boolValue, UdonInputEventArgs args)
    {
        if (boolValue && grounded)
        {
            Jump();
        }

        //flying = boolValue;
    }

    public override void InputMoveHorizontal(float floatValue, UdonInputEventArgs args)
    {
        moveDir.x = floatValue;
    }

    public override void InputMoveVertical(float floatValue, UdonInputEventArgs args)
    {
        moveDir.y = floatValue;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(spherePosition.position, sphereRadius);
    }
}
