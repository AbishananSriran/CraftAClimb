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
    private Vector3 climbNormal;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // if (leftHand.IsGripping())
        // {
        //     StartClimbing(leftHand);
        // }
        // else if (rightHand.IsGripping())
        // {
        //     StartClimbing(rightHand);
        // }
        // else
        // {
        //     StopClimbing();
        // }
        if (!isClimbing)
        {
            if (leftHand.IsGripping()) StartClimbing(leftHand);
            else if (rightHand.IsGripping()) StartClimbing(rightHand);
        }
        else
        {
            if (!activateHand.IsGripping())
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
        if (activateHand == null || activateHand.grippedItem == null) return;

        // Get the target hand position relative to the hold
        Vector3 holdPos = activateHand.GetGrippedPosition();

        // If this is the first frame of gripping, anchor the hand
        if (!activateHand.ctrlAnchored)
        {
            Debug.Log("anchoring hand");
            activateHand.ctrlOffset = activateHand.GetPosition() - holdPos;
            activateHand.ctrlAnchored = true;
            smoothedDelta = Vector3.zero;
            lastHandPosition = holdPos + activateHand.ctrlOffset;
            return; // wait one frame to initialize
        }

        Vector3 currentHandPos = activateHand.GetPosition();
        Vector3 handDelta = currentHandPos - lastHandPosition;

        // Project onto wall plane so no forward/back movement
        handDelta = Vector3.ProjectOnPlane(handDelta, activateHand.grippedNormal);

        // 1. Deadzone
        if (handDelta.magnitude < deadzone)
            handDelta = Vector3.zero;

        // 2. Clamp sudden spikes
        handDelta = Vector3.ClampMagnitude(handDelta, maxDelta);

        // 3. Smooth
        smoothedDelta = Vector3.Lerp(smoothedDelta, handDelta, smoothFactor);

        // 4. Move the body opposite to hand delta
        Vector3 move = -smoothedDelta * climbForce;

        // 5. Clamp velocity
        move = Vector3.ClampMagnitude(move, maxVelocity);

        // 6. Apply movement via Rigidbody or transform
        cameraRig.position += move;

        // 7. Update last hand position
        lastHandPosition = currentHandPos;
    }

}
