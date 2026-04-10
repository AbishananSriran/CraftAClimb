using UnityEngine;

public class TrashBin : MonoBehaviour
{
    [Header("Target Parent To Clear")]
    public GameObject parentToClear;

    [Header("Star Object To Spawn")]
    public GameObject star;

    [Header("Game Manager")]
    public GameManager gameManager;


    // Make sure your bin collider has 'Is Trigger' checked
    private void OnTriggerEnter(Collider other)
    {
        ClearChildren();
    }

    private void ClearChildren()
    {
        if (parentToClear == null) return;

        foreach (Transform child in parentToClear.transform)
        {
            Destroy(child.gameObject);
        }

        gameManager.KillListener();
        star.SetActive(true);
    }
}