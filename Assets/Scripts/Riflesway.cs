
using UnityEngine;

public class Riflesway : MonoBehaviour
{
    [SerializeField] private Transform noAimPlacenholder;
    [SerializeField] private Transform AimingPlaceholder;

    [Header("Aim Transition")]
    [SerializeField] private float aimTransitionSpeed = 5f;

    [Header("Position Sway")]
    [SerializeField] private float followSpeed = 5f;
    [SerializeField] private float maxDistance = 0.15f;

    [Header("Rotation Sway")]
    [SerializeField] private float rotationFollowSpeed = 180f;
    [SerializeField] private float maxRotationAngle = 5f;

    private PlayerMovement playerMovement;

    private bool isAiming;
    private float aimBlend;

    void Start()
    {
        playerMovement = Object.FindAnyObjectByType<PlayerMovement>();

        aimBlend = 0f;

        transform.position = noAimPlacenholder.position;
        transform.rotation = noAimPlacenholder.rotation;
    }

    void FixedUpdate()
    {
        if (noAimPlacenholder == null || AimingPlaceholder == null)
            return;

        float targetAimBlend = isAiming ? 1f : 0f;

        aimBlend = Mathf.MoveTowards(
            aimBlend,
            targetAimBlend,
            aimTransitionSpeed * Time.fixedDeltaTime
        );

        Vector3 targetPosition = Vector3.Lerp(
            noAimPlacenholder.position,
            AimingPlaceholder.position,
            aimBlend
        );

        Quaternion targetRotation = Quaternion.Slerp(
            noAimPlacenholder.rotation,
            AimingPlaceholder.rotation,
            aimBlend
        );


        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            followSpeed * Time.fixedDeltaTime
        );

        Vector3 positionOffset = transform.position - targetPosition;

        if (positionOffset.magnitude > maxDistance)
        {
            transform.position =
                targetPosition + positionOffset.normalized * maxDistance;
        }


        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            rotationFollowSpeed * Time.fixedDeltaTime
        );

        Quaternion rotationDifference =
            Quaternion.Inverse(targetRotation) * transform.rotation;

        rotationDifference.ToAngleAxis(
            out float angle,
            out Vector3 axis
        );

        if (angle > 180f)
            angle -= 360f;

        if (Mathf.Abs(angle) > maxRotationAngle)
        {
            float clampedAngle = Mathf.Clamp(
                angle,
                -maxRotationAngle,
                maxRotationAngle
            );

            transform.rotation =
                targetRotation *
                Quaternion.AngleAxis(clampedAngle, axis);
        }
    }

    public void IsAiming(bool aim)
    {
        isAiming = aim;

        if (playerMovement != null)
        {
            playerMovement.CanRun = !aim;
        }
    }
}

