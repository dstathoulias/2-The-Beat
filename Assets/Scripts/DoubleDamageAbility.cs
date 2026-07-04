using UnityEngine;

public class DoubleDamageAbility : PlayerAbility
{
    public bool isDoubleDamageReady = false;    // Double damage ability active flag

    public AudioSource audioSource;
    public AudioClip doubleDamageSound; // Double damage ability SFX
    public GameObject doubleDamageEffectPrefab; // Double damage ability VFX
    public PlayerController playerController;
    public float effectDuration = 5f;
    private GameObject activeEffect;    // Track if ability VFX is already active


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
        if (!isEnabled || cooldownTimer > 0f || isDoubleDamageReady)
        {
            return;
        }

        // Set ability effect active, put on cooldown and play SFX, VFX
        isDoubleDamageReady = true;
        cooldownTimer = cooldown;
        audioSource.PlayOneShot(doubleDamageSound);
        DisplayAbilityEffect();
    }


    // Display double damage ability VFX
    void DisplayAbilityEffect()
    {
        Vector3 effectPosition = playerController.transform.position + Vector3.up * 0.6f;   // Set effect position
        activeEffect = Instantiate(doubleDamageEffectPrefab, effectPosition, Quaternion.identity);  // instantiate prefab
        activeEffect.transform.SetParent(playerController.transform);   // Set player transform as parent
    }


    // Method to deactivate ability. called when orb successfully hits boss with double damage active
    public override void DeactivateAbility()
    {
        isDoubleDamageReady = false;

        // if ability VFX is active, destroy VFX prefab clone
        if (activeEffect != null)
        {
            Destroy(activeEffect);
        }
    }


    // Wrapper method for deactivate double damage ability
    public void DeactivateDoubleDamage()
    {
        DeactivateAbility();
    }
}
