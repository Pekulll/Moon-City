using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadItem : MonoBehaviour
{
    [HideInInspector] public string save;

    private Image border;

    private Text saveName;
    private Text saveType;

    private Text clock;
    private Text colonist;
    private Text research;

    private SaveDisplay manager;
    private ColorManager colorManager;

    public void Init(SaveDisplay manager)
    {
        this.manager = manager;
        colorManager = FindObjectOfType<ColorManager>();

        gameObject.name = save;

        border = GetComponent<Image>();

        saveName = transform.Find("Background/E_SaveInfo/BC_SaveName/T_SaveName").GetComponent<Text>();
        saveType = transform.Find("Background/E_SaveInfo/BC_SaveType/T_SaveType").GetComponent<Text>();

        clock = transform.Find("Background/BC_Clock/T_Clock").GetComponent<Text>();
        colonist = transform.Find("Background/E_Stats/BC_Colonist/T_Colonist").GetComponent<Text>();
        research = transform.Find("Background/E_Stats/BC_Research/T_Research").GetComponent<Text>();

        UpdateColor();

        SavedScene data = SaveSystem.Load<SavedScene>(save + ".json");

        try { 
            Color colonyColor = new Color(data.player.playerColony.colonyColor[0], data.player.playerColony.colonyColor[1], data.player.playerColony.colonyColor[2], 1);
            border.color = colonyColor;
        }
        catch {
            border.color = Color.black;
        }

        saveName.text = save;
        saveType.text = Traduce(data.saveType) + " <size=13>(v" + data.versionCode + ")</size>";

        clock.text = "Sol " + data.manager.day.ToString("000") + Traduce(" at ") + (data.manager.time / 60).ToString("000") + ":" + (data.manager.time % 60).ToString("00");
        colonist.text = data.player.playerColony.colonist + " " + Traduce("worker(s)");
        research.text = data.research.techsUnlock.Count + " " + Traduce("tech(s)");

        if (!Version.isRetroCompatible && data.versionCode != Version.versionCode)
        {
            border.color = Color.red;
            saveName.text += " (INCOMPATIBLE)";
        }
    }

    private void UpdateColor()
    {
        saveName.color = colorManager.text;
        saveType.color = colorManager.text;
        clock.color = colorManager.text;
        colonist.color = colorManager.text;
        research.color = colorManager.text;

        transform.Find("Background/E_SaveInfo/BC_SaveName").GetComponent<Image>().color = colorManager.forground;
        transform.Find("Background/E_SaveInfo/BC_SaveType").GetComponent<Image>().color = colorManager.forground;

        transform.Find("Background/BC_Clock").GetComponent<Image>().color = colorManager.forground;
        transform.Find("Background/E_Stats/BC_Colonist").GetComponent<Image>().color = colorManager.forground;
        transform.Find("Background/E_Stats/BC_Research").GetComponent<Image>().color = colorManager.forground;

        transform.Find("Background/E_Stats/BC_Colonist/I_Icon").GetComponent<Image>().color = colorManager.icon;
        transform.Find("Background/E_Stats/BC_Research/I_Icon").GetComponent<Image>().color = colorManager.icon;
        transform.Find("Background").GetComponent<Image>().color = colorManager.background;
    }

    public void LoadSave()
    {
        GameObject.Find("MainMenu").GetComponent<FadeMotor>().FadeOut();
        Invoke("Load", 0.5f);
    }

    public void Load()
    {
        PlayerPrefs.SetString("CurrentSave", save);
        SceneManager.LoadSceneAsync("02loading");
    }

    public void DeleteSave()
    {
        manager.AreYouSure(save);
    }

    public string Traduce(string sentence)
    {
        return GameObject.Find("Traductor").GetComponent<TraduceSystem>().GetTraduction(sentence);
    }
}
