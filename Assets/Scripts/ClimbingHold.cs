using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody))]
public class ClimbingHold : Interactable
{
    public enum LocalAxis { X, Y, Z }

    [Header("Haptics (optional)")]
    public bool haptics = true;

    // --- internal state ---
    OVRController controller;

    void Awake()
    {
        // Stable trigger/collision behavior
        var rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    void Start()
    {
        
    }

    public override void OnGripBegin(OVRController ctrl)
    {
        controller = ctrl;

        Debug.Log("Gripping hold");

        if (haptics) controller.HapticClick(0.15f, 0.02f); // small grab tick
    }

    public override void OnGripEnd(OVRController ctrl)
    {
        if (controller != ctrl) return;
        controller = null;
    }

    void Update()
    {
        if (controller == null) return;

        
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