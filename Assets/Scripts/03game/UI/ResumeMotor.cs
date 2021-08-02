using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ResumeMotor : MonoBehaviour
{
    private MoonManager manager;
    private GameObject pauseMenu;
    private GameObject saveDone;
    private GameObject settings;

    void Start()
    {
        manager = GameObject.Find("Manager").GetComponent<MoonManager>();
        pauseMenu = GameObject.Find("PauseMenu");
        saveDone = GameObject.Find("B_GameSaved");
        settings = GameObject.Find("SettingsUI");

        saveDone.SetActive(false);
        pauseMenu.SetActive(false);
        settings.SetActive(false);
    }

    private void Update()
    {
        if (!manager.canPaused) return;

        if (Input.GetKeyDown("escape") && !manager.isPaused) Pause();
        else if (Input.GetKeyDown("escape") && manager.isPaused) Resume();
    }

    public void Pause()
    {
        try
        {
            manager.PauseGame(true);
            manager.HideInterface();
            settings.SetActive(false);
            pauseMenu.SetActive(true);
        }
        catch (Exception e)
        {
            manager.Notify(string.Format(manager.Traduce("03_notif_error"), e.ToString()), priority: 3);
        }
    }

    public void Resume()
    {
        manager.ResetInterface();
        saveDone.SetActive(false);
        manager.PauseGame(false);
    }

    public void Save()
    {
        manager.Save();
    }

    public void Settings()
    {
        settings.SetActive(!settings.activeSelf);
    }

    public void QuitToMenu()
    {
        manager.PauseGame(false, true);
        SceneManager.LoadScene("00loading");
    }

    public void QuitToDesktop()
    {
        Save();
        manager.PauseGame(false, true);
        Application.Quit();
    }

    public void HidePause()
    {
        pauseMenu.SetActive(false);
    }

    public void ShowPause()
    {
        pauseMenu.SetActive(true);
    }
}
