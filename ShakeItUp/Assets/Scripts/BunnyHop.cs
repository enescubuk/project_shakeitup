using UnityEngine;

public class BunnyHop : MonoBehaviour
{
    public float moveSpeed = 7f;
    public float jumpForce = 8f;
    public float airControlMultiplier = 0.8f;
    public float maxSpeed = 20f; 
    public float speedIncreasePerJump = 1.5f;
    public float sensitivity = 2f;

    private Rigidbody rb;
    private bool isGrounded;
    private bool jumpQueued;
    private float currentSpeed;
    private Transform playerBody;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        currentSpeed = moveSpeed;
        playerBody = transform;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        HandleMouseLook();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpQueued = true;
        }
    }

    void FixedUpdate()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.1f);

        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 moveDir = playerBody.forward * moveZ + playerBody.right * moveX;
        moveDir.Normalize();

        if (isGrounded)
        {
            if (jumpQueued)
            {
                // **Zıplayınca hız artırılıyor ve korunuyor**
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
                currentSpeed = Mathf.Min(currentSpeed + speedIncreasePerJump, maxSpeed);
                jumpQueued = false;
            }
            
            // **Havadan zemine inince hız korunuyor**
            Vector3 flatVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            if (flatVelocity.magnitude < currentSpeed)
            {
                flatVelocity = moveDir * currentSpeed;
            }
            rb.linearVelocity = new Vector3(flatVelocity.x, rb.linearVelocity.y, flatVelocity.z);
        }
        else
        {
            // **Havada hız korunuyor ve air-strafe ekleniyor**
            Vector3 airMove = moveDir * currentSpeed * airControlMultiplier;
            rb.linearVelocity += new Vector3(airMove.x * Time.fixedDeltaTime, 0, airMove.z * Time.fixedDeltaTime);

            // **Maksimum hız aşılmasını engelle**
            Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            if (horizontalVelocity.magnitude > maxSpeed)
            {
                horizontalVelocity = horizontalVelocity.normalized * maxSpeed;
                rb.linearVelocity = new Vector3(horizontalVelocity.x, rb.linearVelocity.y, horizontalVelocity.z);
            }
        }
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensitivity;
        playerBody.Rotate(Vector3.up * mouseX);
    }
}
