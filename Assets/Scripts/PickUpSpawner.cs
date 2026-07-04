using UnityEngine;

public class PickUpSpawner : MonoBehaviour
{

    [Header("PickUp Settings")]
    public GameObject pickUpPrefab;

    [Header("Spawn Area")]
    public Vector3 center = Vector3.zero;
    public float spawnRadius = 100f;

    [Header("Spawn Attempts")]
    public int maxAttempts = 30;


    // Public spawn pickup method
    public void SpawnPickUp()
    {
        // Try until maxAttempts to find valid pickup spawn position
        for (int i = 0; i < maxAttempts; i++)
        {
            // Randomly pick pickup spawn position inside designated spawn area
            Vector3 randomPoint = center + Random.insideUnitSphere * spawnRadius;
            randomPoint.y = center.y;

            // Validate spawn position from NavMesh
            if (UnityEngine.AI.NavMesh.SamplePosition(randomPoint, out UnityEngine.AI.NavMeshHit hit, 10f, UnityEngine.AI.NavMesh.AllAreas))
            {
                // If valid, instantiate pickup prefab on target position
                Vector3 spawnPos = hit.position + Vector3.up * 2f;
                Instantiate(pickUpPrefab, spawnPos, Quaternion.identity);
                return;
            }
        }
    }
}
