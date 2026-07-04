using UnityEngine;

public class PickUpController : MonoBehaviour
{
    public bool isThrown = false;   // Flag to track if the orb has been thrown. If yes, ignore it


    void OnTriggerEnter(Collider other)
    {   
        // if player collides with orb collider and has not previously thrown it
        if (other.CompareTag("Player") && !isThrown)
        {
            // And if not currently holding it, pick it up
            if (other.GetComponent<HoldPickUp>().PickUpCollected())
            {
                Destroy(gameObject);    // Destroy orb clone
                return;
            }
        }

        // If orb has been thrown and it collides with arena barrier
        if (isThrown && other.CompareTag("ArenaBarrier"))
        {
            Destroy(gameObject);    // Destroy the orb 
        }
        // if orb has been thrown and it collides with boss collider
        else if (isThrown && other.CompareTag("Boss"))
        {
            BossController boss = other.GetComponent<BossController>();
            DoubleDamageAbility ability = FindAnyObjectByType<DoubleDamageAbility>();


            // Check if double damage ability is currently active. If so, boss takes 2 hits
            if (ability != null && ability.isDoubleDamageReady)
            {
                boss.TakeHit(); // Hit 1
                boss.TakeHit(); // hit 2
                ability.DeactivateDoubleDamage();   // Deactivate double damage ability
            }
            else    // if not
            {
                boss.TakeHit(); // Boss takes 1 hit
            }
            
            Destroy(gameObject);    // Destroy orb
        }
    }


    // Public helper method to destroy orb clone
    public void DestroyPickUp()
    {
        Destroy(gameObject);
    }
}
