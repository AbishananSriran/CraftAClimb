using UnityEngine;
using System.Collections;
using System;

public class SimpleGemsAnim : MonoBehaviour
{
    public bool isRotating = false;
    public bool rotateX = false;
    public bool rotateY = false;
    public bool rotateZ = false;
    public bool canTouch = false;

    public float rotationSpeed = 90f; // Degrees per second
    public event Action OnTouched;

    void OnTriggerEnter(Collider other)
    {
        if (canTouch && other.CompareTag("Player"))
        {
            OnTouched?.Invoke();
        }
    }

    void Update()
    {
        if (isRotating)
        {
            Vector3 rotationVector = new Vector3(
                rotateX ? 1 : 0,
                rotateY ? 1 : 0,
                rotateZ ? 1 : 0
            );
            transform.Rotate(rotationVector * rotationSpeed * Time.deltaTime);
        }
    }

    float EaseInOutQuad(float t)
    {
        return t < 0.5f ? 2 * t * t : 1 - Mathf.Pow(-2 * t + 2, 2) / 2;
    }
}

