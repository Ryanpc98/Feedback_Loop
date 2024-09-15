using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class MainMenuController : MonoBehaviour
{
    // From https://youtu.be/vviFh8HdsFw

    public GameObject settingsPanel;
    public GameObject tutorialPanel;
    private SaveData data = new SaveData();
    [SerializeField] Slider volumeSlider;

    private void Start()
    {
        volumeSlider.value = PlayerPrefs.GetFloat(SaveManager.m_VolumeKey, 1f);
    }

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

    public void StartButtonHandler()
    {
        SceneManager.LoadScene("ZoneOne");
    }

    public void OpenSettings()
    {
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        PlayerPrefs.SetFloat(SaveManager.m_VolumeKey, volumeSlider.value);
        settingsPanel.SetActive(false);
    }

    public void OpenTutorial()
    {
        tutorialPanel.SetActive(true);
    }

    public void CloseTutorial()
    {
        tutorialPanel.SetActive(false);
    }

    public void ClearSave()
    {
        SaveManager.WipeData();
    }

    public void ExitGame()
    {
        Debug.Log("Quitting");
        Application.Quit();
    }
}
