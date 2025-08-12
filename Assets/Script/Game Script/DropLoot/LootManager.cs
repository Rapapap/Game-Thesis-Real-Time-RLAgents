using UnityEngine;

public class LootManager : MonoBehaviour
{
    [Header("Loot Prefabs")]
    public GameObject lootDataPrefab; 
    public GameObject lootGearPrefab; 
    public GameObject lootHealthPrefab;
    
    [Header("Settings")]
    public bool spawnGearLoot = true; // Toggle gear loot spawning
    public float spawnInterval = 3f;
    private float nextSpawnTime; 
    void Start()
    {
        nextSpawnTime = Time.time + spawnInterval; // Set waktu pertama untuk melakukan spawn
    }

    public void SpawnDataLoot(Transform spawnPoint)
    {
        if (lootDataPrefab != null)
        {
            Instantiate(lootDataPrefab, spawnPoint.position, spawnPoint.rotation, spawnPoint);
        }
        else
        {
            return; 
        }
    }
    public void SpawnGearLoot(Transform spawnPoint)
    {
        if (lootGearPrefab != null)
        {
            Instantiate(lootGearPrefab, spawnPoint.position, spawnPoint.rotation, spawnPoint);
        }
        else
        {
            return;
        }
    }
    public void SpawnHealthLoot(Transform spawnPoint)
    {
        if (lootHealthPrefab != null)
        {
            Instantiate(lootHealthPrefab, spawnPoint.position, spawnPoint.rotation, spawnPoint);
        }
        else
        {
            return;
        }
    }

}
