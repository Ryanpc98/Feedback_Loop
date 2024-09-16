using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelCompleteMenu : MonoBehaviour
{
    public GameObject completeMenuUI;

    public void Continue()
    {
        Debug.Log("Moving to Adaptation");
        completeMenuUI.SetActive(false);
        Time.timeScale = 1f;
        PauseMenu.gameIsPaused = false;
        SceneManager.LoadScene("AdaptationSelector");
    }
    public void EnableMenu()
    {
        Debug.Log("Level Cleared, Screen Brought Up");
        completeMenuUI.SetActive(true);
        Time.timeScale = 0f;
        PauseMenu.gameIsPaused = true;
    }
    public void ReturnToMenu()
    {
        Debug.Log("Level cleared, Returning to Menu");
        completeMenuUI.SetActive(false);
        Time.timeScale = 1f;
        PauseMenu.gameIsPaused = false;
        SceneManager.LoadScene("MainMenu");
    }
}
