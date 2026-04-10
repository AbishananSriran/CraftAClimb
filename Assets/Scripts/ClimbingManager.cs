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
        private Vector3 smoothedDelta;


        [Header("Jump Settings")]

        [SerializeField] private float jumpMultiplier = 2.0f;
        [SerializeField] private float jumpThreshold = 0.5f;
        [SerializeField] private float maxJumpForce = 1f;
        [SerializeField] private float jumpBufferTimer = 1f;

        private Vector3 handVelocity;
        private Vector3 bufferedVelocity = Vector3.zero;
        private float bufferTimer = 0f;

        [Header("Fake Gravity")]
        [SerializeField] private float gravity = -9.8f;
        [SerializeField] private float maxFallSpeed = -20f;
        [SerializeField] private float groundY = 0f;

        private float verticalVelocity = 0f;

        private bool isClimbing = false;
        private OVRController activateHand = null;

        private Vector3 lastHandPosition;


    // Update is called once per frame
    void Update()
        {
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
                verticalVelocity = 0f;
                ApplyClimbingMovement();
            }
            else
            {
                ApplyGravity();
            }
        }

        private void StartClimbing(OVRController hand)
        {
            if (isClimbing && activateHand == hand) return;
            
            if (!hand.grippedItem.isClimbable) return;


            isClimbing = true;
            activateHand = hand;

            lastHandPosition = activateHand.GetPosition();
            smoothedDelta = Vector3.zero;
        }

        private void StopClimbing()
        {
            if (!isClimbing) return;

            // Apply jump impulse based on hand velocity
            Vector3 planarVelocity = Vector3.ProjectOnPlane(bufferedVelocity, activateHand.grippedNormal);

            if (planarVelocity.magnitude > jumpThreshold)
            {
                Vector3 jumpForce = -planarVelocity * jumpMultiplier;

                // Clamp jump strength
                jumpForce = Vector3.ClampMagnitude(jumpForce, maxJumpForce);

                // Apply vertical boost too
                verticalVelocity = Mathf.Clamp(-handVelocity.y * jumpMultiplier, 0f, maxJumpForce);

                cameraRig.position += jumpForce * Time.fixedDeltaTime;
            }

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
                activateHand.ctrlOffset = activateHand.GetPosition() - holdPos;
                activateHand.ctrlAnchored = true;
                smoothedDelta = Vector3.zero;
                lastHandPosition = holdPos + activateHand.ctrlOffset;
                return; // wait one frame to initialize
            }

            Vector3 currentHandPos = activateHand.GetPosition();

            Vector3 handDelta = currentHandPos - lastHandPosition;

            // convert to velocity
            handVelocity = handDelta / Time.fixedDeltaTime;

            if (handVelocity.magnitude > bufferedVelocity.magnitude)
            {
                bufferedVelocity = handVelocity;
                bufferTimer = jumpBufferTimer;
            }

            // countdown buffer
            if (bufferTimer > 0f)
            {
                bufferTimer -= Time.fixedDeltaTime;
            }
            else
            {
                bufferedVelocity = Vector3.zero;
            }

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

        private void ApplyGravity()
        {
            if (cameraRig.position.y <= groundY)
            {
                verticalVelocity = 0f;
                Vector3 pos = cameraRig.position;
                pos.y = groundY;
                cameraRig.position = pos;
                return;
            }

            // Apply gravity acceleration
            verticalVelocity += gravity * Time.fixedDeltaTime;

            // Clamp fall speed
            if (verticalVelocity < maxFallSpeed)
                verticalVelocity = maxFallSpeed;

            // Apply movement downward
            Vector3 gravityMove = new Vector3(0f, verticalVelocity, 0f) * Time.fixedDeltaTime;

            cameraRig.position += gravityMove;
        }

    }