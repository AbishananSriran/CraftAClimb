using UnityEngine;

public class FollowPlayerUI : MonoBehaviour
{
    public Transform head; // CenterEyeAnchor
    public float distance = 0.5f;
    public float heightOffset = -0.2f;
    public float smoothSpeed = 5f;

    void Update()
    {
        if (head == null) return;

        Vector3 targetPos = head.position + head.forward * distance;
        targetPos.y += heightOffset;

        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * smoothSpeed);

        transform.LookAt(head);
        transform.Rotate(0, 180, 0);
    }
}