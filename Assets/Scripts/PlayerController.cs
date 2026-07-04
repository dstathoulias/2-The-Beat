using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;


public class PlayerController : MonoBehaviour
{
    public PlayerInput input;
    public GameObject player;
    public PlayerController otherPlayer;    // Other player object reference
    private Rigidbody rb;

    private InputAction move;
    private InputAction look;
    private InputAction interact;
    private InputAction attack;
    private InputAction switchCamera;
    private InputAction pause1;
    private InputAction pause2;

    public Vector2 moveInput;
    public Vector2 lookInput;

    public bool isInteracting;

    public float movingSpeed;
    public float rotationSpeed;

    public bool isPaused;
    public string currentActionMap;

    // Define events to be subscribed to
    public System.Action OnCameraSwitch;
    public System.Action onPause;
    public System.Action onAttack;

    public PauseMenuController menuController;

    // Reference to conductor instance to handle audio source pausing and resuming
    public Conductor conductor;

    // Reference to rhythm hit detector to handle rhythm minigame player actions based on rhythm map input
    public RhythmHitDetector rhythmHitDetector;

    // Reference to HoldPickUp component to manage pick up state and throwing logic
    public HoldPickUp holdPickUp;
    public float attackAnimationDuration = 0.5f; // Duration of the attack animation in seconds

    private CapsuleCollider playerCollider;
    private float colliderRadius;

    public AudioSource audioSource;
    public AudioClip attackSound;


    // Awake is called when the script instance is being loaded
    public virtual void Awake()
    {
        // Initialize player specific input actions
        input.enabled = true;
        move = input.actions[$"{player.name}/Move"];
        look = input.actions[$"{player.name}/Look"];
        interact = input.actions[$"{player.name}/Interact"];
        attack = input.actions[$"{player.name}/Attack"];
        switchCamera = input.actions[$"{player.name}/Switch Camera"];
        pause1 = input.actions["Player1/Pause"]; // Both players can pause using the pause action (Escape)
                                                 // which is defined under Player1 input action map
        pause2 = input.actions["Player1_Rhythm/Pause"]; // Pause action is also defined under Player1_Rhythm map
                                                        // to allow for pausing while player1 is currently in the rhythm minigame

        playerCollider = GetComponent<CapsuleCollider>();
        colliderRadius = playerCollider.radius;
        rb = GetComponent<Rigidbody>();
        holdPickUp = GetComponent<HoldPickUp>();
        currentActionMap = input.currentActionMap.name;
        audioSource = GetComponent<AudioSource>();
    }


    void OnEnable()
    {
        // Enable player specific input actions. Not necessary but good practice

        SwitchInput(player.name); // Enable the correct input action map based on the player name

        // Disable and hide cursor on start
        LockCursor();

        // Subscribe to resume from pause menu event
        menuController.OnResume += CallResume;

        // Game is not paused so set flag to false
        isPaused = false;
    }


    void OnDisable()
    {
        // Disable player specific input actions. Not necessary but good practice
        move.Disable();
        look.Disable();
        interact.Disable();
        attack.Disable();
        switchCamera.Disable();
        pause1.Disable();
        pause2.Disable();
    }


    void FixedUpdate()
    {
        // Handle movement and looking based on input
        Vector3 moveDirection = (transform.forward * moveInput.y) + (transform.right * moveInput.x);
        moveDirection.Normalize();
        Vector3 moveVelocity = moveDirection * movingSpeed;
        moveVelocity.y = rb.linearVelocity.y;
        Quaternion rotationDirection = Quaternion.Euler(0f, lookInput.x * rotationSpeed * Time.deltaTime, 0f);

        rb.linearVelocity = moveVelocity;
        rb.MoveRotation(rb.rotation * rotationDirection);
    }


    void Update()
    {

        // If camera switch input is pressed, invoke camera switch event
        if (switchCamera.WasPressedThisFrame())
        {
            OnCameraSwitch?.Invoke();
        }

        // If pause input is pressed, call method to toggle game state
        // When the method is called through Update game state it active so it pauses
        if (pause1.WasPressedThisFrame() || pause2.WasPressedThisFrame())
        {
            TogglePause();
        }

        // If player is holding an orb and attack input is pressed, execute attack logic
        if (attack.WasPressedThisFrame() && holdPickUp.isHolding)
        {
            StartCoroutine(AttackRoutine());
        }
    }


    void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>().normalized;
    }


    void OnLook(InputValue value)
    {
        lookInput = value.Get<Vector2>();
    }


    // Handle player ability use input method
    void OnInteract(InputValue value)
    {
        UseAbility();
    }


    void LockCursor()
    {
        // Lock and hide cursor. Called when entering active game state
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }


    void UnlockCursor()
    {
        // Unlock and show cursor. Called when entering paused game state
        // The cursor is unlocked and visible by default when launching the game
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }


    void TogglePause()
    {
        // If game is not paused (when pressing pause input), calls method to unlock cursor, invokes pause event
        // and locks input actions
        // Player 1 check is necessary so only 1 player script instance can handle pause input
        if (!isPaused && player.name == "Player1")
        {
            UnlockCursor();
            input.enabled = false;
            otherPlayer.input.enabled = false;  // Disable other player's input as well
            onPause?.Invoke();
            Time.timeScale = 0f;    // Freeze game time
            isPaused = true;    // Game is now paused
            conductor.Pause();  // Pause the conductor to stop audio source
        }
        else if (isPaused && player.name == "Player1")
        {
        // If called when resuming event is invoked from PauseMenuController class, calls method to lock cursor
        // and unlock input actions
            LockCursor();
            input.enabled = true;
            otherPlayer.input.enabled = true;   // Enable other player's input as well
            SwitchInput(currentActionMap); // Ensure correct input action map is enabled based on player name
            otherPlayer.SwitchInput(otherPlayer.currentActionMap);  // Ensure other player's action map is appropriately set as well
            Time.timeScale = 1f;    // Resume game time
            isPaused = false;   // Game is now in active state
            conductor.Resume(); // Resume the conductor to start audio source
        }
    }

    void CallResume()
    {
        // Called when resuming event is invoked from menu controller class.
        // Calls TogglePause method to resume game and update game state
        TogglePause();
    }


    // Player attack routine
    IEnumerator AttackRoutine()
    {
        // Disable input during attack execution
        onAttack?.Invoke();
        move.Disable();
        look.Disable();
        interact.Disable();
        attack.Disable();

        holdPickUp.ThrowPickUp();   // Throw orb
        audioSource.PlayOneShot(attackSound);   // Play orb throw SFX
        yield return new WaitForSeconds(attackAnimationDuration);   // Wait for the attack animation duration

        // Re-enable input
        move.Enable();
        look.Enable();
        interact.Enable();
        attack.Enable();
    }


    // Method to switch current input action map
    public void SwitchInput(string map)
    {
        // Switch player 1 input to player 1 normal input action map
        if (map == "Player1" && player.name == "Player1")
        {
            input.SwitchCurrentActionMap("Player1");
            currentActionMap = "Player1";
            EnableMovementInput();
        }
        // Switch player 2 input to player 2 normal input action map
        else if (map == "Player2" && player.name == "Player2")
        {
            input.SwitchCurrentActionMap("Player2");
            currentActionMap = "Player2";
            EnableMovementInput();
            
        }
        // Swtich player 1 input to player 1 rhythm action map
        else if (map == "Player1_Rhythm" && player.name == "Player1")
        {
            input.SwitchCurrentActionMap("Player1_Rhythm");
            currentActionMap = "Player1_Rhythm";
            DisableMovementInput();
        }
        // Swtich player 2 input to player 2 rhythm action map
        else if (map == "Player2_Rhythm" && player.name == "Player2")
        {
            input.SwitchCurrentActionMap("Player2_Rhythm");
            currentActionMap = "Player2_Rhythm";
            DisableMovementInput();
        }
        else
        {
            Debug.LogError($"Invalid action map name: {map}");
        }
    }


    // Helper method to fetch current player action map
    public string GetCurrentActionMap()
    {
        return input.currentActionMap.name;
    }


    // Link rhythm input to rhythm lane and detect if note is hit
    void OnHitNote(InputValue value) 
    {
        Vector2 hitInput = value.Get<Vector2>();
        int lane = HitInputToLane(hitInput);

        if (lane == -1) return;

        rhythmHitDetector.CheckHit(lane);
    }


    private int HitInputToLane(Vector2 hitInput)
    {
        if (hitInput.x == -1) return 0; // Left lane
        if (hitInput.y == 1) return 1; // Up lane
        if (hitInput.x == 1) return 2; // Right lane
        return -1; // Invalid lane
    }


    // Handle ability use method
    public void UseAbility()
    {
        // if player has child with ability component active, use that ability
        PlayerAbility ability = GetComponentInChildren<PlayerAbility>();
        if (ability != null)
        {
            ability.UseAbility();
        }
    }


    // // Helper method to disable player input
    void DisableMovementInput()
    {
        move.Disable();
        interact.Disable();
        look.Disable();
        attack.Disable();
    }


    // Helper method to enable player input
    void EnableMovementInput()
    {
        move.Enable();
        interact.Enable();
        look.Enable();
        attack.Enable();
    }
}
