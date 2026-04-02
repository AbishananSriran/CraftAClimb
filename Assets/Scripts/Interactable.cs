using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public abstract class Interactable : MonoBehaviour
{
    // Used for controls we typically grip (e.g. dial, lever, slider)
    public virtual void OnGripBegin(OVRController ctrl) { }
    public virtual void OnGripEnd(OVRController ctrl) { }

    public Vector3 GetNormal()
    {
        return transform.up;
    }
}
