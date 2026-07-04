using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    public GameObject rhythmMinigameObject;
    public PlayerController player1;
    public PlayerController player2;
    public PlayerAnimationController player1AnimationController;
    public PlayerAnimationController player2AnimationController;
    public CameraController cameraController;
    public BossSpawner bossSpawner;
    public UIManager uiManager;

    [Header("Runtime")]
    public GameObject boss;

    private Conductor conductor;
    private NoteSpawner noteSpawner;
    private BossController bossController;

    [Header("Timing")]
    public float phase1StartDelay = 5f;


    void Start()
    {
        // Import player character choices from start menu and activate matching model
        string player1Model = PlayerPrefs.GetString("Player1Model");
        string player2Model = PlayerPrefs.GetString("Player2Model");
        ActivateModel(player1, player1Model);
        ActivateModel(player2, player2Model);

        player1AnimationController = player1.GetComponentInChildren<PlayerAnimationController>();
        player2AnimationController = player2.GetComponentInChildren<PlayerAnimationController>();
        cameraController = GameObject.Find("CameraController").GetComponent<CameraController>();
        conductor = rhythmMinigameObject.GetComponent<Conductor>();
        noteSpawner = rhythmMinigameObject.GetComponent<NoteSpawner>();
        StartCoroutine(Phase1Routine());
    }


    // method to place and start the rhythm minigame for the referenced player
    void RhythmMinigameEnter(PlayerController player)
    {
        if (player == player1)
        {
            // Move rhythm player to rhythm minigame location and set looking direction
            MovePlayer(player1, new Vector3(-255f, 101.25f, 2f), Quaternion.Euler(0f, 90f, 0f));
            // Move other player to arena center and reset rotation
            MovePlayer(player2, new Vector3(0f, 0f, 0f), Quaternion.identity);

            player1AnimationController.RhythmMinigameEnter(player1); // Play sit animation for rhythm player
            player2AnimationController.StandUp();   // Transition from sit animation for other player

            // Switch to rhythm camera for rhythm player, and previously used POV camera for the other player
            cameraController.SwitchRhythmCamera1();
            if (cameraController.rhythmCameraActive2)
                cameraController.SwitchRhythmCamera2();

            player1.SwitchInput("Player1_Rhythm");  // Switch to rhythm controlls for rhythm player
            player2.SwitchInput("Player2"); // Switch to normal controlls for other player
            uiManager.SetActivePlayerUI(player2);   // Activate appropriate UI elements
            uiManager.UpdateScore(0);   // Initialize rhythm score
        }
        else if (player == player2) // Similarly to player == player1
        {
            MovePlayer(player2, new Vector3(-255f, 101.25f, 2f), Quaternion.Euler(0f, 90f, 0f));
            MovePlayer(player1, new Vector3(0f, 0f, 0f), Quaternion.identity);

            player2AnimationController.RhythmMinigameEnter(player2);
            player1AnimationController.StandUp();

            cameraController.SwitchRhythmCamera2();
            if (cameraController.rhythmCameraActive1)
                cameraController.SwitchRhythmCamera1();

            player2.SwitchInput("Player2_Rhythm");
            player1.SwitchInput("Player1");
            uiManager.SetActivePlayerUI(player1);
            uiManager.UpdateScore(0);
        }
    }


    // Helper method to set player location, rotation
    void MovePlayer(PlayerController player, Vector3 position, Quaternion rotation)
    {
        Rigidbody rb = player.GetComponent<Rigidbody>();
        rb.linearVelocity = Vector3.zero;
        rb.MovePosition(position);
        rb.MoveRotation(rotation);
    }


    // Play referenced rhythm track
    void StartRhythmTrack(string trackName)
    {
        int bpm;
        AudioClip clip;

        // Set conductor clip and track BPM depending on the track name argument
        if (trackName == "Track1")
        {
            bpm = 110;
            clip = conductor.track1;
        }
        else if (trackName == "Track2")
        {
            bpm = 180;
            clip = conductor.track2;
        }
        else
        {
            Debug.LogError($"Unknown track name: {trackName}");
            return;
        }

        noteSpawner.ResetSpawner(); // Reset note spawning
        conductor.SwitchTrack(clip, bpm);   // Play track and set the corresponding BPM
    }


    // Spawn boss call method
    void SpawnBoss()
    {
        bossSpawner.SpawnBoss();
    }


    void SpawnPet()
    {
        GameObject.Find("PetSpawner").GetComponent<PetSpawner>().SpawnPet();
    }


    // Phase transition method
    void HandlePhaseTransition()
    {
        RhythmMinigameEnter(player2);   // In phase 2 place player 2 in rhythm minigame   
        StartRhythmTrack("Track2"); // Play track 2

        // Deactivate ability if active to destroy VFX clone
        player1.GetComponentInChildren<PlayerAbility>().DeactivateAbility();    
        player2.GetComponentInChildren<PlayerAbility>().DeactivateAbility();

        // Destroy all orb clones
        foreach (PickUpController pickup in FindObjectsByType<PickUpController>())
        {
            pickup.DestroyPickUp();
        }
    }


    // Stop track method
    void HandleRhythmEnd()
    {
        conductor.StopTrack();  // Stop conductor clip

        // Destroy all note clones
        foreach (NoteController note in FindObjectsByType<NoteController>())
        {
            note.DestroyNote();
        }
    }


    // Helper method to activate the referenced player model based on player character selection
    void ActivateModel(PlayerController playerController, string modelName)
    {
        if (modelName == "PlayerModelRed")
        {
            playerController.gameObject.transform.Find("PlayerModelRed").gameObject.SetActive(true);
            playerController.gameObject.transform.Find("PlayerModelBlue").gameObject.SetActive(false);
        }
        else if (modelName == "PlayerModelBlue")
        {
            playerController.gameObject.transform.Find("PlayerModelRed").gameObject.SetActive(false);
            playerController.gameObject.transform.Find("PlayerModelBlue").gameObject.SetActive(true);
        }
    }


    // Transition to victory screen scene when boss dies
    void HandleBossDeath()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("VictoryScene");
    }


    // Stop track when transitioning to phase 2
    void HandleBossPhase2End()
    {
        conductor.StopTrack();
    }


    // Start boss phase 1 routine
    IEnumerator Phase1Routine()
    {
        yield return new WaitForSeconds(phase1StartDelay);  // Wait for phase 1 spawn delay before boss spawns

        RhythmMinigameEnter(player1);   // Move player 1 to rhtyhm minigame in phase 1
        StartRhythmTrack("Track1"); // Play track 1
        SpawnBoss();    // spawn boss
        SpawnPet();   // Spawn pet

        // Enable player 2 ability
        player1.GetComponentInChildren<PlayerAbility>().isEnabled = true;
        player2.GetComponentInChildren<PlayerAbility>().isEnabled = true;

        boss = GameObject.FindWithTag("Boss");
        bossController = boss.GetComponent<BossController>();
        if (boss == null)
        {
            Debug.LogError("Boss not found!");
            yield break;
        }

        yield return new WaitForSeconds(bossController.bossChaseDelay); // Wait for phase 1 chase delay after boss spawns

        bossController.StartChasing();  // Execute boss chasing logic

        // Subscribe to boss controller events
        bossController.OnPhaseTransition += HandlePhaseTransition;
        bossController.OnBossPhase1End += HandleRhythmEnd;
        bossController.OnBossDeath += HandleBossDeath;
        bossController.OnBossPhase2End += HandleBossPhase2End;
    }
}