using UnityEngine;

public class Boulder : MonoBehaviour
{
    public float despawnHeight = -1f;
    private BoulderPool pool;
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>(); ;
    }

    void Update()
    {
        if (transform.position.y < despawnHeight)
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