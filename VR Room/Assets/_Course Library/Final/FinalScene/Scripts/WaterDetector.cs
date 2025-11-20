using UnityEngine;
using System.Collections.Generic;

public class WaterDetector : MonoBehaviour
{
    [Header("Flower Settings")]
    [SerializeField] private GameObject[] flowerPrefabs;
    [SerializeField] private int minFlowers = 3;
    [SerializeField] private int maxFlowers = 7;
    [SerializeField] private float spawnRadius = 1f;
    [SerializeField] private float growDuration = 1.5f;
    
    [Header("Watering Settings")]
    [SerializeField] private float waterCooldown = 0.5f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Performance Limits")] 
    [Tooltip("The absolute maximum number of flowers allowed in the scene at once.")]
    [SerializeField] private int globalMaxFlowers = 40; 
    
    [Tooltip("Radius to check for overcrowding before spawning a new flower.")]
    [SerializeField] private float densityCheckRadius = 0.3f; 
    
    [Tooltip("How many flowers are allowed within the Density Check Radius.")]
    [SerializeField] private int maxFlowersPerCluster = 3;
    
    [Tooltip("Layer required for the Density Check to see other flowers.")]
    [SerializeField] private LayerMask flowerLayer; 

    private HashSet<Vector3> flowersSpawnedPositions = new HashSet<Vector3>();
    private List<GameObject> activeFlowerList = new List<GameObject>(); // Tracks actual objects
    private float lastWaterTime;
    
    private void OnParticleCollision(GameObject other)
    {
        if (Time.time - lastWaterTime < waterCooldown) return;
        
        ParticleSystem ps = other.GetComponent<ParticleSystem>();
        if (ps == null) return;
        
        List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();
        int numCollisionEvents = ps.GetCollisionEvents(gameObject, collisionEvents);
        
        if (numCollisionEvents > 0)
        {
            Vector3 waterHitPos = collisionEvents[0].intersection;
            SpawnFlowers(waterHitPos);
            lastWaterTime = Time.time;
        }
    }
    
    private void SpawnFlowers(Vector3 centerPos)
    {
        // 1. Clean up the list (remove any flowers that were destroyed/deleted)
        activeFlowerList.RemoveAll(item => item == null);

        // 2. GLOBAL CAP CHECK: Stop if we have too many flowers total
        if (activeFlowerList.Count >= globalMaxFlowers)
        {
            // Optional: Add a sound or feedback here indicating "No more room to grow"
            return;
        }

        if (flowerPrefabs.Length == 0)
        {
            Debug.LogWarning("No flower prefabs assigned!");
            return;
        }
        
        int flowerCount = Random.Range(minFlowers, maxFlowers + 1);
        
        for (int i = 0; i < flowerCount; i++)
        {
            // Double check cap inside the loop in case minFlowers pushes us over
            if (activeFlowerList.Count >= globalMaxFlowers) break;

            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPos = centerPos + new Vector3(randomCircle.x, 0.1f, randomCircle.y);
            
            Vector3 gridPos = new Vector3(
                Mathf.Round(spawnPos.x * 4f) / 4f,
                spawnPos.y,
                Mathf.Round(spawnPos.z * 4f) / 4f
            );
            
            // Check 1: Exact Duplicate Position
            if (flowersSpawnedPositions.Contains(gridPos)) continue;

            // Check 2: DENSITY CHECK (The Radius Limit)
            // This looks for colliders on the 'flowerLayer' within 'densityCheckRadius'
            Collider[] nearbyFlowers = Physics.OverlapSphere(spawnPos, densityCheckRadius, flowerLayer);
            if (nearbyFlowers.Length >= maxFlowersPerCluster)
            {
                continue; // Too crowded here, skip this specific flower
            }
            
            if (Physics.Raycast(spawnPos + Vector3.up * 2f, Vector3.down, out RaycastHit hit, 5f, groundLayer))
            {
                GameObject flowerPrefab = flowerPrefabs[Random.Range(0, flowerPrefabs.Length)];
                GameObject flower = Instantiate(flowerPrefab, hit.point, Quaternion.Euler(0, Random.Range(0, 360), 0));
                
                // Add to our tracking list
                activeFlowerList.Add(flower);
                
                Rigidbody rb = flower.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.constraints = RigidbodyConstraints.FreezeRotation;
                    rb.isKinematic = false; 
                }
                
                StartCoroutine(GrowFlower(flower));
                flowersSpawnedPositions.Add(gridPos);
            }
        }
    }
    
    private System.Collections.IEnumerator GrowFlower(GameObject flower)
    {
        Vector3 targetScale = flower.transform.localScale;
        flower.transform.localScale = Vector3.zero;
        
        float elapsed = 0f;
        while (elapsed < growDuration)
        {
            if (flower == null) yield break; // Safety check in case flower is destroyed while growing

            elapsed += Time.deltaTime;
            float t = elapsed / growDuration;
            float scale = Mathf.Sin(t * Mathf.PI * 0.5f);
            flower.transform.localScale = targetScale * scale;
            yield return null;
        }
        
        if(flower != null)
            flower.transform.localScale = targetScale;
    }
    
    public void ResetFlowers()
    {
        // Destroy existing flowers in the scene
        foreach(var flower in activeFlowerList)
        {
            if(flower != null) Destroy(flower);
        }
        activeFlowerList.Clear();
        flowersSpawnedPositions.Clear();
    }
}