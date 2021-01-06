using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DiscordPresence;
using System.Collections;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [Header("Tutorial")]
    [SerializeField] private GameSettings tutoSettings;
    [SerializeField] private Building home;
    [SerializeField] private Units constructor;

    [Header("Save")]
    [SerializeField] private string[] saves;
    [SerializeField] private Image loadButton;

    private GameObject newColony, loadColony, settings, credits, aysUI, confirmation;
    private GameObject videoSet, soundSet, hotkeysSet, gameplaySet;

    private Image video, sound, hotkeys, gameplay;
    private RectTransform content;

    private string temp_save;

    private ColorManager colorManager;

    private void Start()
    {
        colorManager = GetComponent<ColorManager>();

        newColony = GameObject.Find("NewGameUI");
        loadColony = GameObject.Find("LoadGameUI");
        settings = GameObject.Find("SettingsUI");
        credits = GameObject.Find("CreditsUI");
        aysUI = GameObject.Find("AYSUI");
        confirmation = GameObject.Find("ConfirmationUI");

        videoSet = GameObject.Find("VideoSettings");
        soundSet = GameObject.Find("SoundSettings");
        hotkeysSet = GameObject.Find("HotkeysSettings");
        gameplaySet = GameObject.Find("GameplaySettings");

        video = GameObject.Find("Video").GetComponent<Image>();
        sound = GameObject.Find("Sound").GetComponent<Image>();
        hotkeys = GameObject.Find("Hotkeys").GetComponent<Image>();
        gameplay = GameObject.Find("Gameplay").GetComponent<Image>();

        content = GameObject.Find("LoadContent").GetComponent<RectTransform>();

        ResetUI();
        ResetSettingsUI();

        SearchSave();
        ReorderLoadButton();

        //UpdatePresence("Main menu", "", "earth", "", "", "");
    }

    private void ShowMenu(GameObject _menu)
    {
        bool activeSelf = _menu.activeSelf;

        if (activeSelf)
        {
            if(_menu.GetComponent<Animator>() != null)
            {
                _menu.GetComponent<Animator>().Play("up");
                Invoke("ResetUI", 1.1f);
                Invoke("ResetSettingsUI", 1.1f);
            }
            else
            {
                ResetUI();
                ResetSettingsUI();
            }
        }
        else
        {
            if (_menu.GetComponent<Animator>() != null)
            {
                ResetUI();
                ResetSettingsUI();

                if(_menu == settings)
                {
                    VideoSettings();
                }

                CancelInvoke();
                _menu.SetActive(true);
                _menu.GetComponent<Animator>().Play("down");
            }
            else
            {
                CancelInvoke();
                _menu.SetActive(true);
            }
        }
    }

    private IEnumerator ShowMenu_After(GameObject _menu)
    {
        yield return new WaitForSeconds(1.1f);
        _menu.SetActive(true);
    }

    public void NewColony()
    {
        if (PlayerPrefs.HasKey("HaveDoTuto"))
            ShowMenu(newColony);
        else
            LaunchTuto();
    }

    public void LoadColony()
    {
        ShowMenu(loadColony);
    }

    public void Settings()
    {
        ShowMenu(settings);
    }

    public void VideoSettings()
    {
        ResetSettingsUI();
        videoSet.SetActive(true);
        video.color = colorManager.forground;
    }

    public void SoundSettings()
    {
        ResetSettingsUI();
        soundSet.SetActive(true);
        sound.color = colorManager.forground;
    }

    public void HotkeysSettings()
    {
        ResetSettingsUI();
        hotkeysSet.SetActive(true);
        hotkeys.color = colorManager.forground;
    }

    public void GameplaySettings()
    {
        ResetSettingsUI();
        gameplaySet.SetActive(true);
        gameplay.color = colorManager.forground;
    }

    private void ResetUI()
    {
        newColony.SetActive(false);
        loadColony.SetActive(false);
        settings.SetActive(false);
        credits.SetActive(false);
        aysUI.SetActive(false);
        confirmation.SetActive(false);
    }

    private void ResetSettingsUI()
    {
        videoSet.SetActive(false);
        soundSet.SetActive(false);
        hotkeysSet.SetActive(false);
        gameplaySet.SetActive(false);

        video.color = colorManager.background;
        sound.color = colorManager.background;
        hotkeys.color = colorManager.background;
        gameplay.color = colorManager.background;
    }

    private void SearchSave()
    {
        saves = SaveSystem.GetSaved();
    }

    public void Quit()
    {
        Application.Quit();
    }

    private void ReorderLoadButton()
    {
        GameObject[] destr = GameObject.FindGameObjectsWithTag("LoadButton") as GameObject[];

        foreach (GameObject item in destr)
        {
            Destroy(item);
        }

        float count = saves.Length;

        if (count == 0) { SearchSave(); count = saves.Length; if (count == 0) return; }

        List<Image> images = new List<Image>();

        int i = 0;

        foreach(string item in saves)
        {
            Image current = Instantiate(loadButton, content) as Image;
            LoadItem cur = current.GetComponent<LoadItem>();

            images.Add(current);

            cur.save = saves[i];
            cur.Init(this);

            i++;
        }
    }

    public void AreYouSure(string save)
    {
        temp_save = save;
        aysUI.SetActive(true);
    }

    public void Btn_No()
    {
        temp_save = null;
        aysUI.SetActive(false);
    }

    public void Btn_Yes()
    {
        SaveSystem.Delete(temp_save + ".json");
        aysUI.SetActive(false);
        confirmation.SetActive(true);
    }

    public void Btn_HideConfirmation()
    {
        confirmation.SetActive(false);
    }

    public void OpenURL(string url)
    {
        Application.OpenURL(url);
    }

    //Discord rich presence

    public void UpdatePresence(string title, string detail, string largeKey, string smallKey, string largeText, string smallText)
    {
        if (GameObject.Find("Presence Manager") == null) return;
        PresenceManager.UpdatePresence(detail: title, state: detail, largeKey: largeKey, largeText: largeText, smallKey: smallKey, smallText: smallText);
    }

    public void Btn_ShowCredits()
    {
        if (!credits.activeSelf)
        {
            ResetUI();
            credits.SetActive(true);
        }
        else
        {
            ResetUI();
        }
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void LaunchTuto()
    {
        PlayerPrefs.SetInt("HaveDoTuto", 1);
        CreateBlankSave();
        SceneManager.LoadScene("02loading");
    }

    private void CreateBlankSave()
    {
        SavedManager manager = new SavedManager(0);

        SavedPlayer player = new SavedPlayer(
            0, new float[3] { 150, 90, 20 }, "Tutorial", tutoSettings
        );

        SavedUnit[] units = new SavedUnit[2]
        {
            new SavedUnit(constructor, new float[3] { -5, 2, -25 } ),
            new SavedUnit(constructor, new float[3] { 5, 2, -25 } )
        };

        SavedBuilding[] buildings = new SavedBuilding[1]
        {
            new SavedBuilding(home, new float[3] { 0, 0, 0 })
        };

        SavedConfiguration config = new SavedConfiguration(tutoSettings);

        SaveSystem.Save("Tutorial.json", new SavedScene("EMPTY", manager, player, config, buildings, units));
    }
}

[System.Serializable]
public class Save
{
    public string saveName;
    public int saveId;
}