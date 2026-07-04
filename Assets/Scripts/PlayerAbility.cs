using UnityEngine;

// Abstract player ability method. Speed boost ability, double damage ability overrides
public abstract class PlayerAbility : MonoBehaviour
{
    public float cooldown = 30f;
    public float cooldownTimer;
    public Sprite abilityIcon;  // Ability icon sprite

    public bool isEnabled = false;


    public abstract void UseAbility();

    public abstract void DeactivateAbility();
}
