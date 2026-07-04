using UnityEngine;

public class BossAnimationController : MonoBehaviour
{
    public GameObject boss;
    public BossController bossController;
    private Animator animator;


    void Start()
    {
        animator = GetComponent<Animator>();
        bossController = boss.GetComponent<BossController>();

        // Subscribe to boss controller events
        bossController.OnMeleeAttack += ExecuteMeleeAttack;
        bossController.OnRangedAttack += ExecuteRangedAttack;
        bossController.OnBossPhase2End += Die;
        bossController.OnPhaseTransition += StartNewPhase;
        bossController.OnBossPhase1End += EndPhase;
    }


    void Update()
    {
        // Update boss movement animation based on agent speed.
        // Movement blend tree: 0 -> Idle, 1-> Walk
        animator.SetFloat("Speed", bossController.agent.speed);
    }


    void ExecuteMeleeAttack()
    {   
        // Execute melee attack animation
        animator.SetTrigger("Attack");
    }


    void ExecuteRangedAttack()
    {
        // Execute ranged attack animation
        animator.SetTrigger("Cast");
    }


    void Die()
    {
        // Execute death animation at the end of the second phase
        animator.SetBool("Die", true);
    }


    void StartNewPhase()
    {
        // Reset to movement blend tree animations when transitioning to the second phase
        animator.SetBool("Die", false);
    }


    void EndPhase()
    {
        // Execute downed animation at the end of the first phase
        animator.SetBool("Die", true);
    }
}
