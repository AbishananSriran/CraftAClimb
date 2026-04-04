using UnityEngine;

public class OVRController : MonoBehaviour
{
    public enum Hand { Left, Right }
    public Hand hand = Hand.Right;

    [Header("Input")]
    public OVRInput.Button gripButton = OVRInput.Button.PrimaryHandTrigger;

    // 
    public Interactable touchedItem;
    public Interactable grippedItem;
    public Vector3 grippedNormal;
    public bool ctrlAnchored = false;
    public Vector3 ctrlOffset;

    OVRInput.Controller Ctrl =>
        (hand == Hand.Left) ? OVRInput.Controller.LTouch : OVRInput.Controller.RTouch;

    public Transform controllerAnchor;
    bool gripping;


    void Awake()
    {
        Debug.Log("this is the " + hand + " hand");
        Debug.Log("the grip button is " + gripButton);

        controllerAnchor = transform.parent.gameObject.transform;
    }

    void Update()
    {
        // // Release gripped item on grip release

        float grip = GetGripValue();

        if (grippedItem != null && grip < 0.1f && gripping)
        {
            Debug.Log("Release grip");
            grippedItem.OnGripEnd(this);
            grippedItem = null;
            gripping = false;
            ctrlAnchored = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.attachedRigidbody == null) return;
        //Debug.Log("Collided with Dial");
        var interactable = other.attachedRigidbody.GetComponent<Interactable>();
        grippedNormal = interactable.GetComponentInParent<Transform>().transform.forward;

        if (interactable == null || !interactable.enabled) return;

    }

    // Gripping setup is placed here as user might touch component then press grip trigger
    private void OnTriggerStay(Collider other)
    {
        if (other.attachedRigidbody == null) return;
        var interactable = other.attachedRigidbody.GetComponent<Interactable>();
        grippedNormal = interactable.GetComponentInParent<Transform>().transform.forward;


        if (interactable == null || !interactable.enabled) return;

        // Already gripping it
        if (grippedItem == interactable) return;

        // OVRInput.Controller ctrl = (hand == Hand.Left) ? OVRInput.Controller.LTouch : OVRInput.Controller.RTouch;
        float grip = GetGripValue();
        bool IsGrippingNow = grip > 0.5f;

        // If grip trigger held and we haven't already set up grip  
        if (IsGrippingNow && !gripping)
        {
            gripping = true;
            grippedItem = interactable;
            grippedItem.OnGripBegin(this);

            ctrlOffset = GetPosition() - grippedItem.transform.position;
            ctrlAnchored = true;
        }

    }

    private void OnTriggerExit(Collider other)
    {
        if (other.attachedRigidbody == null) return;
        var interactable = other.attachedRigidbody.GetComponent<Interactable>();
        if (interactable == null || !interactable.enabled) return;

        if (gripping && grippedItem != null)
        {
            grippedItem.OnGripEnd(this);
            grippedItem = null;
            gripping = false;
            ctrlAnchored = false;
        }
    }

    float GetGripValue()
    {
        return (hand == Hand.Left)
            ? OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger)
            : OVRInput.Get(OVRInput.Axis1D.SecondaryHandTrigger);
    }

    public bool IsGripping()
    {
        return gripping;
    }

    public Vector3 GetPosition()
    {
        return controllerAnchor.position;
    }

    public Vector3 GetGrippedPosition()
    {
        return (grippedItem != null)
            ? grippedItem.transform.position
            : Vector3.positiveInfinity;
    }


    // Simple haptics helpers
    public void HapticTick(float amplitude = 0.18f, float duration = 0.015f) => HapticPulse(amplitude, duration);
    public void HapticClick(float amplitude = 0.40f, float duration = 0.035f) => HapticPulse(amplitude, duration);

    void HapticPulse(float amplitude, float duration)
    {
        OVRInput.SetControllerVibration(1f, Mathf.Clamp01(amplitude), Ctrl);
        CancelInvoke(nameof(StopHaptics));
        Invoke(nameof(StopHaptics), duration);
    }

    void StopHaptics()
    {
        OVRInput.SetControllerVibration(0f, 0f, Ctrl);
    }
}
