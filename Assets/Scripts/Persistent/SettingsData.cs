using System;
using UnityEngine;

public class SettingsData : MonoBehaviour
{
    public static SettingsData instance;
    public Settings settings;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
            return;
        }
        
        instance = this;
        settings = GetSettings();
    }

    public void SaveSettings()
    {
        SaveSystem.Save("config.json", settings, "");
        Debug.Log("[INFO:SettingsData] Settings saved!");
    }

    public Settings GetSettings()
    {
        try
        {
            settings = SaveSystem.Load<Settings>("config.json", "");
            Debug.Log("[INFO:SettingsData] Settings loaded!");
        }
        catch (Exception e)
        {
            settings = new Settings();
            Debug.LogError(e.Message);
        }

        Settings basicSettings = new Settings();

        if (settings.playerInputs.Length < basicSettings.playerInputs.Length)
        {
            PlayerInput[] newInputs = new PlayerInput[basicSettings.playerInputs.Length];
                
            for (int i = 0; i < newInputs.Length; i++)
            {
                if (i < settings.playerInputs.Length)
                {
                    newInputs[i] = settings.playerInputs[i];
                }
                else
                {
                    newInputs[i] = basicSettings.playerInputs[i];
                }
            }

            settings.playerInputs = newInputs;
        }
        
        return settings;
    }

    [Serializable]
    public class Settings
    {
        [Header("Graphics")]
        public int graphicsID;
        public int vsync;
        public int targetFrameRate;
        public bool fullScreen;

        [Header("Sound")]
        public int musicVolume;
        public int uiEffectVolume;
        public int fxEffectVolume;
        
        [Header("Gameplay")]
        public int languageID;
        public bool runInBackground;

        [Header("Hotkeys")]
        public PlayerInput[] playerInputs;

        public Settings()
        {
            graphicsID = 3;
            vsync = 1;
            targetFrameRate = 60;
            fullScreen = true;

            musicVolume = 100;
            uiEffectVolume = 100;
            fxEffectVolume = 100;
            
            languageID = 0;
            runInBackground = false;
            
            playerInputs = new PlayerInput[]
            {
                new PlayerInput("move_forward", "w", "w"),
                new PlayerInput("move_backward", "s", "s"),
                new PlayerInput("strafe_left", "a", "a"),
                new PlayerInput("strafe_right", "d", "d"),
                new PlayerInput("rotate_left", "q", "q"),
                new PlayerInput("rotate_right", "e", "e"),
                new PlayerInput("reset_camera", "r", "r"),
                new PlayerInput("rotate_preview", "tab", "tab"),
                new PlayerInput("take_screenshot", "f1", "f1"),
                new PlayerInput("hide_hud", "f2", "f2"),
                new PlayerInput("feedback", "f3", "f3"),
                new PlayerInput("hide_mouse_cursor", "f4", "f4"),
                new PlayerInput("speed_pause", "space", "space")
            };
        }
    }

    [Serializable]
    public class PlayerInput
    {
        public string inputLabel;
        public string inputName;
        public string inputDefault;

        public PlayerInput(string label, string name, string def)
        {
            this.inputLabel = label;
            this.inputName = name;
            this.inputDefault = def;
        }
    }
}
