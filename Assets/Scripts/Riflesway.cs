using System.Collections;
using UnityEngine;

public class Riflesway : MonoBehaviour
{
    private Transform riflePlaceholder;
    [SerializeField] private Transform noAimPlacenholder;
    [SerializeField] private Transform AimingPlaceholder;

    [Header("Position Sway")]
    [SerializeField] private float followSpeed = 12f;
    [SerializeField] private float maxDistance = 0.15f;

    [Header("Rotation Sway")]
    [SerializeField] private float rotationFollowSpeed = 12f;
    [SerializeField] private float maxRotationAngle = 5f;

    private PlayerMovement playerMovement;

    void Start()
    {
        playerMovement = Object.FindAnyObjectByType<PlayerMovement>();
        riflePlaceholder = noAimPlacenholder;
    }
    void FixedUpdate()
    {
        if (riflePlaceholder == null)
            return;

        Vector3 targetPosition = riflePlaceholder.position;

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            followSpeed * Time.fixedDeltaTime
        );

        Vector3 offset = transform.position - targetPosition;

        if (offset.magnitude > maxDistance)
        {
            transform.position =
                targetPosition + offset.normalized * maxDistance;
        }

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            riflePlaceholder.rotation,
            rotationFollowSpeed * Time.fixedDeltaTime
        );

        Quaternion rotationDifference =
            Quaternion.Inverse(riflePlaceholder.rotation) * transform.rotation;

        rotationDifference.ToAngleAxis(
            out float angle,
            out Vector3 axis
        );

        if (angle > 180f)
            angle -= 360f;

        if (Mathf.Abs(angle) > maxRotationAngle)
        {
            angle = Mathf.Clamp(angle, -maxRotationAngle, maxRotationAngle);

            transform.rotation =
                riflePlaceholder.rotation *
                Quaternion.AngleAxis(angle, axis);
        }

    }
    public void IsAiming(bool aim)
    {
        if (aim == false)
        {
            riflePlaceholder = noAimPlacenholder;
            playerMovement.CanRun = true;
        }
        else
        {
            riflePlaceholder = AimingPlaceholder;
            playerMovement.CanRun = false;
        }
    }
}

