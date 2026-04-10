using UnityEngine;

public class BoulderSpawner : MonoBehaviour
{
    [System.Serializable]
    public class BoulderType
    {
        public string name;
        public BoulderPool pool;
    }

    [Header("Boulder Types")]
    public BoulderType[] boulderTypes;

    [Header("Spawn Settings")]
    public float spawnRate = 2f;
    public Transform player;
    public float spawnHeightOffset = 10f;
    public float spawnWidthClearance = 2f;
    public float spawnWidthDistance = 5f;
    public AudioClip spawnSoundClip;

    private float spawnTimer = 0f;

    void Update()
    {
        spawnTimer += Time.deltaTime;

        if (spawnTimer >= spawnRate)
        {
            spawnTimer = 0f;
            SpawnBoulder();
        }
    }

    public void SpawnBoulder()
    {
        // Pick random boulder type
        int typeIndex = Random.Range(0, boulderTypes.Length);
        BoulderPool selectedPool = boulderTypes[typeIndex].pool;

        // Pick random spawn point
        float randomSpawnWidth = Random.Range(-spawnWidthDistance, spawnWidthDistance);
        Vector3 spawnWidthOffset = (randomSpawnWidth + Mathf.Sign(randomSpawnWidth) * spawnWidthClearance) * Vector3.right;
        Vector3 spawnOffset = spawnWidthOffset + spawnHeightOffset * Vector3.up;
        Vector3 spawnPoint = player.position + spawnOffset;

        GameObject boulder = selectedPool.Get();
        boulder.transform.position = spawnPoint;
        boulder.transform.rotation = Random.rotation;

        AudioPool.Instance.PlayClip(spawnSoundClip, player.position, 0.5f, spawnRate * 0.75f);
    }
}