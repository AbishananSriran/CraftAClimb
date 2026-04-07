using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody))]
public class Lever : Interactable
{
    public enum Axis { X, Y, Z }

    [Header("Pivot (optional but recommended)")]
    [Tooltip("Point about which the lever rotates. If null, uses this transform.")]
    public Transform hingePivot;

    [Header("Hinge Axis")]
    public Axis hingeAxis = Axis.X;

    [Tooltip("Minimum lever angle (e.g. -75).")]
    public float minAngle = -65f;

    [Tooltip("Maximum lever angle (e.g. 0).")]
    public float maxAngle = 0f;

    [Header("Motion")]
    [Tooltip("Max angular speed (deg/sec). 360�1200 feels good in VR.")]
    public float maxDegreesPerSecond = 900f;

    [Tooltip("Ignore tiny controller-induced angle changes (deg).")]
    public float deadZoneDeg = 0.15f;

    // Lever rest angle when lever off
    public float restAngle = 0f;

    [Header("Events")]
    [Tooltip("Fires continuously with value in range [0..1].")]
    public UnityEvent<float> onValueChanged;

    [Header("Haptics")]
    public bool haptics = false;
    [Range(0f, 1f)] public float dragHaptics = 0.2f;
    public float hapticInterval = 0.05f;

    // State
    Quaternion startLocalRot;
    float currentAngle;

    // Grip state
    OVRController controller;
    bool isGripping;

    // Stable plane basis
    Vector3 axisWorld;
    Vector3 uWorld;
    Vector3 vWorld;

    // --- NO-SNAP GRAB OFFSET STATE ---
    float angleAtGrab;          // lever angle at grip start
    float ctrlAngleAtGrab;      // controller �angle� at grip start (in our u/v basis)

    float lastHapticTime;

    float fullOnAngle; // used for mapping 0..1 (we treat minAngle as full-on with this prefab!)

    void Awake()
    {
        startLocalRot = transform.localRotation;

        var rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        if (hingePivot == null) hingePivot = transform;

        // For this hinge lever, up position is a more negative angle,  rest angle = 0,
        // full-on= minAngle (negative)
        fullOnAngle = minAngle;

        currentAngle = Mathf.Clamp(restAngle, minAngle, maxAngle);
        ApplyRotation(currentAngle);
        EmitValue(currentAngle);
    }

    public override void OnGripBegin(OVRController ctrl)
    {

        Debug.Log("Grip working");
        controller = ctrl;
        isGripping = true;

        // --- Robust hinge plane setup ---
        axisWorld = GetAxisWorld();

        // Pick a fallback vector if axisWorld is parallel to up
        Vector3 arbitrary = Vector3.up;
        if (Vector3.Dot(axisWorld, arbitrary) > 0.99f) // almost parallel
            arbitrary = Vector3.forward;

        uWorld = Vector3.Cross(axisWorld, arbitrary).normalized;
        vWorld = Vector3.Cross(axisWorld, uWorld);

        // Capture grab angles to prevent snapping
        angleAtGrab = currentAngle;
        ctrlAngleAtGrab = ComputeControllerAngle(ctrl.transform.position);

        if (haptics) ctrl.HapticClick(0.2f, 0.02f);
        lastHapticTime = Time.time;
    }
    public override void OnGripEnd(OVRController ctrl)
    {
        if (ctrl == controller)
        {
            isGripping = false;
            controller = null;
        }
    }

    void Update()
    {
        if (isGripping && controller != null)
        {
            // TODO
            // Hint: follow dial.cs and toggleswitch.ca
            // Lever has elements of both

            float ctrlAngleNow = ComputeControllerAngle(controller.transform.position);
            float delta = Mathf.DeltaAngle(ctrlAngleAtGrab, ctrlAngleNow);

            float targetAngle = angleAtGrab + delta;
            targetAngle = Mathf.Clamp(targetAngle, minAngle, maxAngle);

            // Smooth follow
            currentAngle = Mathf.MoveTowards(currentAngle, targetAngle, maxDegreesPerSecond * Time.deltaTime);
            // Apply and emit
            ApplyRotation(currentAngle);
            EmitValue(currentAngle);

            // Haptics
            if (haptics && Time.time - lastHapticTime >= hapticInterval)
            {
                lastHapticTime = Time.time;
                controller.HapticTick(Mathf.Clamp01(dragHaptics), 0.015f);
            }
        }

    }



    float ComputeControllerAngle(Vector3 ctrlWorldPos)
    {
        // TODO - see dial.cs and toggleswitch.cs
        Vector3 switchWorldPos = transform.position;
        Vector3 switchToCtrl = ctrlWorldPos - switchWorldPos;
        Vector3 projected = Vector3.ProjectOnPlane(switchToCtrl, axisWorld);

        if (projected.sqrMagnitude < 1e-8f) return 0f;
        projected.Normalize();

        // Build stable (u,v) basis on hinge plane
        float x = Vector3.Dot(projected, uWorld);
        float y = Vector3.Dot(projected, vWorld);

        return Mathf.Atan2(y, x) * Mathf.Rad2Deg;
    }

    void ApplyRotation(float angle)
    {
        // Rotate relative to the rest angle
        transform.localRotation = startLocalRot * Quaternion.AngleAxis(angle, GetAxisLocal());
    }

    void EmitValue(float angle)
    {
        // 0 at restAngle (0), 1 at fullOnAngle (minAngle, negative)
        float t = Mathf.InverseLerp(restAngle, fullOnAngle, angle);
        onValueChanged?.Invoke(t);
    }

    Vector3 GetAxisLocal()
    {
        return hingeAxis == Axis.X ? Vector3.right :
               hingeAxis == Axis.Y ? Vector3.up :
                                     Vector3.forward;
    }

    Vector3 GetAxisWorld()
    {
        // Axis in world based on the lever�s parent space 
        return transform.parent
            ? transform.parent.TransformDirection(GetAxisLocal()).normalized
            : transform.TransformDirection(GetAxisLocal()).normalized;
    }
}
