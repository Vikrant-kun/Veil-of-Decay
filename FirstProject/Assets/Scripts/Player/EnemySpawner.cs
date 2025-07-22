using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public Transform spawnPoint;
    public float spawnDelay = 3f; // Seconds between spawns
    public bool spawnOnce = true;

    private bool hasSpawned = false;
    private float timer;

    void Update()
    {
        // Count up time
        timer += Time.deltaTime;

        // Time to spawn
        if (timer >= spawnDelay && (!hasSpawned || !spawnOnce))
        {
            Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);

            if (spawnOnce)
                hasSpawned = true;

            timer = 0f; // Reset timer if it's not spawnOnce
        }
    }
}
