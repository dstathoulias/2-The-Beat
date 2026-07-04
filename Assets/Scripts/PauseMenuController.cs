using System;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenuController : MonoBehaviour
{
    public Action OnResume; // Resume event to subscribe to 

    [Header("References")]
    public PlayerController player1;
    public GameObject menuUI;
    public GameObject pauseMenuUI;
    public GameObject tutorialUI;
    public PlayerController player2;

    [Header("Slider Settings")]
    public Slider slider;   // Mouse sensitivity slider for changing mouse sensitivity at runtime


    void Start()
    {
        // At the start of the scene disable the pause menu
        pauseMenuUI.SetActive(false);
        tutorialUI.SetActive(false);
        menuUI.SetActive(false);

        // Subscribe to pause event
        player1.onPause += EnableMenu;

        // Set mouse sensitivity slider min, max and current values
        slider.minValue = 0.1f;
        slider.maxValue = 30f;
        slider.value = player2.rotationSpeed;
        slider.onValueChanged.AddListener(OnSliderChanged); // Add listener to slider
    }


    void EnableMenu()
    {
        // When pause event is invoked activate the pause menu
        menuUI.SetActive(true);
        pauseMenuUI.SetActive(true);
    }


    public void Resume()
    {
        // When resuming from pause menu, deactivate pause menu and invoke resume event
        menuUI.SetActive(false);
        pauseMenuUI.SetActive(false);
        OnResume?.Invoke();
    }


    public void ExitGame()
    {
        // Exit application
        Application.Quit();
    }


    public void ShowTutorial()
    {
        // Set tutorial menu active
        pauseMenuUI.SetActive(false);
        tutorialUI.SetActive(true);
    }


    public void ExitTutorial()
    {
        // Exit tutorial menu and activate pause menu
        tutorialUI.SetActive(false);
        pauseMenuUI.SetActive(true);
    }


    void OnSliderChanged(float value)
    {
        // Change player 2 rotation speed (mouse sensitvity) to slider value when slider value is changed 
        player2.rotationSpeed = value;
    }
}
