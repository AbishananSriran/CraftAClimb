using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody))]
public class Dial : Interactable
{
    [Header("Range")]
    [Tooltip("Half-range in degrees. Example: 45 means dial can go from -45..+45. 0 = unlimited.")]
    public float degreeRange = 60f;

    [Header("Feel")]
    [Tooltip("How fast the dial follows your wrist while gripping. 0 = instant.")]
    [Range(0f, 40f)] public float followSpeed = 25f;

    [Tooltip("Release grip if controller drifts too far from dial (meters).")]
    public float breakDistance = 0.40f;

    public float deadZone = 0.25f;    // 3 mm

    [Header("Output")]
    [Tooltip("Signed dial angle in degrees (about local Y axis).")]
    public float angleDeg;

    [Range(0f, 1f)]
    [Tooltip("Normalized value (0..1) mapped from dial range.")]
    public float value01;
    public UnityEvent<float> onValueChanged01;

    [Header("Haptics")]
    public bool haptics = true;
    [Range(0f, 1f)] public float dragHaptics = 0.25f;
    [Range(0f, 1f)] public float touchHaptics = 0.25f;

    // -------- Internal state --------
    OVRController controller;                 // controller currently gripping
    Vector3 dialRightInCntlrLocal;            // dial reference dir stored in controller local space

    Quaternion quaternionAtStart;
    float angleAtGrab;                        // dial angle when grip begins
    float controllerAngleAtGrab;              // controller-implied angle when grip begins (prevents snapping)

    float lastValue01 = -999f;

    void Awake()
    {
        // Kinematic RB is recommended for trigger-based interaction.
        var rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    void Start()
    {
        // Initialize angle from transform (convert 0..360 to -180..180)
        angleDeg = Normalize180(transform.localEulerAngles.y);

        // Clamp if we have a bounded dial
        if (degreeRange > 0f) angleDeg = Mathf.Clamp(angleDeg, -degreeRange, degreeRange);

        // Set the dial
        ApplyAngle(angleDeg);

        // Output the value to the control panel via an event
        Emit01IfChanged(force: true);
    }

    public override void OnTouchEnter(OVRController c)
    {
        if (haptics) c.HapticTick(touchHaptics, 0.015f);
    }

    public override void OnGripBegin(OVRController c)
    {
        controller = c;

        // TODO - Store the dial's "reference direction" (dial's right vector) in controller-local space.
        // Later, we transform it back out to see how the controller has rotated.
        // Store it in dialRightInCntlrLocal
        dialRightInCntlrLocal = controller.transform.InverseTransformDirection(transform.right);

        quaternionAtStart = transform.rotation;

        // Save dial angle at grab start
        angleAtGrab = angleDeg;

        // Save controller-implied angle at grab start (so delta begins at 0).
        controllerAngleAtGrab = ComputeControllerAngle();
    }

    public override void OnGripEnd(OVRController c)
    {
        if (controller == c) controller = null;
    }

    void Update()
    {
        if (controller == null) return;

        // Break if controller moves too far away
        if (Vector3.Distance(controller.transform.position, transform.position) > breakDistance)
        {
            if (haptics) controller.HapticClick(0.6f, 0.03f);
            controller = null;
            return;
        }

        // Compute current controller-implied angle (signed, -180..180)
        float controllerAngleNow = ComputeControllerAngle();

        // TODO - Compute how much did the controller rotated since grab? (wrap-safe)
        // Hint: use Mathf.DeltaAngle
        float deltaAngle = Mathf.DeltaAngle(controllerAngleAtGrab, controllerAngleNow);
        if (Mathf.Abs(deltaAngle) < deadZone) deltaAngle = 0;

        // TODO - Compute target dial angle = dial angle at grab + delta
        float targetAngle = angleAtGrab + deltaAngle;

        // TODO - Clamp it if bounded using degree range
        if (degreeRange > 0f) targetAngle = Mathf.Clamp(targetAngle, -degreeRange, degreeRange);


        // TODO - Smooth follow (optional) - update angleDeg with target angle computed above
        if (followSpeed <= 0f)
            angleDeg = targetAngle; // replace 0 with target angle
        else
        {
            // TODO - replace with Mathf.LerpAngle to target angle
            // (don't forget to multiply follow speed by Time.deltaTime)
            angleDeg = Mathf.LerpAngle(angleAtGrab, targetAngle, followSpeed * Time.deltaTime);
        }
        // Apply to transform + emit value
        ApplyAngle(angleDeg);

        if (haptics) controller.HapticTick(dragHaptics, 0.015f);

        // Output value to control panel
        Emit01IfChanged(force: false);
    }

    // ---------------- Core math ----------------

    float ComputeControllerAngle()
    {
        // TODO - Take the saved dialRightInCntlrLocal and transform it into WORLD space
        Vector3 worldRight = controller.transform.TransformDirection(dialRightInCntlrLocal);

        // TODO - Then bring this world direction vector into the *dial's* LOCAL space
        Vector3 rightInDialLocal = transform.InverseTransformDirection(worldRight);

        // TODO - We only care about rotation around dial local Y, so we then project onto the XZ plane
        Vector3 projected = Vector3.ProjectOnPlane(rightInDialLocal, transform.up).normalized;

        // Use Atan2 to give the angle of this projected vector in the dial XZ plane (signed)
        // Convert to degrees and Normalize180
        float rads = Mathf.Atan2(projected.x, projected.z);
        float degs = rads * Mathf.Rad2Deg;

        degs = Normalize180(degs);
        return degs;
    }

    void ApplyAngle(float a)
    {
        // TODO - Update dial local rotation
        // Use a Quaternion.AngleAxis to avoid 0/360 Euler snapping (i.e. convert to quat, use Vector3.up as axis)

        transform.localRotation = Quaternion.Euler(0f, a, 0f);
    }

    // ---------------- Output mapping ----------------

    float AngleTo01(float a)
    {
        if (degreeRange <= 1e-6f) return Mathf.Repeat(a / 360f, 1f);
        return Mathf.InverseLerp(-degreeRange, degreeRange, a);
    }

    void Emit01IfChanged(bool force)
    {
        float t = AngleTo01(angleDeg);
        value01 = t;

        if (force || Mathf.Abs(t - lastValue01) > 0.0005f)
        {
            lastValue01 = t;
            onValueChanged01?.Invoke(t);
        }
    }

    // Normalize to [-180, +180]
    static float Normalize180(float a)
    {
        a = (a + 180f) % 360f;
        if (a < 0f) a += 360f;
        return a - 180f;
    }
}

