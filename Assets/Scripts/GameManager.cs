using UnityEngine;

public class GameManager : MonoBehaviour
{
    public bool started = false;
    public GameObject uiCanvas;
    public GameObject ovrCameraRig;
    public GameObject boulders;
    public SimpleGemsAnim star;
    public RayGrab rayGrab;
    public GameObject originalStar;
    public GameObject obstacles;
    public float health = 100f;
    public Vector3 origPosition = new Vector3(0, 0.5f, 0);
    public BoulderSpawner boulderSpawner;


    public void StartGame()
    {
        started = true;

        if (uiCanvas != null)
            uiCanvas.SetActive(false);
    }

    // Update is called once per frame
    public void SetupListener()
    {
        if (star != null)
            star.OnTouched += HandleTouched;
    }

    public void KillListener()
    {
        if (star != null)
            star.OnTouched -= HandleTouched; // always unsubscribe

        star = null;
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
        rayGrab.ready = true;
        boulderSpawner.enabled = false;

    }
}
