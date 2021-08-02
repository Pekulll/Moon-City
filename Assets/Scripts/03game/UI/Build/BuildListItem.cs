using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BuildListItem : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    private GameObject techBuild, resources;

    private Text colonyDetails, energyDetails, moneyDetails, regolythDetails, metalDetails, polymerDetails, foodDetails, buildName;
    private Image colonyBackground, energyBackground, moneyBackground, regolythBackground, metalBackground, polymerBackground, foodBackground;

    private Image iconImg;
    private Image buttonImg;

    private Building currentBuild;

    private MoonManager man;
    private ResearchSystem researchSystem;
    private BuildSystem buildSystem;

    private bool canBuild;

    private ColorManager colorManager;
    private Color classic, notBuildable, haventTech;

    #region Initialization

    public void OnInstantiate(Building _currentBuild, GameObject resources, Text colony, Text energy, Text money, Text regolyth, Text metal, Text polymer, Text food, GameObject _techBuild, BuildSystem buildSystem)
    {
        currentBuild = _currentBuild;
        this.resources = resources;

        colonyDetails = colony;
        energyDetails = energy;
        moneyDetails = money;
        regolythDetails = regolyth;
        metalDetails = metal;
        polymerDetails = polymer;
        foodDetails = food;
        techBuild = _techBuild;

        colonyBackground = colonyDetails.transform.parent.GetComponent<Image>();
        energyBackground = energyDetails.transform.parent.GetComponent<Image>();
        moneyBackground = moneyDetails.transform.parent.GetComponent<Image>();
        regolythBackground = regolythDetails.transform.parent.GetComponent<Image>();
        metalBackground = metalDetails.transform.parent.GetComponent<Image>();
        polymerBackground = polymerDetails.transform.parent.GetComponent<Image>();
        foodBackground = foodDetails.transform.parent.GetComponent<Image>();

        this.buildSystem = buildSystem;

        Assign();
        UpdateUI();
    }

    private void Assign()
    {
        man = FindObjectOfType<MoonManager>();
        researchSystem = FindObjectOfType<ResearchSystem>();

        colorManager = FindObjectOfType<ColorManager>();
        classic = colorManager.forground;
        notBuildable = colorManager.importantColor;
        haventTech = colorManager.veryImportantColor;

        iconImg = gameObject.transform.Find("I_Icon").GetComponent<Image>();
        buttonImg = GetComponent<Image>();
        buildName = gameObject.transform.Find("T_Name").GetComponent<Text>();

        iconImg.color = colorManager.icon;
        buildName.color = colorManager.text;

        TooltipCaller ttc = GetComponent<TooltipCaller>();
        ttc.info = man.Traduce(currentBuild.description);
    }

    #endregion

    #region Update interface

    public void UpdateUI()
    {
        iconImg.sprite = currentBuild.icon;
        buildName.text = man.Traduce(currentBuild.name);

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
            UpdateDetailTexts();
            techBuild.SetActive(false);
        }
    }

    private void UpdateDetailTexts()
    {
        colonyDetails.text = currentBuild.colonist + " / " + currentBuild.maxColonist.SignedString();
        energyDetails.text = currentBuild.energy.SignedString() + " [" + currentBuild.energyStorage + "]";
        moneyDetails.text = currentBuild.money + " (" + currentBuild.profit.SignedString() + ")";
        regolythDetails.text = currentBuild.regolith + " (" + currentBuild.rigolyteOutput.SignedString() + ")";
        metalDetails.text = currentBuild.metal + " (" + currentBuild.metalOutput.SignedString() + ")";
        polymerDetails.text = currentBuild.polymer + " (" + currentBuild.polymerOutput.SignedString() + ")";
        foodDetails.text = currentBuild.food + " (" + currentBuild.foodOutput.SignedString() + ")";

        if (currentBuild.energy < 0 && energyBackground.color != notBuildable) energyDetails.color = colorManager.importantColor;
        else if (currentBuild.energy > 0 && energyBackground.color != notBuildable) energyDetails.color = colorManager.finished;
        else energyDetails.color = colorManager.text;
        
        if (currentBuild.profit < 0 && moneyBackground.color != notBuildable) moneyDetails.color = colorManager.importantColor;
        else if (currentBuild.profit > 0 && moneyBackground.color != notBuildable) moneyDetails.color = colorManager.finished;
        else moneyDetails.color = colorManager.text;
        
        if (currentBuild.rigolyteOutput < 0 && regolythBackground.color != notBuildable) regolythDetails.color = colorManager.importantColor;
        else if (currentBuild.rigolyteOutput > 0 && regolythBackground.color != notBuildable) regolythDetails.color = colorManager.finished;
        else regolythDetails.color = colorManager.text;
        
        if (currentBuild.metalOutput < 0 && metalBackground.color != notBuildable) metalDetails.color = colorManager.importantColor;
        else if (currentBuild.metalOutput > 0 && metalBackground.color != notBuildable) metalDetails.color = colorManager.finished;
        else metalDetails.color = colorManager.text;
        
        if (currentBuild.polymerOutput < 0 && polymerBackground.color != notBuildable) polymerDetails.color = colorManager.importantColor;
        else if (currentBuild.polymerOutput > 0 && polymerBackground.color != notBuildable) polymerDetails.color = colorManager.finished;
        else polymerDetails.color = colorManager.text;
        
        if (currentBuild.foodOutput < 0 && foodBackground.color != notBuildable) foodDetails.color = colorManager.importantColor;
        else if (currentBuild.foodOutput > 0 && foodBackground.color != notBuildable) foodDetails.color = colorManager.finished;
        else foodDetails.color = colorManager.text;
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
        List<int> ints = man.HaveResources(currentBuild.colonist, currentBuild.energy, currentBuild.money, currentBuild.regolith, currentBuild.metal, currentBuild.polymer, currentBuild.food);

        if (ints.Count != 0)
        {
            bool isInteractable = true;

            if (ints.Contains(0)) { colonyBackground.color = notBuildable; isInteractable = false; }
            if (ints.Contains(1)) { energyBackground.color = notBuildable; isInteractable = false; }
            if (ints.Contains(2)) { moneyBackground.color = notBuildable; isInteractable = false; }
            if (ints.Contains(3)) { regolythBackground.color = notBuildable; isInteractable = false; }
            if (ints.Contains(3)) { metalBackground.color = notBuildable; isInteractable = false; }
            if (ints.Contains(4)) { polymerBackground.color = notBuildable; isInteractable = false; }
            if (ints.Contains(5)) { foodBackground.color = notBuildable; isInteractable = false; }
            if (ints.Contains(6)) { metalBackground.color = notBuildable; isInteractable = false; }

            buttonImg.color = (isInteractable) ? classic : notBuildable;
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
        metalBackground.color = classic;
        polymerBackground.color = classic;
        foodBackground.color = classic;

        buttonImg.color = classic;
    }

    #endregion

    #region Trigger

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        CreatePreview();
    }

    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        UpdateUI();
        resources.SetActive(true);
    }

    public void OnPointerExit(PointerEventData pointerEventData)
    {
        if(techBuild.activeSelf)
            resources.SetActive(false);

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
        }
        
        current.side = man.side;
        buildSystem.currentPreview = current;
    }
}
