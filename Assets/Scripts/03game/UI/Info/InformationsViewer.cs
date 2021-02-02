using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InformationsViewer : MonoBehaviour
{
    private MoonManager manager;
    private EffectManager effectManager;
    private RessourceData ressources;
    private ColorManager colorManager;

    private GameObject informationUI;
    private GameObject bckEmpty, bckFactoryButton, bckFactoryQueue, bckUnits, bckUnitCost, bckBuilding, bckPreview, bckEnemy;
    private GameObject[] backgrounds;

    private ObjectGroup currentGroup;
    private Entity selectedObject;
    private EntityType entityType;

    // For every entities
    private Text objectName;
    private Image objectIcon;
    private float healthRatio, shieldRatio, energyRatio;
    private int side;

    /// Buildings
    private Button repairBtn, disableBtn;
    private GameObject linkBtn;
    private Image b_healthBar, b_shieldBar;
    private Text b_linked, b_damage, b_range, b_kill;

    /// Factory
    private Image factoryOne, factoryTwo, factoryThree, factoryFour, factoryFive;
    private Image queueOne, queueTwo, queueThree, queueFour, queueFive;
    private Text queueAdvencement;

    /// Units
    private Text u_kill, u_level, u_damage, u_xp;
    private Text colons, money, food;
    private GameObject formationBtn, aggressivityBtn;
    private Image u_healthBar, u_shieldBar, u_energyBar;

    /// Previews
    private GameObject cancelBtn;
    private Image progressBar;

    private Sprite baseIcon;
    private Sprite defend, searchAndDestroy, passif;
    private Sprite square, line, triangle;

    private void Start()
    {
        Initialize();
    }

    #region Initialization

    private void Initialize()
    {
        Assign();
        HideInformation(forced:true);
    }

    private void Assign()
    {
        manager = FindObjectOfType<MoonManager>();
        effectManager = GetComponent<EffectManager>();
        ressources = FindObjectOfType<RessourceData>();
        colorManager = FindObjectOfType<ColorManager>();

        informationUI = GameObject.Find("E_Informations");

        backgrounds = GameObject.FindGameObjectsWithTag("Backgrounds");

        bckFactoryButton = GameObject.Find("BC_FactoryButtons");
        bckFactoryQueue = GameObject.Find("E_Factory");
        bckUnitCost = GameObject.Find("E_UnitCosts");

        bckEmpty = GameObject.Find("E_Basic");
        bckUnits = GameObject.Find("E_UnitSpecs");
        bckEnemy = GameObject.Find("E_EnemySpecs");
        bckBuilding = GameObject.Find("E_BuildingSpecs");
        bckPreview = GameObject.Find("E_PreviewSpecs");

        objectName = GameObject.Find("T_EntityName").GetComponent<Text>();
        objectIcon = GameObject.Find("I_EntityIcon").GetComponent<Image>();

        b_healthBar = GameObject.Find("I_HealthBar (b)").GetComponent<Image>();
        b_shieldBar = GameObject.Find("I_ShieldBar (b)").GetComponent<Image>();
        
        u_healthBar = GameObject.Find("I_HealthBar (u)").GetComponent<Image>();
        u_shieldBar = GameObject.Find("I_ShieldBar (u)").GetComponent<Image>();
        u_energyBar = GameObject.Find("I_BatteryBar (u)").GetComponent<Image>();

        progressBar = GameObject.Find("I_ProgressBar").GetComponent<Image>();

        repairBtn = GameObject.Find("Btn_Repair").GetComponent<Button>();
        disableBtn = GameObject.Find("Btn_Disable").GetComponent<Button>();
        linkBtn = GameObject.Find("Btn_Link");

        factoryOne = GameObject.Find("Btn_FactoryOne/Image").GetComponent<Image>();
        factoryTwo = GameObject.Find("Btn_FactoryTwo/Image").GetComponent<Image>();
        factoryThree = GameObject.Find("Btn_FactoryThree/Image").GetComponent<Image>();
        factoryFour = GameObject.Find("Btn_FactoryFour/Image").GetComponent<Image>();
        factoryFive = GameObject.Find("Btn_FactoryFive/Image").GetComponent<Image>();

        queueOne = GameObject.Find("BC_Queue (1)/Image").GetComponent<Image>();
        queueTwo = GameObject.Find("BC_Queue (2)/Image").GetComponent<Image>();
        queueThree = GameObject.Find("BC_Queue (3)/Image").GetComponent<Image>();
        queueFour = GameObject.Find("BC_Queue (4)/Image").GetComponent<Image>();
        queueFive = GameObject.Find("BC_Queue (5)/Image").GetComponent<Image>();
        queueAdvencement = GameObject.Find("T_QueueAdvencement").GetComponent<Text>();

        aggressivityBtn = GameObject.Find("Btn_Agressivity");
        formationBtn = GameObject.Find("Btn_Formation");
        u_kill = GameObject.Find("T_Property (u_kill)").GetComponent<Text>();
        u_damage = GameObject.Find("T_Property (u_damage)").GetComponent<Text>();
        u_level = GameObject.Find("T_Property (u_level)").GetComponent<Text>();
        u_xp = GameObject.Find("T_Property (u_xp)").GetComponent<Text>();

        b_kill = GameObject.Find("T_Property (b_kill)").GetComponent<Text>();
        b_damage = GameObject.Find("T_Property (b_damage)").GetComponent<Text>();
        b_linked = GameObject.Find("T_Property (b_linked)").GetComponent<Text>();
        b_range = GameObject.Find("T_Property (b_range)").GetComponent<Text>();

        colons = GameObject.Find("U_ColonyTextDetail").GetComponent<Text>();
        money = GameObject.Find("U_MoneyTextDetail").GetComponent<Text>();
        food = GameObject.Find("U_FoodTextDetail").GetComponent<Text>();

        baseIcon = ressources.baseIcon;
        defend = ressources.defend;
        searchAndDestroy = ressources.searchAndDestroy;
        passif = ressources.passif;
        square = ressources.square;
        line = ressources.line;
        triangle = ressources.triangle;

        bckFactoryButton.SetActive(false);
        bckFactoryQueue.SetActive(false);
        bckUnitCost.SetActive(false);
        bckUnits.SetActive(false);
        bckBuilding.SetActive(false);
        bckPreview.SetActive(false);
        bckEnemy.SetActive(false);
    }

    #endregion

    private void Update()
    {
        if (selectedObject != null)
        {
            UpdateSingle(selectedObject);
        }
        else if (currentGroup != null)
        {
            UpdateGroup(currentGroup);
        }
        else
        {
            HideInformation(forced: true);
        }
    }

    #region Group's informations

    public void ShowGroup(ObjectGroup group)
    {
        currentGroup = group;
        entityType = group.groupType;

        healthRatio = CalculateHealth();
        shieldRatio = CalculateShield();
        energyRatio = 0;                                                                                           //? NOT IMPLEMENTED FEATURE
        side = currentGroup.groupSide;

        UpdateInterface();
        ShowLinkButton();
    }

    public void UpdateGroup(ObjectGroup group)
    {
        if (group != currentGroup) return;

        healthRatio = CalculateHealth();
        shieldRatio = CalculateShield();
        energyRatio = 0;                                                                                           //? NOT IMPLEMENTED FEATURE

        UpdateInterface();
    }

    #region Calculate Health / MaxHealth

    private float CalculateHealth()
    {
        if(entityType == EntityType.Building)
        {
            return CalculateBuildingHealth();
        }
        else if (entityType == EntityType.Unit)
        {
            return CalculateUnitHealth();
        }
        else if (entityType == EntityType.Preview)
        {
            return CalculatePreviewProgress();
        }
        else
        {
            return 0;
        }
    }

    private float CalculateBuildingHealth()
    {
        float health = 0;
        float maxHealth = 0;

        foreach(Buildings b in currentGroup.objectsInGroup)
        {
            health += b.health;
            maxHealth += b.maxHealth;
        }

        if (maxHealth != 0)
            return health / maxHealth;
        else
            return 0;
    }

    private float CalculateUnitHealth()
    {
        float health = 0;
        float maxHealth = 0;

        foreach (Unit u in currentGroup.objectsInGroup)
        {
            health += u.health;
            maxHealth += u.maxHealth;
        }

        if (maxHealth != 0)
            return health / maxHealth;
        else
            return 0;
    }

    private float CalculatePreviewProgress()
    {
        float progress = 0;
        float maxProgress = 0;

        foreach (Preview p in currentGroup.objectsInGroup)
        {
            progress += p.health;
            maxProgress += p.maxHealth;
        }

        if (maxProgress != 0)
            return progress / maxProgress;
        else
            return 0;
    }

    #endregion

    #region Calculate Shield / MaxShield

    private float CalculateShield()
    {
        if (entityType == EntityType.Building)
        {
            return CalculateBuildingShield();
        }
        else if (entityType == EntityType.Unit)
        {
            return CalculateUnitShield();
        }
        else
        {
            return 0;
        }
    }

    private float CalculateBuildingShield()
    {
        float shield = 0;
        float maxShield = 0;

        foreach (Buildings b in currentGroup.objectsInGroup)
        {
            shield += b.shield;
            maxShield += b.maxShield;
        }

        if (maxShield != 0)
            return shield / maxShield;
        else
            return 0;
    }

    private float CalculateUnitShield()
    {
        float shield = 0;
        float maxShield = 0;

        foreach (Unit u in currentGroup.objectsInGroup)
        {
            shield += u.shield;
            maxShield += u.maxShield;
        }

        if (maxShield != 0)
            return shield / maxShield;
        else
            return 0;
    }

    #endregion

    #region Calculate Stats

    private int CalculateExperience()
    {
        float experience = 0;
        float maxExperience = 0;

        foreach(Unit u in currentGroup.objectsInGroup)
        {
            experience += u.experience;
            maxExperience += u.maxExperience;
        }

        if (maxExperience != 0)
            return (int)(experience / maxExperience * 100);
        else return 0;
    }

    private int CalculateKills()
    {
        int kills = 0;

        foreach (Unit u in currentGroup.objectsInGroup)
        {
            kills += u.killCount;
        }

        return kills;
    }

    #endregion

    private string GroupTitle()
    {
        if(entityType == EntityType.Building)
        {
            return manager.Traduce("Building group") + " (" + currentGroup.objectsNumber + ")";
        }
        else if(entityType == EntityType.Unit)
        {
            return manager.Traduce("Unit group") + " (" + currentGroup.objectsNumber + ")";
        }
        else if(entityType == EntityType.Preview)
        {
            return manager.Traduce("Preview group") + " (" + currentGroup.objectsNumber + ")";
        }
        else
        {
            Debug.LogError("[ERROR] A error has occured when we try to get the information's title for a group object.");
            return "GROUP_ERROR";
        }
    }

    private Sprite GroupSprite()
    {
        if (entityType != EntityType.Unit)
        {
            return manager.buildData[currentGroup.objectsInGroup[0].id].icon;
        }
        else if (entityType == EntityType.Unit)
        {
            return manager.unitData[currentGroup.objectsInGroup[0].id].unitIcon;
        }
        else
        {
            Debug.LogError("[ERROR] A error has occured when we try to get the sprite for a group object.");
            return null;
        }
    }

    private void ShowGroupStats()
    {
        bckUnits.SetActive(true);
    }

    private void ShowLinkButton()
    {
        linkBtn.SetActive(false);

        if(entityType == EntityType.Building)
        {
            if(currentGroup.groupSide == manager.side)
            {
                if (currentGroup.objectsInGroup.Count == 2)
                {
                    /*if (currentGroup.objectsInGroup[0].canBeLink && currentGroup.objectsInGroup[1].canBeLink)
                    {
                        if (!currentGroup.objectsInGroup[0].linkedTo.Contains(currentGroup.objectsInGroup[1].gameObject))
                        {
                            if (Vector3.Distance(currentGroup.objectsInGroup[0].transform.position, currentGroup.objectsInGroup[1].transform.position) < 50)
                            {
                                linkBtn.SetActive(true);
                            }
                        }
                    }*/
                }
            }
        }
    }

    #endregion

    #region Individual informations

    public void ShowSingle(Entity selected)
    {
        selectedObject = selected;
        entityType = selectedObject.entityType;

        healthRatio = CalculateSingleHealth();
        shieldRatio = CalculateSingleShield();
        energyRatio = 0;                                                                                           //? NOT IMPLEMENTED FEATURE
        side = selectedObject.side;

        linkBtn.SetActive(false);
        UpdateInterface();
    }

    public void UpdateSingle(Entity selected)
    {
        if (selected != selectedObject) return;

        healthRatio = CalculateSingleHealth();
        shieldRatio = CalculateSingleShield();
        energyRatio = 0;                                                                                           //? NOT IMPLEMENTED FEATURE

        UpdateInterface();
    }

    #region Calculate Health and Shield

    private float CalculateSingleHealth()
    {
        return selectedObject.health / selectedObject.maxHealth;
    }

    private float CalculateSingleShield()
    {
        return selectedObject.shield / selectedObject.maxShield;
    }

    #endregion

    private string ObjectTitle()
    {
        return manager.Traduce(selectedObject.entityName);
    }

    private Sprite ObjectSprite()
    {
        if (entityType != EntityType.Unit)
        {
            return manager.buildData[selectedObject.id].icon;
        }
        else if (entityType == EntityType.Unit)
        {
            return manager.unitData[selectedObject.id].unitIcon;
        }
        else
        {
            Debug.LogError("[ERROR] A error has occured when we try to get the sprite for a single object.");
            return null;
        }
    }

    private void ShowSingleStats()
    {
        Unit unit = selectedObject.GetComponent<Unit>();
        bckUnits.SetActive(true);
    }

    #region Factory

    private void ShowFactory()
    {
        if (entityType != EntityType.Building || (currentGroup != null && selectedObject == null) || selectedObject.side != manager.side)
        {
            HideFactory();
        }
        else
        {
            TrainingArea factory = selectedObject.GetComponent<TrainingArea>();

            if (factory != null)
            {
                ResetFactory();
                ShowUnitAvailable(factory);
                UpdateQueue(selectedObject.gameObject, factory);

                bckFactoryButton.SetActive(true);
                bckFactoryQueue.SetActive(true);
            }
            else
            {
                HideFactory();
            }
        }
    }

    private void ShowUnitAvailable(TrainingArea factory)
    {
        if (factory.unitID.Count > 0)
        {
            factoryOne.sprite = manager.unitData[factory.unitID[0]].unitIcon;
            factoryOne.transform.parent.GetComponent<TooltipCaller>().title = manager.Traduce(manager.unitData[factory.unitID[0]].name);
            factoryOne.transform.parent.gameObject.SetActive(true);

            if (factory.unitID.Count > 1)
            {
                factoryTwo.sprite = manager.unitData[factory.unitID[1]].unitIcon;
                factoryTwo.transform.parent.GetComponent<TooltipCaller>().title = manager.Traduce(manager.unitData[factory.unitID[1]].name);
                factoryTwo.transform.parent.gameObject.SetActive(true);

                if (factory.unitID.Count > 2)
                {
                    factoryThree.sprite = manager.unitData[factory.unitID[2]].unitIcon;
                    factoryThree.transform.parent.GetComponent<TooltipCaller>().title = manager.Traduce(manager.unitData[factory.unitID[2]].name);
                    factoryThree.transform.parent.gameObject.SetActive(true);

                    if (factory.unitID.Count > 3)
                    {
                        factoryFour.sprite = manager.unitData[factory.unitID[3]].unitIcon;
                        factoryFour.transform.parent.GetComponent<TooltipCaller>().title = manager.Traduce(manager.unitData[factory.unitID[3]].name);
                        factoryFour.transform.parent.gameObject.SetActive(true);

                        if (factory.unitID.Count > 4)
                        {
                            factoryFive.sprite = manager.unitData[factory.unitID[4]].unitIcon;
                            factoryFive.transform.parent.GetComponent<TooltipCaller>().title = manager.Traduce(manager.unitData[factory.unitID[4]].name);
                            factoryFive.transform.parent.gameObject.SetActive(true);
                        }
                    }
                }
            }
        }
    }

    private void UpdateQueue(GameObject current, TrainingArea factory)
    {
        if (current != selectedObject.gameObject) return;
        ResetQueue();

        if (factory.queue.Count >= 1)
        {
            queueOne.sprite = manager.unitData[factory.queue[0]].unitIcon;
            queueOne.transform.parent.GetComponent<TooltipCaller>().title = manager.Traduce(manager.unitData[factory.queue[0]].name);

            queueAdvencement.text = ((int)(factory.currentTrainingTime / manager.unitData[factory.queue[0]].time * 100)).ToString("00") + "%";

            if (factory.queue.Count >= 2)
            {
                queueTwo.sprite = manager.unitData[factory.queue[1]].unitIcon;
                queueTwo.transform.parent.GetComponent<TooltipCaller>().title = manager.Traduce(manager.unitData[factory.queue[1]].name);

                if (factory.queue.Count >= 3)
                {
                    queueThree.sprite = manager.unitData[factory.queue[2]].unitIcon;
                    queueThree.transform.parent.GetComponent<TooltipCaller>().title = manager.Traduce(manager.unitData[factory.queue[2]].name);

                    if (factory.queue.Count >= 4)
                    {
                        queueFour.sprite = manager.unitData[factory.queue[3]].unitIcon;
                        queueFour.transform.parent.GetComponent<TooltipCaller>().title = manager.Traduce(manager.unitData[factory.queue[3]].name);

                        if (factory.queue.Count == 5)
                        {
                            queueFive.sprite = manager.unitData[factory.queue[4]].unitIcon;
                            queueFive.transform.parent.GetComponent<TooltipCaller>().title = manager.Traduce(manager.unitData[factory.queue[4]].name);
                        }
                    }
                }
            }
        }

        queueOne.gameObject.SetActive(true);
        queueTwo.gameObject.SetActive(true);
        queueThree.gameObject.SetActive(true);
        queueFour.gameObject.SetActive(true);
        queueFive.gameObject.SetActive(true);
    }

    private void HideFactory()
    {
        bckFactoryButton.SetActive(false);
        bckFactoryQueue.SetActive(false);
        bckUnitCost.SetActive(false);

        ResetFactory();
    }

    private void ResetFactory()
    {
        ResetQueue();

        factoryOne.sprite = baseIcon;
        factoryTwo.sprite = baseIcon;
        factoryThree.sprite = baseIcon;
        factoryFour.sprite = baseIcon;
        factoryFive.sprite = baseIcon;

        factoryOne.transform.parent.gameObject.SetActive(false);
        factoryTwo.transform.parent.gameObject.SetActive(false);
        factoryThree.transform.parent.gameObject.SetActive(false);
        factoryFour.transform.parent.gameObject.SetActive(false);
        factoryFive.transform.parent.gameObject.SetActive(false);
    }

    private void ResetQueue()
    {
        queueOne.sprite = baseIcon;
        queueTwo.sprite = baseIcon;
        queueThree.sprite = baseIcon;
        queueFour.sprite = baseIcon;
        queueFive.sprite = baseIcon;

        queueOne.transform.parent.GetComponent<TooltipCaller>().title = manager.Traduce("03_ui_info_empty");
        queueTwo.transform.parent.GetComponent<TooltipCaller>().title = manager.Traduce("03_ui_info_empty");
        queueThree.transform.parent.GetComponent<TooltipCaller>().title = manager.Traduce("03_ui_info_empty");
        queueFour.transform.parent.GetComponent<TooltipCaller>().title = manager.Traduce("03_ui_info_empty");
        queueFive.transform.parent.GetComponent<TooltipCaller>().title = manager.Traduce("03_ui_info_empty");

        queueAdvencement.text = "00%";
    }

    public void OnOverCost(int index)
    {
        Units u = manager.unitData[selectedObject.GetComponent<TrainingArea>().unitID[index]];

        colons.text = u.place.ToString();
        money.text = u.money + " (" + u.moneyOut + ")";
        food.text = u.food + " (" + u.foodOut + ")";

        bckUnitCost.SetActive(true);
    }

    public void OnQuitCost()
    {
        bckUnitCost.SetActive(false);
    }

    #endregion

    #endregion

    #region Interface updates

    private void UpdateInterface()
    {
        UpdateEntity();
        UpdateBars();
        UpdateProperties();
        UpdateBackground();
        UpdateButton();
        AssignColor();

        informationUI.SetActive(true);
    }

    private void UpdateEntity()
    {
        objectName.text = GetTitle();
        objectIcon.sprite = GetSprite();
    }

    private void UpdateBars()
    {
        if(entityType == EntityType.Unit)
        {
            u_healthBar.fillAmount = healthRatio;
            u_shieldBar.fillAmount = shieldRatio;
            u_energyBar.fillAmount = energyRatio;
        }
        else if (entityType == EntityType.Building)
        {
            b_healthBar.fillAmount = healthRatio;
            b_shieldBar.fillAmount = shieldRatio;
            ShowFactory();
        }
        else if (entityType == EntityType.Preview)
        {
            progressBar.fillAmount = healthRatio;
        }
    }

    #region Properties

    private void UpdateProperties()
    {
        HideProperties();

        if (selectedObject == null || currentGroup != null) return;

        if(entityType == EntityType.Unit)
        {
            ShowProperties_Unit();

            Unit uMtr = selectedObject.GetComponent<Unit>();

            u_level.text = uMtr.level.ToString("00");
            u_xp.text = ((int)(uMtr.experience / uMtr.maxExperience * 100)).ToString("000") + "%";
            u_damage.text = ((int)(uMtr.damageAmount / uMtr.cooldown)).ToString("0.0");
            u_kill.text = uMtr.killCount.ToString("00");
        }
        else if(entityType == EntityType.Building)
        {
            ShowProperties_Building();

            Buildings bMtr = selectedObject.GetComponent<Buildings>();

            b_linked.text = manager.Traduce("yes");

            TurretMotor tMtr = selectedObject.GetComponent<TurretMotor>();

            if(tMtr != null)
            {
                b_range.text = tMtr.range.ToString("00");
                b_damage.text = (tMtr.damage / tMtr.cooldown).ToString("0.0");
                b_kill.text = "???";
            }
            else
            {
                HideTurretProperties();
            }
        }
    }

    private void HideProperties()
    {
        u_kill.transform.parent.gameObject.SetActive(false);
        u_damage.transform.parent.gameObject.SetActive(false);
        u_level.transform.parent.gameObject.SetActive(false);
        u_xp.transform.parent.gameObject.SetActive(false);

        b_linked.transform.parent.gameObject.SetActive(false);
        b_kill.transform.parent.gameObject.SetActive(false);
        b_damage.transform.parent.gameObject.SetActive(false);
        b_range.transform.parent.gameObject.SetActive(false);
    }

    private void ShowProperties_Unit()
    {
        u_kill.transform.parent.gameObject.SetActive(true);
        u_damage.transform.parent.gameObject.SetActive(true);
        u_level.transform.parent.gameObject.SetActive(true);
        u_xp.transform.parent.gameObject.SetActive(true);
    }

    private void ShowProperties_Building()
    {
        b_kill.transform.parent.gameObject.SetActive(true);
        b_damage.transform.parent.gameObject.SetActive(true);
        b_linked.transform.parent.gameObject.SetActive(true);
        b_range.transform.parent.gameObject.SetActive(true);
    }

    private void HideTurretProperties()
    {
        b_kill.transform.parent.gameObject.SetActive(false);
        b_damage.transform.parent.gameObject.SetActive(false);
        b_range.transform.parent.gameObject.SetActive(false);
    }

    #endregion

    private string GetTitle()
    {
        if(currentGroup != null && currentGroup.objectsNumber != 0)
        {
            return GroupTitle();
        }
        else if(selectedObject != null)
        {
            return ObjectTitle();
        }
        else
        {
            return "EMPTY_SELECTION";
        }
    }

    private Sprite GetSprite()
    {
        if (currentGroup != null && currentGroup.objectsNumber != 0)
        {
            return GroupSprite();
        }
        else if (selectedObject != null)
        {
            return ObjectSprite();
        }
        else
        {
            return null;
        }
    }

    private void AssignColor()
    {
        Color high = colorManager.forground;
        Color low = colorManager.veryImportantColor;

        if(entityType == EntityType.Unit)
        {
            if (healthRatio <= .2f) u_healthBar.color = low;
            else u_healthBar.color = high;

            if (shieldRatio <= .2f) u_shieldBar.color = low;
            else u_shieldBar.color = high;

            if (energyRatio <= .2f) u_energyBar.color = low;
            else u_energyBar.color = high;
        }
        else if(entityType == EntityType.Building)
        {
            if (healthRatio <= .2f) b_healthBar.color = low;
            else b_healthBar.color = high;

            if (shieldRatio <= .2f) b_shieldBar.color = low;
            else b_shieldBar.color = high;
        }
        else if(entityType == EntityType.Preview)
        {
            if (healthRatio <= .2f) progressBar.color = low;
            else progressBar.color = high;
        }

        Color color;

        if (side != 0)
        {
            color = new Color(0.85f, 0, 0, 1);
        }
        else
        {
            color = manager.colonyColor;
        }

        foreach (GameObject go in backgrounds)
        {
            go.GetComponent<Image>().color = color;
        }
    }

    private void ResetColor()
    {
        foreach (GameObject go in backgrounds)
        {
            go.GetComponent<Image>().color = new Color(0, 0, 0, 1);
        }
    }

    #region Buttons

    private void UpdateButton()
    {
        ResetButton();

        if(entityType == EntityType.Building)
        {
            ShowBuildingButton();
        }
        else if(entityType == EntityType.Unit)
        {
            ShowUnitButton();
        }
    }

    private void ResetButton()
    {
        repairBtn.interactable = false;
    }

    #region Building

    private void ShowBuildingButton()
    {
        if (selectedObject != null)
        {
            ShowSingleBuildingButton();
        }
        else if (currentGroup != null)
        {
            ShowGroupBuildingButton();
        }
    }

    private void ShowSingleBuildingButton()
    {
        Buildings b = selectedObject.GetComponent<Buildings>();

        if(b.side == manager.side)
        {
            repairBtn.interactable = (b.health < b.maxHealth);
            disableBtn.interactable = true;
        }
    }

    private void ShowGroupBuildingButton()
    {
        Buildings b = currentGroup.objectsInGroup[0].GetComponent<Buildings>();

        if (b.side == manager.side)
        {
            repairBtn.interactable = true;
            disableBtn.interactable = AvailableToPause();
        }
    }

    #endregion

    #region Unit

    private void ShowUnitButton()
    {
        if(selectedObject != null)
        {
            Unit mtr = selectedObject.GetComponent<Unit>();
            UpdateAgressivityButton(mtr.agressivity);
            UpdateFormationButton(mtr.formation);
        }
        else if(currentGroup != null)
        {
            Unit mtr = currentGroup.objectsInGroup[0].GetComponent<Unit>();
            UpdateAgressivityButton(mtr.agressivity);
            UpdateFormationButton(mtr.formation);
        }
    }

    #endregion

    #endregion

    #region Background

    private void UpdateBackground()
    {
        ResetBackground();

        if(entityType == EntityType.Building)
        {
            bckBuilding.SetActive(true);
        }
        else if (entityType == EntityType.Unit)
        {
            bckUnits.SetActive(true);
        }
        else if (entityType == EntityType.Preview)
        {
            bckPreview.SetActive(true);
        }
    }

    private void ResetBackground()
    {
        bckBuilding.SetActive(false);
        bckUnits.SetActive(false);
        bckPreview.SetActive(false);
        bckEnemy.SetActive(false);
    }

    #endregion

    public void HideInformation(GameObject cur = null, ObjectGroup group = null, bool forced = false)
    {
        if((cur == selectedObject && selectedObject != null) || (group == currentGroup && currentGroup != null) || forced)
        {
            informationUI.SetActive(false);

            selectedObject = null;
            currentGroup = null;

            healthRatio = 0;
            shieldRatio = 0;
            energyRatio = 0;

            ResetColor();
        }
    }

    private void DisableSelectors()
    {
        if(selectedObject != null)
        {
            selectedObject.Marker(false);
        }
        else if(currentGroup != null && currentGroup != new ObjectGroup())
        {
            foreach (Entity e in currentGroup.objectsInGroup)
            {
                e.Marker(false);
            }
        }
    }

    #endregion

    #region Actions

    public void Btn_Destroy()
    {
        if (entityType == EntityType.Building)
        {
            DestroyBuilding();
        }
        else if (entityType == EntityType.Unit)
        {
            DestroyUnit();
        }
    }

    public void SendAttackOrder(Transform target)
    {
        if (entityType == EntityType.Unit)
        {
            UnitAttack(target);
        }
        else if (entityType == EntityType.Building)
        {
            TurretAttack(target);
        }
    }

    public void Btn_Hide()
    {
        DisableSelectors();
        HideInformation(forced: true);
    }

    #region Building

    public void Btn_Repair()
    {
        if(currentGroup != null)
        {
            foreach(Entity b in currentGroup.objectsInGroup)
            {
                b.EngageUnits(UnitType.Worker);
            }
        }
        else if(selectedObject != null)
        {
            selectedObject.EngageUnits(UnitType.Worker);
        }
    }

    public void Btn_Disable()
    {
        if (entityType == EntityType.Building)
        {
            if (selectedObject != null)
            {
                Buildings health = selectedObject.GetComponent<Buildings>();

                if (health.side == manager.side)
                {
                    health.Pause();
                }
            }
            else if (currentGroup != null && currentGroup.objectsInGroup.Count != 0)
            {
                foreach (Buildings bh in currentGroup.objectsInGroup)
                {
                    bh.Pause();
                }
            }
        }
    }

    private void DestroyBuilding()
    {
        if (selectedObject != null && selectedObject.GetComponent<Buildings>().side == manager.side)
        {
            Destroy(selectedObject.gameObject);
            HideInformation(forced:true);
        }
        else if (currentGroup != null && currentGroup.objectsInGroup.Count != 0)
        {
            foreach (Buildings bh in currentGroup.objectsInGroup)
            {
                Destroy(bh.gameObject);
            }

            HideInformation(forced: true);
        }
    }

    public void Btn_Factory(int index)
    {
        selectedObject.GetComponent<TrainingArea>().Enqueue(index);
    }

    public void Btn_Link()
    {
        /*GameObject startBuilding = currentGroup.objectsInGroup[0].gameObject;
        GameObject endBuilding = currentGroup.objectsInGroup[1].gameObject;

        Vector3 startPos = startBuilding.GetComponent<BoxCollider>().ClosestPointOnBounds(transform.position);
        Vector3 endPos = endBuilding.GetComponent<BoxCollider>().ClosestPointOnBounds(startPos);

        Vector3[] positions = WayCreator.GeneratePath(startPos, endPos, 0f, 1f);

        foreach (Vector3 pos in positions)
        {
            GameObject go = Instantiate(ressources.tunelPrefab);
            go.transform.position = pos;
            go.transform.SetParent(startBuilding.transform);

            Vector3 dir = pos - startPos;
            Quaternion lookRotation = Quaternion.LookRotation(dir);
            go.transform.rotation = lookRotation;
        }

        currentGroup.objectsInGroup[0].linkedTo.Add(endBuilding);
        currentGroup.objectsInGroup[1].linkedTo.Add(startBuilding);

        linkBtn.SetActive(false);*/
    }

    public void SetRallyPoint(Vector3 position)
    {
        if(entityType == EntityType.Building)
        {
            if(selectedObject != null && selectedObject.side == manager.side)
            {
                TrainingArea ta;
                selectedObject.TryGetComponent(out ta);

                if(ta != null)
                {
                    ta.SetRallyPoint(position);
                    effectManager.GroundTargetEffect(position);
                }
            }
            else if(currentGroup != null && currentGroup.groupSide == manager.side)
            {
                foreach(Entity b in currentGroup.objectsInGroup)
                {
                    TrainingArea ta;
                    b.TryGetComponent(out ta);

                    if (ta != null)
                    {
                        ta.SetRallyPoint(position);
                    }
                }

                effectManager.GroundTargetEffect(position);
            }
        }
    }

    #region Attack

    private void TurretAttack(Transform target)
    {
        if (selectedObject != null)
        {
            TurretAttackSingle(target);
        }
        else if (currentGroup != null)
        {
            TurretAttackGroup(target);
        }
    }

    private void TurretAttackSingle(Transform target)
    {
        Buildings motor = selectedObject.GetComponent<Buildings>();

        if (motor.GetComponent<TurretMotor>() != null)
        {
            if (target.GetComponent<Entity>().entityType == EntityType.Building)
            {
                AttackBuilding(motor, target);
            }
            else if (target.GetComponent<Entity>().entityType == EntityType.Unit)
            {
                AttackUnit(motor, target);
            }
        }
    }

    private void TurretAttackGroup(Transform target)
    {
        foreach (Buildings b in currentGroup.objectsInGroup)
        {
            if (b.GetComponent<TurretMotor>() != null)
            {
                if (target.GetComponent<Entity>().entityType == EntityType.Building)
                {
                    AttackBuilding(b, target);
                }
                else if (target.GetComponent<Entity>().entityType == EntityType.Unit)
                {
                    AttackUnit(b, target);
                }
            }
        }
    }

    private void AttackUnit(Buildings motor, Transform target)
    {
        try
        {
            if (manager.GetWarStatut(motor.side, target.GetComponent<Unit>().side))
            {
                motor.GetComponent<TurretMotor>().SetTarget(target);
            }
            else if (motor.side != target.GetComponent<Unit>().side)
            {
                manager.Notify(manager.Traduce("03_notif_needwar"), priority: 2);
            }
        }
        catch
        {
            Debug.LogError("  [StatsViewer] GetWarStatut : " + motor.side + " / " + target.GetComponent<Unit>().side);
            manager.Notify(manager.Traduce("03_notif_needwar"), priority: 2);
        }
    }

    private void AttackBuilding(Buildings motor, Transform target)
    {
        try
        {
            if (manager.GetWarStatut(motor.side, target.GetComponent<Buildings>().side))
            {
                motor.GetComponent<TurretMotor>().SetTarget(target);
            }
            else if (motor.side != target.GetComponent<Buildings>().side)
            {
                manager.Notify(manager.Traduce("03_notif_needwar"), priority: 2);
            }
        }
        catch
        {
            Debug.LogError("  [StatsViewer] GetWarStatut : " + motor.side + " / " + target.GetComponent<Buildings>().side);
            manager.Notify(manager.Traduce("03_notif_needwar"), priority: 2);
        }
    }

    #endregion

    #endregion

    #region Unit

    public void Btn_Aggressivity()
    {
        int agressivity = -1;

        if(selectedObject != null && selectedObject.side == manager.side)
        {
            agressivity = selectedObject.GetComponent<Unit>().ChangeAgressivity();
        }
        else if(currentGroup != null/* && currentGroup.objectsInGroup.Count != 0*/ && currentGroup.groupSide == manager.side)
        {
            agressivity = currentGroup.objectsInGroup[0].GetComponent<Unit>().ChangeAgressivity();

            foreach(Unit u in currentGroup.objectsInGroup)
            {
                u.agressivity = agressivity;
            }
        }

        UpdateAgressivityButton(agressivity);
    }

    private void UpdateAgressivityButton(int agressivity)
    {
        if (agressivity == 0)
        {
            aggressivityBtn.transform.Find("Image").GetComponent<Image>().sprite = passif;
            aggressivityBtn.GetComponent<TooltipCaller>().title = manager.Traduce("03_ui_info_btn_agressivity_passive");
        }
        else if (agressivity == 1)
        {
            aggressivityBtn.transform.Find("Image").GetComponent<Image>().sprite = defend;
            aggressivityBtn.GetComponent<TooltipCaller>().title = manager.Traduce("03_ui_info_btn_agressivity_inrange");
        }
        else if (agressivity == 2)
        {
            aggressivityBtn.transform.Find("Image").GetComponent<Image>().sprite = searchAndDestroy;
            aggressivityBtn.GetComponent<TooltipCaller>().title = manager.Traduce("03_ui_info_btn_agressivity_search");
        }
    }

    public void Btn_Formation()
    {
        int formation = -1;

        if(selectedObject != null && selectedObject.side == manager.side)
        {
            formation = selectedObject.GetComponent<Unit>().ChangeFormation();
        }
        else if(currentGroup != null && currentGroup.groupSide == manager.side)
        {
            formation = currentGroup.objectsInGroup[0].GetComponent<Unit>().ChangeFormation();

            foreach (Unit u in currentGroup.objectsInGroup)
            {
                u.formation = formation;
            }
        }

        UpdateFormationButton(formation);
    }

    private void UpdateFormationButton(int formation)
    {
        if (formation == 0)
        {
            formationBtn.transform.Find("Image").GetComponent<Image>().sprite = square;
            formationBtn.GetComponent<TooltipCaller>().title = manager.Traduce("03_ui_info_btn_formation_square");
        }
        else if (formation == 1)
        {
            formationBtn.transform.Find("Image").GetComponent<Image>().sprite = line;
            formationBtn.GetComponent<TooltipCaller>().title = manager.Traduce("03_ui_info_btn_formation_line");
        }
        else if (formation == 2)
        {
            formationBtn.transform.Find("Image").GetComponent<Image>().sprite = triangle;
            formationBtn.GetComponent<TooltipCaller>().title = manager.Traduce("03_ui_info_btn_formation_triangle");
        }
    }

    #region Attack

    private void UnitAttack(Transform target)
    {
        if(selectedObject != null)
        {
            AttackUnitSolo(target);
        }
        else if(currentGroup != null)
        {
            AttackUnitInGroup(target);
        }
    }

    private void AttackUnitSolo(Transform target, Unit motor = null)
    {
        if(motor == null)
            motor = selectedObject.GetComponent<Unit>();

        if (motor.side != manager.side) return;

        Entity e = target.GetComponent<Entity>();

        if(motor.side != e.side)
        {
            if (manager.GetWarStatut(motor.side, e.side))
            {
                if (motor.unitType == UnitType.Military)
                {
                    motor.AddOrder(new Order(OrderType.Entity, e: e));
                }
            }
        }
        else
        {
            if(motor.unitType == UnitType.Worker && e.entityType != EntityType.Unit)
            {
                motor.AddOrder(new Order(OrderType.Entity, e: e));
            }
        }
    }

    private void AttackUnitInGroup(Transform target)
    {
        if (currentGroup.objectsInGroup[0].side != manager.side) return;

        foreach (Unit unit in currentGroup.objectsInGroup)
        {
            AttackUnitSolo(target, unit);
        }
    }

    #endregion

    #region Player order

    public void SetPlayerOrder(Vector3 target)
    {
        if (entityType == EntityType.Unit)
        {
            if (selectedObject != null)
            {
                SendPlayerOrder(target);
            }
            else if (currentGroup != null && currentGroup.objectsNumber != 0)
            {
                SendPlayerOrderAtGroup(target);
            }
        }
    }

    private void SendPlayerOrder(Vector3 target)
    {
        if (selectedObject.side != manager.side) return;

        selectedObject.GetComponent<Unit>().AddOrder(new Order(OrderType.Position, target));
        effectManager.GroundTargetEffect(target);
    }

    private void SendPlayerOrderAtGroup(Vector3 target)
    {
        if (currentGroup.objectsInGroup[0].side != manager.side) return;

        Vector3[] positions = Utility.GetSquareFormationPositions(currentGroup.objectsInGroup.Count, 2);
        int positionIndex = 0;

        foreach (Unit unit in currentGroup.objectsInGroup)
        {
            unit.AddOrder(new Order(OrderType.Position, target + positions[positionIndex]));
            effectManager.GroundTargetEffect(target + positions[positionIndex]);
            positionIndex++;
        }
    }

    #endregion

    private void DestroyUnit()
    {
        if (selectedObject != null && selectedObject.side == manager.side)
        {
            Destroy(selectedObject.gameObject);
            HideInformation(forced:true);
        }
        else if (currentGroup != null && currentGroup.objectsInGroup[0].side == manager.side)
        {
            foreach (Unit um in currentGroup.objectsInGroup)
            {
                Destroy(um.gameObject);
            }

            HideInformation(forced: true);
        }
    }

    #endregion

    #region Preview

    public void Btn_Cancel()
    {
        if (selectedObject != null && selectedObject.side == manager.side)
        {
            selectedObject.GetComponent<Preview>().Cancel();
            HideInformation(forced:true);
        }
        else if(currentGroup != null && currentGroup.objectsInGroup[0].side == manager.side)
        {
            foreach(Preview p in currentGroup.objectsInGroup)
            {
                p.Cancel();
                HideInformation(forced:true);
            }
        }
    }

    #endregion

    #endregion

    #region Verification

    private bool AvailableToPause()
    {
        return true;

        if(currentGroup != null && currentGroup != new ObjectGroup())
        {
            foreach(Buildings b in currentGroup.objectsInGroup)
            {
                
            }
        }

        return true;
    }

    public void WhenEntityIsDestroyed(Entity mtr)
    {
        if(selectedObject != null && mtr.gameObject == selectedObject)
        {
            HideInformation(forced: true);
        }
        else if(currentGroup != null)
        {
            currentGroup.objectsInGroup.Remove(mtr);
            ShowGroup(currentGroup);
        }
    }

    #endregion
}
