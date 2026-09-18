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

    public GameObject deathScreen;

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
        if (GlobalPlayerVars.PlayerHealth <= 0)
        {
            deathScreen.SetActive(true);
            return;
        }

        HandleMouseLook();
        HandleMovement();

        if (Input.GetKey(KeyCode.R) &&
            GlobalPlayerVars.BloodCount > 0f &&
            GlobalPlayerVars.PlayerHealth < 100f)
        {
            float amount = Mathf.Min(
                5f * Time.deltaTime,
                GlobalPlayerVars.BloodCount,
                100f - GlobalPlayerVars.PlayerHealth
            );

            GlobalPlayerVars.BloodCount -= amount;
            GlobalPlayerVars.PlayerHealth += amount;
        }

        if (Input.GetKey(KeyCode.F) &&
            GlobalPlayerVars.BloodCount > 0f &&
            GlobalPlayerVars.JetFuel < 100f)
        {
            float amount = Mathf.Min(
                5f * Time.deltaTime,
                GlobalPlayerVars.JetFuel,
                100f - GlobalPlayerVars.JetFuel
            );

            GlobalPlayerVars.BloodCount -= amount;
            GlobalPlayerVars.JetFuel += amount;
        }
    }

    void HandleMovement()
    {
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

        if (hasGround &&
            groundDistance >= maxGroundHeight &&
            verticalVelocity > 0f)
        {
            verticalVelocity = 0f;
        }

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
