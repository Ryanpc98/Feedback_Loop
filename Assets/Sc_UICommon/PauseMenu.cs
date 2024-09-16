using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public static bool gameIsPaused = false;
    public GameObject pauseMenuUI;

    public GameObject settingsPanel;
    [SerializeField] Slider volumeSlider;
    [SerializeField] TurnHandler handler;

    private void Start()
    {
        volumeSlider.value = PlayerPrefs.GetFloat(SaveManager.m_VolumeKey, 1f);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (gameIsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }
    public void Resume()
    {
        AudioListener.volume = PlayerPrefs.GetFloat(SaveManager.m_VolumeKey, 0.75f);
        Debug.Log("Resuming");
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        gameIsPaused = false;
    }
    public void Pause()
    {
        Debug.Log("Pausing");
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        gameIsPaused = true;
    }

    public void OpenSettings()
    {
        //pauseMenuUI.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        PlayerPrefs.SetFloat(SaveManager.m_VolumeKey, volumeSlider.value);
        //pauseMenuUI.SetActive(true);
        settingsPanel.SetActive(false);
    }

    public void ReturnToMenu()
    {
        SaveManager.SaveJsonData(handler.PopulateSaveData());
        Debug.Log("Returning to Menu");
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        gameIsPaused = false;
        SceneManager.LoadScene("MainMenu");
    }
}
