using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class VictorySceneController : MonoBehaviour
{
    private SaveData data = new SaveData();

    private SaveData PopulateSaveData(int i)
    {
        SaveData sd = new SaveData();

        //Adaptations
        sd.d_adaptations.increasedRAM = data.d_adaptations.increasedRAM;
        sd.d_adaptations.biggerHardDrives = data.d_adaptations.biggerHardDrives;
        sd.d_adaptations.hotSpares = data.d_adaptations.hotSpares;
        sd.d_adaptations.upgradedFirewall = data.d_adaptations.upgradedFirewall;
        sd.d_adaptations.efficientRouting = data.d_adaptations.efficientRouting;

        //Player Bots
        sd.d_roster = data.d_roster;

        //Level Info (Likely Not Needed)
        sd.d_zone_one_ld = data.d_zone_one_ld;

        //Level Name
        sd.d_level_name = data.d_level_name;

        return sd;
    }

    private void LoadFromSaveData(SaveData sd)
    {
        //Adaptations
        data.d_adaptations.increasedRAM = sd.d_adaptations.increasedRAM;
        data.d_adaptations.biggerHardDrives = sd.d_adaptations.biggerHardDrives;
        data.d_adaptations.hotSpares = sd.d_adaptations.hotSpares;
        data.d_adaptations.upgradedFirewall = sd.d_adaptations.upgradedFirewall;
        data.d_adaptations.efficientRouting = sd.d_adaptations.efficientRouting;

        //Player Bots
        data.d_roster = sd.d_roster;

        //Level Info (Likely Not Needed)
        data.d_zone_one_ld = sd.d_zone_one_ld;

        //Level Name
        data.d_level_name = sd.d_level_name;
    }

    public void MainMenuButtonHandler()
    {
        SaveManager.WipeData();
        SceneManager.LoadScene("MainMenu");
    }

    public void ExitGame()
    {
        SaveManager.WipeData();
        Debug.Log("Quitting");
        Application.Quit();
    }
}
