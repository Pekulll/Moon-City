using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SaveDisplay : MonoBehaviour
{
    [Header("Save display")]
    [SerializeField] private string[] saves;
    [SerializeField] private Image loadButton;
    
    private string temp_save;
    private GameObject aysUI, confirmation;
    private RectTransform content;
    
    void Start()
    {
        content = GameObject.Find("LoadContent").GetComponent<RectTransform>();
        aysUI = GameObject.Find("AYSUI");
        confirmation = GameObject.Find("ConfirmationUI");
        
        aysUI.SetActive(false);
        confirmation.SetActive(false);
        
        SearchSave();
        ReorderSaveButton();
    }

    private void SearchSave()
    {
        saves = SaveSystem.GetSaved();
    }
    
    private void ReorderSaveButton()
    {
        GameObject[] destr = GameObject.FindGameObjectsWithTag("LoadButton") as GameObject[];

        foreach (GameObject item in destr)
        {
            Destroy(item);
        }

        SearchSave();
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
        ReorderSaveButton();
        aysUI.SetActive(false);
        confirmation.SetActive(true);
    }
    
    public void Btn_HideConfirmation()
    {
        confirmation.SetActive(false);
    }
}
