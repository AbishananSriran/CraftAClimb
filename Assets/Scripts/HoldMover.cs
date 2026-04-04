using UnityEngine;

public class HoldMover : MonoBehaviour
{
    public enum Axis { X, Y };
    [Header("Movement Settings")]
    public float speed = 2f;
    public float distance = 1f;
    public Axis axis = Axis.X;

    private Vector3 startPosition;
    private ClimbingHold hold;
    private float internalTimer = 0f;

    void Start()
    {
        startPosition = transform.position;
        hold = GetComponent<ClimbingHold>();
    }

    void Update()
    {
        // Stop moving if grabbed
        if (hold != null && hold.IsGrabbed) return;

        internalTimer += Time.deltaTime;
        float offset = Mathf.Sin(internalTimer * speed) * distance;
        transform.position = startPosition + offset * MoveAxis();
    }

    Vector3 MoveAxis()
        => axis == Axis.X ? Vector3.right
        : Vector3.up;
}
