using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public Transform spawnPoint;
    public float spawnDelay = 3f;
    public bool spawnOnce = true;

    private bool hasSpawned = false;
    private float timer;
    private Transform playerTarget;

    void Start()
    {
        timer = spawnDelay; 
        Debug.Log("EnemySpawner: Initialized. Waiting for player target and/or spawn conditions.", this);
    }

    public void SetPlayerTargetForSpawner(Transform target)
    {
        if (target != null && playerTarget == null)
        {
            playerTarget = target;
            Debug.Log("EnemySpawner: Received player target from GameRestartManager: " + playerTarget.name, this);
        }
    }

    void Update()
    {
        if (playerTarget == null)
        {
            return;
        }

        timer += Time.deltaTime;

        if (timer >= spawnDelay)
        {
            if (spawnOnce && hasSpawned)
            {
                return; 
            }

            GameObject newEnemy = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
            Debug.Log("EnemySpawner: Spawned new enemy: " + newEnemy.name, this);

            EnemyAI enemyAI = newEnemy.GetComponent<EnemyAI>();
            if (enemyAI != null)
            {
                enemyAI.SetPlayerTarget(playerTarget);
                Debug.Log("EnemySpawner: Assigned player target to spawned EnemyAI.", this);
            }
            else
            {
                Debug.LogWarning("EnemySpawner: Spawned enemy '" + newEnemy.name + "' does not have an EnemyAI script!", newEnemy);
            }

            BelerickAI belerickAI = newEnemy.GetComponent<BelerickAI>();
            if (belerickAI != null)
            {
                belerickAI.SetPlayerTarget(playerTarget);
                Debug.Log("EnemySpawner: Assigned player target to spawned BelerickAI.", this);
            }

            if (spawnOnce)
            {
                hasSpawned = true;
            }
            else
            {
                timer = 0f;
            }
        }
    }
}