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
    bool requireGripReset = false;
    bool inChalkBag = false;


    void Awake()
    {
        controllerAnchor = transform.parent.gameObject.transform;
        currentStamina = maxStamina;
    }

    void Update()
    {
        if (inChalkBag)
        {
            UseChalk();
            Debug.Log("using chalkf");
        }

        // // Release gripped item on grip release

        float grip = GetGripValue();

        if (requireGripReset && grip < 0.1f)
        {
            requireGripReset = false;
            Debug.Log("grip reset");
        }

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
        if (other.CompareTag("ChalkBag"))
        {
            inChalkBag = true;
            Debug.Log("entered chalk bag");
        }

        if (other.attachedRigidbody == null) return;

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

        //Debug.Log("Collided with Dial");
        var interactable = other.attachedRigidbody != null
            ? other.attachedRigidbody.GetComponent<Interactable>() : null;

        if (interactable == null)
        {
            interactable = other.GetComponentInParent<Interactable>();
        }

        if (interactable == null || !interactable.enabled) return;

        // Already gripping it
        if (grippedItem == interactable) return;

        float grip = GetGripValue();
        bool IsGrippingNow = grip > 0.5f;

        bool grip = gripValue > 0.5f;

        // If grip trigger held and we haven't already set up grip  
        if (IsGrippingNow && !gripping && !requireGripReset && !exhausted)
        {
            gripping = true;
            grippedItem = interactable;
            grippedItem.OnGripBegin(this);
        }

        interactable.OnTouchStay(this);

    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("ChalkBag"))
        {
            inChalkBag = false;
        }

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

    public void ForceRelease()
    {
        Debug.Log("ovr force release");
        if (grippedItem != null)
        {
            grippedItem.OnGripEnd(this);
            grippedItem = null;
        }

        gripping = false;
        ctrlAnchored = false;

        requireGripReset = true;

        HapticClick();
    }

    void UseChalk()
    {
        if (chalkParticles != null)
        {
            chalkParticles.Play();
        }
    }


    void FatigueHaptics()
    {
        float staminaPercent = Mathf.Clamp01(currentStamina / maxStamina);
        float intensity = 1f - staminaPercent;


        bool isHeartbeatPhase = staminaPercent <= 0.5f;
        bool isPanicMode = staminaPercent < panicModeThreshold;

        // slow rumble while not very tired
        if (!isHeartbeatPhase)
        {
            float ramp = Mathf.InverseLerp(1f, 0.5f, staminaPercent);
            ramp *= ramp;

            float rumbleStrength = ramp * fatigueVibrationFactor * 0.4f;

            HapticPulse(rumbleStrength, Time.deltaTime);
            return;
        }

        float heartbeatIntensity = Mathf.InverseLerp(0.5f, 0f, staminaPercent);
        float interval = Mathf.Lerp(maxBeatInterval, minBeatInterval, intensity);

        // panic mode activates
        if (isPanicMode)
        {
            interval *= Random.Range(0.7f, 1.3f); // jitter timing
            fatigueVibrationFactor *= 1.2f;
        }

        // Timer
        heartbeatTimer += Time.deltaTime;

        if (heartbeatTimer >= interval)
        {
            heartbeatTimer = 0f;

            float pulseStrength = heartbeatIntensity * heartbeatIntensity * fatigueVibrationFactor;

            StartCoroutine(HeartbeatPulse(pulseStrength));
        }
    }

    IEnumerator HeartbeatPulse(float strength)
    {
        HapticPulse(strength, 0.08f); // first strong beat
        yield return new WaitForSeconds(0.12f);
        HapticPulse(strength * 0.6f, 0.06f); // second softer beat
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
