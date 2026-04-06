using UnityEngine;

public class ControlPanelManager : MonoBehaviour
{
    [Header("Main Target")]
    public GameObject target;

    public float maxSpinSpeed = 180f; // degrees per second
    float currentSpinSpeed;

    // Dial -> particle hue
    public void OnDialHueChanged(float t)
    {
    }

    // Slider -> particle emission rate (0..1)
    public void OnSliderParticleRateChanged(float t)
    {

    }

    // Switch -> particles on/off
    public void OnParticlesToggled(bool on)
    {

    }

    // Button -> particle and main target reset
    public void OnResetPressed()
    {

    }

    void Update()
    {

    }

    // Hinge Slider -> onValueChanged(float 0..1) 
    public void OnRotationSpeedChanged(float t)
    {
    }

}
