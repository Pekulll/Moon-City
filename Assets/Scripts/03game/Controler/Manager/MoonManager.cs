using System;
using System.Collections;
using System.Collections.Generic;
using DiscordPresence;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class MoonManager : MonoBehaviour {

    [Header("Time properties")]
    public int day;
    public int seconds;
    [SerializeField] private PonctualEvent[] ponctualEvents;

    [Header("Colony properties")]
    public int side;
    public Color colonyColor = new Color(0, 0, 0.9f, 1);
    [HideInInspector] public bool canPaused = true, isPaused = false, canInteractWithUI = true;

    [Header("Save properties")]
    [SerializeField] private string versionCode;
    [HideInInspector] public string saveName;

    [Header("Database")]
    public Building[] buildData;
    public Units[] unitData;
    public TechtreeDatabase techData;
    [SerializeField] private GameObject enemyColonyCenter;

    [Header ("Other")]
    public bool isOverUI;
    [HideInInspector] public string notificationHistory;

    private int enemyCount;

    private WaitForSeconds timeScale;

    [HideInInspector] public int seed;

    [HideInInspector] public ColonyStats colonyStats;
    private NotificationSystem notif;
    private InformationsViewer viewer;
    private TagSystem tagSystem;
    private EventSystem eventSystem;
    private WaveSystem waveSystem;
    private DiplomacySystem diplomacySystem;
    private TraduceSystem translate;
    private MapGenerator mapGenerator;
    private ResearchSystem researchSystem;
    private InterfaceManager interfaceManager;
    private ColorManager colorManager;
    private CommandSystem commandSystem;
    private RessourceData resources;
    private EndlessTerrain endlessTerrain;
    private GroupSystem groupSystem;
    private QuestManager questManager;
    private SectorManager sectorManager;
    private TutorialManager tutorialManager;
    private EndgameChecker endgameChecker;
    private TradingSystem tradingSystem;

    private Text dayText, hoursText;
    private Text colonyText, energyText, moneyText, regolithText, metalText, polymerText, foodText;
    private Text notificationText, nextWaveText;

    private Text debugInfo;

    private GameObject gamePaused;
    private GameObject notifHistory;
    private GameObject saveDone;
    private GameObject loadingScreen;

    private List<ColonyStats> players;

    public SavedScene data;

    private Text warnText;

    private AudioSource playerAudio;

    public Vector2 dateNextWave;

    private Animator popup;
    private Text outMoney;
    private Text outRegolith;
    private Text outMetal;
    private Text outPolymer;
    private Text outFood;

    public int resourceWarning;

    private void Awake() {
        AssignAll();
    }

    private void Start() {
        data = SaveSystem.Load<SavedScene>(saveName + ".json");
        seed = data.configuration.seed;
        mapGenerator.PregenerateMap (seed, LoadDataAfterMapGeneration);
    }

    private void Init () {
        canInteractWithUI = true;
        ManageUIStats ();
        ManageUITime ();
        StartCoroutine (HoursMotor ());
    }

    private void Update () {
        debugInfo.text = "<b>Game version : " + versionCode + "       Save loaded : " + saveName + "</b>\n" +
            "Player pos : " + colonyStats.transform.position + "    <i>(Mouse pos : " + Input.mousePosition + ")</i>\n" +
            "Current side : " + side + "\t\t\t<size=15><i>(Ctrl + H to view more)</i></size>";

        ManageUIStats ();
    }

    #region Main methods

    private void AssignAll () {
        //Prioritary assignement
        saveName = PlayerPrefs.GetString ("CurrentSave");
        loadingScreen = GameObject.Find ("LoadingScreen");

        //Main assignements
        colonyStats = GameObject.Find ("Player").GetComponent<ColonyStats> ();
        notif = GameObject.Find ("E_Notification").GetComponent<NotificationSystem> ();
        tagSystem = GetComponent<TagSystem> ();
        eventSystem = GetComponent<EventSystem> ();
        waveSystem = GetComponent<WaveSystem> ();
        diplomacySystem = GetComponent<DiplomacySystem> ();
        translate = GameObject.Find ("Traductor").GetComponent<TraduceSystem> ();
        mapGenerator = GetComponent<MapGenerator> ();
        researchSystem = GetComponent<ResearchSystem> ();
        interfaceManager = GetComponent<InterfaceManager> ();
        playerAudio = GameObject.Find ("Player").GetComponent<AudioSource> ();
        commandSystem = GetComponent<CommandSystem> ();
        resources = GetComponent<RessourceData> ();
        viewer = GetComponent<InformationsViewer> ();
        endlessTerrain = GetComponent<EndlessTerrain> ();
        groupSystem = GetComponent<GroupSystem> ();
        questManager = GetComponent<QuestManager>();
        colorManager = GetComponent<ColorManager>();
        sectorManager = GetComponent<SectorManager>();
        tutorialManager = FindObjectOfType<TutorialManager>();
        endgameChecker = GetComponent<EndgameChecker>();
        tradingSystem = GetComponent<TradingSystem>();

        //Time assignements
        dayText = GameObject.Find ("DayText").GetComponent<Text> ();
        hoursText = GameObject.Find ("HoursText").GetComponent<Text> ();

        //Colony assignements
        colonyText = GameObject.Find ("ColonyText").GetComponent<Text> ();
        energyText = GameObject.Find ("EnergyText").GetComponent<Text> ();
        moneyText = GameObject.Find ("MoneyText").GetComponent<Text> ();
        regolithText = GameObject.Find ("RegolithText").GetComponent<Text> ();
        metalText = GameObject.Find ("MetalText").GetComponent<Text> ();
        polymerText = GameObject.Find ("BioplasticText").GetComponent<Text> ();
        foodText = GameObject.Find ("FoodText").GetComponent<Text> ();

        //Popup
        popup = GameObject.Find("E_Output").GetComponent<Animator>();
        outMoney = GameObject.Find("T_OutMoney").GetComponent<Text>();
        outRegolith = GameObject.Find("T_OutRegolith").GetComponent<Text>();
        outMetal = GameObject.Find("T_OutMetal").GetComponent<Text>();
        outPolymer = GameObject.Find("T_OutBioplastic").GetComponent<Text>();
        outFood = GameObject.Find("T_OutFood").GetComponent<Text>();

        //Others
        UpdatePresence (colonyStats.colony.name, "Sol " + day + " :: " + (seconds / 60) + " o'clock", "moon", "", versionCode, "");
        debugInfo = GameObject.Find ("DebugInfo").GetComponent<Text> ();
        gamePaused = GameObject.Find ("B_GamePaused");
        timeScale = new WaitForSeconds (0.4f);

        notificationText = GameObject.Find ("T_NotificationHistory").GetComponent<Text> ();
        notificationHistory = "";
        notifHistory = GameObject.Find ("BC_NotificationMenu");
        saveDone = GameObject.Find ("B_GameSaved");

        warnText = GameObject.Find ("T_BuildingWarn").GetComponent<Text> ();
        nextWaveText = GameObject.Find ("T_NextWave").GetComponent<Text> ();

        //SetActive(false)
        debugInfo.transform.parent.gameObject.SetActive (false);
        gamePaused.SetActive (false);
        notifHistory.SetActive (false);
        HideWarnText ();
        Invoke ("UpdateTimeUntilNextWave", 0.001f);
    }

    //Main UI method

    private void ManageUIStats () {
        colonyText.text = colonyStats.workers + " / " + colonyStats.maxColonist;
        energyText.text = colonyStats.energy + " / " + colonyStats.energyStorage + "\n<size=12>" + colonyStats.energyOutput.SignedString() + "</size>";

        UpdateStatsColor();

        if (colonyStats.energy > 0 || colonyStats.energyOutput > 0)
        {
            energyText.color = colorManager.text;
            moneyText.text = colonyStats.money + " M€\n<size=12>" + colonyStats.profit.SignedString() + "</size>";
            regolithText.text = colonyStats.regolith.ToString ("0.0") + " / " + colonyStats.regolithStock.ToString ("000") + "\n<size=12>" + colonyStats.regolithOutput.SignedString("0.0") + "</size>";
            metalText.text = colonyStats.metal.ToString ("0.0") + " / " + colonyStats.metalStock.ToString ("000") + "\n<size=12>" + colonyStats.metalOutput.SignedString("0.0") + "</size>";
            polymerText.text = colonyStats.polymer.ToString ("0.0") + " / " + colonyStats.polymerStock.ToString ("000") + "\n<size=12>" + colonyStats.polymerOutput.SignedString("0.0") + "</size>";
            foodText.text = colonyStats.food.ToString ("0.0") + " / " + colonyStats.foodStock.ToString ("000") + "\n<size=12>" + colonyStats.foodOutput.SignedString("0.0") + "</size>";
        }
        else
        {
            energyText.color = colorManager.textWarning;

            moneyText.text = colonyStats.money + "$\n<size=12>-" + colonyStats.moneyLoss + "</size>";
            regolithText.text = colonyStats.regolith.ToString ("0.0") + " / " + colonyStats.regolithStock.ToString ("000") + "\n<size=12>" + (colonyStats.regolithLoss).SignedString("0.0") + "</size>";
            metalText.text = colonyStats.regolith.ToString ("0.0") + " / " + colonyStats.metalStock.ToString ("000") + "\n<size=12>" + (colonyStats.metalLoss).SignedString("0.0") + "</size>";
            polymerText.text = colonyStats.polymer.ToString ("0.0") + " / " + colonyStats.polymerStock.ToString ("000") + "\n<size=12>" + (colonyStats.polymerLoss).SignedString("0.0") + "</size>";
            foodText.text = colonyStats.food.ToString ("0.0") + " / " + colonyStats.foodStock.ToString ("000") + "\n<size=12>" + (colonyStats.foodLoss).SignedString("0.0") + "</size>";
        }
    }

    private void UpdateStatsColor()
    {
        if (colonyStats.profit <= 0)
            moneyText.color = colorManager.textWarning;
        else
            moneyText.color = colorManager.text;

        if (colonyStats.regolithOutput < 0)
            regolithText.color = colorManager.textWarning;
        else
            regolithText.color = colorManager.text;

        if (colonyStats.polymerOutput < 0)
            polymerText.color = colorManager.textWarning;
        else
            polymerText.color = colorManager.text;

        if (colonyStats.foodOutput < 0)
            foodText.color = colorManager.textWarning;
        else
            foodText.color = colorManager.text;
    }

    /*private void CheckShortcut()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (gameIsPaused)
            {
                gameIsPaused = false;
                Time.timeScale = 1;
                gamePaused.SetActive(false);
            }
            else
            {
                gameIsPaused = true;
                gamePaused.SetActive(true);
                Time.timeScale = 0;
            }
        }
    }*/

    #endregion

    #region Time methods

    private void ManageUITime () {
        dayText.text = translate.GetTraduction ("Sol") + " " + string.Format ("{0:000}", day);
        hoursText.text = string.Format ("{0:000}:{1:00}", Mathf.Floor (seconds / 60), seconds % 60);
    }

    private IEnumerator HoursMotor ()
    {
        while (true) {
            yield return timeScale;
            seconds++;

            CheckTimeAction ();
            CheckEvent ();

            hoursText.text = string.Format ("{0:000}:{1:00}", seconds / 60, seconds % 60);

            if (loadingScreen.activeSelf) {
                loadingScreen.SetActive (false);
            }
        }
    }

    private void CheckTimeAction () {
        if (seconds % 60 == 0) {
            ActionEveryOneHour ();
            ActionEveryOneHourEnemy ();
        }

        if (seconds % 120 == 0) {
            ActionEveryTwoHours ();
            ActionEveryTwoHoursEnemy ();
        }

        if (seconds % 180 == 0) {
            ActionEveryThreeHours ();
            ActionEveryThreeHoursEnemy ();
        }

        if (seconds % 600 == 0) {
            ActionEveryTenHours ();
            ActionEveryTenHoursEnemy ();
        }

        if (seconds >= 20160) {
            NewDayPassed ();
        }
    }

    private void NewDayPassed () {
        seconds = 0;
        day++;
        dayText.text = translate.GetTraduction ("Sol") + " " + string.Format ("{0:000}", day);

        ApplyAllDamageOnBuilding ();

        Notify (Traduce("03_notif_daysurvived"));
        Debug.Log ("[INFO:MoonManager] New day survived !");
    }

    #region Action every x hours

    private void ActionEveryOneHour () {
        Debug.Log ("[INFO:MoonManager] Output added !");

        AddOutput();

        if (dateNextWave.x == 0 && dateNextWave.y == 0) {
            waveSystem.UpdateDateNextWave ();
        }

        UpdateTimeUntilNextWave ();
        researchSystem.IncreaseResearch(colonyStats.research);

        UpdatePresence(saveName, "Working...", "moon", "", "Sol " + day + " :: " + (seconds / 60) + " o'clock", "");
    }

    private void ActionEveryTwoHours () {
        if (CheckResources())
        {
            if (resourceWarning >= 2)
            {
                endgameChecker.DeclareDefeat(EndgameType.Economic);
            }
            else
            {
                resourceWarning++;
            }
        }
        else
        {
            CheckRessourcesAmount();
        }
    }

    private void ActionEveryThreeHours () {

    }

    private void ActionEveryTenHours () {
        if (day != 0 || seconds >= 1800) {
            waveSystem.EngageWave ();
            Debug.Log ("[INFO:MoonManager] Everythings good.");
        }
    }

    private void ActionEveryOneHourEnemy ()
    {
        GameObject[] gos = FindTags (new Tag[2] { Tag.Enemy, Tag.Core });

        foreach (GameObject cur in gos)
        {
            cur.GetComponent<EnemyColony>().Output();
        }
    }

    private void ActionEveryTwoHoursEnemy ()
    {
        GameObject[] gos = FindTags (new Tag[2] { Tag.Enemy, Tag.Core });

        foreach (GameObject cur in gos)
        {
            cur.GetComponent<EnemyColony>().CheckCritical();
        }
    }

    private void ActionEveryThreeHoursEnemy () {

    }

    private void ActionEveryTenHoursEnemy () {

    }

    #endregion

    private void AddOutput ()
    {
        if (colonyStats.energy > 0 || colonyStats.energyOutput > 0)
        {
            colonyStats.GainOutput();
            StartCoroutine(ShowPopup());
            
            if (colonyStats.energy <= 10) {
                Notify(Traduce("03_notif_energy_low"), priority: 2);
            }
        }
        else
        {
            colonyStats.GainOutput(false);
            Notify(Traduce("03_notif_energy_critic"), priority: 3);
        }

        BuildListItem[] buildItems = FindObjectsOfType<BuildListItem> ();

        foreach (BuildListItem bi in buildItems) {
            bi.UpdateUI ();
        }
    }

    private void CheckRessourcesAmount () {
        if (colonyStats.food < -10) {
            Notify(Traduce("03_notif_food_critic"), priority: 3);
        } else if (colonyStats.food <= 5) {
            Notify(Traduce("03_notif_food_low"), priority: 2);
        }

        if (colonyStats.money < -200) {
            Notify(Traduce("03_notif_money_critic"), priority: 3);
        } else if (colonyStats.money <= 50) {
            Notify(Traduce("03_notif_money_low"), priority: 2);
        }

        if (colonyStats.regolith < -20) {
            Notify(Traduce("03_notif_regolith_critic"), priority: 3);
        } else if (colonyStats.regolith <= 5) {
            Notify(Traduce("03_notif_regolith_low"), priority: 2);
        }
        
        if (colonyStats.metal < -20) {
            Notify(Traduce("03_notif_metal_critic"), priority: 3);
        } else if (colonyStats.metal <= 5) {
            Notify(Traduce("03_notif_metal_low"), priority: 2);
        }

        if (colonyStats.polymer < -20) {
            Notify(Traduce("03_notif_polymer_critic"), priority: 3);
        } else if (colonyStats.polymer <= 5) {
            Notify(Traduce("03_notif_polymer_low"), priority: 2);
        }
    }

    private void CheckEvent () {
        foreach (PonctualEvent evt in ponctualEvents) {
            if (seconds == evt.time && day == evt.day) {
                eventSystem.InstantiateEvent (evt);
            }
        }
    }

    private void ApplyAllDamageOnBuilding () {

    }

    private IEnumerator ShowPopup () {
        outMoney.text = colonyStats.profit.SignedString();
        outRegolith.text = colonyStats.regolithOutput.SignedString();
        outMetal.text = colonyStats.metalOutput.SignedString();
        outPolymer.text = colonyStats.polymerOutput.SignedString();
        outFood.text = colonyStats.foodOutput.SignedString();

        popup.Play("Show");
        yield return new WaitForSeconds(2f);
        popup.Play("Hide");
    }

    private bool CheckResources () {
        int criticalRessources = 0;

        if (colonyStats.food <= 0 && colonyStats.foodOutput <= 0) {
            Notify(Traduce("03_notif_food_critic"), priority: 3);
            criticalRessources++;
        }

        if (colonyStats.money <= 0 && colonyStats.profit <= 0) {
            Notify(Traduce("03_notif_money_critic"), priority: 3);
            criticalRessources++;
        }

        if (colonyStats.regolith <= 0 && colonyStats.regolithOutput <= 0) {
            Notify(Traduce("03_notif_regolith_critic"), priority: 3);
            criticalRessources++;
        }
        
        if (colonyStats.metal <= 0 && colonyStats.metalOutput <= 0) {
            Notify(Traduce("03_notif_metal_critic"), priority: 3);
            criticalRessources++;
        }

        if (colonyStats.polymer <= 0 && colonyStats.polymerOutput <= 0) {
            Notify(Traduce("03_notif_polymer_critic"), priority: 3);
            criticalRessources++;
        }

        return criticalRessources >= 3;
    }

    public void UpdateTimeUntilNextWave () {
        if (dateNextWave == new Vector2 ()) {
            nextWaveText.text = Traduce ("Time until next wave:") + " ???";
        } else {
            int currentHours = seconds / 60;

            int greatHour = (int)dateNextWave.y;
            int greatDay = (int)dateNextWave.x;

            if (currentHours > greatHour)
            {
                greatDay--;
                greatHour = 336 - (currentHours - greatHour);
            }

            nextWaveText.text = Traduce ("Time until next wave:") + " " + (greatDay - day) + " " + Traduce ("sol(s)") + " " + (greatHour) + " " + Traduce ("hour(s)");
        }

    }

    #endregion

    #region Ressources methods

    public bool HaveEnoughResource (int colonist, int energy, int money, float regolith, float metal, float polymer, float food)
    {
        return colonyStats.HaveEnoughResource(colonist, energy, money, regolith, metal, polymer, food);
    }

    public List<int> HaveResources (int colonist, int energy, int money, float regolith, float metal, float polymer, float food)
    {
        return colonyStats.HaveResources(colonist, energy, money, regolith, metal, polymer, food);
    }

    public List<int> HaveResources(int buildID)
    {
        Building b = buildData[buildID];
        return colonyStats.HaveResources(b);
    }

    #region Add / remove output

    public void AddOutput(int energy, int money, float regolith, float metal, float polymer, float food, float research)
    {
        colonyStats.AddOutput(energy, money, regolith, metal, polymer, food, research);
    }

    public void RemoveOutput(int energy, int money, float regolith, float metal, float polymer, float food, float research)
    {
        colonyStats.RemoveOutput(energy, money, regolith, metal, polymer, food, research);
    }

    #endregion

    #region Add / remove resources, workers and storage

    public void AddResources(int energy, int money, float regolith, float metal, float polymer, float food)
    {
        colonyStats.AddResources(energy, money, regolith, metal, polymer, food);
    }

    public void RemoveResources(int energy, int money, float regolith, float metal, float polymer, float food)
    {
        colonyStats.RemoveResources(energy, money, regolith, metal, polymer, food);
    }

    public void AddSettlers(int workers, int colonist)
    {
        colonyStats.AddSettlers(workers, colonist);
    }

    public void RemoveSettlers(int workers, int colonist)
    {
        colonyStats.RemoveSettlers(workers, colonist);
    }

    public void ManageStorage (int energy, float regolith, float metal, float polymer, float food)
    {
        colonyStats.ManageStorage(energy, regolith, metal, polymer, food);
    }

    #endregion

    public void GiveRessources (string arg) {
        string[] args = arg.Split (' ');

        if (args[0] == "-m") {
            try {
                int amount = int.Parse (args[1]);
                colonyStats.money += amount;
            } catch {

            }
        }
    }

    public void GiveRessources(RewardType reward, int amount)
    {
        if(reward == RewardType.Money)
        {
            colonyStats.money += amount;
        }
        else if (reward == RewardType.Regolith)
        {
            colonyStats.regolith += amount;
        }
        else if (reward == RewardType.Bioplastic)
        {
            colonyStats.polymer += amount;
        }
        else if (reward == RewardType.Food)
        {
            colonyStats.food += amount;
        }
        else if (reward == RewardType.Energy)
        {
            colonyStats.energy += amount;
        }
    }

    public void AnticipateResources (int energy) {
        colonyStats.AnticipateResources(energy);
    }

    public void RemoveAnticipateResources (int energy) {
        colonyStats.RemoveAnticipateResources(energy);
    }

    #endregion

    #region Notification

    public void Notify (string notification, float duration = 4f, int priority = 0, string cmd = "") {
        notif.Notify(notification, duration, priority, cmd);
        AddToHistory (Traduce(notification) + ";");
    }

    public void AddToHistory (string notification) {
        if (notificationHistory != "") {
            try {
                string[] notifs = notificationHistory.Split (';');

                if (notifs.Length > 10) {
                    notificationHistory = "";

                    for (int i = 1; i < notifs.Length - 1; i++) {
                        if (notifs[i] != "")
                            notificationHistory += notifs[i] + ";;";
                    }
                }
            } catch {

            }
        }

        notificationHistory += notification;
        notificationText.text = notificationHistory.Replace (';', '\n');
    }

    public void Btn_DisplayHistory () {
        notifHistory.SetActive (!notifHistory.activeSelf);
    }

    #endregion

    #region Stats methods

    public void IndividualStats (Entity obj, EntityType type) {
        viewer.ShowSingle (obj);
    }

    public void GroupStats (ObjectGroup group) {
        viewer.ShowGroup (group);
    }

    public void UpdateStats (Entity _current) {
        viewer.UpdateSingle (_current);
    }

    public Color GetColonyColor (int side) {
        ColonyStats[] cur = FindObjectsOfType<ColonyStats> ();

        foreach (ColonyStats mc in cur) {
            if (mc.colony.side == side) {
                return new Color (colonyColor[0], colonyColor[1], colonyColor[2], colonyColor[3]);
            }
        }

        return new Color (0.9f, 0, 0, 1);
    }

    public void ForcedHideStats () {
        viewer.HideInformation (forced: true);
    }

    public void HideStats (GameObject go) {
        viewer.HideInformation (cur: go);
    }

    public void UpdateFactoryQueue (Entity cur) {
        viewer.UpdateSingle (cur);
    }

    public void SendAttackOrder (Transform target) {
        viewer.SendAttackOrder (target);
    }

    public void UnitOrder (Vector3 target) {
        viewer.SetPlayerOrder (target);
    }

    public List<Entity> GetUnitsInArea (Vector3 minPosition, Vector3 maxPosition) {
        Unit[] allUnits = FindObjectsOfType<Unit>();
        List<Entity> playerUnitsInArea = new List<Entity> ();

        foreach (Unit unit in allUnits) {
            if (unit.side == side) {
                Vector3 simplifiedPosition = new Vector3 (unit.transform.position.x, 0, unit.transform.position.z);

                if (simplifiedPosition.x >= minPosition.x && simplifiedPosition.z >= minPosition.z && simplifiedPosition.x <= maxPosition.x && simplifiedPosition.z <= maxPosition.z) {
                    playerUnitsInArea.Add (unit);
                }
            }
        }

        ///Debug.Log("  [INFO:MoonManager] " + playerUnitsInArea.Count + " unit(s) found in this area. " + minPosition + " / " + maxPosition);
        return playerUnitsInArea;
    }

    #endregion

    #region Tag method

    public GameObject[] FindTag (Tag tag, List<GameObject> toStudy = null) {
        return tagSystem.FindTag (tag, toStudy);
    }

    public List<GameObject> FindTagList (Tag tag, List<GameObject> toStudy = null) {
        return tagSystem.FindTagList (tag, toStudy);
    }

    public GameObject[] FindTags (Tag[] tags, List<GameObject> toStudy = null) {
        return tagSystem.FindTags (tags, toStudy);
    }

    public List<GameObject> FindTagsList (Tag[] tags, List<GameObject> toStudy = null) {
        return tagSystem.FindTagsList (tags, toStudy);
    }

    /*public List<TagIdentifier> Find (Tag tag, List<GameObject> toStudy = null) {
        return tagSystem.Find (tag, toStudy);
    }*/

    public void DeleteGameObjectOfTagList (GameObject current) {
        tagSystem.allWithComponent.Remove(current);
    }

    #endregion

    #region Save and load methods

    #region Save section

    public void Save () {
        Time.timeScale = 0;
        saveDone.SetActive (false);
        SaveData (saveName);
        saveDone.SetActive (true);
    }

    private void SaveData (string saveName) {
        SaveSystem.Save(
            saveName + ".json",
            new SavedScene (
                versionCode, data.iteration + 1, colonyStats, this,
                FindObjectsOfType<Buildings>(), FindObjectsOfType<Preview>(), FindObjectsOfType<Unit>(),
                questManager, data.configuration
           )
        );
    }

    #endregion

    #region Load section

    public void LoadDataAfterMapGeneration () {
        Time.timeScale = 0;
        LoadData ();
        tutorialManager.InitTutorial();
        Init ();
        Time.timeScale = 1;
    }

    public void LoadData () {
        SavedManager savedManager = data.manager;
        SavedPlayer savedPlayer = data.player;

        SavedStockMarket savedMarket = data.stockMarket;
        SavedDiplomacy savedDiplomacy = data.diplomacy;
        SavedResearch savedResearch = data.research;
        SavedWaveManager savedWaveManager = data.wave;
        SavedNotificationQueue savedNotif = data.notification;

        SavedBuilding[] savedBuildings = data.buildings;
        SavedPreview[] savedPreviews = data.previews;
        SavedUnit[] savedUnits = data.units;

        LoadPlayer(savedPlayer);
        LoadManager(savedManager);
        LoadStockMarket(savedMarket);
        LoadConfiguration();

        if (data.iteration != 0) {
            LoadWave(savedWaveManager);
            LoadQuest(data.killQuest, data.buildQuest);

            try { LoadDiplomacy(savedDiplomacy); } catch (Exception e) { Debug.Log ("<color=#CD7F00>[WARN:MoonManager] Can't load diplomacy!</color> " + e.Message); }
            try { LoadResearch(savedResearch); } catch (Exception e) { Debug.Log ("<color=#CD7F00>[WARN:MoonManager] Can't load technologies!</color> " + e.Message); }
            try { LoadNotification(savedNotif); } catch (Exception e) { Debug.Log ("<color=#CD7F00>[WARN:MoonManager] Can't load notifications!</color> " + e.Message); }
        }

        try { waveSystem.UpdateDateNextWave(); }
        catch (Exception e)
        {
            Debug.Log ("<color=#CD7F00>[WARN:MoonManager] Date of the next wave can't be calculated!</color> " + e.Message);
            Notify (string.Format(Traduce("03_notif_error"), e.ToString()), duration: 10f, priority: 3);
        }

        LoadEntities(savedUnits, savedBuildings, savedPreviews);
    }

    private void LoadNotification (SavedNotificationQueue savedNotif) {
        
    }

    private void LoadResearch (SavedResearch savedResearch) {
        researchSystem.techUnlock = savedResearch.techsUnlock;

        researchSystem.techQueue = savedResearch.queue;
        researchSystem.Dequeue ();
        researchSystem.progress = savedResearch.progress;

        researchSystem.UpdateQueueList ();
        researchSystem.IncreaseResearch (0);
    }

    private void LoadPlayer (SavedPlayer savedPlayer) {
        colonyStats.colony.side = 0;
        colonyStats.colony.name = savedPlayer.playerColony.colonyName;

        colonyStats.colony.colonyColor = savedPlayer.playerColony.colonyColor;
        this.colonyColor = new Color(savedPlayer.playerColony.colonyColor[0], savedPlayer.playerColony.colonyColor[1], savedPlayer.playerColony.colonyColor[2], 1);

        colonyStats.energy = savedPlayer.playerColony.energy;
        colonyStats.money = savedPlayer.playerColony.money;
        colonyStats.regolith = savedPlayer.playerColony.regolith;
        colonyStats.metal = savedPlayer.playerColony.metal;
        colonyStats.polymer = savedPlayer.playerColony.polymer;
        colonyStats.food = savedPlayer.playerColony.food;

        colonyStats.regolithSold = savedPlayer.playerColony.regolithSold;
        colonyStats.metalSold = savedPlayer.playerColony.metalSold;
        colonyStats.polymerSold = savedPlayer.playerColony.polymerSold;
        colonyStats.foodSold = savedPlayer.playerColony.foodSold;
        colonyStats.regolithBought = savedPlayer.playerColony.regolithBought;
        colonyStats.metalBought = savedPlayer.playerColony.metalBought;
        colonyStats.polymerBought = savedPlayer.playerColony.polymerBought;
        colonyStats.foodBought = savedPlayer.playerColony.foodBought;

        Transform player = GameObject.Find ("Player").transform;
        player.transform.position = new Vector3 (savedPlayer.position[0], savedPlayer.position[1], savedPlayer.position[2]);
    }

    private void LoadManager (SavedManager savedManager) {
        this.day = savedManager.day;
        this.seconds = savedManager.time;
        this.side = savedManager.side;
        this.notificationHistory = savedManager.history;
        this.resourceWarning = savedManager.resourceWarning;
    }

    private void LoadStockMarket(SavedStockMarket savedMarket)
    {
        tradingSystem.Initialize(new StockMarket(savedMarket.regolithValue, savedMarket.metalValue, savedMarket.polymerValue, savedMarket.foodValue));
    }

    private void LoadConfiguration () {
        waveSystem.probablity = data.configuration.waveProbability;
    }

    private void LoadWave (SavedWaveManager savedWaveManager) {
        waveSystem.currentWaveIndex = savedWaveManager.currentWaveIndex;
    }

    private void LoadDiplomacy (SavedDiplomacy savedDiplomacy) {
        diplomacySystem.Initialize (true);
        diplomacySystem.m_diplomacyStates = savedDiplomacy.status;
    }

    private void LoadEntities (SavedUnit[] savedUnits, SavedBuilding[] savedBuildings, SavedPreview[] savedPreviews) {
        LoadBuildings (savedBuildings);
        LoadPreviews (savedPreviews);
        LoadUnits (savedUnits);
    }

    private void LoadUnits (SavedUnit[] savedUnits) {
        foreach (SavedUnit u in savedUnits) {
            Units unit = unitData[u.id];

            if (unit.model != null) {
                GameObject p = Instantiate (unit.model, new Vector3 (u.position[0], u.position[1], u.position[2]), new Quaternion (u.rotation[0], u.rotation[1], u.rotation[2], u.rotation[3])) as GameObject;
                Unit mtr = p.GetComponent<Unit>();

                mtr.health = u.health;
                mtr.shield = u.shield;
                mtr.energy = u.energy;

                mtr.agressivity = u.agressivity;
                mtr.formation = u.formation;

                mtr.orders = u.orders;

                mtr.level = u.level;
                mtr.experience = u.experience;

                mtr.id = u.id;
                mtr.side = u.side;
                mtr.groupID = u.groupId;

                mtr.Initialize(onSave: true);
            }
        }
    }

    private void LoadBuildings (SavedBuilding[] savedBuildings) {
        foreach (SavedBuilding b in savedBuildings) {
            Building bld = buildData[b.id];

            if (bld.building != null)
            {
                GameObject p;
                
                if(b.id == 0 && b.side != side) p = Instantiate (enemyColonyCenter, new Vector3 (b.position[0], b.position[1], b.position[2]), new Quaternion (b.rotation[0], b.rotation[1], b.rotation[2], b.rotation[3])) as GameObject;
                else p = Instantiate (bld.building, new Vector3 (b.position[0], b.position[1], b.position[2]), new Quaternion (b.rotation[0], b.rotation[1], b.rotation[2], b.rotation[3])) as GameObject;
                
                if (b.side != 0 && b.id == 0)
                {
                    EnemyColony ec = p.GetComponent<EnemyColony>();
                    ec.Initialize(Random.Range(0, 2));
                    ec.colony = new MoonColony($"E{b.side}", b.side);
                }
                
                Buildings mtr = p.GetComponent<Buildings> ();
                mtr.id = b.id;

                mtr.health = b.health;
                mtr.shield = b.shield;
                mtr.energy = b.energy;

                mtr.isEnable = b.isEnable;

                mtr.side = b.side;
                mtr.groupID = b.groupId;

                mtr.Initialize(onSave: true);

                if (!b.isEnable) {
                    mtr.Pause();
                }

                if (b.isFactory) {
                    TrainingArea t = p.GetComponent<TrainingArea> ();
                    t.Initialize();

                    if(t != null)
                    {
                        SavedTrainingArea sta = b.factory;
                        t.queue = sta.queue;
                        t.currentTrainingTime = sta.time;
                        t.rallyPoint = new Vector3(sta.rallyPoint[0], sta.rallyPoint[1], sta.rallyPoint[2]);

                        t.InitializeTraining();
                    }
                }
            }
        }
    }

    private void LoadPreviews (SavedPreview[] savedPreviews) {
        foreach (SavedPreview pr in savedPreviews) {
            Building bld = buildData[(pr.id)];

            if (bld.preview != null) {
                GameObject p = Instantiate (bld.preview, new Vector3 (pr.position[0], pr.position[1], pr.position[2]), new Quaternion(pr.rotation[0], pr.rotation[1], pr.rotation[2], pr.rotation[3])) as GameObject;
                Preview mtr = p.GetComponent<Preview> ();

                mtr.id = pr.id;

                mtr.health = pr.health;
                mtr.priority = pr.priority;

                mtr.side = pr.side;
                mtr.groupID = pr.groupId;
                mtr.isEngaged = true;

                mtr.Initialize();
                mtr.EngageUnits(UnitType.Worker);
                mtr.CheckSave();
            }
        }
    }

    private void LoadQuest(List<KillQuest> killQuest, List<BuildQuest> buildQuest)
    {
        questManager.Init(killQuest, buildQuest);
    }

    /*private void LoadEnemies (List<SaveEnemy> savedEnemies) {
        return; //TODO: Save and load of the enemies
    }*/

    #endregion

    #endregion

    #region Translate

    public string Traduce (string sentence) {
        return translate.GetTraduction (sentence);
    }

    #endregion

    #region Diplomacy

    public bool GetWarStatut (int pov, int mc) {
        return diplomacySystem.IsInWar (pov, mc);
    }

    public void GenerateDiplomacy () {
        diplomacySystem.RegenerateDiplimacy ();
    }

    #endregion

    #region Discord

    public void UpdatePresence (string title, string detail, string largeKey, string smallKey, string largeText, string smallText) {
        if (GameObject.Find ("Presence Manager") == null || true) return;
        PresenceManager.UpdatePresence (detail: title, state: detail, largeKey: largeKey, largeText: largeText, smallKey: smallKey, smallText: smallText);
    }

    #endregion

    #region Interface managment

    public void HideInterface () {
        interfaceManager.HideInterface ();
    }

    public void ResetInterface () {
        interfaceManager.ResetInterface ();
    }

    public void ResetRight () {
        interfaceManager.ResetRight ();
    }

    public void ResetLeft () {
        interfaceManager.ResetLeft ();
    }

    public void PauseGame (bool paused, bool noRule = false) {
        if (!noRule && !canPaused) return;

        if (paused) {
            Time.timeScale = 0;
            isPaused = true;
            gamePaused.SetActive (true);
        } else {
            Time.timeScale = 1;
            isPaused = false;
            gamePaused.SetActive (false);
        }
    }

    public void ChangeWarnText (string text, bool correct = false) {
        warnText.transform.parent.gameObject.SetActive (true);

        if (correct) {
            warnText.color = colorManager.finished;
        } else {
            warnText.color = colorManager.veryImportantColor;
        }

        warnText.text = Traduce (text);
    }

    public void HideWarnText () {
        try { warnText.transform.parent.gameObject.SetActive (false); } catch { }
    }

    #endregion

    #region Debug methods

    public void DebugInfo () {
        debugInfo.transform.parent.gameObject.SetActive (!debugInfo.transform.parent.gameObject.activeSelf);
    }

    #endregion

    #region Wave methods

    public void SpawnWave (int index, bool forced=false) {
        waveSystem.NewWave (index, forced);
    }

    #endregion

    #region Command

    public void SendCommand (string cmd) {
        if (cmd == "") return;

        if(tutorialManager != null)
        {
            if (cmd == "/tutoend") FindObjectOfType<TutorialManager>().EndTutorial();
            if (cmd == "/tutonextquest") FindObjectOfType<TutorialManager>().AddQuests();
        }
        
        commandSystem.CheckCommand (cmd, showLog:false);
    }

    #endregion

    #region Sectors

    public bool OwnSector(Vector3 position, int side)
    {
        Vector2 pos = new Vector2((int)(position.x / mapGenerator.chunkSize()), (int)(position.z / mapGenerator.chunkSize()));
        Sector sector = sectorManager.GetSector(pos);

        if (sector == null) return false;

        return sector.m_side == side;
    }

    public void ControlSector(Vector3 position, int side)
    {
        Vector2 pos = new Vector2((int)(position.x / mapGenerator.chunkSize()), (int)(position.z / mapGenerator.chunkSize()));
        sectorManager.ControlSector(pos, side);
    }

    #endregion

    #region Endgame methods

    public void OnColonyDeath () {
        //CallSuperEvent(1);
    }

    #endregion

    #region Others

    public void ChangeOverUI (bool isOver) {
        isOverUI = isOver;
    }

    private void OnApplicationQuit () {
        Save ();
    }

    public void TeleportPlayer (Vector3 position) {
        colonyStats.transform.position = position;
    }

    public void AddEnemyColony () {
        enemyCount++;
    }

    public void RemoveEnemyColony () {
        enemyCount--;
    }

    public void PlayPreviewSound () {
        try { playerAudio.PlayOneShot(resources.previewSound); } catch { }
    }

    #endregion
}