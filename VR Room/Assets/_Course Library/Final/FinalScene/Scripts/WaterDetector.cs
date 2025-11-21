using UnityEngine;
using System.Collections.Generic;

public class WaterDetector : MonoBehaviour
{
    [Header("Flower & Mushroom Settings")]
    [Tooltip("Drag both Flower AND Mushroom prefabs here. The script picks randomly.")]
    [SerializeField] private GameObject[] flowerPrefabs;
    [SerializeField] private int minFlowers = 1; // Changed default to 1 for smoother spawning
    [SerializeField] private int maxFlowers = 3; // Lowered slightly so they don't explode out all at once
    [SerializeField] private float spawnRadius = 1f;
    [SerializeField] private float growDuration = 1.5f;
    
    [Header("Watering Settings")]
    [SerializeField] private float waterCooldown = 0.2f; // Lowered to make watering feel more responsive
    [SerializeField] private LayerMask groundLayer;

    [Header("Visual Feedback (Brown to Green)")]
    [SerializeField] private Renderer groundRenderer; // Drag your Ground Plane here!
    [SerializeField] private Color dryColor = new Color(0.4f, 0.25f, 0.1f); // Brown
    [SerializeField] private Color lushColor = Color.green;   // Green
    [Tooltip("How many water particles need to hit the ground to make it 100% Green.")]
    [SerializeField] private int particlesToFullyGreen = 200; 

    [Header("Performance Limits")] 
    [Tooltip("The absolute maximum number of flowers/mushrooms allowed in the scene at once.")]
    [SerializeField] private int globalMaxFlowers = 40; 
    
    [Tooltip("Radius to check for overcrowding before spawning a new item.")]
    [SerializeField] private float densityCheckRadius = 0.3f; 
    
    [Tooltip("How many items are allowed within the Density Check Radius.")]
    [SerializeField] private int maxFlowersPerCluster = 3;
    
    [Tooltip("Layer required for the Density Check to see other flowers.")]
    [SerializeField] private LayerMask flowerLayer; 

    // Private State Variables
    private HashSet<Vector3> flowersSpawnedPositions = new HashSet<Vector3>();
    private List<GameObject> activeFlowerList = new List<GameObject>(); 
    private float lastWaterTime;
    private int currentWaterHits = 0; // Tracks how wet the ground is

    private void Start()
    {
        // Initialize ground color to Dry
        if (groundRenderer != null)
        {
            groundRenderer.material.color = dryColor;
        }
    }

    private void OnParticleCollision(GameObject other)
    {
        ParticleSystem ps = other.GetComponent<ParticleSystem>();
        if (ps == null) return;
        
        List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();
        int numCollisionEvents = ps.GetCollisionEvents(gameObject, collisionEvents);
        
        if (numCollisionEvents > 0)
        {
            // 1. UPDATE VISUALS (Happens every single hit)
            currentWaterHits += numCollisionEvents;
            UpdateGroundColor();

            // 2. SPAWN LOGIC (Throttled by cooldown so we don't spawn 1000 flowers instantly)
            if (Time.time - lastWaterTime >= waterCooldown)
            {
                // Use the intersection of the first collision to determine spawn point
                Vector3 waterHitPos = collisionEvents[0].intersection;
                SpawnFlowers(waterHitPos);
                lastWaterTime = Time.time;
            }
        }
    }

    private void UpdateGroundColor()
    {
        if (groundRenderer == null) return;

        // Calculate percentage (0.0 to 1.0)
        float progress = Mathf.Clamp01((float)currentWaterHits / particlesToFullyGreen);

        // Blend color
        groundRenderer.material.color = Color.Lerp(dryColor, lushColor, progress);
    }
    
    private void SpawnFlowers(Vector3 centerPos)
    {
        // Clean up the list (remove any objects that were destroyed/deleted)
        activeFlowerList.RemoveAll(item => item == null);

        // GLOBAL CAP CHECK
        if (activeFlowerList.Count >= globalMaxFlowers) return;

        if (flowerPrefabs.Length == 0)
        {
            Debug.LogWarning("No flower prefabs assigned!");
            return;
        }
        
        int spawnCount = Random.Range(minFlowers, maxFlowers + 1);
        
        for (int i = 0; i < spawnCount; i++)
        {
            if (activeFlowerList.Count >= globalMaxFlowers) break;

            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPos = centerPos + new Vector3(randomCircle.x, 0.1f, randomCircle.y);
            
            // Snap to a small grid to prevent z-fighting or weird overlaps
            Vector3 gridPos = new Vector3(
                Mathf.Round(spawnPos.x * 4f) / 4f,
                spawnPos.y,
                Mathf.Round(spawnPos.z * 4f) / 4f
            );
            
            if (flowersSpawnedPositions.Contains(gridPos)) continue;

            // DENSITY CHECK
            Collider[] nearbyFlowers = Physics.OverlapSphere(spawnPos, densityCheckRadius, flowerLayer);
            if (nearbyFlowers.Length >= maxFlowersPerCluster) continue; 
            
            // RAYCAST TO FIND GROUND (Ensures items sit on the surface)
            if (Physics.Raycast(spawnPos + Vector3.up * 2f, Vector3.down, out RaycastHit hit, 5f, groundLayer))
            {
                GameObject prefabToSpawn = flowerPrefabs[Random.Range(0, flowerPrefabs.Length)];
                GameObject newObject = Instantiate(prefabToSpawn, hit.point, Quaternion.Euler(0, Random.Range(0, 360), 0));
                
                activeFlowerList.Add(newObject);
                
                // Physics Setup (Preserving Claude's logic)
                Rigidbody rb = newObject.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.constraints = RigidbodyConstraints.FreezeRotation;
                    rb.isKinematic = false; 
                }
                
                StartCoroutine(GrowFlower(newObject));
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
            if (flower == null) yield break; 

            elapsed += Time.deltaTime;
            float t = elapsed / growDuration;
            // Elastic bounce effect
            float scale = Mathf.Sin(t * Mathf.PI * 0.5f); 
            flower.transform.localScale = targetScale * scale;
            yield return null;
        }
        
        if(flower != null)
            flower.transform.localScale = targetScale;
    }
    
    public void ResetFlowers()
    {
        foreach(var flower in activeFlowerList)
        {
            if(flower != null) Destroy(flower);
        }
        activeFlowerList.Clear();
        flowersSpawnedPositions.Clear();
        currentWaterHits = 0; // Reset visual wetness too
        if (groundRenderer != null) groundRenderer.material.color = dryColor;
    }
}