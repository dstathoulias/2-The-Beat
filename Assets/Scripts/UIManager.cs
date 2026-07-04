using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Score UI")]
    public TextMeshProUGUI player1ScoreText;
    public TextMeshProUGUI player2ScoreText;

    [Header("Pickup UI")]
    public TextMeshProUGUI player1PickupText;
    public TextMeshProUGUI player2PickupText;

    [Header("Ability UI")]
    public GameObject player1AbilityUI;
    public TextMeshProUGUI player1CooldownText;
    public GameObject player2AbilityUI;
    public TextMeshProUGUI player2CooldownText;


    // Active/Inactive UI elements references
    private TextMeshProUGUI activeScoreText;
    private TextMeshProUGUI activePickupText;
    private TextMeshProUGUI inactiveScoreText;
    private TextMeshProUGUI inactivePickupText;
    private PlayerAbility activeAbility;
    private TextMeshProUGUI activeCooldownText;
    private GameObject inactiveAbilityUI;


    void Start()
    {
        player1ScoreText.gameObject.SetActive(false);
        player2ScoreText.gameObject.SetActive(false);
        player1PickupText.gameObject.SetActive(false);
        player2PickupText.gameObject.SetActive(false);

        player1AbilityUI.SetActive(false);
        player2AbilityUI.SetActive(false);
    }


    void Update()
    {
        if (activePickupText != null)
        {
            UpdatePickUpCount();
        }

        UpdateAbilityCooldown();
    }


    // Handle player specific UI elements method
    public void SetActivePlayerUI(PlayerController player)
    {
        Image activeIcon;

        // Activate orb count, ability sprite and cooldown for active player (boss fight).
        // Activate score count for rhythm minigame player
        if (player.player.name == "Player1")
        {
            activeScoreText = player2ScoreText;
            activePickupText = player1PickupText;
            inactiveScoreText = player1ScoreText;
            inactivePickupText = player2PickupText;

            player1AbilityUI.SetActive(true);
            player2AbilityUI.SetActive(false);
            activeAbility = player.GetComponentInChildren<PlayerAbility>();
            activeCooldownText = player1CooldownText;
            inactiveAbilityUI = player2AbilityUI;
            activeIcon = player1AbilityUI.GetComponentInChildren<Image>();
        }
        else
        {
            activeScoreText = player1ScoreText;
            activePickupText = player2PickupText;
            inactiveScoreText = player2ScoreText;
            inactivePickupText = player1PickupText;

            player1AbilityUI.SetActive(false);
            player2AbilityUI.SetActive(true);
            activeAbility = player.GetComponentInChildren<PlayerAbility>();
            activeCooldownText = player2CooldownText;
            inactiveAbilityUI = player1AbilityUI;
            activeIcon = player2AbilityUI.GetComponentInChildren<Image>();
        }

        activeScoreText.gameObject.SetActive(true);
        activePickupText.gameObject.SetActive(true);
        inactiveScoreText.gameObject.SetActive(false);
        inactivePickupText.gameObject.SetActive(false);
        inactiveAbilityUI.SetActive(false);
        activeCooldownText.gameObject.SetActive(false);

        // Activate the appropriate ability sprite based on ability component of active character model
        if (activeAbility != null && activeAbility.abilityIcon != null)
        {
            activeIcon.sprite = activeAbility.abilityIcon;
        }
    }


    // Handle rhythm minigame score UI display
    public void UpdateScore(int score)
    {
        if (activeScoreText != null)
        {
            // Display score range of (0-9) instead of (1-10)
            if (score == 10)
            {
                score = 0;
            }
            activeScoreText.text = $"Score: {score}";
        }
    }


    // Update orb count UI helper method
    public void UpdatePickUpCount()
    {
        int count = FindObjectsByType<PickUpController>().Length;   // Find how many orb clones exist currently in scene
        activePickupText.text = $"Orbs: {count}";   
    }


    // Update ability countdown UI helper method
    void UpdateAbilityCooldown()
    {
        if (activeAbility == null) return;

        bool onCooldown = activeAbility.cooldownTimer > 0f;
        // if ability is on cooldown, display cooldown timer. Else, deactivate cooldown timer
        activeCooldownText.gameObject.SetActive(onCooldown);

        // Convert cooldown timer to int
        if (onCooldown)
        {
            activeCooldownText.text = Mathf.CeilToInt(activeAbility.cooldownTimer).ToString();
        }
    }
}