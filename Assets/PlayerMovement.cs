using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerCamera;
    [SerializeField] private CharacterController characterController;

    [Header("Movement")]
    public float movementSpeed = 5f;
    public int runSpeed = 10;

    [Header("Camera Shake")]
    [SerializeField] private float walkShakeAmount = 0.03f;
    [SerializeField] private float walkShakeSpeed = 8f;

    [SerializeField] private float runShakeAmount = 0.06f;
    [SerializeField] private float runShakeSpeed = 12f;

    [SerializeField] private float walkRotationAmount = 2f;
    [SerializeField] private float runRotationAmount = 4f;

    [SerializeField] private float shakeSmoothness = 10f;

    private float shakeTimer = 0f;
    private Vector3 originalCameraPosition;

    [Header("CameraSensitivity")]
    public float horizontalCameraSpeed = 200f;
    public float verticalCameraSpeed = 200f;

    [Header("Gravity")]
    public float gravity = -20f;

    [Header("Camera Limits")]
    [SerializeField] private float minVerticalAngle = -90f;
    [SerializeField] private float maxVerticalAngle = 90f;

    private float verticalRotation;
    private float verticalVelocity;

    [SerializeField] float verticalstick = -5;
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        originalCameraPosition = playerCamera.localPosition;
    }
    private void FixedUpdate()
    {
        Move();
        Look();
        CameraShake();
    }

    private void Move()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 inputDirection =
            transform.right * horizontal +
            transform.forward * vertical;

        inputDirection = Vector3.ClampMagnitude(inputDirection, 1f);

        float currentSpeed = Input.GetKey(KeyCode.LeftShift)
            ? runSpeed
            : movementSpeed;

        Vector3 movement = inputDirection * currentSpeed;

        // Detect the ground.
        if (Physics.SphereCast(
            transform.position,
            characterController.radius * 0.9f,
            Vector3.down,
            out RaycastHit hit,
            characterController.height / 2f + 0.5f))
        {
            // Make movement follow the slope.
           

            // Very small downward force to stay attached.
            verticalVelocity = verticalstick;
        }
        else
        {
            // Actual airborne gravity.
            verticalVelocity += gravity * Time.deltaTime;
        }

        movement.y = verticalVelocity;

        characterController.Move(
            movement * Time.deltaTime
        );
    }

    private void CameraShake()
    {
        Vector3 horizontalVelocity = new Vector3(
            characterController.velocity.x,
            0f,
            characterController.velocity.z
        );

        bool isMoving = horizontalVelocity.magnitude > 0.1f;

        if (characterController.isGrounded && isMoving)
        {
            bool isRunning = Input.GetKey(KeyCode.LeftShift);

            float shakeAmount = isRunning
                ? runShakeAmount
                : walkShakeAmount;

            float shakeSpeed = isRunning
                ? runShakeSpeed
                : walkShakeSpeed;

            float rotationAmount = isRunning
                ? runRotationAmount
                : walkRotationAmount;

            shakeTimer += shakeSpeed * Time.deltaTime;

            // Up/down movement
            float verticalShake =
                Mathf.Sin(shakeTimer) * shakeAmount;

            // Left/right movement
            float horizontalShake =
                Mathf.Cos(shakeTimer * 0.5f) * shakeAmount;

            // Left/right camera rotation
            float rotationShake =
                Mathf.Sin(shakeTimer * 0.5f) * rotationAmount;

            Vector3 targetPosition = originalCameraPosition;

            targetPosition.y += verticalShake;
            targetPosition.x += horizontalShake;

            playerCamera.localPosition = Vector3.Lerp(
                playerCamera.localPosition,
                targetPosition,
                shakeSmoothness * Time.deltaTime
            );

            // Add left/right tilt
            Quaternion targetRotation = Quaternion.Euler(
                verticalRotation,
                0f,
                rotationShake
            );

            playerCamera.localRotation = Quaternion.Slerp(
                playerCamera.localRotation,
                targetRotation,
                shakeSmoothness * Time.deltaTime
            );
        }
        else
        {
            shakeTimer = 0f;

            playerCamera.localPosition = Vector3.Lerp(
                playerCamera.localPosition,
                originalCameraPosition,
                shakeSmoothness * Time.deltaTime
            );

            // Return the camera to its normal rotation.
            Quaternion targetRotation = Quaternion.Euler(
                verticalRotation,
                0f,
                0f
            );

            playerCamera.localRotation = Quaternion.Slerp(
                playerCamera.localRotation,
                targetRotation,
                shakeSmoothness * Time.deltaTime
            );
        }
    }

    private void Look()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        transform.Rotate(
            Vector3.up,
            mouseX * horizontalCameraSpeed * Time.deltaTime
        );

        verticalRotation -=
            mouseY * verticalCameraSpeed * Time.deltaTime;

        verticalRotation = Mathf.Clamp(
            verticalRotation,
            minVerticalAngle,
            maxVerticalAngle
        );

        playerCamera.localRotation =
            Quaternion.Euler(verticalRotation, 0f, 0f);
    }
}

