using UnityEngine;
using UnityEngine.AI;

public class PetSpawner : MonoBehaviour
{
    public GameObject petPrefab;

    [Header("Spawn Area")]
    public Vector3 center = Vector3.zero;
    public float spawnRadius = 100f;
    
    [Header("Spawn Attempts")]
    public int maxAttempts = 30;


    // Public spawn pet method
    public void SpawnPet()
    {
        // Try until maxAttempts to find valid pet spawn position
        for (int i = 0; i < maxAttempts; i++)
        {
            // Randomly pick pet spawn position inside designated spawn area
            Vector3 randomPoint = center + Random.insideUnitSphere * spawnRadius;
            randomPoint.y = center.y;

            // Validate spawn position from NavMesh
            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 10f, NavMesh.AllAreas))
            {   
                // If valid, instantiate pet prefab on target position
                petPrefab = Instantiate(petPrefab, hit.position, Quaternion.identity);
                return;
            }
        }
    }
}
