using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;
    public float jumpHeight = 2f;
    public float gravity = -20f;
    public float groundedForce = -2f;

    [Header("Mouse Look")]
    public Camera playerCamera;
    public float mouseSensitivity = 2f;
    public float maxLookAngle = 89f;

    [Header("Jetpack")]
    public float jetFuelUseRate = 15f;
    public float jetpackAcceleration = 25f;
    public float jetpackMaxUpwardSpeed = 8f;

    [Header("Forward Jet")]
    public float forwardBoostForce = 10f;

    private CharacterController controller;

    private float verticalVelocity;
    private float cameraPitch;
    [Header("Ground Height Limit")]
    public LayerMask groundLayer;
    public float maxGroundHeight = 10f;
    public float groundRaycastDistance = 100f;
    private Vector3 velocity;


    void Start()
    {
        controller = GetComponent<CharacterController>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleMouseLook();
        HandleMovement();
    }

    void HandleMovement()
{
    // -------------------------
    // Horizontal movement
    // -------------------------

    
    float horizontal = Input.GetAxisRaw("Horizontal");
    float vertical = Input.GetAxisRaw("Vertical");

    Vector3 move =
        transform.right * horizontal +
        transform.forward * vertical;

    move.Normalize();

    if (GlobalPlayerVars.ArmState != 'B')
    {
    velocity = move * moveSpeed;
    }
    else
    {
    velocity = move * (moveSpeed / 2);
    }


    // -------------------------
    // Ground height check
    // -------------------------

    RaycastHit groundHit;
    bool hasGround =
        Physics.Raycast(
            transform.position,
            Vector3.down,
            out groundHit,
            groundRaycastDistance,
            groundLayer
        );

    float groundDistance = Mathf.Infinity;

    if (hasGround)
    {
        groundDistance = groundHit.distance;
    }


    // -------------------------
    // Grounding
    // -------------------------

    if (controller.isGrounded)
    {
        if (verticalVelocity < 0f)
        {
            verticalVelocity = groundedForce;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            verticalVelocity =
                Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }


    // -------------------------
    // Gravity + Jetpack
    // -------------------------

    if (!controller.isGrounded)
    {
        verticalVelocity += gravity * Time.deltaTime;

        if (Input.GetKey(KeyCode.Space) &&
            GlobalPlayerVars.JetFuel > 0f)
        {
            verticalVelocity +=
                jetpackAcceleration * Time.deltaTime;

            verticalVelocity = Mathf.Min(
                verticalVelocity,
                jetpackMaxUpwardSpeed
            );

            GlobalPlayerVars.JetFuel -=
                jetFuelUseRate * Time.deltaTime;

            GlobalPlayerVars.JetFuel =
                Mathf.Max(GlobalPlayerVars.JetFuel, 0f);
        }
    }


    // -------------------------
    // Maximum height
    // -------------------------

    if (hasGround &&
        groundDistance >= maxGroundHeight &&
        verticalVelocity > 0f)
    {
        verticalVelocity = 0f;
    }


    // -------------------------
    // Forward jet
    // -------------------------

    if (Input.GetKey(KeyCode.LeftShift) &&
        GlobalPlayerVars.JetFuel > 0f)
    {
        velocity += transform.forward * forwardBoostForce;

        GlobalPlayerVars.JetFuel -=
            jetFuelUseRate * Time.deltaTime;

        GlobalPlayerVars.JetFuel =
            Mathf.Max(GlobalPlayerVars.JetFuel, 0f);

        GlobalPlayerVars.ArmState = 'Z';
    }


    // -------------------------
    // Apply movement once
    // -------------------------

    velocity.y = verticalVelocity;

    controller.Move(velocity * Time.deltaTime);
}


    void HandleMouseLook()
    {
        float mouseX =
            Input.GetAxis("Mouse X") * mouseSensitivity;

        float mouseY =
            Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        cameraPitch -= mouseY;

        cameraPitch = Mathf.Clamp(
            cameraPitch,
            -maxLookAngle,
            maxLookAngle
        );

        playerCamera.transform.localRotation =
            Quaternion.Euler(
                cameraPitch,
                0f,
                0f
            );
    }
}
