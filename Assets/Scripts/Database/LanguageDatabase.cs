using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewLanguageDatabase", menuName = "Database/Language")]
public class LanguageDatabase : ScriptableObject
{
    public List<Language> langs = new List<Language>();

    public Language GetLanguage(string name)
    {
        return langs.Find(lang => lang.langName == name);
    }

    public string GetTraduction(string langName, string sentence)
    {
        if(langName == "") langName = "en-EN";

        if (langName == "en-EN")
        {
            Language cur = GetLanguage("fr-FR");
            Sentence current = cur.sentences.Find(sent => sent.sentence == sentence);

            try { return current.baseSentence; } catch { return sentence; }
        }
        else
        {
            Language cur = GetLanguage(langName);
            Sentence current = cur.sentences.Find(sent => sent.baseSentence == sentence);

            try { return current.sentence; } catch { return sentence; }
        }
    }

    public string GetTraduction(string langName, string refLangName, string sentence)
    {
        if (langName == "en-EN")
        {
            try
            {
                List<Sentence> baseLanguage = GetLanguage("fr-FR").sentences;
                Sentence current = baseLanguage.Find(sent => sent.sentence == sentence);
                return current.baseSentence;
            }
            catch
            {
                return sentence;
            }
        }
        else if (refLangName == "en-EN")
        {
            return GetTraduction(langName, sentence);
        }
        else
        {
            try
            {
                List<Sentence> baseLanguage = GetLanguage(refLangName).sentences;
                Sentence refSentence = baseLanguage.Find(sent => sent.sentence == sentence);
                Sentence baseSentence = GetLanguage(langName).sentences.Find(sent => sent.baseSentence == refSentence.baseSentence);
                return baseSentence.sentence;
            }
            catch
            {
                return sentence;
            }
        }
    }
}
