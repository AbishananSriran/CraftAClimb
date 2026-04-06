using System.Collections.Generic;
using UnityEngine;

public class BoulderPool : MonoBehaviour
{
    public GameObject prefab;
    public int initialSize = 1;

    private Queue<GameObject> pool = new Queue<GameObject>();

    void Awake()
    {
        for (int i = 0; i < initialSize; i++)
        {
            GameObject obj = Instantiate(prefab, transform);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    public GameObject Get()
    {
        GameObject obj;

        if (pool.Count > 0)
        {
            obj = pool.Dequeue();
        }
        else
        {
            obj = Instantiate(prefab, transform);
        }

        obj.SetActive(true);

        // Assign pool reference
        Boulder boulder = obj.GetComponent<Boulder>();
        if (boulder != null)
            boulder.SetPool(this);

        return obj;
    }

    public void Return(GameObject obj)
    {
        obj.SetActive(false);
        pool.Enqueue(obj);
    }
}