using UnityEngine;
using UnityEngine.Events;

public class VRButton : MonoBehaviour
{
    public UnityEvent onPress;

    public void Press()
    {
        onPress.Invoke();
    }
}