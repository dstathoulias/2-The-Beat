// StartMenuController.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenuController : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainMenu;
    public GameObject characterSelect;
    public GameObject tutorial;
    public GameObject credits;

    [Header("Character Select")]
    public GameObject startGameButton;

    // Character selection pressed highlights
    [Header("Player 1 - Selection Highlights")]
    public GameObject p1Model1Highlight;
    public GameObject p1Model2Highlight;

    [Header("Player 2 - Selection Highlights")]
    public GameObject p2Model1Highlight;
    public GameObject p2Model2Highlight;

    // Character model names. Matching model GameObject names
    public string model1Name = "PlayerModelRed";
    public string model2Name = "PlayerModelBlue";

    private string player1Model = null;
    private string player2Model = null;

    void Start()
    {
        ShowMainMenu();
    }


    public void ShowMainMenu()
    {
        // Activate main menu UI
        mainMenu.SetActive(true);
        characterSelect.SetActive(false);
        tutorial.SetActive(false);
        credits.SetActive(false);
    }

    public void ShowCharacterSelect()
    {
        // Activate character select menu UI
        mainMenu.SetActive(false);
        characterSelect.SetActive(true);
        player1Model = null;
        player2Model = null;
        startGameButton.SetActive(false);   // Deactivate start game button. Will be activated when both players pick a character
        UpdateSelectionHighlights(); // Reset highlights
    }

    public void ShowTutorial()
    {
        // Activate tutorial menu UI
        mainMenu.SetActive(false);
        tutorial.SetActive(true);
    }

    public void ShowCredits()
    {
        // Activate credits menu UI
        mainMenu.SetActive(false);
        credits.SetActive(true);
    }

    public void Back()
    {
        // Return to start menu UI from credits and tutorial UI
        ShowMainMenu();
    }

    public void ExitGame()
    {
        // Exit application
        Application.Quit();
    }


    // handle player character selections methods
    public void Player1SelectModel1() => SelectModel(ref player1Model, model1Name);
    public void Player1SelectModel2() => SelectModel(ref player1Model, model2Name);
    public void Player2SelectModel1() => SelectModel(ref player2Model, model1Name);
    public void Player2SelectModel2() => SelectModel(ref player2Model, model2Name);


    // Player character select helper method
    void SelectModel(ref string playerModel, string modelName)
    {
        playerModel = modelName;
        UpdateSelectionHighlights();
        CheckBothSelected();
    }


    // Display start game button logic method
    void CheckBothSelected()
    {
        // Only display start game button when both players have selected a character.
        // Choices can be changed until start game is pressed
        startGameButton.SetActive(player1Model != null && player2Model != null);
    }


    // Handle character select highlights method
    void UpdateSelectionHighlights()
    {
        // Only 1 highlight can be active for each player. If a player select another character while they have
        // already selected one, switch to new one.
        if (p1Model1Highlight != null) p1Model1Highlight.SetActive(player1Model == model1Name);
        if (p1Model2Highlight != null) p1Model2Highlight.SetActive(player1Model == model2Name);
        if (p2Model1Highlight != null) p2Model1Highlight.SetActive(player2Model == model1Name);
        if (p2Model2Highlight != null) p2Model2Highlight.SetActive(player2Model == model2Name);
    }


    public void StartGame()
    {
        // Check if both players selected a character
        if (player1Model == null || player2Model == null) return;

        // Set cross scene player model strings
        PlayerPrefs.SetString("Player1Model", player1Model);
        PlayerPrefs.SetString("Player2Model", player2Model);
        PlayerPrefs.Save();
        SceneManager.LoadScene("MainScene");    // Transition to main scene
    }
}