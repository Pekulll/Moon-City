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

    private GameObject credits;

    private void Start()
    {
        credits = GameObject.Find("CreditsUI");
        ResetUI();

        //UpdatePresence("Main menu", "", "earth", "", "", "");
    }

    private IEnumerator ShowMenu_After(GameObject _menu)
    {
        yield return new WaitForSeconds(1.1f);
        _menu.SetActive(true);
    }

    private void ResetUI()
    {
        credits.SetActive(false);
    }

    public void Quit()
    {
        Application.Quit();
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

    public void LaunchTuto()
    {
        PlayerPrefs.SetInt("HaveDoTuto", 1);
        CreateBlankSave();
        SceneManager.LoadScene("02loading");
    }

    private void CreateBlankSave()
    {
        SaveSystem.Save("Tutorial.json", SaveSystem.BlankSave(tutoSettings, "Cherished Moon", constructor, home));
    }
}