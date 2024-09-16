using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NodeMapController : MonoBehaviour
{
    [SerializeField] protected Button[] buttons = new Button[10];

    [SerializeField] protected Sprite base_cpu;
    [SerializeField] protected Sprite[] broken_cpus = new Sprite[3];


    protected SaveData data = new SaveData();

    protected virtual void Start()
    {
        LoadFromSaveData(SaveManager.LoadJsonData());
    }

    protected SaveData PopulateSaveData()
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

        //Zone Info
        sd.d_zone_index = data.d_zone_index;

        //Level Info
        sd.d_zone_one_ld = data.d_zone_one_ld;
        sd.d_zone_two_ld = data.d_zone_two_ld;
        sd.d_zone_three_ld = data.d_zone_three_ld;

        //Level Name
        sd.d_level_name = CurLevelNumLookup();

        return sd;
    }

    protected void LoadFromSaveData(SaveData sd)
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

    public virtual void NodeButtonHandler(int node)
    {
        SaveManager.SaveJsonData(PopulateSaveData());
        switch (node)
        {
            case 1:
                data.d_zone_one_ld.level_index = node;
                SceneManager.LoadScene("CharacterSelect");
                break;
            case 2:
                data.d_zone_one_ld.level_index = node;
                SceneManager.LoadScene("CharacterSelect");
                break;
            case 3:
                data.d_zone_one_ld.level_index = node;
                SceneManager.LoadScene("CharacterSelect");
                break;
            case 4:
                data.d_zone_one_ld.level_index = node;
                SceneManager.LoadScene("CharacterSelect");
                break;
            default:
                break;
        }
    }

    public virtual string LevelNumLookup(int node)
    {
        Debug.Log("Level: " + node.ToString() + " selected");
        switch (node)
        {
            case (1):
                return "Node One";
            case (2):
                return "Node Two";
            case (3):
                return "Node Three";
            case (4):
                return "Node Four";
            default:
                return "DEV ERROR: " + node.ToString();
        }
    }

    public string CurLevelNumLookup()
    {
        switch (data.d_zone_index)
        {
            case 1:
                return LevelNumLookup(data.d_zone_one_ld.level_index);
            case 2:
                return LevelNumLookup(data.d_zone_two_ld.level_index);
            case 3:
                return LevelNumLookup(data.d_zone_three_ld.level_index);
            default:
                return LevelNumLookup(data.d_zone_one_ld.level_index);
        }
    }
}