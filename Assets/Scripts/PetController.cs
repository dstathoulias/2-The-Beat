using UnityEngine;
using UnityEngine.AI;

public class PetController : MonoBehaviour
{
    private NavMeshAgent agent;
    private Transform closestPlayer;

    void Start()
    {
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
    }

    void Update()
    {   
        // Follow closest player
        closestPlayer = GetClosestPlayer();

        if (closestPlayer != null)
            agent.SetDestination(closestPlayer.position);
    }

    // Get closest player
    Transform GetClosestPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        Transform closest = null;
        float shortestDistance = Mathf.Infinity;

        foreach (GameObject player in players)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                closest = player.transform;
            }
        }

        return closest;
    }
}
