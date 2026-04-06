using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody))]
public class ToggleSwitch : Interactable
{
    public enum Axis { X, Y, Z }

    [Header("Rotation")]
    public Axis hingeAxis = Axis.Z;

    // Typical switch angle range, e.g. -25..+25 (or 0..-25)
    public float offAngle = -25f;
    public float onAngle = 25f;

    [Header("Motion")]
    [Tooltip("How fast it follows while touching.")]
    public float followSpeed = 25f;

    [Tooltip("How fast it snaps after release.")]
    public float snapSpeed = 14f;

    [Header("State")]
    public bool state; // switch state - on or off

    [Header("Haptics")]
    public bool haptics = true;
    public float toggleAmplitude = 0.45f;
    public float toggleDuration = 0.035f;

    [Header("Event")]
    public UnityEvent<bool> OnSwitch; // OnSwitch?.Invoke(state);

    Quaternion startLocalRot;

    // Touch state
    OVRController touchingCntlr;
    bool touching;

    // Motion state
    float currentAngle;
    float targetAngle;

    // --- No-snap  clutch  style offset ---
    float switchAngleAtTouch;      // switch angle when touch began
    float cntlrAngleAtTouch;  // controller angle when touch began

    void Awake()
    {
        // Remember angle at start
        startLocalRot = transform.localRotation;

        var rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        currentAngle = state ? onAngle : offAngle;
        targetAngle = currentAngle;

        Apply(currentAngle);

        // Optional: emit initial state once
        OnSwitch?.Invoke(state);
    }

    public override void OnTouchEnter(OVRController ctrl)
    {
        touching = true;
        touchingCntlr = ctrl;

        // Capture  no snap  offsets
        switchAngleAtTouch = currentAngle;
        cntlrAngleAtTouch = ComputeControllerAngleLocal(ctrl.transform.position);
    }

    public override void OnTouchExit(OVRController cntlr)
    {
        if (cntlr != touchingCntlr) return;

        touching = false;
        // Keep touchingCntlr if you want haptics on snap; otherwise clear it here:
        // touchingCntlr = null;
    }
    void Update()
    {
        if (touching && touchingCntlr != null)
        {
            // TODO - Compute controller angle around hinge (in parent-local space)
            float cntlrAngle = ComputeControllerAngleLocal(touchingCntlr.transform.position);

            // TODO - Compute delta angle since touch began
            // Hint: use Mathf.DeltaAngle()
            float delta = Mathf.DeltaAngle(switchAngleAtTouch, cntlrAngle);



            // TODO - Target Angle = switchAngleAtTouch + delta (prevents jumping)
            targetAngle = switchAngleAtTouch + delta;

            // TODO - Clamp target angle to the [off..on] range (works regardless of which is smaller)
            targetAngle = ClampAngle(targetAngle, offAngle, onAngle);
        }
        else
        {
            // Not touching: snap to nearest state (off or on)
            float mid = 0.5f * (offAngle + onAngle);
            bool newState = currentAngle >= mid; // choose whichever side of mid we re on

            targetAngle = newState ? onAngle : offAngle;

            // Fire event once when state changes
            if (newState != state)
            {
                state = newState;
                OnSwitch?.Invoke(state);

                // Haptic click (we don't have a controller anymore if touch ended,
                // so we only click if we still have a reference)
                if (haptics && touchingCntlr != null)
                    touchingCntlr.HapticClick(toggleAmplitude, toggleDuration);
            }
        }

        // Smooth follow/snap
        float speed = touching ? followSpeed : snapSpeed;
        currentAngle = Mathf.LerpAngle(currentAngle, targetAngle, speed * Time.deltaTime);

        Apply(currentAngle);
    }

    // ---- Helpers ----

    // Update the switch local rotation 
    void Apply(float angle)
    {
        // TODO - Convert angle to quaternion using Quaternion.AngleAxis
        Quaternion rot = Quaternion.AngleAxis(angle, GetAxisLocal());

        // TODO - Multiply quaternions to get updated local rotation 
        transform.localRotation = rot * startLocalRot;
    }

    Vector3 GetAxisLocal()
    {
        return hingeAxis == Axis.X ? Vector3.right :
               hingeAxis == Axis.Y ? Vector3.up :
                                     Vector3.forward;
    }

    // Clamp angle even if offAngle > onAngle (handles reversed ranges)
    static float ClampAngle(float a, float lo, float hi)
    {
        float mn = Mathf.Min(lo, hi);
        float mx = Mathf.Max(lo, hi);
        return Mathf.Clamp(a, mn, mx);
    }

    // Given plane normal and a vector projected onto this plane
    // contruct an orthonormal CS then define the projected vector in this CS
    Vector2 ConstructPlaneCS(Vector3 n, Vector3 projected)
    {
        Vector3 refDir = Vector3.up;
        if (Mathf.Abs(Vector3.Dot(refDir, n)) > 0.85f) refDir = Vector3.right;

        Vector3 u = Vector3.ProjectOnPlane(refDir, n).normalized;
        Vector3 v = Vector3.Cross(n, u).normalized;

        float x = Vector3.Dot(projected, u);
        float y = Vector3.Dot(projected, v);

        return new Vector2(x, y);
    }

    // Compute a controller  angle  around hinge axis using parent-local coordinates.
    //  - Work in the plane perpendicular to the hinge axis.
    //  - Use Atan2 in a stable (u,v) basis on that plane.
    float ComputeControllerAngleLocal(Vector3 cntlrWorldPos)
    {
        // We assume the switch has a parent 
        Transform p = transform.parent;
        if (p == null) return 0f;

        // TODO - Convert switch transform position (world) to parent-local coords
        // so scaling/rotation is handled cleanly
        Vector3 switchLocalPos = p.InverseTransformPoint(transform.position);

        // TODO - Convert cntlr transform position (world) to parent-local coords
        // so scaling/rotation is handled cleanly
        Vector3 cntlrLocalPos = p.InverseTransformPoint(cntlrWorldPos);

        // TODO - Normalize hinge axis (use GetAxisLocal helper above)
        Vector3 axisLocal = GetAxisLocal().normalized;

        // TODO - Compute vector from switch to controller in parent-local
        Vector3 switchToCntlr = cntlrLocalPos - switchLocalPos;

        // TODO - Project this vector onto a plane perpendicular to hinge axis (the "hinge plane")
        // if magnitude of this projected vector is tiny (< 1e-8f) just return 0 
        // Then normalize this vector
        Vector3 projected = Vector3.ProjectOnPlane(switchToCntlr, axisLocal);
        if (projected.sqrMagnitude < 1e-8f) return 0f;
        projected.Normalize();

        // TODO - Build a stable basis (u,v) on the hinge plane and get the projected (x,y) vector
        // defined in this plane. Do this by calling ConstructPlaneCS (see above)
        // passing in normalized hinge axis and projected vector
        Vector2 uv = ConstructPlaneCS(axisLocal, projected);

        // TODO - Imagine you are looking down on the plane - you have the projected vector (x, y) in this
        // plane - if you want the angle this vector forms with the plane's u axis then use Atan2
        // to compute opposite (y) over adjacent (x). Convert to degrees and return this angle
        return Mathf.Atan2(uv.y, uv.x) * Mathf.Rad2Deg;
    }
}