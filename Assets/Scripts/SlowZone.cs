using UnityEngine;
using System.Collections.Generic;

public class SlowZone : MonoBehaviour
{
    public float slowMultiplier = 0.5f; // Player movement speed multiplier while in the slow zone
    private List<PlayerController> slowedPlayers = new List<PlayerController>();    // List of currently slowed players.
                                                                                    // Used to check if a player is currently 
                                                                                    // being slowed so it wont apply multiple times
    

    void OnTriggerEnter(Collider other)
    {
        //  If player enters slow zone collider, reduce player movement speed
        if (other.CompareTag("Player"))
        {
            PlayerController playerController = other.GetComponent<PlayerController>();
            playerController.movingSpeed *= slowMultiplier;
            slowedPlayers.Add(playerController);    // Add to list of slowed players
        }
    }

    
    void OnTriggerExit(Collider other)
    {
        // if player leaves slow zone, reset player movement speed to original
        if (other.CompareTag("Player"))
        {
            PlayerController playerController = other.GetComponent<PlayerController>();
            playerController.movingSpeed /= slowMultiplier;
            slowedPlayers.Remove(playerController); // Remove from list of slowed players
        }
    }


    // Method handling slow zone destroy logic
    void OnDestroy()
    {
        // When a slow zone is destroyed, reset movement speed of slowed players in list.
        // Necessary if slow zone gets destroyed while a player is currenlty in its collider which would was
        // permanent movement speed reduction otherwise.
        foreach(PlayerController player in slowedPlayers)
        {
            if (player != null)
            {
                player.movingSpeed /= slowMultiplier;
            }
        }
    }
}
