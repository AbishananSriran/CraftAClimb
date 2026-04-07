using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OVRController : MonoBehaviour
{
    public enum Hand { Left, Right }
    public Hand hand = Hand.Right;

    [SerializeField] private ParticleSystem chalkParticles;

    [Header("Input")]
    public OVRInput.Button gripButton = OVRInput.Button.PrimaryHandTrigger;

    [Header("Stamina")]
    public float maxStamina = 5f;
    public float staminaDrainRate = 1f;
    public float staminaRegenRate = 1.5f;
    public float currentStamina;
    public float fatigueVibrationFactor = 1f;
    public float minBeatInterval = 0.2f;
    public float maxBeatInterval = 1.0f;
    public float panicModeThreshold = 0.2f;
    bool exhausted = false;
    float heartbeatTimer = 0f;

    // 
    public Interactable touchedItem;
    public Interactable grippedItem;
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
        }

        if (gripping && grippedItem != null && grippedItem.isClimbable)
        {
            currentStamina -= staminaDrainRate * Time.deltaTime;

            if (currentStamina <= maxStamina * 0.75f)
            {
                FatigueHaptics();
            }

            if (currentStamina <= 0f && !exhausted)
            {
                exhausted = true;
                currentStamina = 0f;

                ForceRelease();
            }
        }
        else
        {
            currentStamina += staminaRegenRate * Time.deltaTime;
            currentStamina = Mathf.Min(currentStamina, maxStamina);

            if (currentStamina > 0.2f * maxStamina)
            {
                exhausted = false;
            }
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.attachedRigidbody == null) return;
        //Debug.Log("Collided with Dial");
        var interactable = other.attachedRigidbody.GetComponent<Interactable>();
        grippedNormal = interactable.GetComponentInParent<Transform>().transform.forward;

        if (interactable == null || !interactable.enabled) return;

        //Debug.Log("Call Dial on Touch Enter");
        interactable.OnTouchEnter(this);
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
        }

        interactable.OnTouchStay(this);

    }

    private void OnTriggerExit(Collider other)
    {
        if (other.attachedRigidbody == null) return;
        var interactable = other.attachedRigidbody.GetComponent<Interactable>();
        if (interactable == null || !interactable.enabled) return;

        if (gripping && grippedItem != null)
        {
            grippedItem.OnGripEnd(this);
            gripping = false;
        }
        interactable.OnTouchExit(this);
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
