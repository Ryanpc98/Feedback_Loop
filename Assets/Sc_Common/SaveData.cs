using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    //Values
    //# = Number of Times that Adaptaion is Owned
    [System.Serializable]
    public struct AdaptationData
    {
        public int increasedRAM;
        public int biggerHardDrives;
        public int hotSpares;
        public int upgradedFirewall;
        public int efficientRouting;

    }

    //Values
    //-1 = Completed By Proxy
    //0 = Untouched, untavelable
    //1 = Travelable, untouched
    //2 = Completed
    //3 = Current Level, Lost
    //4 = Current Level, Won
    [System.Serializable]
    public struct LevelData
    {
        public int level_index;
        public int[] level_status_arr;
    }

    public AdaptationData d_adaptations;
    public List<TurnHandler.CharType> d_roster = new List<TurnHandler.CharType>();
    public int d_zone_index;
    public LevelData d_zone_one_ld;
    public LevelData d_zone_two_ld;
    public LevelData d_zone_three_ld;
    public string d_level_name;

    public string ToJson()
    {
        return JsonUtility.ToJson(this);
    }

    public void LoadFromJson(string a_Json)
    {
        JsonUtility.FromJsonOverwrite(a_Json, this);
    }
}