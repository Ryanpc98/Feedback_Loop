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

    public GameObject tooptipPanel;

    [SerializeField] private GameObject[] infoBoxes;

    [SerializeField] private TMP_Dropdown[] dropdowns;

    private List<TurnHandler.CharType> actors = new List<TurnHandler.CharType>();

    private List<string> characterList = new List<string>();

    private SaveData data = new SaveData();

    public void Start()
    {
        for (int i = 0; i < 3; i++)
        {
            actors.Add(TurnHandler.CharType.Basic);
        }

        LoadFromSaveData(SaveManager.LoadJsonData());

        PopulateChatacterList();
        PopulateDDList();

        levelName.text = "Now Entering: " + data.d_level_name;
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

    private void UpdateActorList(int val, int i)
    {
        int picked_EntryIndex = dropdowns[i].value;

        outputOne.text = dropdowns[i].options[picked_EntryIndex].text;

        switch (dropdowns[i].options[picked_EntryIndex].text)
        {
            case ("Soldier"):
                actors[i] = TurnHandler.CharType.Basic;
                break;
            case ("Pirate"):
                actors[i] = TurnHandler.CharType.DPS;
                break;
            case ("Knight"):
                actors[i] = TurnHandler.CharType.Tank;
                break;
            case ("Doctor"):
                actors[i] = TurnHandler.CharType.Healer;
                break;
            case ("Ninja"):
                actors[i] = TurnHandler.CharType.Ninja;
                break;
            case ("Gladiator"):
                actors[i] = TurnHandler.CharType.Gladiator;
                break;
            case ("Shaman"):
                actors[i] = TurnHandler.CharType.Shaman;
                break;
            case ("Cowboy"):
                actors[i] = TurnHandler.CharType.Cowboy;
                break;
            case ("Viking"):
                actors[i] = TurnHandler.CharType.Viking;
                break;
            case ("Cleric"):
                actors[i] = TurnHandler.CharType.Cleric;
                break;
            default:
                actors[i] = TurnHandler.CharType.Base;
                break;
        }
    }

    private void PopulateChatacterList()
    {
        characterList.Clear();

        if (data.d_zone_index >= 0)
        {
            characterList.Add("Soldier");
            if(data.d_zone_index >= 1)
            {
                characterList.Add("Pirate");
                characterList.Add("Knight");
                characterList.Add("Doctor");
                if(data.d_zone_index >= 2)
                {
                    characterList.Add("Ninja");
                    characterList.Add("Gladiator");
                    characterList.Add("Shaman");
                    if(data.d_zone_index >= 3)
                    {
                        characterList.Add("Cowboy");
                        characterList.Add("Viking");
                        characterList.Add("Cleric");
                    }
                }
            }
        }
    }

    private void PopulateDDList()
    {
        for (int i = 0; i < dropdowns.Length; i++)
        {
            for (int j = 0; j < characterList.Count; j++)
            {
                dropdowns[i].options.Add(new TMP_Dropdown.OptionData(characterList[j]));
            }
            dropdowns[i].RefreshShownValue();
        }

    }

    public void HandleInputDataOne(int val)
    {
        UpdateActorList(val, 0);
    }

    public void HandleInputDataTwo(int val)
    {
        UpdateActorList(val, 1);
    }

    public void HandleInputDataThree(int val)
    {
        UpdateActorList(val, 2);
    }

    public void HandleInputDataInfo(int val)
    {
        for(int i = 0; i < infoBoxes.Length; i++)
        {
            if (i == val)
            {
                infoBoxes[i].SetActive(true);
            }
            else
            {
                infoBoxes[i].SetActive(false);
            }
        }
    }

    public void HandleStartButton()
    {
        SaveManager.SaveJsonData(PopulateSaveData());
        SceneManager.LoadScene("DevLevel");
    }

    public void OpenTooltip()
    {
        tooptipPanel.SetActive(true);
    }

    public void CloseTooltip()
    {
        tooptipPanel.SetActive(false);
    }
}
