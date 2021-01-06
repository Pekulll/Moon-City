using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ResumeMotor : MonoBehaviour
{
    private MoonManager manager;
    private GameObject pauseMenu;
    private GameObject saveDone;

    void Start()
    {
        manager = GameObject.Find("Manager").GetComponent<MoonManager>();
        pauseMenu = GameObject.Find("PauseMenu");
        saveDone = GameObject.Find("B_GameSaved");

        saveDone.SetActive(false);
        pauseMenu.SetActive(false);
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
            pauseMenu.SetActive(true);
        }
        catch (Exception e)
        {
            manager.Notify("Error", e.ToString(), null, new Color(1, 0, 1, 1), 20);
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
        Resume();
    }

    public void QuitToMenu()
    {
        Save();
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
