using UnityEngine;

public class FollowBirdLookDirection : MonoBehaviour
{
    public Transform bird;
    public float rotationSpeed = 5f;

    void LateUpdate()
    {
        if (bird == null) return;

        Vector3 flatForward = Vector3.ProjectOnPlane(bird.forward, Vector3.up).normalized;
        if (flatForward.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(flatForward);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }

        transform.position = bird.position;
    }
}
