﻿using UnityEngine;
using UnityEngine.UI;

public class GraphicsSettings : MonoBehaviour
{
    private GameObject[] graphics;
    private GameObject vsyncButton;
    private GameObject fullscreenButton;

    private ColorManager colorManager;

    [SerializeField] private bool vsync;
    [SerializeField] private bool fullScreen;

    private void Start()
    {
        colorManager = FindObjectOfType<ColorManager>();

        graphics = new GameObject[6];

        graphics[0] = GameObject.Find("GVery low");
        graphics[1] = GameObject.Find("GLow");
        graphics[2] = GameObject.Find("GNormal");
        graphics[3] = GameObject.Find("GHigh");
        graphics[4] = GameObject.Find("GVery high");
        graphics[5] = GameObject.Find("Custom graphics");

        vsyncButton = GameObject.Find("Btn_Vsync");
        fullscreenButton = GameObject.Find("Btn_Fullscreen");

        if(!PlayerPrefs.HasKey("Graphics"))
        {
            ChangeQualityLevel(2);
            PlayerPrefs.SetInt("Graphics", 2);
        }
        else
        {
            ChangeQualityLevel(PlayerPrefs.GetInt("Graphics"));
        }

        if(!PlayerPrefs.HasKey("VSync"))
        {
            PlayerPrefs.SetString("VSync", "true");
            vsync = true;
            QualitySettings.vSyncCount = 1;
        }
        else
        {
            vsync = bool.Parse(PlayerPrefs.GetString("VSync"));
            if (vsync) QualitySettings.vSyncCount = 1;
            else QualitySettings.vSyncCount = 0;
        }

        UpdateVSyncButton();

        if (!PlayerPrefs.HasKey("Fullscreen"))
        {
            PlayerPrefs.SetString("Fullscreen", "true");
            fullScreen = true;
        }
        else
        {
            fullScreen = bool.Parse(PlayerPrefs.GetString("Fullscreen"));
        }

        Screen.fullScreen = fullScreen;
        UpdateFullscreenButton();
    }

    public void ChangeQualityLevel(int graphicsID)
    {
        QualitySettings.SetQualityLevel(graphicsID);
        PlayerPrefs.SetInt("Graphics", graphicsID);

        for(int i = 0; i < graphics.Length; i++)
        {
            if(i != graphicsID)
            {
                graphics[i].GetComponent<Button>().interactable = true;
                graphics[i].GetComponentInChildren<Outline>().effectColor = colorManager.backgroundBorder;
                graphics[i].GetComponentInChildren<Text>().color = colorManager.text;
            }
            else
            {
                graphics[i].GetComponentInChildren<Outline>().effectColor = colorManager.finished;
                graphics[i].GetComponentInChildren<Text>().color = colorManager.finished;
                graphics[i].GetComponent<Button>().interactable = false;
            }
        }

        // Désactivation car mode custom indisponnible
        graphics[5].GetComponentInChildren<Outline>().effectColor = colorManager.importantColor;
        graphics[5].GetComponentInChildren<Text>().color = colorManager.importantColor;
        graphics[5].GetComponent<Button>().interactable = false;
        
    }

    public void ChangeVSyncStatue()
    {
        vsync = !vsync;
        PlayerPrefs.SetString("VSync", vsync.ToString());

        if (vsync)
        {
            QualitySettings.vSyncCount = 1;
        }
        else
        {
            QualitySettings.vSyncCount = 0;
        }

        UpdateVSyncButton();

    }

    private void UpdateVSyncButton()
    {
        if (vsync)
        {
            vsyncButton.GetComponent<Outline>().effectColor = colorManager.finished;
            vsyncButton.GetComponentInChildren<Text>().text = "On";
            vsyncButton.GetComponentInChildren<Text>().color = colorManager.finished;
        }
        else
        {
            vsyncButton.GetComponent<Outline>().effectColor = colorManager.unavailable;
            vsyncButton.GetComponentInChildren<Text>().text = "Off";
            vsyncButton.GetComponentInChildren<Text>().color = colorManager.unavailable;
        }
    }

    public void ChangeFullscreen()
    {
        fullScreen = !fullScreen;
        PlayerPrefs.SetString("Fullscreen", fullScreen.ToString());
        Screen.fullScreen = fullScreen;

        if (fullScreen)
        {
            Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
        }

        UpdateFullscreenButton();
    }

    private void UpdateFullscreenButton()
    {
        if (fullScreen)
        {
            fullscreenButton.GetComponent<Outline>().effectColor = colorManager.finished;
            fullscreenButton.GetComponentInChildren<Text>().text = "On";
            fullscreenButton.GetComponentInChildren<Text>().color = colorManager.finished;
        }
        else
        {
            fullscreenButton.GetComponent<Outline>().effectColor = colorManager.unavailable;
            fullscreenButton.GetComponentInChildren<Text>().text = "Off";
            fullscreenButton.GetComponentInChildren<Text>().color = colorManager.unavailable;
        }
    }
}
