using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class ZoneTwoNodeMapController : NodeMapController
{
    protected override void Start()
    {
        LoadFromSaveData(SaveManager.LoadJsonData());

        for(int i = 1; i < 10; i++)
        {
            buttons[i - 1].interactable = false;

            if (data.d_zone_two_ld.level_status_arr[i] == -1)
            {
                //buttons[i - 1].image.color = Color.cyan;
                buttons[i - 1].image.sprite = broken_cpus[(i - 1) % 3];

            }
            else if (data.d_zone_two_ld.level_status_arr[i] == 1)
            {
                //buttons[i - 1].image.color = Color.yellow;
                buttons[i - 1].interactable = true;
            }
            else if (data.d_zone_two_ld.level_status_arr[i] == 2)
            {
                //buttons[i - 1].image.color = Color.green;
                buttons[i - 1].image.sprite = broken_cpus[(i - 1) % 3];

                if (i <= 4)
                {
                    //buttons[i + 3].image.color = Color.cyan;
                    buttons[i + 3].image.sprite = broken_cpus[(i + 3) % 3];
                }
                else
                {
                    //buttons[i - 5].image.color = Color.cyan;
                    buttons[i - 5].image.sprite = broken_cpus[(i - 5) % 3];
                }
            }
            else if (data.d_zone_two_ld.level_status_arr[i] == 3)
            {
                //buttons[i - 1].image.color = Color.yellow;
                buttons[i - 1].interactable = true;

                if (i == 9)
                {
                    Debug.Log("Level Not Yet Complete");
                }
                else if (i <= 4)
                {
                    //buttons[i + 3].image.color = Color.cyan;
                    buttons[i + 3].image.sprite = broken_cpus[(i + 3) % 3];
                    data.d_zone_two_ld.level_status_arr[(i + 4) % 10] = -1;
                }
                else
                {
                    //buttons[i - 5].image.color = Color.cyan;
                    buttons[i - 5].image.sprite = broken_cpus[(i - 5) % 3];
                    buttons[i - 5].interactable = false; ;
                    data.d_zone_two_ld.level_status_arr[i - 4] = -1;
                }
            }
            else if (data.d_zone_two_ld.level_status_arr[i] == 4)
            {
                //buttons[i - 1].image.color = Color.magenta;
                buttons[i - 1].image.sprite = broken_cpus[(i - 1) % 3];

                if (i == 9)
                {
                    Debug.Log("Level Complete");
                    data.d_zone_index = 3;
                    data.d_zone_two_ld.level_index = 1;
                    SaveManager.SaveJsonData(PopulateSaveData());
                    SceneManager.LoadScene("ZoneThree");
                }
                else if (i <= 4)
                {
                    //buttons[i + 3].image.color = Color.cyan;
                    buttons[i + 3].image.sprite = broken_cpus[(i + 3) % 3];
                    data.d_zone_two_ld.level_status_arr[(i + 4) % 10] = -1;
                    if (i != 4)
                    {
                        data.d_zone_two_ld.level_status_arr[i + 1] = 1;
                    }
                    data.d_zone_two_ld.level_status_arr[(i + 5) % 10] = 1;
                }
                else
                {
                    //buttons[i - 5].image.color = Color.cyan;
                    buttons[i - 5].image.sprite = broken_cpus[(i - 5) % 3];
                    buttons[i - 5].interactable = false; ;
                    data.d_zone_two_ld.level_status_arr[i - 4] = -1;

                    data.d_zone_two_ld.level_status_arr[i + 1] = 1;
                    if (i != 8)
                    {
                        data.d_zone_two_ld.level_status_arr[i - 3] = 1;
                        Debug.Log("Enabling Button: " + i.ToString() + " (" + (i - 3).ToString() + ")");
                        //buttons[i - 4].image.color = Color.yellow;
                        buttons[i - 4].interactable = true;
                    }
                }
                data.d_zone_two_ld.level_status_arr[i] = 2;
            }
        }
    }

    public override void NodeButtonHandler(int node)
    {
        data.d_zone_index = 2;
        data.d_zone_two_ld.level_index = node;
        SaveManager.SaveJsonData(PopulateSaveData());
        SceneManager.LoadScene("CharacterSelect");
    }

    public override string LevelNumLookup(int node)
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
            case (5):
                return "Node Five";
            case (6):
                return "Node Six";
            case (7):
                return "Node Seven";
            case (8):
                return "Node Eight";
            case (9):
                return "Node Nine";
            default:
                return "DEV ERROR: " + node.ToString();
        }
    }

    public static List<TurnHandler.CharType> GetEnemyBotArray(string node)
    {
        switch (node)
        {
            case "Node One":
                return new List<TurnHandler.CharType>() { TurnHandler.CharType.Healer, TurnHandler.CharType.Basic, TurnHandler.CharType.Healer };
            case "Node Two":
                return new List<TurnHandler.CharType>() { TurnHandler.CharType.DPS, TurnHandler.CharType.Tank, TurnHandler.CharType.Tank };
            case "Node Three":
                return new List<TurnHandler.CharType>() { TurnHandler.CharType.Tank, TurnHandler.CharType.DPS, TurnHandler.CharType.Healer };
            case "Node Four":
                return new List<TurnHandler.CharType>() { TurnHandler.CharType.DPS, TurnHandler.CharType.Tank, TurnHandler.CharType.Healer };
            case "Node Five":
                return new List<TurnHandler.CharType>() { TurnHandler.CharType.Basic, TurnHandler.CharType.Basic, TurnHandler.CharType.Healer };
            case "Node Six":
                return new List<TurnHandler.CharType>() { TurnHandler.CharType.Basic, TurnHandler.CharType.Tank, TurnHandler.CharType.Tank };
            case "Node Seven":
                return new List<TurnHandler.CharType>() { TurnHandler.CharType.DPS, TurnHandler.CharType.DPS, TurnHandler.CharType.Healer };
            case "Node Eight":
                return new List<TurnHandler.CharType>() { TurnHandler.CharType.Tank, TurnHandler.CharType.Tank, TurnHandler.CharType.Healer };
            case "Node Nine":
                return new List<TurnHandler.CharType>() { TurnHandler.CharType.DPS, TurnHandler.CharType.B_DPS, TurnHandler.CharType.Healer };
            default:
                return new List<TurnHandler.CharType>() { TurnHandler.CharType.Basic, TurnHandler.CharType.Basic, TurnHandler.CharType.Basic };
        }
    }
}
