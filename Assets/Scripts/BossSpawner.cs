using UnityEngine;
using UnityEngine.AI;

public class BossSpawner : MonoBehaviour
{
    [Header("Boss Settings")]
    public GameObject bossPrefab;
    public GameObject bossHealthBar;    // Boss healthbar UI

    [Header("Spawn Area")]
    public Vector3 center = Vector3.zero;
    public float spawnRadius = 100f;

    [Header("Spawn Attempts")]
    public int maxAttempts = 30;


    // Public spawn boss method
    public void SpawnBoss()
    {
        // Try until maxAttempts to find valid boss spawn position
        for (int i = 0; i < maxAttempts; i++)
        {
            // Randomly pick boss spawn position inside designated spawn area
            Vector3 randomPoint = center + Random.insideUnitSphere * spawnRadius;
            randomPoint.y = center.y;

            // Validate spawn position from NavMesh
            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 10f, NavMesh.AllAreas))
            {   
                // If valid, instantiate boss prefab on target position and activate boss healthbar UI
                bossPrefab = Instantiate(bossPrefab, hit.position, Quaternion.identity);
                bossHealthBar.SetActive(true);
                return;
            }
        }
    }
}