using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public const string m_VolumeKey = "vol";

    public static void SaveJsonData(SaveData sd)
    {
        if (FileManager.WriteToFile("SaveData01.dat", sd.ToJson()))
        {
            Debug.Log("Save successful");
        }
    }

    public static SaveData LoadJsonData()
    {
        SaveData sd = new SaveData();
        if (FileManager.LoadFromFile("SaveData01.dat", out var json))
        {
            sd.LoadFromJson(json);
            Debug.Log("Load complete");

            return sd;
        }
        Debug.Log("!!!!!==Load failed==!!!!!");
        return sd;
    }

    public static void WipeData()
    {
        SaveData sd = new SaveData();

        //Adaptations
        sd.d_adaptations.increasedRAM = 0;
        sd.d_adaptations.biggerHardDrives = 0;
        sd.d_adaptations.hotSpares = 0;
        sd.d_adaptations.upgradedFirewall = 0;
        sd.d_adaptations.efficientRouting = 0;

        //Player Bots
        sd.d_roster = new List<TurnHandler.CharType>();

        //Level Info
        sd.d_zone_one_ld.level_index = 0;
        sd.d_zone_one_ld.level_status_arr = new int[10];

        sd.d_zone_one_ld.level_status_arr[1] = 1;
        sd.d_zone_one_ld.level_status_arr[5] = 1;

        sd.d_level_name = "";

        SaveJsonData(sd);
    }
}
