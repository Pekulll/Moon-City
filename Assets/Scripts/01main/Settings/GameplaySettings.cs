using UnityEngine;
using UnityEngine.UI;

public class GameplaySettings : MonoBehaviour
{
    private Text languageTxt;
    private GameObject ribBtn;

    private TraduceSystem translate;
    private ColorManager colorManager;

    private int m_languageId;
    private bool runInBackground;

    private void Start()
    {
        translate = GameObject.Find("Traductor").GetComponent<TraduceSystem>();
        colorManager = FindObjectOfType<ColorManager>();

        languageTxt = GameObject.Find("LanguageText").GetComponent<Text>();
        ribBtn = GameObject.Find("Btn_SwitchRIB");

        if (!PlayerPrefs.HasKey("LanguageId"))
        {
            m_languageId = 0;
        }
        else
        {
            m_languageId = PlayerPrefs.GetInt("LanguageId");
        }

        if (!PlayerPrefs.HasKey("RunInBackground"))
        {
            PlayerPrefs.SetString("RunInBackground", "false");
        }
        else
        {
            runInBackground = bool.Parse(PlayerPrefs.GetString("RunInBackground"));
        }

        UpdateRIBColor();
        ChangeLanguage(m_languageId);
    }

    public void ChangeLanguage(int langId)
    {
        m_languageId = langId;

        if (m_languageId == 0) { translate.Traduce("en-EN"); PlayerPrefs.SetInt("LanguageId", 0); }
        else if (m_languageId == 1) { translate.Traduce("fr-FR"); PlayerPrefs.SetInt("LanguageId", 1); }
        else if (m_languageId == 2) { translate.Traduce("es-ES"); PlayerPrefs.SetInt("LanguageId", 2); }
        else if (m_languageId == 3) { translate.Traduce("de-DE"); PlayerPrefs.SetInt("LanguageId", 3); }
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
        PlayerPrefs.SetString("RunInBackground", runInBackground.ToString());
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
