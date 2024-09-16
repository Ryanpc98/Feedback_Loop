using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class CharacterSelectController : MonoBehaviour
{
    public TextMeshProUGUI outputOne;
    public TextMeshProUGUI outputTwo;
    public TextMeshProUGUI outputThree;

    public TextMeshProUGUI levelName;

    private List<TurnHandler.CharType> actors = new List<TurnHandler.CharType>();

    private SaveData data = new SaveData();

    public void Start()
    {
        for (int i = 0; i < 3; i++)
        {
            actors.Add(TurnHandler.CharType.Basic);
        }

        LoadFromSaveData(SaveManager.LoadJsonData());

        levelName.text = data.d_level_name;
    }

    private SaveData PopulateSaveData()
    {
        SaveData sd = new SaveData();

        //Adaptations
        sd.d_adaptations.increasedRAM = data.d_adaptations.increasedRAM;
        sd.d_adaptations.biggerHardDrives = data.d_adaptations.biggerHardDrives;
        sd.d_adaptations.hotSpares = data.d_adaptations.hotSpares;
        sd.d_adaptations.upgradedFirewall = data.d_adaptations.upgradedFirewall;
        sd.d_adaptations.efficientRouting = data.d_adaptations.efficientRouting;

        //Player Bots
        sd.d_roster = actors;

        //Zone Info
        sd.d_zone_index = data.d_zone_index;

        //Level Info
        sd.d_zone_one_ld = data.d_zone_one_ld;
        sd.d_zone_two_ld = data.d_zone_two_ld;
        sd.d_zone_three_ld = data.d_zone_three_ld;

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

        Debug.Log("======ADAPT RAM: " + data.d_adaptations.increasedRAM);
        Debug.Log("======ADAPT HDD: " + data.d_adaptations.biggerHardDrives);
        Debug.Log("======ADAPT HSP: " + data.d_adaptations.hotSpares);
        Debug.Log("======ADAPT FIR: " + data.d_adaptations.upgradedFirewall);
        Debug.Log("======ADAPT ROU: " + data.d_adaptations.efficientRouting);

        //Player Bots
        data.d_roster = sd.d_roster;

        // Zone Info
        data.d_zone_index = sd.d_zone_index;

        //Level Info
        data.d_zone_one_ld = sd.d_zone_one_ld;
        data.d_zone_two_ld = sd.d_zone_two_ld;
        data.d_zone_three_ld = sd.d_zone_three_ld;

        //Level Name
        data.d_level_name = sd.d_level_name;
    }

    public void HandleInputDataOne(int val)
    {
        if (val == 0)
        {
            actors[0] = TurnHandler.CharType.Basic;
            outputOne.text = "Basic";
        }
        if (val == 1)
        {
            actors[0] = TurnHandler.CharType.DPS;
            outputOne.text = "DPS";
        }
        if (val == 2)
        {
            actors[0] = TurnHandler.CharType.Tank;
            outputOne.text = "Tank";
        }
        if (val == 3)
        {
            actors[0] = TurnHandler.CharType.Healer;
            outputOne.text = "Healer";
        }
    }

    public void HandleInputDataTwo(int val)
    {
        if (val == 0)
        {
            actors[1] = TurnHandler.CharType.Basic;
            outputTwo.text = "Basic";
        }
        if (val == 1)
        {
            actors[1] = TurnHandler.CharType.DPS;
            outputTwo.text = "DPS";
        }
        if (val == 2)
        {
            actors[1] = TurnHandler.CharType.Tank;
            outputTwo.text = "Tank";
        }
        if (val == 3)
        {
            actors[1] = TurnHandler.CharType.Healer;
            outputTwo.text = "Healer";
        }
    }

    public void HandleInputDataThree(int val)
    {
        if (val == 0)
        {
            actors[2] = TurnHandler.CharType.Basic;
            outputThree.text = "Basic";
        }
        if (val == 1)
        {
            actors[2] = TurnHandler.CharType.DPS;
            outputThree.text = "DPS";
        }
        if (val == 2)
        {
            actors[2] = TurnHandler.CharType.Tank;
            outputThree.text = "Tank";
        }
        if (val == 3)
        {
            actors[2] = TurnHandler.CharType.Healer;
            outputThree.text = "Healer";
        }
    }

    public void HandleStartButton()
    {
        SaveManager.SaveJsonData(PopulateSaveData());
        SceneManager.LoadScene("DevLevel");
    }
}
