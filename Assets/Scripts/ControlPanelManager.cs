using System.Drawing;
using UnityEngine;

public class ControlPanelManager : MonoBehaviour
{
    [Header("Main Target")]
    public GameObject target;

    [Header("Controllers")]
    public OVRController leftHand;
    public OVRController rightHand;

    float minSize = 1f;
    float maxSize = 1.5f;

    float minRotation = 90;
    float maxRotation = -90;


    // Dial -> particle hue
    public void OnDialHueChanged(float t)
    {
        float rot = minRotation + (maxRotation - minRotation) * t;
        foreach (Transform child in target.transform)
        {
            if (child.gameObject.name == "Star")
            {
                continue;
            }

            child.localRotation = Quaternion.Euler(new Vector3(0, child.localRotation.y, rot));
        }
    }

    // Slider -> particle emission rate (0..1)
    public void OnSliderParticleRateChanged(float t)
    {
        float sz = minSize + (maxSize - minSize) * t;
        Vector3 newScale = new Vector3(sz, sz, sz);

        foreach (Transform child in target.transform)
        {
            if (child.gameObject.name == "Star")
            {
                continue;
            }
            child.localScale = newScale;
        }
    }

    // Switch -> particles on/off
    public void OnParticlesToggled(bool on)
    {
        // set limited grip
        if (leftHand != null && rightHand != null)
        {
            leftHand.staminaOn = on;
            rightHand.staminaOn = on;
        }

    }

    // Button -> particle and main target reset
    public void OnResetPressed()
    {

    }

    // Hinge Slider -> onValueChanged(float 0..1) 
    public void OnRotationSpeedChanged(float t)
    {

    }

    void Update()
    {

    }
}
