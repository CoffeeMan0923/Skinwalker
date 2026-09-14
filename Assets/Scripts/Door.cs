using UnityEngine;
using System.Collections;

public class Door : MonoBehaviour
{
    private bool isClosed = true;
    private bool isMoving = false;

    public bool IsClosed => isClosed;
    public bool IsMoving => isMoving;

    void Update()
    {
    }

    public void UseDoor()
    {
        if (isMoving)
            return;

        float currentY = transform.localEulerAngles.y;
        float targetY;

        if (isClosed)
        {
            targetY = currentY + 90f;
            isClosed = false;
        }
        else
        {
            targetY = currentY - 90f;
            isClosed = true;
        }

        StartCoroutine(RotateDoor(targetY));
    }

    private IEnumerator RotateDoor(float targetY)
    {
        isMoving = true;

        Quaternion startRotation = transform.localRotation;

        Quaternion targetRotation = Quaternion.Euler(
            transform.localEulerAngles.x,
            targetY,
            transform.localEulerAngles.z
        );

        float elapsed = 0f;
        float duration = 1f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = elapsed / duration;
            t = Mathf.SmoothStep(0f, 1f, t);

            transform.localRotation = Quaternion.Slerp(
                startRotation,
                targetRotation,
                t
            );

            yield return null;
        }

        transform.localRotation = targetRotation;
        isMoving = false;
    }
}