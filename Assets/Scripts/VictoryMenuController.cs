using UnityEngine;

public class VictoryMenuController : MonoBehaviour
{
    void Start()
    {
        // Unlock and display cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }


    public void BackToMainMenu()
    {
        // Transition to start menu scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("StartScene");
    }


    public void QuitGame()
    {
        // Exit application
        Application.Quit();
    }
}
