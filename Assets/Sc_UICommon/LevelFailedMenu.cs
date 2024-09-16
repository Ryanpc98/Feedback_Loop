using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelFailedMenu : MonoBehaviour
{
    public GameObject failedMenuUI;
    int zone;

    public void Continue()
    {
        string zoneScene = "";
        switch (zone)
        {
            case 1:
                zoneScene = "ZoneOne";
                break;
            case 2:
                zoneScene = "ZoneTwo";
                break;
            case 3:
                zoneScene = "ZoneThree";
                break;
            default:
                zoneScene = "MainMenu";
                break;
        }
        Debug.Log("Level Failed, Moving to " + zoneScene);
        failedMenuUI.SetActive(false);
        Time.timeScale = 1f;
        PauseMenu.gameIsPaused = false;
        SceneManager.LoadScene(zoneScene);
    }
    public void EnableMenu(int i)
    {
        zone = i;
        Debug.Log("Level Failed, Screen Brought Up");
        failedMenuUI.SetActive(true);
        Time.timeScale = 0f;
        PauseMenu.gameIsPaused = true;
    }
    public void ReturnToMenu()
    {
        Debug.Log("Level Failed, Returning to Menu");
        failedMenuUI.SetActive(false);
        Time.timeScale = 1f;
        PauseMenu.gameIsPaused = false;
        SceneManager.LoadScene("MainMenu");
    }
}
