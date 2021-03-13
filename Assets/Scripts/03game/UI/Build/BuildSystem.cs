using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildSystem : MonoBehaviour
{
    private GameObject buildMenu, buildingList, buildingDetail;

    private List<GameObject> curBuilds = new List<GameObject>();

    private RectTransform content;
    [SerializeField] private Image buildingButton;

    [SerializeField] private Building[] buildings;

    private Text colDetails, enerDetails, monDetails, rigDetails, metDetails, polDetails, foodDetails;
    private Text categoryName;
    private GameObject techBuild;

    private MoonManager manager;
    private ResearchSystem researchSystem;

    private Categorie displayCategory;

    public Preview currentPreview;

    private void Start()
    {
        buildMenu = GameObject.Find("BC_BuildMenu");
        buildingList = GameObject.Find("BC_Body (Build)");
        buildingDetail = GameObject.Find("I_Costs (Build)");
        techBuild = GameObject.Find("I_Tech (Build)");

        colDetails = GameObject.Find("ColonyTextDetail").GetComponent<Text>();
        enerDetails = GameObject.Find("EnergyTextDetail").GetComponent<Text>();
        monDetails = GameObject.Find("MoneyTextDetail").GetComponent<Text>();
        rigDetails = GameObject.Find("RigolyteTextDetail").GetComponent<Text>();
        metDetails = GameObject.Find("MetalTextDetail").GetComponent<Text>();
        polDetails = GameObject.Find("BioPlastiqueTextDetail").GetComponent<Text>();
        foodDetails = GameObject.Find("FoodTextDetail").GetComponent<Text>();
        categoryName = GameObject.Find("T_CategoryName").GetComponent<Text>();

        content = GameObject.Find("E_AvailableBuilding").GetComponent<RectTransform>();

        manager = FindObjectOfType<MoonManager>();
        researchSystem = FindObjectOfType<ResearchSystem>();

        buildMenu.SetActive(false);
        buildingList.SetActive(false);
        buildingDetail.SetActive(false);
        techBuild.SetActive(false);
    }

    public void ShowCategory(Categorie curCat)
    {
        if(displayCategory == curCat)
        {
            HideCategory();
        }
        else
        {
            displayCategory = curCat;
            ReorderBuilds(displayCategory);
        }
    }

    private void DestroyItem()
    {
        if (curBuilds.Count == 0) return;

        foreach (GameObject item in curBuilds)
        {
            Destroy(item);
        }
    }

    private void ReorderBuilds(Categorie curCat)
    {
        float count = curCat.builsId.Length;

        DestroyItem();

        if (count == 0) return;

        List<Image> images = new List<Image>();
        int i = 0;

        foreach (int id in curCat.builsId)
        {
            Image current = Instantiate(buildingButton, content) as Image;
            Building cur = buildings[id];

            images.Add(current);
            curBuilds.Add(current.gameObject);

            current.GetComponent<BuildListItem>().OnInstantiate(cur, buildingDetail, colDetails, enerDetails,
                monDetails, rigDetails, metDetails,polDetails, foodDetails, techBuild, this);

            i++;
        }

        categoryName.text = manager.Traduce(curCat.name);
        techBuild.SetActive(false);
        buildingDetail.SetActive(false);
        buildingList.SetActive(true);
    }

    public void ShowMenu()
    {
        bool isActive = buildMenu.activeSelf;

        if (isActive)
        {
            manager.ResetLeft();
        }
        else
        {
            manager.ResetLeft();
            buildMenu.SetActive(true);
        }
    }

    public void HideCategory()
    {
        displayCategory = null;

        DestroyItem();

        buildingDetail.SetActive(false);
        buildingList.SetActive(false);
        techBuild.SetActive(false);
    }

    public void HideBuildMenu()
    {
        buildMenu.SetActive(false);
        buildingDetail.SetActive(false);
        buildingList.SetActive(false);
        techBuild.SetActive(false);
    }
}
