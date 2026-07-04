using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    public int score = 0;
    public int hitsToSpawn = 10; // Number of hits required to spawn a new orb

    public PickUpSpawner pickUpSpawner;
    public UIManager uiManager;


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }


    // Method to handle score
    public void RegisterHit()
    {
        // Increment score and update score UI
        score++;
        uiManager.UpdateScore(score);

        // if score to spawn is reached, spawn orb and reset score
        // Score works as a counter for how many hits have been scored towards spawning an orb
        if (score >= hitsToSpawn)
        {
            pickUpSpawner.SpawnPickUp();
            ResetScore();
        }
    }


    // helper method to reset score
    public void ResetScore()
    {
        score = 0;
    }
}
