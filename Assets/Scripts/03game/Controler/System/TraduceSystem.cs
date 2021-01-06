using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TraduceSystem : MonoBehaviour
{
    private N_Language language; 
    [SerializeField] private string languageName;

    private Text[] allText;

    private void Awake()
    {
        Traduce();
    }

    private void LoadLanguageFile(string lang="en-EN")
    {
        //if (languageName == "") return;

        TextAsset json = Resources.Load<TextAsset>("lang/" + lang);
        language = JsonUtility.FromJson<N_Language>(json.text);

        Debug.Log("[INFO:TraduceSystem] Language loaded: " + lang + " (" + language.traductions.Length + " traductions)");
    }

    public void Traduce()
    {
        languageName = PlayerPrefs.GetString("LanguageName");

        if(languageName == "")
        {
            languageName = "en-EN";
            PlayerPrefs.SetString("LanguageName", "en-EN");
        }

        LoadLanguageFile(languageName);
        allText = FindObjectsOfType<Text>();

        if (allText.Length != 0)
        {
            foreach (Text txt in allText)
            {
                string traduction = GetTraduction(txt.text);
                txt.text = traduction;
            }
        }
    }

    public void Traduce(string langName)
    {
        if (allText.Length != 0)
        {
            foreach (Text txt in allText)
            {
                string traduction = GetKey(txt.text);
                txt.text = traduction;
            }
        }

        languageName = langName;
        LoadLanguageFile(languageName);
        PlayerPrefs.SetString("LanguageName", languageName);

        if (allText.Length != 0)
        {
            foreach (Text txt in allText)
            {
                string traduction = GetTraduction(txt.text);
                txt.text = traduction;
            }
        }
    }

    public string GetTraduction(string sentence)
    {
        foreach(Traduction t in language.traductions)
        {
            if(t.key.Equals(sentence))
            {
                return t.translation;
            }
        }

        return sentence;
    }

    public string GetKey(string sentence)
    {
        foreach (Traduction t in language.traductions)
        {
            if (t.translation.Equals(sentence))
            {
                return t.key;
            }
        }

        return sentence;
    }
}

[System.Serializable]
public struct N_Language
{
    public string name;
    public string code;
    public Traduction[] traductions;
}

[System.Serializable]
public struct Traduction
{
    public string key;
    public string translation;
}