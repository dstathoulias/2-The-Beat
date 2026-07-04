using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{

    [Header("Cameras")]
    public Camera fpsCamera1;
    public Camera thirdPersonCamera1;
    public Camera fpsCamera2;
    public Camera thirdPersonCamera2;    
    public Camera rhythmCamera1;
    public Camera rhythmCamera2;

    // Player 1,2 instances of PlayerController script
    [Header("Players")]
    public PlayerController player1;
    public PlayerController player2;

    //false: 3rd person
    //true: 1st person
    private bool cameraState1;
    private bool cameraState2;

    public bool rhythmCameraActive1;
    public bool rhythmCameraActive2;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Set initial camera states at 3rd person view for both players
        fpsCamera1.enabled = false;
        thirdPersonCamera1.enabled = true;
        fpsCamera2.enabled = false;
        thirdPersonCamera2.enabled = true;
        rhythmCamera1.enabled = false;
        rhythmCamera2.enabled = false;

        // Set initial camera state variaables
        cameraState1 = false;
        cameraState2 = false;
        rhythmCameraActive1 = false;
        rhythmCameraActive2 = false;

        // Subscibe to camera switch event for each player
        player1.OnCameraSwitch += SwitchCamera1;
        player2.OnCameraSwitch += SwitchCamera2;
    }
    

    public void SwitchCamera1()
    {
        // When player 1 camera switch event is invoked, switch to the other camera view and update state
        // 1dt person -> 3rd person
        // 3rd person -> 1st person

        // Make sure the player can only switch perspectives while outside the rhythm minigame
        if (!rhythmCameraActive1){
            cameraState1 = !cameraState1;
            fpsCamera1.enabled = cameraState1;
            thirdPersonCamera1.enabled = !cameraState1;
        }
    }

    public void SwitchCamera2()
    {
        // When player 2 camera switch event is invoked, switch to the other camera view and update state
        // 1dt person -> 3rd person
        // 3rd person -> 1st person

        // Make sure the player can only switch perspectives while outside the rhythm minigame
        if (!rhythmCameraActive2)
        {
            cameraState2 = !cameraState2;
            fpsCamera2.enabled = cameraState2;
            thirdPersonCamera2.enabled = !cameraState2;
        }
    }

    public void SwitchRhythmCamera1()
    {
        // Handles switching between rhythm and active (1st or 3rd person) camera views for player 1.
        // If player 1 is in active player view switch to rhythm camera view.
        // If already in rhythm camera view, switch back to previous active camera view and update state
        rhythmCameraActive1 = !rhythmCameraActive1;
        if (rhythmCameraActive1)
        {
            fpsCamera1.enabled = false;
            thirdPersonCamera1.enabled = false;
            rhythmCamera1.enabled = true;
        }
        else
        {
            rhythmCamera1.enabled = false;
            if (cameraState1)
            {
                fpsCamera1.enabled = true;
            }
            else
            {
                thirdPersonCamera1.enabled = true;
            }
        }
    }

    public void SwitchRhythmCamera2()
    {
        // Handles switching between rhythm and active (1st or 3rd person) camera views for player 2.
        // If player 2 is in active player view switch to rhythm camera view.
        // If already in rhythm camera view, switch back to previous active camera view and update state
        rhythmCameraActive2 = !rhythmCameraActive2;
        if (rhythmCameraActive2)
        {
            fpsCamera2.enabled = false;
            thirdPersonCamera2.enabled = false;
            rhythmCamera2.enabled = true;
        }
        else
        {
            rhythmCamera2.enabled = false;
            if (cameraState2)
            {
                fpsCamera2.enabled = true;
            }
            else
            {
                thirdPersonCamera2.enabled = true;
            }
        }
    }
}
