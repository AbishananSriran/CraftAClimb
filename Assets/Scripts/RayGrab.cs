using UnityEngine;

public class RayGrab : MonoBehaviour
{
    [Header("Controller & Ray Settings")]
    public Transform controllerTransform;
    public LayerMask grabbableLayer;
    public LayerMask surfaceLayer;
    public float fallbackDistance = 3f;
    public float moveSpeed = 15f;
    public float triggerThreshold = 0.8f;
    public bool ready = false;

    [Header("Boulders Parent")]
    public GameObject parent;

    [Header("Laser Pointer")]
    public LineRenderer laserPointer;
    public Color inactiveColor = Color.gray;
    public Color hoverColor = Color.green;
    public Color uiHoverColor = Color.cyan;

    private Transform grabbedObject = null;
    private Vector3 grabOffset;
    private RaycastHit lastGrabbableHit;
    private bool hasValidHit = false;

    private float lastClickTime = 0f;
    public float clickCooldown = 0.25f;

    void Start()
    {
        laserPointer = gameObject.AddComponent<LineRenderer>();
        laserPointer.material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));

        laserPointer.startColor = laserPointer.endColor = inactiveColor;
        laserPointer.startWidth = laserPointer.endWidth = 0.005f;
        laserPointer.positionCount = 2;

        ready = true;
    }

    void Update()
    {
        if (!ready) return;

        float triggerValue = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.LTouch);

        Ray pointerRay = new Ray(controllerTransform.position, controllerTransform.forward);

        // 🔵 FIRST: Check UI interaction
        if (Physics.Raycast(pointerRay, out RaycastHit hitAll, fallbackDistance))
        {
            Vector3 endPosUI = hitAll.point;
            laserPointer.SetPosition(0, pointerRay.origin);
            laserPointer.SetPosition(1, endPosUI);

            if (hitAll.collider.CompareTag("UIButton"))
            {
                // UI hover color
                laserPointer.startColor = laserPointer.endColor = uiHoverColor;

                // Click
                if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch) &&
                    Time.time - lastClickTime > clickCooldown)
                {
                    Debug.Log("UI Button Clicked: " + hitAll.collider.name);
                    VRButton btn = hitAll.collider.GetComponent<VRButton>();
                    if (btn != null)
                    {
                        btn.Press();
                        lastClickTime = Time.time;
                    }
                }

                // STOP → don’t grab UI
                return;
            }
        }

        // 🟢 NORMAL GRAB RAYCAST
        hasValidHit = Physics.Raycast(pointerRay, out lastGrabbableHit, fallbackDistance, grabbableLayer);

        Vector3 endPos = hasValidHit ? lastGrabbableHit.point : pointerRay.origin + pointerRay.direction * fallbackDistance;

        laserPointer.SetPosition(0, pointerRay.origin);
        laserPointer.SetPosition(1, endPos);
        laserPointer.startColor = laserPointer.endColor = hasValidHit ? hoverColor : inactiveColor;

        // 🖐 GRAB LOGIC
        if (triggerValue > triggerThreshold)
        {
            if (grabbedObject == null && hasValidHit)
            {
                Transform original = lastGrabbableHit.collider.transform;

                if (original.parent == parent.transform) return;

                GameObject clone = Instantiate(original.gameObject);
                clone.transform.SetParent(parent.transform, true);

                clone.transform.position = original.position;
                clone.transform.rotation = original.rotation;

                if (original.gameObject.name == "Star")
                {
                    original.gameObject.SetActive(false);
                }

                grabbedObject = clone.transform;
                grabOffset = Vector3.zero;
            }

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
            grabbedObject = null;
        }
    }
}