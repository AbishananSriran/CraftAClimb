using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody))]
public class ClimbingHold : Interactable
{
    public enum LocalAxis { X, Y, Z }

    [Header("Grip Fatigue")]
    public float maxGripTime = 5f;
    float currentGripTime;

    [Header("Haptics (optional)")]
    public bool haptics = true;

    public bool IsGrabbed { get; private set; }

    // --- internal state ---
    OVRController controller;

    void Awake()
    {
        // Stable trigger/collision behavior
        var rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        isClimbable = true;
    }

    void Start()
    {

    }

    public override void OnGripBegin(OVRController ctrl)
    {
        controller = ctrl;

        IsGrabbed = true;

        currentGripTime = maxGripTime;

        if (haptics) controller.HapticClick(0.15f, 0.02f); // small grab tick
    }

    public override void OnGripEnd(OVRController ctrl)
    {
        if (controller != ctrl) return;
        controller = null;
        IsGrabbed = false;
    }

    void Update()
    {
        if (controller == null) return;

        currentGripTime -= Time.deltaTime;

        if (currentGripTime <= 0f)
        {
            ForceRelease();
        }


    }

    void ForceRelease()
    {
        if (controller != null)
        {
            controller.ForceRelease();
            controller = null;
        }

        IsGrabbed = false;
    }

    // ---------------- Helper Methods ----------------

    // Vector3 AxisLocal()
    //     => axis == LocalAxis.X ? Vector3.right :
    //        axis == LocalAxis.Y ? Vector3.up :
    //                              Vector3.forward;

    // float GetHandlePos()
    //     => Vector3.Dot(handle.localPosition, AxisLocal());

    // float GetControllerLocalPos(OVRController ctrl)
    // {
    //     // Controller position expressed in Slider local space, then projected onto axis.
    //     Vector3 ctrlLocalPos = transform.InverseTransformPoint(ctrl.transform.position);
    //     return Vector3.Dot(ctrlLocalPos, AxisLocal());
    // }
}