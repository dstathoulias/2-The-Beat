using UnityEngine;
using System.Collections;

public class SpeedBoostAbility : PlayerAbility
{
    public float speedMultiplier = 2f;  // Player movement speed ability mutliplier
    public float duration = 5f;

    private PlayerController playerController;

    private bool isActive = false;

    public AudioSource audioSource;
    public AudioClip speedBoostSound;   // Speed boost ability SFX
    private GameObject activeEffect;    // Track if ability VFX is already active

    public GameObject speedBoostEffectPrefab;   // Speed boost ability VFX


    void Start()
    {
        playerController = GetComponentInParent<PlayerController>();
        audioSource = GetComponent<AudioSource>();
    }


    void Update()
    {
        // Tick ability cooldown timer if its on cooldown
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
        }
    }


    // Use double damage ability method
    public override void UseAbility()
    {
        // if ability is not available yet, is on cooldown or its effect is already active, do not execute
        if (!isEnabled || isActive || cooldownTimer > 0f)
        {
            return;
        }

        StartCoroutine(SpeedBoostCoroutine());  // Execute speed boost logic
    }


    // Speed boost ability routine
    IEnumerator SpeedBoostCoroutine()
    {
        isActive = true;    // Activate ability
        cooldownTimer = cooldown;   // Start cooldown
        audioSource.PlayOneShot(speedBoostSound);   // Play ability SFX
        DisplayAbilityEffect(); // Display ability VFX
        float originalSpeed = playerController.movingSpeed;
        playerController.movingSpeed *= speedMultiplier;    // Apply speed boost
        yield return new WaitForSeconds(duration);  // Wait for the speed boost duration
        playerController.movingSpeed = originalSpeed;   //Reset original movement speed
        isActive = false;   // Deactivate ability
    }


    // Display ability VFX helper method
    void DisplayAbilityEffect()
    {
        Vector3 effectPosition = playerController.transform.position + Vector3.up * 0.6f;   // Set effect position
        activeEffect = Instantiate(speedBoostEffectPrefab, effectPosition, Quaternion.identity);    // instantiate prefab
        activeEffect.transform.SetParent(playerController.transform);   // Set player transform as parent
        Destroy(activeEffect, duration);
    }


    // Method to deactivate ability. called when speed boost duration ends
    public override void DeactivateAbility()
    {
        // if ability VFX is active, destroy VFX prefab clone
        if (activeEffect != null)
        {
            Destroy(activeEffect);
        }
    }
}
