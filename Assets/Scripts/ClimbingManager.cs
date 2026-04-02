using UnityEngine;

public class ClimbingManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cameraRig;
    
    [Header("Hands")]
    [SerializeField] private OVRController leftHand;
    [SerializeField] private OVRController rightHand;

    [Header("Settings")]
    [SerializeField] private float climbForce = 1.0f;
    [SerializeField] private float maxVelocity = 5.0f;

    [SerializeField] float smoothFactor = 0.2f;
    [SerializeField] float deadzone = 0.002f;
    [SerializeField] float maxDelta = 0.1f;
    Vector3 smoothedDelta;

    private bool isClimbing = false;
    private OVRController activateHand = null;

    private Vector3 lastHandPosition;
    private Vector3 velocity;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (leftHand.IsGripping())
        {
            StartClimbing(leftHand);
        }
        else if (rightHand.IsGripping())
        {
            StartClimbing(rightHand);
        }
        else
        {
            StopClimbing();
        }
    }

    void FixedUpdate()
    {
        if (isClimbing)
        {
            ApplyClimbingMovement();
        }
    }

    private void StartClimbing(OVRController hand)
    {
        if (isClimbing && activateHand == hand) return;
        
        isClimbing = true;
        activateHand = hand;

        lastHandPosition = activateHand.GetPosition();
        smoothedDelta = Vector3.zero;
    }

    private void StopClimbing()
    {
        if (!isClimbing) return;
    
        isClimbing = false;
        activateHand = null;
    }

    private void ApplyClimbingMovement()
{
    if (activateHand == null) return;

    Vector3 currentHandPos = activateHand.GetPosition();
    Vector3 rawDelta = currentHandPos - lastHandPosition;

    // 1. Deadzone (kill micro jitter)
    if (rawDelta.magnitude < deadzone)
        rawDelta = Vector3.zero;

    // 2. Clamp sudden spikes
    rawDelta = Vector3.ClampMagnitude(rawDelta, maxDelta);

    // 3. Smooth the movement
    smoothedDelta = Vector3.Lerp(smoothedDelta, rawDelta, smoothFactor);

    // 4. Apply movement
    Vector3 move = -smoothedDelta * climbForce;
    move = Vector3.ClampMagnitude(move, maxVelocity);

    cameraRig.position += move;

    lastHandPosition = currentHandPos;
}
}
