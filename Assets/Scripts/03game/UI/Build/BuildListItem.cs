using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BuildListItem : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    private GameObject techBuild, ressources;

    private Text colonyDetails, energyDetails, moneyDetails, regolythDetails, bioplasticDetails, foodDetails, buildName;
    private Image colonyBackground, energyBackground, moneyBackground, regolythBackground, bioplasticBackground, foodBackground;

    private Image iconImg;
    private Image buttonImg;

    private Building currentBuild;

    private MoonManager man;
    private ResearchSystem researchSystem;
    private BuildSystem buildSystem;

    private bool canBuild;

    private Color classic, notBuildable, haventTech;

    #region Initialization

    public void OnInstantiate(Building _currentBuild, GameObject ressources, Text colony, Text energy, Text money, Text regolyth, Text bioplastic, Text food, GameObject _techBuild, BuildSystem buildSystem)
    {
        currentBuild = _currentBuild;
        this.ressources = ressources;

        colonyDetails = colony;
        energyDetails = energy;
        moneyDetails = money;
        regolythDetails = regolyth;
        bioplasticDetails = bioplastic;
        foodDetails = food;
        techBuild = _techBuild;

        colonyBackground = colonyDetails.transform.parent.GetComponent<Image>();
        energyBackground = energyDetails.transform.parent.GetComponent<Image>();
        moneyBackground = moneyDetails.transform.parent.GetComponent<Image>();
        regolythBackground = regolythDetails.transform.parent.GetComponent<Image>();
        bioplasticBackground = bioplasticDetails.transform.parent.GetComponent<Image>();
        foodBackground = foodDetails.transform.parent.GetComponent<Image>();

        this.buildSystem = buildSystem;

        Assign();
        UpdateUI();
    }

    private void Assign()
    {
        man = FindObjectOfType<MoonManager>();
        researchSystem = FindObjectOfType<ResearchSystem>();

        ColorManager cm = FindObjectOfType<ColorManager>();
        classic = cm.forground;
        notBuildable = cm.importantColor;
        haventTech = cm.veryImportantColor;

        iconImg = gameObject.transform.Find("I_Icon").GetComponent<Image>();
        buttonImg = GetComponent<Image>();
        buildName = gameObject.transform.Find("T_Name").GetComponent<Text>();

        iconImg.color = cm.icon;
        buildName.color = cm.text;

        TooltipCaller ttc = GetComponent<TooltipCaller>();
        ttc.info = man.Traduce(currentBuild.description);
    }

    #endregion

    #region Update interface

    public void UpdateUI()
    {
        iconImg.sprite = currentBuild.icon;
        buildName.text = man.Traduce(currentBuild.name);

        UpdateDetailTexts();
        ResetUI();
        
        if (!HaveTechnologies())
        {
            canBuild = false;
            buttonImg.color = haventTech;
            techBuild.SetActive(true);
        }
        else
        {
            UpdateDetails();
            techBuild.SetActive(false);
        }
    }

    private void UpdateDetailTexts()
    {
        colonyDetails.text = currentBuild.colonist + " / " + currentBuild.maxColonist;
        energyDetails.text = currentBuild.energy + " (" + currentBuild.energyStorage + ")";
        moneyDetails.text = currentBuild.money + " (" + currentBuild.profit + ")";
        regolythDetails.text = currentBuild.regolith + " (" + currentBuild.rigolyteOutput + ")";
        bioplasticDetails.text = currentBuild.bioPlastique + " (" + currentBuild.bioPlastiqueOutput + ")";
        foodDetails.text = currentBuild.food + " (" + currentBuild.foodOutput + ")";
    }

    private bool HaveTechnologies()
    {
        bool haveTech = true;

        foreach (int techId in currentBuild.techsNeeded)
        {
            if (!researchSystem.CheckTechIsUnlock(techId))
                haveTech = false;
        }

        return haveTech;
    }

    private void UpdateDetails()
    {
        List<int> ints = man.HaveRessources(currentBuild.colonist, currentBuild.energy, currentBuild.money, currentBuild.regolith, currentBuild.bioPlastique, currentBuild.food);

        if (ints.Count != 0)
        {
            bool isInteractable = true;

            if (ints.Contains(0) && currentBuild.colonist != 0) { colonyBackground.color = notBuildable; isInteractable = false; }
            if (ints.Contains(1) && currentBuild.energy != 0) { energyBackground.color = notBuildable; isInteractable = false; }
            if (ints.Contains(2) && currentBuild.money != 0) { moneyBackground.color = notBuildable; isInteractable = false; }
            if (ints.Contains(3) && currentBuild.regolith != 0) { regolythBackground.color = notBuildable; isInteractable = false; }
            if (ints.Contains(4) && currentBuild.bioPlastique != 0) { bioplasticBackground.color = notBuildable; isInteractable = false; }
            if (ints.Contains(5) && currentBuild.food != 0) { foodBackground.color = notBuildable; isInteractable = false; }

            if (isInteractable) buttonImg.color = classic;
            else buttonImg.color = notBuildable;

            canBuild = isInteractable;
        }
        else
        {
            canBuild = true;
        }
    }

    private void ResetUI()
    {
        colonyBackground.color = classic;
        energyBackground.color = classic;
        moneyBackground.color = classic;
        regolythBackground.color = classic;
        bioplasticBackground.color = classic;
        foodBackground.color = classic;

        buttonImg.color = classic;
    }

    #endregion

    #region Trigger

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        if (!canBuild) return;
        
        CreatePreview();
    }

    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        UpdateUI();
        ressources.SetActive(true);
    }

    public void OnPointerExit(PointerEventData pointerEventData)
    {
        if(techBuild.activeSelf)
            ressources.SetActive(false);

        techBuild.SetActive(false);
    }

    #endregion

    public void CreatePreview()
    {
        if (Time.timeScale == 0 || !canBuild) return;

        MoonManager man = FindObjectOfType<MoonManager>();

        GameObject go = Instantiate(currentBuild.preview, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
        Preview current = go.GetComponent<Preview>();

        if (buildSystem.currentPreview != null)
        {
            if (!buildSystem.currentPreview.isEngaged)
            {
                Debug.Log("  [INFO:BuildListItem] Destroying preview not placed!");
                man.DeleteGameObjectOfTagList(buildSystem.currentPreview.gameObject);
                Destroy(buildSystem.currentPreview.gameObject);
            }

            buildSystem.currentPreview = current;
        }
        else
        {
            buildSystem.currentPreview = current;
        }
    }
}
