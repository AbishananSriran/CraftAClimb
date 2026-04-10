using UnityEngine;

public class PlayerFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0, 1.5f, -2f);

    void Update()
    {
        if (target == null) return;

        // Get forward direction but flatten it (remove vertical tilt)
        Vector3 flatForward = Vector3.ProjectOnPlane(target.forward, Vector3.up).normalized;

        // Create rotation using only Y axis
        Quaternion yawRotation = Quaternion.LookRotation(flatForward);

        // Apply rotated offset
        Vector3 desiredPosition = target.position + yawRotation * offset;

        transform.position = desiredPosition;
        transform.rotation = Quaternion.identity;

    }
}
