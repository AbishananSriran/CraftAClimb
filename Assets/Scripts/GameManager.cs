using UnityEngine;

public class GameManager : MonoBehaviour
{
    public bool started = false;
    public GameObject uiCanvas;
    public GameObject ovrCameraRig;
    public GameObject boulders;
    public SimpleGemsAnim star;
    public GameObject originalStar;
    public GameObject obstacles;
    public float health = 100f;
    public Vector3 origPosition = new Vector3(0, 0.5f, 0);


    public void StartGame()
    {
        started = true;

        if (uiCanvas != null)
            uiCanvas.SetActive(false);
    }

    // Update is called once per frame
    private void OnEnable()
    {
        if (star != null)
            star.OnTouched += HandleTouched;
    }

    private void OnDisable()
    {
        if (star != null)
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

        started = false;
        uiCanvas.SetActive(true);
    }
}
