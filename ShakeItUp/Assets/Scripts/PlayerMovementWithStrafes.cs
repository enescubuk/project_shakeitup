using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementWithStrafes : MonoBehaviour
{
    public CharacterController controller;
    public Transform GroundCheck;
    public LayerMask GroundMask;

    private float wishspeed2;
    private float gravity = -20f;
    float wishspeed;

    public float GroundDistance = 0.4f;
    public float moveSpeed = 10.0f;  // Hızlandırıldı
    public float runAcceleration = 20f;   // Hızlandırıldı
    public float runDeacceleration = 15f;   // Hızlandırıldı
    public float airAcceleration = 4.0f;  // Hızlandırıldı
    public float airDeacceleration = 2.0f;    // Hızlandırıldı
    public float airControl = 0.6f;  // Hızlandırıldı
    public float sideStrafeAcceleration = 80f;   // Hızlandırıldı
    public float sideStrafeSpeed = 2f;    // Hızlandırıldı
    public float jumpSpeed = 12.0f; // Yüksek zıplama
    public float friction = 5f; // Yavaşlatma azaltıldı
    private float playerTopVelocity = 0;
    public float playerFriction = 0f;
    float addspeed;
    float accelspeed;
    float currentspeed;
    float zspeed;
    float speed;
    float dot;
    float k;
    float accel;
    float newspeed;
    float control;
    float drop;

    public bool JumpQueue = false;
    public bool wishJump = false;

    //UI
    private Vector3 lastPos;
    private Vector3 moved;
    public Vector3 PlayerVel;
    public float ModulasSpeed;
    public float ZVelocity;
    public float XVelocity;
    //End UI

    public Vector3 moveDirection;
    public Vector3 moveDirectionNorm;
    private Vector3 playerVelocity;
    Vector3 wishdir;
    Vector3 vec;

    public Transform playerView;

    public float x;
    public float z;

    public bool IsGrounded;

    public Transform player;
    Vector3 udp;


    private void Start()
    {
        lastPos = player.position;
    }

    void Update()
    {
        #region //UI, Feel free to remove the region.

        moved = player.position - lastPos;
        lastPos = player.position;
        PlayerVel = moved / Time.fixedDeltaTime;

        ZVelocity = Mathf.Abs(PlayerVel.z);
        XVelocity = Mathf.Abs(PlayerVel.x);

        ModulasSpeed = Mathf.Sqrt(PlayerVel.z * PlayerVel.z + PlayerVel.x * PlayerVel.x);

        #endregion

        IsGrounded = Physics.CheckSphere(GroundCheck.position, GroundDistance, GroundMask);

        QueueJump();

        /* Movement, here's the important part */
        if (controller.isGrounded)
            GroundMove();
        else if (!controller.isGrounded)
            AirMove();

        // Move the controller
        controller.Move(playerVelocity * Time.deltaTime);

        // Calculate top velocity
        udp = playerVelocity;
        udp.y = 0;
        if (udp.magnitude > playerTopVelocity)
            playerTopVelocity = udp.magnitude;
    }

    public void SetMovementDir()
    {
        x = Input.GetAxis("Horizontal");
        z = Input.GetAxis("Vertical");
    }

    void QueueJump()
    {
        if (Input.GetButtonDown("Jump") && IsGrounded)
        {
            wishJump = true;
        }

        if (!IsGrounded && Input.GetButtonDown("Jump"))
        {
            JumpQueue = true;
        }
        if (IsGrounded && JumpQueue)
        {
            wishJump = true;
            JumpQueue = false;
        }
    }

    public void Accelerate(Vector3 wishdir, float wishspeed, float accel)
    {
        currentspeed = Vector3.Dot(playerVelocity, wishdir);
        addspeed = wishspeed - currentspeed;
        if (addspeed <= 0)
            return;
        accelspeed = accel * Time.deltaTime * wishspeed;
        if (accelspeed > addspeed)
            accelspeed = addspeed;

        playerVelocity.x += accelspeed * wishdir.x;
        playerVelocity.z += accelspeed * wishdir.z;
    }

    public void AirMove()
    {
        SetMovementDir();

        wishdir = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        wishdir = transform.TransformDirection(wishdir);

        wishspeed = wishdir.magnitude;

        wishspeed *= 7f;

        wishdir.Normalize();
        moveDirectionNorm = wishdir;

        // Aircontrol
        wishspeed2 = wishspeed;
        if (Vector3.Dot(playerVelocity, wishdir) < 0)
            accel = airDeacceleration;
        else
            accel = airAcceleration;

        // If the player is ONLY strafing left or right
        if (Input.GetAxis("Horizontal") == 0 && Input.GetAxis("Vertical") != 0)
        {
            if (wishspeed > sideStrafeSpeed)
                wishspeed = sideStrafeSpeed;
            accel = sideStrafeAcceleration;
        }

        Accelerate(wishdir, wishspeed, accel);

        AirControl(wishdir, wishspeed2);

        // Apply gravity
        playerVelocity.y += gravity * Time.deltaTime;

        void AirControl(Vector3 wishdir, float wishspeed)
        {
            if (Input.GetAxis("Horizontal") == 0 || wishspeed == 0)
                return;

            zspeed = playerVelocity.y;
            playerVelocity.y = 0;
            speed = playerVelocity.magnitude;
            playerVelocity.Normalize();

            dot = Vector3.Dot(playerVelocity, wishdir);
            k = 32;
            k *= airControl * dot * dot * Time.deltaTime;

            if (dot > 0)
            {
                playerVelocity.x = playerVelocity.x * speed + wishdir.x * k;
                playerVelocity.y = playerVelocity.y * speed + wishdir.y * k;
                playerVelocity.z = playerVelocity.z * speed + wishdir.z * k;

                playerVelocity.Normalize();
                moveDirectionNorm = playerVelocity;
            }

            playerVelocity.x *= speed;
            playerVelocity.y = zspeed;
            playerVelocity.z *= speed;
        }
    }

    public void GroundMove()
    {
        if (!wishJump)
            ApplyFriction(1.0f);
        else
            ApplyFriction(0);

        SetMovementDir();

        wishdir = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        wishdir = transform.TransformDirection(wishdir);
        wishdir.Normalize();
        moveDirectionNorm = wishdir;

        wishspeed = wishdir.magnitude;
        wishspeed *= moveSpeed;

        Accelerate(wishdir, wishspeed, runAcceleration);

        playerVelocity.y = 0;

        if (wishJump)
        {
            playerVelocity.y = jumpSpeed;
            wishJump = false;
        }

        void ApplyFriction(float t)
        {
            vec = playerVelocity;
            vec.y = 0f;
            speed = vec.magnitude;
            drop = 0f;

            if (controller.isGrounded)
            {
                control = speed < runDeacceleration ? runDeacceleration : speed;
                drop = control * friction * Time.deltaTime * t;
            }

            newspeed = speed - drop;
            playerFriction = newspeed;
            if (newspeed < 0)
                newspeed = 0;
            if (speed > 0)
                newspeed /= speed;

            playerVelocity.x *= newspeed;
            playerVelocity.z *= newspeed;
        }
    }
}
