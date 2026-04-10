using UnityEngine;

public class Boulder : MonoBehaviour
{
    public float despawnTime = 5f;
    private BoulderPool pool;
    private Rigidbody rb;
    private float timer = 0f;

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

    void OnTriggerEnter(Collider other)
    {
        var hitReceiver = other.GetComponentInParent<IPlayerHitReceiver>();

        if (hitReceiver != null)
        {
            hitReceiver.OnHitByBoulder();
            ReturnToPool();
        }
    }

    public void SetPool(BoulderPool poolRef)
    {
        pool = poolRef;
    }

    public void ReturnToPool()
    {
        timer = 0;
        ResetVelocity();
        pool.Return(gameObject);
    }

    public void ResetVelocity()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }
}