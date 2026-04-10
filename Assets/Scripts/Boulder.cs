using UnityEngine;

public class Boulder : MonoBehaviour
{
    public float despawnTime = 5f;
    private BoulderPool pool;
    private Rigidbody rb;
    private float timer;

    void Awake()
    {
        rb = GetComponent<Rigidbody>(); ;
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= despawnTime)
        {
            ReturnToPool();
        }
    }

    public void SetPool(BoulderPool poolRef)
    {
        pool = poolRef;
    }

    public void ReturnToPool()
    {
        ResetVelocity();
        pool.Return(gameObject);
    }

    public void ResetVelocity()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }
}