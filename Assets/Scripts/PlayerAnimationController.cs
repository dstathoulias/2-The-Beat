using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    public GameObject player;

    public PlayerController playerController;
    private Animator animator;

    public float animationSmoothTime = 0.05f;

    void Start()
    {
        animator = GetComponent<Animator>();
        playerController = player.GetComponent<PlayerController>();
        animator.SetBool("IsSitting", false);

        playerController.onAttack += ExecuteAttack;
    }


    void Update()
    {
        // While player is not sitting update movement blend tree moveX,Y values to update animations
        if (!animator.GetBool("IsSitting"))
        {
            Vector2 animationInput = playerController.moveInput;
            animator.SetFloat("MoveX", animationInput.x, animationSmoothTime, Time.deltaTime);
            animator.SetFloat("MoveY", animationInput.y, animationSmoothTime, Time.deltaTime);
        }
    }


    // Rhythm game animation handling method
    public void RhythmMinigameEnter(PlayerController RhythmPlayer)
    {
        // When called, if player is not in rhythm minigame initiate sitting animation.
        // If in rhythm minigame, transition to movement animations blend tree
        if (RhythmPlayer == playerController)
        {
            if (!animator.GetBool("IsSitting"))
            {
                animator.SetBool("IsSitting", true);
            }
            else if (animator.GetBool("IsSitting"))
            {
                animator.SetBool("IsSitting", false);
            }
        }
    }

    
    void ExecuteAttack()
    {
        // Transition to attack animation
        animator.SetTrigger("Attack");
    }


    public void StandUp()
    {
        // Transition to movement animations blend tree
        animator.SetBool("IsSitting", false);
    }
}