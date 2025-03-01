using UnityEngine;

public class StrafeMovement : MonoBehaviour
{
    [SerializeField] private float accel = 200f;
    [SerializeField] private float airAccel = 200f;
    [SerializeField] private float maxSpeed = 6.4f;
    [SerializeField] private float maxAirSpeed = 0.6f;
    [SerializeField] private float friction = 8f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private LayerMask groundLayers;
    [SerializeField] private GameObject camObj;

    [Header("Footstep Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] footstepSounds;
    [SerializeField] private float stepInterval = 0.5f;

    [Header("Landing Sound")]
    [SerializeField] private AudioClip landSound;

    [Header("Speed Effect")]
    [SerializeField] private ParticleSystem speedEffect; // Partikül sistemi referansı
    [SerializeField] private float speedThreshold = 20f; // Partikül tetikleme hızı

    private float lastJumpPress = -1f;
    private float jumpPressDuration = 0.1f;
    private bool onGround = false;
    private bool wasInAir = false;
    private float stepTimer = 0f;

    private void Update()
    {
        float speed = new Vector3(GetComponent<Rigidbody>().linearVelocity.x, 0f, GetComponent<Rigidbody>().linearVelocity.z).magnitude;
        print(speed);
        
        if (Input.GetButton("Jump"))
        {
            lastJumpPress = Time.time;
        }

        PlayFootstepSounds();
        PlayLandingSound();
        HandleSpeedEffect(speed); // Partikül efektini kontrol et
    }

    private void FixedUpdate()
    {
        Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        Vector3 playerVelocity = GetComponent<Rigidbody>().linearVelocity;
        playerVelocity = CalculateFriction(playerVelocity);
        playerVelocity += CalculateMovement(input, playerVelocity);
        GetComponent<Rigidbody>().linearVelocity = playerVelocity;
    }

    private void HandleSpeedEffect(float speed)
    {
        if (speed > speedThreshold)
        {
            if (!speedEffect.isPlaying)
                speedEffect.Play();
        }
        else
        {
            if (speedEffect.isPlaying)
                speedEffect.Stop();
        }
    }

    private Vector3 CalculateFriction(Vector3 currentVelocity)
    {
        onGround = CheckGround();
        float speed = currentVelocity.magnitude;

        if (!onGround || Input.GetButton("Jump") || speed == 0f)
            return currentVelocity;

        float drop = speed * friction * Time.deltaTime;
        return currentVelocity * (Mathf.Max(speed - drop, 0f) / speed);
    }

    private Vector3 CalculateMovement(Vector2 input, Vector3 velocity)
    {
        onGround = CheckGround();
        float curAccel = onGround ? accel : airAccel;
        float curMaxSpeed = onGround ? maxSpeed : maxAirSpeed;
        Vector3 camRotation = new Vector3(0f, camObj.transform.rotation.eulerAngles.y, 0f);
        Vector3 inputVelocity = Quaternion.Euler(camRotation) * new Vector3(input.x * curAccel, 0f, input.y * curAccel);
        Vector3 alignedInputVelocity = new Vector3(inputVelocity.x, 0f, inputVelocity.z) * Time.deltaTime;
        Vector3 currentVelocity = new Vector3(velocity.x, 0f, velocity.z);
        float max = Mathf.Max(0f, 1 - (currentVelocity.magnitude / curMaxSpeed));
        float velocityDot = Vector3.Dot(currentVelocity, alignedInputVelocity);
        Vector3 modifiedVelocity = alignedInputVelocity * max;
        Vector3 correctVelocity = Vector3.Lerp(alignedInputVelocity, modifiedVelocity, velocityDot);
        correctVelocity += GetJumpVelocity(velocity.y);
        return correctVelocity;
    }

    private Vector3 GetJumpVelocity(float yVelocity)
    {
        Vector3 jumpVelocity = Vector3.zero;

        if (Time.time < lastJumpPress + jumpPressDuration && yVelocity < jumpForce && CheckGround())
        {
            lastJumpPress = -1f;
            jumpVelocity = new Vector3(0f, jumpForce - yVelocity, 0f);
        }

        return jumpVelocity;
    }

    private bool CheckGround()
    {
        Ray ray = new Ray(transform.position, Vector3.down);
        return Physics.Raycast(ray, GetComponent<Collider>().bounds.extents.y + 0.1f, groundLayers);
    }

    private void PlayFootstepSounds()
    {
        onGround = CheckGround();
        if (!onGround) return;

        Vector3 horizontalVelocity = new Vector3(GetComponent<Rigidbody>().linearVelocity.x, 0f, GetComponent<Rigidbody>().linearVelocity.z);

        if (horizontalVelocity.magnitude > 1f)
        {
            stepTimer += Time.deltaTime;
            if (stepTimer >= stepInterval)
            {
                stepTimer = 0f;
                if (footstepSounds.Length > 0)
                {
                    int index = Random.Range(0, footstepSounds.Length);
                    audioSource.PlayOneShot(footstepSounds[index]);
                }
            }
        }
        else
        {
            stepTimer = 0f;
        }
    }

    private void PlayLandingSound()
    {
        onGround = CheckGround();

        if (onGround && wasInAir)
        {
            if (landSound != null)
            {
                audioSource.PlayOneShot(landSound);
            }
        }

        wasInAir = !onGround;
    }
}
