using UnityEngine;
using System.Collections.Generic;

public class WaterDetector : MonoBehaviour
{
    [Header("Flower Settings")]
    [SerializeField] private GameObject[] flowerPrefabs; // Assign flower prefabs
    [SerializeField] private int minFlowers = 3;
    [SerializeField] private int maxFlowers = 7;
    [SerializeField] private float spawnRadius = 1f;
    [SerializeField] private float growDuration = 1.5f;
    
    [Header("Watering Settings")]
    [SerializeField] private float waterCooldown = 0.5f; // Time between flower spawns
    [SerializeField] private LayerMask groundLayer; // Set to your plane's layer
    
    private HashSet<Vector3> flowersSpawned = new HashSet<Vector3>();
    private float lastWaterTime;
    
    private void OnParticleCollision(GameObject other)
    {
        // Check if enough time has passed since last watering
        if (Time.time - lastWaterTime < waterCooldown) return;
        
        // Get particle system from the watering can
        ParticleSystem ps = other.GetComponent<ParticleSystem>();
        if (ps == null) return;
        
        // Get collision events
        List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();
        int numCollisionEvents = ps.GetCollisionEvents(gameObject, collisionEvents);
        
        if (numCollisionEvents > 0)
        {
            // Use the first collision point
            Vector3 waterHitPos = collisionEvents[0].intersection;
            SpawnFlowers(waterHitPos);
            lastWaterTime = Time.time;
        }
    }
    
    private void SpawnFlowers(Vector3 centerPos)
    {
        if (flowerPrefabs.Length == 0)
        {
            Debug.LogWarning("No flower prefabs assigned!");
            return;
        }
        
        int flowerCount = Random.Range(minFlowers, maxFlowers + 1);
        
        for (int i = 0; i < flowerCount; i++)
        {
            // Random position within radius
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPos = centerPos + new Vector3(randomCircle.x, 0.1f, randomCircle.y);
            
            // Round to grid to avoid duplicate positions
            Vector3 gridPos = new Vector3(
                Mathf.Round(spawnPos.x * 4f) / 4f,
                spawnPos.y,
                Mathf.Round(spawnPos.z * 4f) / 4f
            );
            
            // Check if flower already exists at this position
            if (flowersSpawned.Contains(gridPos)) continue;
            
            // Raycast to ensure flower spawns on plane surface
            if (Physics.Raycast(spawnPos + Vector3.up * 2f, Vector3.down, out RaycastHit hit, 5f, groundLayer))
            {
                // Random flower prefab
                GameObject flowerPrefab = flowerPrefabs[Random.Range(0, flowerPrefabs.Length)];
                GameObject flower = Instantiate(flowerPrefab, hit.point, Quaternion.Euler(0, Random.Range(0, 360), 0));
                
                // Freeze rotation to keep flowers upright
                Rigidbody rb = flower.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.constraints = RigidbodyConstraints.FreezeRotation;
                    rb.isKinematic = false; // Keep physics for grabbing
                }
                
                // Add grow animation
                StartCoroutine(GrowFlower(flower));
                
                flowersSpawned.Add(gridPos);
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
            elapsed += Time.deltaTime;
            float t = elapsed / growDuration;
            // Ease out curve for natural growth
            float scale = Mathf.Sin(t * Mathf.PI * 0.5f);
            flower.transform.localScale = targetScale * scale;
            yield return null;
        }
        
        flower.transform.localScale = targetScale;
    }
    
    // Optional: Reset flowers for testing
    public void ResetFlowers()
    {
        flowersSpawned.Clear();
    }
}