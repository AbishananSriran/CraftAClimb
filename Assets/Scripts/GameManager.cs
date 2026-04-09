using UnityEngine;

public class GameManager : MonoBehaviour
{
    public bool started = false;
    public GameObject ovrCameraRig;
    public GameObject boulders;
    public SimpleGemsAnim star;
    public GameObject originalStar;
    public GameObject obstacles;
    public float health = 100f;
    public Vector3 origPosition = new Vector3(0, 0.5f, 0);

    // Update is called once per frame
    private void OnEnable()
    {
        star.OnTouched += HandleTouched;
    }

    private void OnDisable()
    {
        star.OnTouched -= HandleTouched; // always unsubscribe
    }

    private void HandleTouched()
    {
        if (!started)
        {
            return;
        }

        ovrCameraRig.transform.position = origPosition;

        foreach (Transform boulder in obstacles.transform)
        {
            Destroy(boulder.gameObject);
        }

        foreach (Transform child in boulders.transform)
        {
            Destroy(child.gameObject);
        }

        Destroy(star.gameObject);
        star = null;

        started = false;
        originalStar.SetActive(true);
    }
}
