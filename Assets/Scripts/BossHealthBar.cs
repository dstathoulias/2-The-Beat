using UnityEngine;
using UnityEngine.UI;

public class BossHealthBar : MonoBehaviour
{
    public Slider healthBarSlider;  // Boss healthbar UI
    public BossController bossController;


    void OnEnable()
    {
        bossController = FindAnyObjectByType<BossController>();
        bossController.OnHit += UpdateHealthBar;
        bossController.OnPhaseTransition += ResetPhase2;
        SetPhase1();
    }


    // Set phase 1 healthbar UI using hits required to end phase from boss controller
    void SetPhase1()
    {
        healthBarSlider.maxValue = bossController.hitsToPhase2;
        healthBarSlider.value = bossController.hitsToPhase2;
    }


    // Set phase 2 healthbar UI using hits required to end phase from boss controller
    void ResetPhase2()
    {
        healthBarSlider.maxValue = bossController.hitsToKill;
        healthBarSlider.value = bossController.hitsToKill;
    }


    // Update healthbar when boss is hit
    void UpdateHealthBar()
    {
        healthBarSlider.value = healthBarSlider.maxValue - bossController.hitCount;
    }
}
