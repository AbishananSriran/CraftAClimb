using UnityEngine;

public class RayGrab : MonoBehaviour
{
    [Header("Controller & Ray Settings")]
    public Transform controllerTransform;
    public LayerMask grabbableLayer;    // Only objects that can be grabbed
    public LayerMask surfaceLayer;      // Only layers that represent surfaces to place objects on  
    public float fallbackDistance = 3f;
    public float moveSpeed = 15f;
    public float triggerThreshold = 0.8f;
    public bool ready = false;

    [Header("Boulders Parent")]
    public GameObject parent;

    [Header("Laser Pointer")]
    public LineRenderer laserPointer;   // Assign a LineRenderer prefab for the laser
    public Color inactiveColor = Color.gray;
    public Color hoverColor = Color.green;

    private Transform grabbedObject = null;
    private Vector3 grabOffset;
    private RaycastHit lastGrabbableHit;
    private bool hasValidHit = false;

    void Start()
    {
        laserPointer = gameObject.AddComponent<LineRenderer>();
        laserPointer.material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));

        laserPointer.startColor = laserPointer.endColor = inactiveColor;
        laserPointer.startWidth = laserPointer.endWidth = 0.005f;
        laserPointer.positionCount = 2;

        laserPointer.SetPosition(0, controllerTransform.position);
        laserPointer.SetPosition(1, controllerTransform.position + controllerTransform.forward * fallbackDistance);

        ready = true;
    }

    void Update()
    {
        if (!ready) return;

        float triggerValue = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.LTouch);
        Debug.Log($"Trigger Value: {triggerValue}");

        // 1️⃣ Raycast for potential objects (always, for UX)
        Ray pointerRay = new Ray(controllerTransform.position, controllerTransform.forward);
        hasValidHit = Physics.Raycast(pointerRay, out lastGrabbableHit, fallbackDistance, grabbableLayer);
        Debug.Log($"Raycast Hit: {hasValidHit}, Hit Object: {(hasValidHit ? lastGrabbableHit.collider.name : "None")}");

        // 2️⃣ Update laser pointer visuals
        Vector3 endPos = hasValidHit ? lastGrabbableHit.point : pointerRay.origin + pointerRay.direction * fallbackDistance;
        laserPointer.SetPosition(0, pointerRay.origin);
        laserPointer.SetPosition(1, endPos);
        laserPointer.startColor = laserPointer.endColor = hasValidHit ? hoverColor : inactiveColor;

        // 3️⃣ Handle trigger grab
        if (triggerValue > triggerThreshold)
        {
            // If not holding anything and we have a valid hit, grab it
            if (grabbedObject == null && hasValidHit)
            {
                Transform original = lastGrabbableHit.collider.transform;

                // Prevent cloning already spawned objects
                if (original.parent == parent.transform) return;

                // Create a copy
                GameObject clone = Instantiate(original.gameObject);

                // Parent it
                clone.transform.SetParent(parent.transform);

                // Optional: match position & rotation exactly
                clone.transform.position = original.position;
                clone.transform.rotation = original.rotation;
                if (original.gameObject.name == "Star")
                {
                    original.gameObject.SetActive(false);
                }

                // Assign as grabbed
                grabbedObject = clone.transform;
                grabOffset = Vector3.zero;
            }

            // If holding an object, move it to the surface or fallback
            if (grabbedObject != null)
            {
                Ray moveRay = new Ray(controllerTransform.position, controllerTransform.forward);
                Vector3 targetPos;

                if (Physics.Raycast(moveRay, out RaycastHit hitMove, fallbackDistance, surfaceLayer))
                    targetPos = hitMove.point + grabOffset;
                else
                    targetPos = moveRay.origin + moveRay.direction * fallbackDistance + grabOffset;

                grabbedObject.position = Vector3.Lerp(grabbedObject.position, targetPos, Time.deltaTime * moveSpeed);
                grabbedObject.rotation = Quaternion.Euler(0f, grabbedObject.rotation.eulerAngles.y, 0f);
            }
        }
        else
        {
            // Release object when trigger not pressed
            grabbedObject = null;
        }
    }
}