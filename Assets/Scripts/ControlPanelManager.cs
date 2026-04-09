using System.Drawing;
using UnityEngine;

public class ControlPanelManager : MonoBehaviour
{
    [Header("Main Target")]
    public GameObject target;

    float minSize = 0.0075f;
    float maxSize = 0.009f;

    // Dial -> particle hue
    public void OnDialHueChanged(float t)
    {

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
            Debug.Log($"Set scale of {child.name} to {newScale}");
        }
    }

    // Switch -> particles on/off
    public void OnParticlesToggled(bool on)
    {

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
