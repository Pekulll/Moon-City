using UnityEngine;
using UnityEngine.UI;

public class GameplaySettings : MonoBehaviour
{
    private GameObject ribBtn;

    private TraduceSystem translate;
    private ColorManager colorManager;

    private int languageID;
    private bool runInBackground;

    private void Start()
    {
        translate = GameObject.Find("Traductor").GetComponent<TraduceSystem>();
        colorManager = FindObjectOfType<ColorManager>();
        ribBtn = GameObject.Find("Btn_SwitchRIB");

        languageID = SettingsData.instance.settings.languageID;
        runInBackground = SettingsData.instance.settings.runInBackground;

        UpdateRIBColor();
        ChangeLanguage(languageID);
    }

    public void ChangeLanguage(int langId)
    {
        languageID = langId;

        if (languageID == 0) { translate.Traduce("en-EN"); }
        else if (languageID == 1) { translate.Traduce("fr-FR"); }
        else if (languageID == 2) { translate.Traduce("es-ES"); }
        else if (languageID == 3) { translate.Traduce("de-DE"); }

        translate.Traduce(GetLanguageCode(languageID));
        SettingsData.instance.settings.languageID = languageID;
        SettingsData.instance.SaveSettings();
    }

    public string GetLanguageCode(int id)
    {
        if (id == 0) return "en-EN";
        if (id == 1) return "fr-FR";
        if (id == 2) return "es-ES";
        if (id == 3) return "de-DE";

        throw new System.Exception("[ERROR] Language id not found !");
    }

    public void RunInBackground()
    {
        runInBackground = !runInBackground;
        SettingsData.instance.settings.runInBackground = runInBackground;
        SettingsData.instance.SaveSettings();
        Application.runInBackground = runInBackground;
        UpdateRIBColor();
    }

    private void UpdateRIBColor()
    {
        if (runInBackground)
        {
            ribBtn.GetComponent<Outline>().effectColor = colorManager.finished;
            ribBtn.GetComponentInChildren<Text>().color = colorManager.finished;
            ribBtn.GetComponentInChildren<Text>().text = "On";
        }
        else
        {
            ribBtn.GetComponent<Outline>().effectColor = colorManager.unavailable;
            ribBtn.GetComponentInChildren<Text>().color = colorManager.unavailable;
            ribBtn.GetComponentInChildren<Text>().text = "Off";
        }
    }

    public void ResetGame()
    {
        PlayerPrefs.DeleteAll();
        Application.Quit();
    }
}
