using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class AdaptationSelectController : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown dropdown;

    public TextMeshProUGUI outputOne;

    private static EnumTypes.AdaptationType selection = EnumTypes.AdaptationType.None;

    private SaveData data = new SaveData();

    private void Start()
    {
        LoadFromSaveData(SaveManager.LoadJsonData());

        AddOptions(TurnHandler.adaptations);
    }

    private SaveData PopulateSaveData()
    {
        SaveData sd = new SaveData();

        sd.d_adaptations.increasedRAM = data.d_adaptations.increasedRAM;
        sd.d_adaptations.biggerHardDrives = data.d_adaptations.biggerHardDrives;
        sd.d_adaptations.hotSpares = data.d_adaptations.hotSpares;
        sd.d_adaptations.upgradedFirewall = data.d_adaptations.upgradedFirewall;
        sd.d_adaptations.efficientRouting = data.d_adaptations.efficientRouting;

        //Adaptations
        switch (selection)
        {
            case (EnumTypes.AdaptationType.IncreaseRAM):
                sd.d_adaptations.increasedRAM += 1;
                break;
            case (EnumTypes.AdaptationType.BiggerHardDrives):
                sd.d_adaptations.biggerHardDrives += 1;
                break;
            case (EnumTypes.AdaptationType.HotSpares):
                sd.d_adaptations.hotSpares += 1;
                break;
            case (EnumTypes.AdaptationType.UpgradedFirewall):
                sd.d_adaptations.upgradedFirewall += 1;
                break;
            case (EnumTypes.AdaptationType.EfficientRouting):
                sd.d_adaptations.efficientRouting += 1;
                break;
            default:
                break;
        }

        //Player Bots
        sd.d_roster = data.d_roster;

        //Zone Info
        sd.d_zone_index = data.d_zone_index;

        //Level Info
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

        //Level Name
        data.d_level_name = sd.d_level_name;
    }

    public void HandleInputData()
    {
        int picked_EntryIndex = dropdown.value;

        outputOne.text = dropdown.options[picked_EntryIndex].text;

        switch (dropdown.options[picked_EntryIndex].text)
        {
            case ("Increase RAM"):
                selection = EnumTypes.AdaptationType.IncreaseRAM;
                break;
            case ("Bigger Hard Drives"):
                selection = EnumTypes.AdaptationType.BiggerHardDrives;
                break;
            case ("Hot Spares"):
                selection = EnumTypes.AdaptationType.HotSpares;
                break;
            case ("Upgraded Firewall"):
                selection = EnumTypes.AdaptationType.UpgradedFirewall;
                break;
            case ("Efficient Routing"):
                selection = EnumTypes.AdaptationType.EfficientRouting;
                break;
            default:
                selection = EnumTypes.AdaptationType.None;
                break;
        }
    }

    private void AddOptions(List<string> adaptations)
    {
        dropdown.options.Add(new TMP_Dropdown.OptionData("None"));
        for (int i = 0; i < adaptations.Count; i++)
        {
            dropdown.options.Add(new TMP_Dropdown.OptionData(adaptations[i]));
        }
        dropdown.RefreshShownValue();
    }

    public void HandleContinueButton()
    {
        SaveManager.SaveJsonData(PopulateSaveData());
        SceneManager.LoadScene("ZoneOne");
    }
}
