using System;
using System.Collections;
using System.Collections.Generic;
using DiscordPresence;
using UnityEngine;
using UnityEngine.UI;

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
    private RessourceData ressources;
    private EndlessTerrain endlessTerrain;
    private GroupSystem groupSystem;
    private QuestManager questManager;
    private SectorManager sectorManager;
    private TutorialManager tutorialManager;
    private EndgameChecker endgameChecker;
    private TradingSystem tradingSystem;

    private Text dayText, hoursText;
    private Text colonyText, energyText, moneyText, rigolyteText, bioPlastiqueText, foodText;
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
    private Text outBioplastic;
    private Text outFood;

    public int resourceWarning;

    private void Awake() {
        AssignAll();
    }

    private void Start() {
        data = SaveSystem.LoadSave(saveName + ".json");
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
        //CheckShortcut();
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
        ressources = GetComponent<RessourceData> ();
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
        rigolyteText = GameObject.Find ("RegolithText").GetComponent<Text> ();
        bioPlastiqueText = GameObject.Find ("BioplasticText").GetComponent<Text> ();
        foodText = GameObject.Find ("FoodText").GetComponent<Text> ();

        //Popup
        popup = GameObject.Find("E_Output").GetComponent<Animator>();
        outMoney = GameObject.Find("T_OutMoney").GetComponent<Text>();
        outRegolith = GameObject.Find("T_OutRegolith").GetComponent<Text>();
        outBioplastic = GameObject.Find("T_OutBioplastic").GetComponent<Text>();
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
        colonyText.text = colonyStats.colonist + " / " + colonyStats.maxColonist;

        if(colonyStats.energyOutput >= 0)
            energyText.text = colonyStats.energy + " / " + colonyStats.energyStorage + "\n<size=12>+" + colonyStats.energyOutput + "</size>";
        else
            energyText.text = colonyStats.energy + " / " + colonyStats.energyStorage + "\n<size=12>" + colonyStats.energyOutput + "</size>";

        UpdateStatsColor();

        if (colonyStats.energy > 0 || colonyStats.energyOutput > 0)
        {
            energyText.color = colorManager.text;

            if (colonyStats.profit >= 0)
                moneyText.text = colonyStats.money + " M€\n<size=12>+" + colonyStats.profit + "</size>";
            else
                moneyText.text = colonyStats.money + " M€\n<size=12>-" + (-colonyStats.profit) + "</size>";

            if (colonyStats.regolithOutput >= 0)
                rigolyteText.text = colonyStats.regolith.ToString ("0.0") + " / " + colonyStats.regolithStock.ToString ("000") + "\n<size=12>+" + colonyStats.regolithOutput.ToString ("0.0") + "</size>";
            else
                rigolyteText.text = colonyStats.regolith.ToString ("0.0") + " / " + colonyStats.regolithStock.ToString ("000") + "\n<size=12>-" + (-colonyStats.regolithOutput).ToString ("0.0") + "</size>";

            if (colonyStats.bioPlastiqueOutput >= 0)
                bioPlastiqueText.text = colonyStats.bioPlastique.ToString ("0.0") + " / " + colonyStats.bioPlasticStock.ToString ("000") + "\n<size=12>+" + colonyStats.bioPlastiqueOutput.ToString ("0.0") + "</size>";
            else
                bioPlastiqueText.text = colonyStats.bioPlastique.ToString ("0.0") + " / " + colonyStats.bioPlasticStock.ToString ("000") + "\n<size=12>-" + (-colonyStats.bioPlastiqueOutput).ToString ("0.0") + "</size>";

            if (colonyStats.foodOutput >= 0)
                foodText.text = colonyStats.food.ToString ("0.0") + " / " + colonyStats.foodStock.ToString ("000") + "\n<size=12>+" + colonyStats.foodOutput.ToString ("0.0") + "</size>";
            else
                foodText.text = colonyStats.food.ToString ("0.0") + " / " + colonyStats.foodStock.ToString ("000") + "\n<size=12>-" + (-colonyStats.foodOutput).ToString ("0.0") + "</size>";
        }
        else
        {
            energyText.color = colorManager.textWarning;

            moneyText.text = colonyStats.money + "$\n<size=12>-" + colonyStats.moneyLoss + "</size>";
            rigolyteText.text = colonyStats.regolith.ToString ("0.0") + " / " + colonyStats.regolithStock.ToString ("000") + "\n<size=12>-" + (colonyStats.regolithLoss).ToString ("0.0") + "</size>";
            bioPlastiqueText.text = colonyStats.bioPlastique.ToString ("0.0") + " / " + colonyStats.bioPlasticStock.ToString ("000") + "\n<size=12>-" + (colonyStats.bioPlastiqueLoss).ToString ("0.0") + "</size>";
            foodText.text = colonyStats.food.ToString ("0.0") + " / " + colonyStats.foodStock.ToString ("000") + "\n<size=12>-" + (colonyStats.foodLoss).ToString ("0.0") + "</size>";
        }
    }

    private void UpdateStatsColor()
    {
        if (colonyStats.profit <= 0)
            moneyText.color = colorManager.textWarning;
        else
            moneyText.color = colorManager.text;

        if (colonyStats.regolithOutput < 0)
            rigolyteText.color = colorManager.textWarning;
        else
            rigolyteText.color = colorManager.text;

        if (colonyStats.bioPlastiqueOutput < 0)
            bioPlastiqueText.color = colorManager.textWarning;
        else
            bioPlastiqueText.color = colorManager.text;

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

    private IEnumerator HoursMotor () {
        while (true) {
            yield return timeScale;
            seconds++;

            CheckTimeAction ();
            CheckEvent ();

            hoursText.text = string.Format ("{0:000}:{1:00}", Mathf.Floor (seconds / 60), seconds % 60);

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

    private void ActionEveryOneHourEnemy () {
        GameObject[] gos = FindTags (new Tag[2] { Tag.Enemy, Tag.Core });

        foreach (GameObject cur in gos)
        {
            cur.GetComponent<EnemyColony>().Output();
        }
    }

    private void ActionEveryTwoHoursEnemy () {

    }

    private void ActionEveryThreeHoursEnemy () {

    }

    private void ActionEveryTenHoursEnemy () {

    }

    #endregion

    private void AddOutput () {
        colonyStats.energy += colonyStats.energyOutput;
        colonyStats.energy = Mathf.Clamp(colonyStats.energy, 0, colonyStats.energyStorage);

        if (colonyStats.energy > 0 || colonyStats.energyOutput > 0)
        {
            StartCoroutine(ShowPopup());

            colonyStats.money += colonyStats.profit;
            colonyStats.regolith += colonyStats.regolithOutput;
            colonyStats.bioPlastique += colonyStats.bioPlastiqueOutput;
            colonyStats.food += colonyStats.foodOutput;

            if (colonyStats.energy <= 10) {
                Notify(Traduce("03_notif_energy_low"), priority: 2);
            }
        }
        else
        {
            colonyStats.money -= colonyStats.moneyLoss;
            colonyStats.regolith -= colonyStats.regolithLoss;
            colonyStats.bioPlastique -= colonyStats.bioPlastiqueLoss;
            colonyStats.food -= colonyStats.foodLoss;

            Notify(Traduce("03_notif_energy_critic"), priority: 3);
        }

        colonyStats.regolith = Mathf.Clamp(colonyStats.regolith, 0, colonyStats.regolithStock);
        colonyStats.bioPlastique = Mathf.Clamp(colonyStats.bioPlastique, 0, colonyStats.bioPlasticStock);
        colonyStats.food = Mathf.Clamp(colonyStats.food, 0, colonyStats.foodStock);

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

        if (colonyStats.bioPlastique < -20) {
            Notify(Traduce("03_notif_bioplastic_critic"), priority: 3);
        } else if (colonyStats.bioPlastique <= 5) {
            Notify(Traduce("03_notif_bioplastic_low"), priority: 2);
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
        outMoney.text = (colonyStats.profit >= 0) ? "+" + colonyStats.profit.ToString("0") : colonyStats.profit.ToString("0");
        outRegolith.text = (colonyStats.regolithOutput >= 0) ? "+" + colonyStats.regolithOutput.ToString("0") : colonyStats.regolithOutput.ToString("0");
        outBioplastic.text = (colonyStats.bioPlastiqueOutput >= 0) ? "+" + colonyStats.bioPlastiqueOutput.ToString("0") : colonyStats.bioPlastiqueOutput.ToString("0");
        outFood.text = (colonyStats.foodOutput >= 0) ? "+" + colonyStats.foodOutput.ToString("0") : colonyStats.foodOutput.ToString("0");

        popup.Play("Show");
        yield return new WaitForSeconds(2f);
        popup.Play("Hide");
    }

    private bool CheckResources () {
        int criticalRessources = 0;

        if (colonyStats.food == 0 && colonyStats.foodOutput <= 0) {
            Notify(Traduce("03_notif_food_critic"), priority: 2);
            criticalRessources++;
        }

        if (colonyStats.money == 0 && colonyStats.profit <= 0) {
            Notify(Traduce("03_notif_money_critic"), priority: 2);
            criticalRessources++;
        }

        if (colonyStats.regolith == 0 && colonyStats.regolithOutput <= 0) {
            Notify(Traduce("03_notif_regolith_critic"), priority: 2);
            criticalRessources++;
        }

        if (colonyStats.bioPlastique == 0 && colonyStats.bioPlastiqueOutput <= 0) {
            Notify(Traduce("03_notif_bioplastic_critic"), priority: 2);
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

    public bool HaveEnoughResource (int _colonist, int _energy, int _money, float _regolith, float _bioPlastique, float _food) {
        if (colonyStats.maxColonist < colonyStats.colonist + _colonist) return false;
        if (colonyStats.energy + colonyStats.anticipatedEnergy < _energy) return false;
        if (colonyStats.money < _money) return false;
        if (colonyStats.regolith < _regolith) return false;
        if (colonyStats.bioPlastique < _bioPlastique) return false;
        if (colonyStats.food < _food) return false;

        return true;
    }

    public List<int> HaveRessources (int _colonist, int _energy, int _money, float _regolith, float _bioPlastique, float _food) {
        List<int> ints = new List<int> ();

        if (colonyStats.maxColonist < colonyStats.colonist + _colonist) ints.Add (0);
        if (colonyStats.energyOutput + colonyStats.anticipatedEnergy + _energy < 0) ints.Add (1);
        if (colonyStats.money < _money) ints.Add (2);
        if (colonyStats.regolith < _regolith) ints.Add (3);
        if (colonyStats.bioPlastique < _bioPlastique) ints.Add (4);
        if (colonyStats.food < _food) ints.Add (5);

        return ints;
    }

    public List<int> HaveRessources(int buildID)
    {
        Building b = buildData[buildID];

        int _colonist = b.colonist;
        int _energy = b.energy;
        int _money = b.money;
        float _regolith = b.regolith;
        float _bioPlastique = b.bioPlastique;
        float _food = b.food;

        return HaveRessources(_colonist, _energy, _money, _regolith, _bioPlastique, _food);
    }

    #region Add / remove output

    public void AddOutput(int _energy, int _money, float _regolith, float _bioplastic, float _food, float _research)
    {
        Debug.Log("[INFO:MoonManager] Adding output...");

        if (_energy > 0) colonyStats.energyGain += Mathf.Abs(_energy);
        else colonyStats.energyLoss += Mathf.Abs(_energy);

        if (_money > 0) colonyStats.moneyGain += Mathf.Abs(_money);
        else colonyStats.moneyLoss += Mathf.Abs(_money);

        if (_regolith > 0) colonyStats.regolithGain += Mathf.Abs(_regolith);
        else colonyStats.regolithLoss += Mathf.Abs(_regolith);

        if (_bioplastic > 0) colonyStats.bioPlastiqueGain += Mathf.Abs(_bioplastic);
        else colonyStats.bioPlastiqueLoss += Mathf.Abs(_bioplastic);

        if (_food > 0) colonyStats.foodGain += Mathf.Abs(_food);
        else colonyStats.foodLoss += Mathf.Abs(_food);

        colonyStats.research += _research;

        colonyStats.CalculateOutput();
    }

    public void RemoveOutput(int _energy, int _money, float _regolith, float _bioplastic, float _food, float _research)
    {
        Debug.Log("[INFO:MoonManager] Removing output...");

        if (_energy > 0) colonyStats.energyGain -= Mathf.Abs(_energy);
        else colonyStats.energyLoss -= Mathf.Abs(_energy);

        if (_money > 0) colonyStats.moneyGain -= Mathf.Abs(_money);
        else colonyStats.moneyLoss -= Mathf.Abs(_money);

        if (_regolith > 0) colonyStats.regolithGain -= Mathf.Abs(_regolith);
        else colonyStats.regolithLoss -= Mathf.Abs(_regolith);

        if (_bioplastic > 0) colonyStats.bioPlastiqueGain -= Mathf.Abs(_bioplastic);
        else colonyStats.bioPlastiqueLoss -= Mathf.Abs(_bioplastic);

        if (_food > 0) colonyStats.foodGain -= Mathf.Abs(_food);
        else colonyStats.foodLoss -= Mathf.Abs(_food);

        colonyStats.research -= _research;

        colonyStats.CalculateOutput();
    }

    #endregion

    #region Add / remove ressources, workers and storage

    public void AddRessources(int _energy, int _money, float _regolith, float _bioplastic, float _food)
    {
        colonyStats.energy += _energy;
        colonyStats.money += _money;
        colonyStats.regolith += _regolith;
        colonyStats.bioPlastique += _bioplastic;
        colonyStats.food += _food;
    }

    public void RemoveRessources(int _energy, int _money, float _regolith, float _bioplastic, float _food)
    {
        colonyStats.energy -= _energy;
        colonyStats.money -= _money;
        colonyStats.regolith -= _regolith;
        colonyStats.bioPlastique -= _bioplastic;
        colonyStats.food -= _food;
    }

    public void AddSettlers(int _workers, int _colonist)
    {
        colonyStats.colonist += _workers;
        colonyStats.maxColonist += _colonist;
    }

    public void RemoveSettlers(int _workers, int _colonist)
    {
        colonyStats.colonist -= _workers;
        colonyStats.maxColonist -= _colonist;
    }

    public void ManageStorage (int _energy, float _regolith, float _bioplastic, float _food) {
        colonyStats.energyStorage += _energy;
        colonyStats.regolithStock += _regolith;
        colonyStats.bioPlasticStock += _bioplastic;
        colonyStats.foodStock += _food;
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
            colonyStats.bioPlastique += amount;
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

    public void AnticipateRessources (int energy) {
        colonyStats.anticipatedEnergy += energy;
    }

    public void RemoveAnticipateRessources (int energy) {
        colonyStats.anticipatedEnergy -= energy;
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
                string[] __notifs = notificationHistory.Split (';');

                if (__notifs.Length > 10) {
                    notificationHistory = "";

                    for (int i = 1; i < __notifs.Length - 1; i++) {
                        if (__notifs[i] != "")
                            notificationHistory += __notifs[i] + ";;";
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

    public void SaveData (string _saveName) {
        SaveSystem.Save(
            _saveName + ".json",
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
        LoadConfiguration();

        if (data.iteration != 0) {
            LoadStockMarket(savedMarket);
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
        colonyStats.bioPlastique = savedPlayer.playerColony.bioplastic;
        colonyStats.food = savedPlayer.playerColony.food;

        colonyStats.regolithSold = savedPlayer.playerColony.regolithSold;
        colonyStats.bioplasticSold = savedPlayer.playerColony.bioplasticSold;
        colonyStats.foodSold = savedPlayer.playerColony.foodSold;
        colonyStats.regolithBought = savedPlayer.playerColony.regolithBought;
        colonyStats.bioplasticBought = savedPlayer.playerColony.bioplasticBought;
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
        tradingSystem.market = new StockMarket(savedMarket.regolithValue, savedMarket.bioplasticValue, savedMarket.foodValue);
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

            if (bld.building != null) {
                GameObject p = Instantiate (bld.building, new Vector3 (b.position[0], b.position[1], b.position[2]), new Quaternion (b.rotation[0], b.rotation[1], b.rotation[2], b.rotation[3])) as GameObject;
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
                    FactoryMotor f = p.GetComponent<FactoryMotor> ();
                    f.Init();

                    if(f != null)
                    {
                        SavedFactory sf = b.factory;

                        if (sf.queue.Count != 0)
                        {
                            foreach (int u in sf.queue)
                            {
                                foreach (int i in f.unitsAvailable)
                                {
                                    if (i == u)
                                    {
                                        f.AddQueue(i, true);
                                    }
                                }
                            }
                        }

                        f.time = sf.time;
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

                mtr.isEngaged = true;
                mtr.id = pr.id;

                mtr.health = pr.health;
                mtr.priority = pr.priority;

                mtr.side = pr.side;
                mtr.groupID = pr.groupId;
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

    #region Event methods

    public void InstantiateEvent (int id) {
        eventSystem.InstantiateEvent (id);
    }

    public void InstantiateEvent (string name) {
        eventSystem.InstantiateEvent (name);
    }

    public void InstantiateEvent (Event evt) {
        eventSystem.InstantiateEvent (evt);
    }

    public void RandomEvent () {
        eventSystem.RandomizeEvent ();
    }

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

    #region Map height

    public float GetPointHeight (Vector2 pos) {
        int chunkSize = mapGenerator.chunkSize ();
        Vector2 chunkPos = pos / chunkSize;

        chunkPos.x = (int) chunkPos.x;
        chunkPos.y = (int) chunkPos.y;

        List<HeightPoint> heightData = endlessTerrain.terrainChunkDictonary[chunkPos].lodMeshes[0].meshData.heightData;

        Vector2 realPosition = new Vector2 ((pos.x % 95) / -2f, (pos.y % 95) / -2f);
        realPosition.x = Mathf.Round (realPosition.x);
        realPosition.y = Mathf.Round (realPosition.y);

        foreach (HeightPoint h in heightData) {
            if (h.CheckPosition (realPosition)) {
                //Debug.Log ("  [INFO:MoonManager] " + realPosition + " / " + chunkPos + " : " + h.GetHeight ());
                return h.GetHeight ();
            }
        }

        throw new Exception ("[ERROR] Impossible to find the height of the point " + realPosition + " on the chunk " + chunkPos);
    }

    public float GetPointHeightByColliders(Vector3 position)
    {
        int chunkSize = mapGenerator.chunkSize();

        Vector3 chunkPosV3 = position / chunkSize;
        chunkPosV3.x = (int)chunkPosV3.x;
        chunkPosV3.z = (int)chunkPosV3.z;
        Vector2 chunkPosV2 = new Vector2(chunkPosV3.x, chunkPosV3.z);

        EndlessTerrain.TerrainChunk chunk = endlessTerrain.terrainChunkDictonary[chunkPosV2];

        Vector3 closestPointOnCollider = chunk.meshObject.GetComponent<MeshCollider>().ClosestPointOnBounds(position);

        //Debug.Log(closestPointOnCollider);
        return closestPointOnCollider.y;
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
        try { playerAudio.PlayOneShot(ressources.previewSound); } catch { }
    }

    #endregion
}