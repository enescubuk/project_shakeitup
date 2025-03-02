using System.Collections;
using System.Collections.Generic;
using UnityEngine;

struct Cmd
{
    public float forwardMove;
    public float rightMove;
    public float upMove;
}

public class MovementController : MonoBehaviour
{
    public Transform playerView;
    public float playerViewYOffset = 0.6f;
    public float xMouseSensitivity = 30.0f;
    public float yMouseSensitivity = 30.0f;

    public float gravity = 20.0f;
    public float friction = 6;

    public float moveSpeed = 7.0f;
    public float runAcceleration = 14.0f;
    public float runDeacceleration = 10.0f;
    public float airAcceleration = 2.0f;
    public float airDecceleration = 2.0f;
    public float airControl = 0.3f;
    public float sideStrafeAcceleration = 50.0f;
    public float sideStrafeSpeed = 1.0f;
    public float jumpSpeed = 8.0f;
    public bool holdJumpToBhop = false;

    private CharacterController _controller;
    private float rotX = 0.0f;
    private float rotY = 0.0f;
    private Vector3 moveDirectionNorm = Vector3.zero;
    private Vector3 playerVelocity = Vector3.zero;
    private float playerTopVelocity = 0.0f;

    private bool wishJump = false;
    private Cmd _cmd;

    private int streakCounter = 0;
    private bool canIncreaseStreak = false;

    public float GetCurrentSpeed()
    {
        Vector3 ups = _controller.velocity;
        ups.y = 0;
        return Mathf.Round(ups.magnitude * 100) / 100;
    }

    private void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        _controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        rotX -= Input.GetAxisRaw("Mouse Y") * xMouseSensitivity * 0.02f;
        rotY += Input.GetAxisRaw("Mouse X") * yMouseSensitivity * 0.02f;

        rotX = Mathf.Clamp(rotX, -90, 90);

        this.transform.rotation = Quaternion.Euler(0, rotY, 0);

        QueueJump();
        if (_controller.isGrounded)
            GroundMove();
        else
            AirMove();

        _controller.Move(playerVelocity * Time.deltaTime);
    }

    private void QueueJump()
    {
        if (holdJumpToBhop)
        {
            wishJump = Input.GetButton("Jump") || Input.GetAxis("Mouse ScrollWheel") < 0;
            return;
        }

        if ((Input.GetButtonDown("Jump") || Input.GetAxis("Mouse ScrollWheel") < 0) && !wishJump)
        {
            if (canIncreaseStreak) 
            {
                streakCounter++;
                Debug.Log("Bhop Streak: " + streakCounter);
                canIncreaseStreak = false;
            }
            else
            {
                streakCounter = 0;
                Debug.Log("Bhop Streak sıfırlandı!");
            }

            wishJump = true;
        }

        if (Input.GetButtonUp("Jump"))
            wishJump = false;
    }

    private void SetMovementDir()
    {
        _cmd.forwardMove = Input.GetAxisRaw("Vertical");
        _cmd.rightMove = Input.GetAxisRaw("Horizontal");
    }

    private void GroundMove()
    {
        if (!wishJump)
            ApplyFriction(1.0f);
        else
            ApplyFriction(0);

        SetMovementDir();

        Vector3 wishdir = new Vector3(_cmd.rightMove, 0, _cmd.forwardMove);
        wishdir = transform.TransformDirection(wishdir);
        wishdir.Normalize();
        moveDirectionNorm = wishdir;

        float wishspeed = wishdir.magnitude * moveSpeed;
        Accelerate(wishdir, wishspeed, runAcceleration);

        playerVelocity.y = -gravity * Time.deltaTime;

        if (wishJump)
        {
            playerVelocity.y = jumpSpeed;
            wishJump = false;
        }

        canIncreaseStreak = true;
    }

    private void AirMove()
    {
        Vector3 wishdir;
        float accel;

        SetMovementDir();

        wishdir = new Vector3(_cmd.rightMove, 0, _cmd.forwardMove);
        wishdir = transform.TransformDirection(wishdir);
        wishdir.Normalize();
        moveDirectionNorm = wishdir;

        float wishspeed = wishdir.magnitude * moveSpeed;

        if (Vector3.Dot(playerVelocity, wishdir) < 0)
            accel = airDecceleration;
        else
            accel = airAcceleration;

        Accelerate(wishdir, wishspeed, accel);

        if (airControl > 0)
            AirControl(wishdir, wishspeed);

        playerVelocity.y -= gravity * Time.deltaTime;
    }

    private void AirControl(Vector3 wishdir, float wishspeed)
    {
        if (Mathf.Abs(_cmd.forwardMove) < 0.001 || Mathf.Abs(wishspeed) < 0.001)
            return;

        float zspeed = playerVelocity.y;
        playerVelocity.y = 0;

        float speed = playerVelocity.magnitude;
        playerVelocity.Normalize();

        float dot = Vector3.Dot(playerVelocity, wishdir);
        float k = 32 * airControl * dot * dot * Time.deltaTime;

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

    private void Accelerate(Vector3 wishdir, float wishspeed, float accel)
    {
        float currentspeed = Vector3.Dot(playerVelocity, wishdir);
        float addspeed = wishspeed - currentspeed;
        if (addspeed <= 0)
            return;

        float accelspeed = accel * Time.deltaTime * wishspeed;
        if (accelspeed > addspeed)
            accelspeed = addspeed;

        playerVelocity.x += accelspeed * wishdir.x;
        playerVelocity.z += accelspeed * wishdir.z;
    }

    private void ApplyFriction(float t)
    {
        Vector3 vec = playerVelocity;
        vec.y = 0;
        float speed = vec.magnitude;
        float drop = 0.0f;

        if (_controller.isGrounded)
        {
            float control = speed < runDeacceleration ? runDeacceleration : speed;
            drop = control * friction * Time.deltaTime * t;
        }

        float newspeed = speed - drop;
        if (newspeed < 0)
            newspeed = 0;
        if (speed > 0)
            newspeed /= speed;

        playerVelocity.x *= newspeed;
        playerVelocity.z *= newspeed;
    }
}
