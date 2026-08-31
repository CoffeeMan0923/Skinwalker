using UnityEngine;

public class Carcontroller : MonoBehaviour
{
    [Header("Car")]
    public float acceleration = 10f;
    public float maxSpeed = 20f;
    public float turnSpeed = 80f;

    [Header("References")]
    [SerializeField] private Rigidbody carRigidbody;

    private void FixedUpdate()
    {
        Move();
        Turn();
    }

    private void Move()
    {
        float input = Input.GetAxisRaw("Vertical");

        // Forward/backward on the car's local Z axis.
        Vector3 movement = transform.forward * input;

        carRigidbody.AddForce(
            movement * acceleration,
            ForceMode.Acceleration
        );

        // Get the car's velocity relative to its own orientation.
        Vector3 localVelocity =
            transform.InverseTransformDirection(carRigidbody.linearVelocity);

        // Limit forward/backward speed.
        localVelocity.z = Mathf.Clamp(
            localVelocity.z,
            -maxSpeed,
            maxSpeed
        );

        // Convert the velocity back into world space.
        carRigidbody.linearVelocity =
            transform.TransformDirection(localVelocity);
    }

    private void Turn()
    {
        float steering = Input.GetAxisRaw("Horizontal");

        // Don't turn while completely stopped.
        if (Mathf.Abs(carRigidbody.linearVelocity.magnitude) >= 0f)
        {
            float direction = Mathf.Sign(
                Vector3.Dot(carRigidbody.linearVelocity, transform.forward)
            );

            float turnAmount =
                steering *
                turnSpeed *
                direction *
                Time.fixedDeltaTime;

            Quaternion rotation = Quaternion.Euler(
                0f,
                turnAmount,
                0f
            );

            carRigidbody.MoveRotation(
                carRigidbody.rotation * rotation
            );
        }
    }
}
