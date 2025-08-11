using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemySpawner : MonoBehaviour
{
    public GameObject[] enemyPrefab;
    public GameObject corner1;
    public GameObject corner2;
    public GameObject corner3;
    public GameObject corner4;
    public Transform[] waypoints;
    private int enemyCount = 0;
    public int maxEnemyCount = 10;
    public float spawnTime = 0.5f;
    private bool spawnTriggered = false;
    private CameraFollow cameraFollow;
    private GameProgression gameProgression;
    public GameObject Gate;
    
    [Header("NavMesh Spawn Settings")]
    public float navMeshSampleDistance = 5f; // Distance to search for valid NavMesh positions
    public int maxSpawnAttempts = 10; // Maximum attempts to find a valid spawn position
    
    private void Start()
    {
        gameProgression = GameProgression.Instance;
        cameraFollow = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraFollow>();
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && other.gameObject.layer == LayerMask.NameToLayer("Hitbox"))
        {
            if (cameraFollow != null)
            {
                cameraFollow.CombatMode();
            }
            
            if (!spawnTriggered)
            {
                if (AudioManager.instance != null)
                {
                    AudioManager.instance.PlaySFX(AudioManager.instance.gateClose);
                }
                StartCoroutine(SpawnEnemy());
                
                if (Gate != null)
                {
                    GateInteraction gateInteraction = Gate.GetComponent<GateInteraction>();
                    if (gateInteraction != null)
                    {
                        gateInteraction.CloseGate();
                    }
                }
                spawnTriggered = true;
            }
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && other.gameObject.layer == LayerMask.NameToLayer("Hitbox"))
        {
            if (cameraFollow != null)
            {
                cameraFollow.NormalMode();
            }
        }
    }

    IEnumerator SpawnEnemy()
    {
        while (enemyCount < maxEnemyCount)
        {
            if (gameProgression != null)
            {
                float spawnChance = (float)gameProgression.EnemyTotalSpawnCount / gameProgression.EnemyTotalCount;
                float randomValue = Random.value;

                GameObject enemyToSpawn;

                if (randomValue < 0.5f * spawnChance)
                {
                    enemyToSpawn = enemyPrefab[2]; // Medium enemy
                }
                else if (randomValue < spawnChance)
                {
                    enemyToSpawn = enemyPrefab[1]; // Normal enemy
                }
                else
                {
                    enemyToSpawn = enemyPrefab[0]; // Creep enemy
                }

                Vector3 spawnPosition = GetValidNavMeshPosition();
                
                if (spawnPosition != Vector3.zero) // Only spawn if we found a valid position
                {
                    GameObject enemy = Instantiate(enemyToSpawn, spawnPosition, Quaternion.identity);
                    EnemyController enemyController = enemy.GetComponent<EnemyController>();
                    
                    if (enemyController != null && waypoints != null)
                    {
                        // Only assign waypoints up to the enemy's waypoints array capacity
                        int minLength = Mathf.Min(waypoints.Length, enemyController.waypoints.Length);
                        for (int i = 0; i < minLength; i++)
                        {
                            enemyController.waypoints[i] = waypoints[i];
                        }
                    }
                    
                    enemyCount++;
                    if (gameProgression != null)
                    {
                        gameProgression.EnemySpawn();
                    }
                }
                else
                {
                    Debug.LogWarning("EnemySpawner: Could not find valid NavMesh position for enemy spawn");
                }
            }
            
            yield return new WaitForSeconds(spawnTime);
        }
    }
    
    private Vector3 GetValidNavMeshPosition()
    {
        for (int attempt = 0; attempt < maxSpawnAttempts; attempt++)
        {
            Vector3 randomPosition = GetRandomPosition();
            NavMeshHit hit;
            
            // Try to find the nearest valid NavMesh position
            if (NavMesh.SamplePosition(randomPosition, out hit, navMeshSampleDistance, NavMesh.AllAreas))
            {
                // Additional check to make sure the position is not too close to other enemies
                if (IsPositionValidForSpawn(hit.position))
                {
                    return hit.position;
                }
            }
        }
        
        return Vector3.zero; // Return zero vector if no valid position found
    }
    
    private bool IsPositionValidForSpawn(Vector3 position)
    {
        // Check if there are any enemies too close to this position
        Collider[] nearbyEnemies = Physics.OverlapSphere(position, 2f); // 2f minimum distance between enemies
        foreach (Collider col in nearbyEnemies)
        {
            if (col.GetComponent<EnemyController>() != null)
            {
                return false; // Too close to another enemy
            }
        }
        return true;
    }
    
    private Vector3 GetRandomPosition()
    {
        if (!corner1 || !corner2 || !corner3 || !corner4)
        {
            Debug.LogWarning("EnemySpawner: One or more corner references are missing.");
            return Vector3.zero;
        }

        // Gather corner positions
        Vector3[] corners =
        {
            corner1.transform.position,
            corner2.transform.position,
            corner3.transform.position,
            corner4.transform.position
        };

        // Calculate min and max for each axis
        float minX = Mathf.Min(corners[0].x, Mathf.Min(corners[1].x, Mathf.Min(corners[2].x, corners[3].x)));
        float maxX = Mathf.Max(corners[0].x, Mathf.Max(corners[1].x, Mathf.Max(corners[2].x, corners[3].x)));

        float minZ = Mathf.Min(corners[0].z, Mathf.Min(corners[1].z, Mathf.Min(corners[2].z, corners[3].z)));
        float maxZ = Mathf.Max(corners[0].z, Mathf.Max(corners[1].z, Mathf.Max(corners[2].z, corners[3].z)));

        // Pick a random position inside the bounds
        float randomX = Random.Range(minX, maxX);
        float randomZ = Random.Range(minZ, maxZ);

        // Use the average Y position of corners, or add a small offset above ground
        float avgY = (corners[0].y + corners[1].y + corners[2].y + corners[3].y) / 4f;

        return new Vector3(randomX, avgY + 0.1f, randomZ); // Small Y offset to ensure spawning above ground
    }
}