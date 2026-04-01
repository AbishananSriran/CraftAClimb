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
        Debug.Log("the " + hand.hand + " is climbing");
        Debug.Log("the grip button is " + hand.gripButton);
        isClimbing = true;
        activateHand = hand;

        lastHandPosition = hand.GetPosition();
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

        // Debug.Log("climbing movment");
        Vector3 currentHandPos = activateHand.GetPosition();
        Vector3 handDelta = currentHandPos - lastHandPosition;

        velocity = -handDelta * climbForce;
        velocity = Vector3.ClampMagnitude(velocity, maxVelocity);

        // Debug.Log(currentHandPos);

        cameraRig.position += velocity;

        lastHandPosition = currentHandPos;

    }
}
