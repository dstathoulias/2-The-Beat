using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class BossController : MonoBehaviour
{
    // Boss States
    public enum BossState {Idle, Attacking, Chasing, Casting, Stunned, Crouching }
    public BossState currentState = BossState.Idle;

    public UnityEngine.AI.NavMeshAgent agent;   // Boss NavMesh agent used for movement and pathfinding
    public GameObject player1;
    public GameObject player2;
    public AudioSource audioSource; // Audio source for playing boss SFX
    
    [Header("Chase Settings")]
    public float chaseSpeed = 10f;
    public float bossChaseDelay = 5f;   // Initial chase delay after boss spawns. Give player time to create distance
                                        // if necessary. Used in game controller script 

    [Header("Melee Attack")]
    public float meleeRange = 2f;
    public float meleeCooldown = 2f;
    private float meleeTimer = 0f;
    public AudioClip meleeAttackSound;


    [Header("Ranged Attack")]
    public float rangedCooldown = 8f;
    public float rangedRange = 30f;
    public float SlowZoneRadius = 3f;
    public float slowZoneDuration = 5f;
    public float indicatorDuration = 1f;
    public GameObject slowZonePrefab;
    public GameObject indicatorPrefab;
    private float rangedTimer = 0f;
    public AudioClip rangedAttackSound;

    [Header("Stun Settings")]
    public float stunDuration = 2f;
    private float stunTimer = 0f;

    [Header("Phase Transition")]
    public int hitsToPhase2 = 8;    // orb hits required to end phase 1
    public int hitsToKill = 12; // orb hits required to kill boss in phase 2
    public int hitCount = 0;
    public int currentPhase = 1;
    public float transitionTimer = 5f;
    private bool isCrouching = false;
    public AudioClip phaseTransitionSound;
    public AudioClip deathSound;
    public GameObject phase2EffectPrefab;   // damaged VFX in phase 2

    [Header("Knockback Settings")]
    public float knockbackForce = 8f;
    public float knockbackDuration = 0.3f;

    // Events declaration
    public System.Action OnMeleeAttack;
    public System.Action OnRangedAttack;
    public System.Action OnBossPhase1End;
    public System.Action OnBossPhase2End;
    public System.Action OnBossDeath;
    public System.Action OnPhaseTransition;
    public System.Action OnHit;

    private Transform currentTarget;    // transform of the currently targeted player (closest)


    void Start()
    {
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        agent.speed = chaseSpeed;
        TransitionTo(BossState.Idle);   // Start in idle state

        player1 = GameObject.Find("Player1");
        player2 = GameObject.Find("Player2");

        audioSource = GetComponent<AudioSource>();
    }


    void Update()
    {
        currentTarget = GetClosestPlayer(); // Update closest player transform

        meleeTimer -= Time.deltaTime;
        rangedTimer -= Time.deltaTime;

        switch (currentState)
        {
            case BossState.Idle:
                UpdateIdle();
                break;
            case BossState.Chasing:
                UpdateChasing();
                break;
            case BossState.Attacking:
                UpdateAttacking();
                break;
            case BossState.Casting:
                UpdateCasting();
                break;
            case BossState.Stunned:
                UpdateStunned();
                break;
            case BossState.Crouching:
                break;
        }
    }


    // Idle state update method
    void UpdateIdle()
    {   
        // Force boss to not move
        agent.ResetPath();
        agent.speed = 0f;

        if (currentTarget == null) return;

        // Rotate boss to face the closest player while idle
        Vector3 directionToPlayer = (currentTarget.position - transform.position).normalized;
        directionToPlayer.y = 0f;
        Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
    }


    // Chasing update method
    void UpdateChasing()
    {
        if (currentTarget == null)  return;

        // Set boss speed to chase speed. Needed when prior state set it to 0
        agent.speed = chaseSpeed;
        // Set NavMesh agent target to closest player
        agent.SetDestination(currentTarget.position);

        // Calculate distance to closest player
        float distance = Vector3.Distance(transform.position, currentTarget.position);

        // If in range for a melee attack and not on cooldown, execute
        if (distance <= meleeRange && meleeTimer <= 0f)
        {
            TransitionTo(BossState.Attacking);
            return;
        }

        // Ranged attack is only availiable in phase 2.
        // If in range for a ranged attack while not in range of a melee attack, and not on cooldown, execute
        if (currentPhase == 2 && distance <= rangedRange && rangedTimer <= 0f)
        {
            TransitionTo(BossState.Casting);
            return;
        }
    }


    // Helper method to transition boss states
    void TransitionTo(BossState newState)
    {
        currentState = newState;
        if (currentState == BossState.Stunned)
        {
            // If new state is Stunned, set stun duration
            stunTimer = stunDuration;
            // Disabling and enabling NavMesh agent is needed to instantly force boss to stop moving (due to acceleration).
            // Does setting agent acceleration and speed to 0 here create the same effect while not having to disable agent?
            agent.enabled = false;
            agent.enabled = true;
            agent.ResetPath();
        }
    }


    // Method to find closest player transform
    Transform GetClosestPlayer()
    {
        if (player1 == null) return player2.transform;
        if (player2 == null) return player1.transform;

        float distanceToP1 = Vector3.Distance(transform.position, player1.transform.position);
        float distanceToP2 = Vector3.Distance(transform.position, player2.transform.position);

        if (distanceToP1 < distanceToP2)
            return player1.transform;
        else
            return player2.transform;
    }


    // Method to be used when a THROWN orb collides with boss collider.
    // Called from PickUpController
    public void TakeHit()
    {
        if (!isCrouching)   // Boss cannot be hit and execute relevant logic during phase transition and death.
                            // This is also required to prevent double damage ability from causing boss to skip 
                            // phase 2 transition.
        {
            hitCount++;
            OnHit?.Invoke();    // Invoke boss hit event

            // If hit would kill boss phase 1, transition to phase 2
            if (hitCount >= hitsToPhase2 && currentPhase == 1)
            {
                TransitionTo(BossState.Crouching);
                TransitionToPhase2();
                return;
            }
            // If hit would kill boss phase 2, transition to kill boss logic
            else if (hitCount >= hitsToKill && currentPhase == 2)
            {
                TransitionTo(BossState.Crouching);
                DestroyBoss();
                return;
            }

            // If hit doesnt end a phase, execute stun boss on hit logic
            StunBoss();
        }
    }


    // Transition to stun state
    void StunBoss()
    {
        TransitionTo(BossState.Stunned);
    }


    // Phase 2 transition logic
    void TransitionToPhase2()
    {
        // Update current phase, reset hit count and ranged attack cooldown, and flag phase transition
        currentPhase = 2;
        hitCount = 0;
        rangedTimer = rangedCooldown + transitionTimer;
        isCrouching = true;
        StartCoroutine(PhaseTransitionRoutine());
    }


    // Melee attack state update
    void UpdateAttacking()
    {
        agent.ResetPath();

        if (currentTarget == null) return;

        // Find closest player distance
        float distance = Vector3.Distance(transform.position, currentTarget.position);

        // If not in range for a melee attack, chase
        if (distance > meleeRange)
        {
            TransitionTo(BossState.Chasing);
            return;
        }

        // if in range, execute melee attack logic and reset cooldown
        if (meleeTimer <= 0f)
        {
            PerformMeleeAttack();
            meleeTimer = meleeCooldown;
        }
    }


    // Ranged attack state logic
    void UpdateCasting()
    {
        agent.ResetPath();

        // Ensure ranged attack is used when there is a valid target and only in phase 2
        if (currentTarget == null || currentPhase != 2) return;

        // Find closest player distance
        float distance = Vector3.Distance(transform.position, currentTarget.position);

        // if not in range for a ranged attack or ranged attack is on cooldown, chase
        if (distance > rangedRange || rangedTimer > 0f)
        {
            TransitionTo(BossState.Chasing);    
            return;
        }

        // If in range and ranged attack is available, execute ranged attack logic and reset cooldown
        if (rangedTimer <= 0f)
        {
            PerformRangedAttack();
            rangedTimer = rangedCooldown;
        }
    }


    // Melee attack logic method
    void PerformMeleeAttack()
    {
        if (currentTarget == null) return;
        OnMeleeAttack?.Invoke();    // Invoke melee attack event
        audioSource.PlayOneShot(meleeAttackSound);  // Play melee attack SFX

        // Get closest player rigidbody to execute knockback physics
        Rigidbody playerRb = currentTarget.GetComponent<Rigidbody>();
        if (playerRb != null)
        {   
            // Calculate knockback direction
            Vector3 knockbackDirection = (currentTarget.position - transform.position).normalized;
            knockbackDirection.y = 0f;  // Prevent attack from launching the player vertically
            StartCoroutine(KnockbackRoutine(playerRb, knockbackDirection)); // Execute knockback routine using closest player's rigidbody
                                                                            // and knockback direction vector
        }
    }


    // Ranged attack logic method
    void PerformRangedAttack()
    {
        if (currentTarget == null) return;
        OnRangedAttack?.Invoke();   // Invoke ranged attack event
        audioSource.PlayOneShot(rangedAttackSound); // Play ranged attack SFX
        StartCoroutine(RangedAttackRoutine());  // Execute ranged attack routine
    }


    // knockback routine
    IEnumerator KnockbackRoutine(Rigidbody playerRb, Vector3 direction)
    {
        // Find player controller component of closest player
        PlayerController playerController = playerRb.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.enabled = false;   // Temporarily disable input to properly execute knockback logic
            playerRb.AddForce(direction * knockbackForce, ForceMode.Impulse);   // Add force to the closest player's rigidbody
                                                                                // toward the knockback direction

            yield return new WaitForSeconds(knockbackDuration); // Wait for knockback to end

            playerRb.linearVelocity = Vector3.zero; // Reset player's velocity
            playerController.enabled = true;    // Re-enable input
        }
    }


    // Ranged attack routine
    IEnumerator RangedAttackRoutine()
    {
        if (currentTarget == null) yield break;

        // Find closest player's position vector
        Vector3 TargetPos = currentTarget.position;
        TargetPos.y = 0.1f; // Slightly elevate the position so the indicator and slow zone properly display
        GameObject indicator = Instantiate(indicatorPrefab, TargetPos, Quaternion.identity);    // Instantiate indicator prefab
                                                                                                // at target position

        yield return new WaitForSeconds(indicatorDuration); // Wait for indicator duration to end

        Destroy(indicator); // Destroy indicator clone

        GameObject slowZone = Instantiate(slowZonePrefab, TargetPos, Quaternion.identity);  // instantiate slow zone prefab
                                                                                            // at target position
        Destroy(slowZone, slowZoneDuration);    // Destroy slow zone clone after its duration ends
    }


    // Stunned state update
    void UpdateStunned()
    {   
        // Set agent speed to 0 and tick stun timer
        agent.speed = 0;
        stunTimer -= Time.deltaTime;

        // If stun timer runs out, reset agent speed to chasing speed and transition to chasing state
        if (stunTimer <= 0f)
        {
            agent.speed = chaseSpeed;
            StartChasing();
        }
    }


    // Transition to chasing state public method.
    // Also, used in game controller script
    public void StartChasing()
    {
        TransitionTo(BossState.Chasing);
    }


    // Destroy boss prefab clone public method
    public void DestroyBoss()
    {
        agent.enabled = false;  // Disable NavMesh agent
        audioSource.PlayOneShot(deathSound);    // Play death SFX
        OnBossPhase2End?.Invoke();  // Invoke phase 2 end event
        StartCoroutine(WaitAndDestroy());   // Execute destroy routine
    }


    // Destroy boss routine
    IEnumerator WaitAndDestroy()
    {
        yield return new WaitForSeconds(3f);    // Wait for 3 seconds to visually display boss dying (crouching)
        Destroy(gameObject);    // Destroy boss prefab clone
        OnBossDeath?.Invoke();  // Invoke boss death event
    }


    // Phase transition routine
    IEnumerator PhaseTransitionRoutine()
    {
        OnBossPhase1End?.Invoke();  // Invoke phase 1 end event
        agent.enabled = false;  // Temporarily disable NavMesh agent

        // Find boss model's spine component to attach damaged VFX for phase 2.
        // Attaching to spine is preferable so that VFX properly displays in the right height when boss is crouching
        Transform bossSpine = null;
        foreach (Transform t in GetComponentsInChildren<Transform>())
        {
            if (t.name == "CC_Base_Spine01")
            {
                bossSpine = t;
                break;
            }
        }

        // Set damaged VFX position, instantiate VFX prefab at target position and attach to spine
        Vector3 effectPosition = bossSpine.position + Vector3.up;
        GameObject effect = Instantiate(phase2EffectPrefab, effectPosition, Quaternion.identity);
        effect.transform.SetParent(bossSpine);

        yield return new WaitForSeconds(transitionTimer);   // Wait for the phase transition duration
        audioSource.PlayOneShot(phaseTransitionSound);  // Play boss power up SFX
        yield return new WaitForSeconds(phaseTransitionSound.length);   // Wait for the boss power up SFX duration

        OnPhaseTransition?.Invoke();    // Invoke phase transition event
        agent.enabled = true;   // Re-enable agent
        isCrouching = false;    // Flag end of transition
        TransitionTo(BossState.Chasing);    // Transition to chasing state when phase transition ends
    }
}
