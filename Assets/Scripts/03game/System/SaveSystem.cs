using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class SaveSystem
{
    public static bool Save(string filename = "FILE.json", SavedScene obj = null, string filepath = "/Saves")
    {
        if (filename == "" || obj == null) return false;

        if (!Directory.Exists(Application.persistentDataPath + filepath))
        {
            Directory.CreateDirectory(Application.persistentDataPath + filepath);
        }

        string path = Application.persistentDataPath + filepath + "/" + filename;
        string jsonObj = JsonUtility.ToJson(obj, true);

        File.WriteAllText(path, jsonObj, System.Text.Encoding.UTF8);
        return true;
    }

    public static SavedScene LoadSave(string filename = "FILE.json", string filepath = "/Saves")
    {
        if (!Directory.Exists(Application.persistentDataPath + filepath))
        {
            Directory.CreateDirectory(Application.persistentDataPath + filepath);
            Debug.Log("<color=#FFB100>[WARN:SaveSystem] The directory " + Application.persistentDataPath + filepath + " doesn't exist !</color>");
            return null;
        }

        string path = Application.persistentDataPath + filepath + "/" + filename;

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            SavedScene data = JsonUtility.FromJson<SavedScene>(json);

            return data;
        }
        else
        {
            Debug.Log("<color=#FFB100>[WARN:SaveSystem] The file at: " + path + ", doesn't exist !</color>");
            return null;
        }
    }

    public static Object LoadSettings(string filename = "FILE.json", string filepath = "/Saves")
    {
        if (!Directory.Exists(Application.persistentDataPath + filepath))
        {
            Directory.CreateDirectory(Application.persistentDataPath + filepath);
            Debug.Log("<color=#FFB100>[WARN:SaveSystem] The directory " + Application.persistentDataPath + filepath + " doesn't exist !</color>");
            return null;
        }

        string path = Application.persistentDataPath + filepath + "/" + filename;

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            Object data = null;

            return data;
        }
        else
        {
            Debug.Log("<color=#FFB100>[WARN:SaveSystem] The file at: " + path + ", doesn't exist !</color>");
            return null;
        }
    }

    public static bool Delete(string filename = "FILE.json", string filepath = "/Saves")
    {
        if (!Directory.Exists(Application.persistentDataPath + filepath)) {
            Debug.Log("<color=#FFB100>[WARN:SaveSystem] The directory " + Application.persistentDataPath + filepath + " doesn't exist !</color>");
            return false;
        }

        string path = Application.persistentDataPath + filepath + "/" + filename;

        if (File.Exists(path)){
            File.Delete(path);
            return true;
        }
        else {
            Debug.Log("<color=#FFB100>[WARN:SaveSystem] The file at: " + path + " doesn't exist !</color>");
            return false;
        }
    }

    public static string[] GetSaved(string filepath = "/Saves")
    {
        string directoryPath = Application.persistentDataPath + filepath;

        if (!Directory.Exists(directoryPath))
        {
            Debug.Log("<color=#FFB100>[WARN:SaveSystem] The directory " + directoryPath + " doesn't exist !</color>");
            return null;
        }

        List<string> files = new List<string>();

        foreach(string f in Directory.GetFiles(directoryPath))
        {
            string[] folders = f.Replace("\\", "/").Split('/');

            files.Add(folders[folders.Length - 1].Replace(".json", ""));
        }

        return files.ToArray();
    }
}

[System.Serializable]
public class SavedScene
{
    public string versionCode;
    public int iteration = 0;

    public SavedManager manager;
    public SavedPlayer player;

    public SavedStockMarket stockMarket;
    public SavedDiplomacy diplomacy;
    public SavedNotificationQueue notification;
    public SavedResearch research;
    public SavedWaveManager wave;
    public SavedConfiguration configuration;

    public SavedBuilding[] buildings;
    public SavedPreview[] previews;
    public SavedUnit[] units;

    public List<BuildQuest> buildQuest;
    public List<KillQuest> killQuest;

    //Used for new save
    public SavedScene(string versionCode, SavedManager manager, SavedPlayer player, SavedConfiguration configuration, SavedBuilding[] buildings, SavedUnit[] units)
    {
        this.versionCode = versionCode;

        this.manager = manager;
        this.player = player;
        this.configuration = configuration;

        this.buildings = buildings;
        this.units = units;
    }

    //Use for save an already created game
    public SavedScene(string versionCode, int iteration, ColonyStats playerColony, MoonManager manager, Buildings[] buildings, Preview[] previews, Unit[] units, QuestManager questManager, SavedConfiguration config)
    {
        this.versionCode = versionCode;
        this.iteration = iteration;

        this.manager = new SavedManager(manager);
        this.player = new SavedPlayer(playerColony);

        this.stockMarket = new SavedStockMarket(manager.GetComponent<TradingSystem>().market);
        this.diplomacy = new SavedDiplomacy(manager.GetComponent<DiplomacySystem>());
        try { this.notification = new SavedNotificationQueue(manager.GetComponent<NotificationSystem>()); } catch { }
        this.research = new SavedResearch(manager.GetComponent<ResearchSystem>());
        this.wave = new SavedWaveManager(manager.GetComponent<WaveSystem>());
        this.configuration = config;

        if (questManager.buildingQuests != null) buildQuest = questManager.buildingQuests;
        if (questManager.killingQuests != null) killQuest = questManager.killingQuests;

        if (buildings.Length != 0)
        {
            List<Buildings> temp = new List<Buildings>();

            foreach(Buildings b in buildings)
            {
                if (b.GetComponent<TagIdentifier>()._tags.Contains(Tag.Saved))
                    temp.Add(b);
            }

            this.buildings = new SavedBuilding[temp.Count];

            for(int i = 0; i < temp.Count; i++)
            {
                this.buildings[i] = new SavedBuilding(temp[i]);
            }
        }

        if (previews.Length != 0)
        {
            List<Preview> temp = new List<Preview>();

            foreach (Preview p in previews)
            {
                if (p.GetComponent<TagIdentifier>()._tags.Contains(Tag.Saved))
                    temp.Add(p);
            }

            this.previews = new SavedPreview[temp.Count];

            for (int i = 0; i < temp.Count; i++)
            {
                this.previews[i] = new SavedPreview(temp[i]);
            }
        }

        if (units.Length != 0)
        {
            List<Unit> temp = new List<Unit>();

            foreach (Unit u in units)
            {
                if (u.GetComponent<TagIdentifier>()._tags.Contains(Tag.Saved))
                    temp.Add(u);
            }

            this.units = new SavedUnit[temp.Count];

            for (int i = 0; i < temp.Count; i++)
            {
                this.units[i] = new SavedUnit(temp[i]);
            }
        }
    }
}

#region Manager / System

[System.Serializable]
public class SavedManager
{
    public int day = 0;
    public int time = 0;

    public string history;
    public int resourceWarning;

    public int side;

    public SavedManager(int side)
    {
        this.side = side;
    }

    public SavedManager(MoonManager manager)
    {
        day = manager.day;
        time = manager.seconds;
        side = manager.side;

        resourceWarning = manager.resourceWarning;
        history = manager.notificationHistory;
    }
}

[System.Serializable]
public class SavedDiplomacy
{
    public DiplomacyState[] status;

    public SavedDiplomacy(DiplomacySystem motor)
    {
        status = motor.m_diplomacyStates;
    }
}

#region Notification

[System.Serializable]
public class SavedNotificationQueue
{
    public List<SavedNotification> queue;

    public SavedNotificationQueue(NotificationSystem motor)
    {
        queue = new List<SavedNotification>();
    }
}

[System.Serializable]
public class SavedNotification
{
    public string title;
    public string description;
    public float duration;
    public float[] color;
    public string command;

    public SavedNotification(Notification notif)
    {
        title = notif.title;
        description = notif.description;
        duration = notif.duration;
        color = new float[4] { notif.color.r, notif.color.g, notif.color.b, notif.color.a };
        command = notif.command;
    }
}

#endregion

[System.Serializable]
public class SavedResearch
{
    public float progress;
    public int indentity;
    public List<int> techsUnlock;
    public List<int> queue;

    public SavedResearch(ResearchSystem motor)
    {
        progress = motor.progress;
        indentity = motor.currentTech;
        techsUnlock = motor.techUnlock;
        queue = motor.techQueue;
    }
}

[System.Serializable]
public class SavedWaveManager
{
    public int currentWaveIndex;

    public SavedWaveManager(WaveSystem motor)
    {
        currentWaveIndex = motor.currentWaveIndex;
    }
}

[System.Serializable]
public class SavedStockMarket
{
    public float regolithValue;
    public float bioplasticValue;
    public float foodValue;

    public SavedStockMarket()
    {
        regolithValue = 10;
        bioplasticValue = 15;
        foodValue = 25;
    }

    public SavedStockMarket(StockMarket sm)
    {
        regolithValue = sm.regolithValue;
        bioplasticValue = sm.bioplasticValue;
        foodValue = sm.foodValue;
    }
}

[System.Serializable]
public class SavedConfiguration
{
    public int seed;
    public int waveProbability;
    public int destroyCost;
    public int enemyNumber;

    public SavedConfiguration(GameSettings settings)
    {
        seed = settings.seed;
        waveProbability = settings.gameDifficulty.enemyWaveRate;
        enemyNumber = settings.gameDifficulty.enemiesCount;
        destroyCost = settings.gameDifficulty.destroyedCost;
    }
}

#endregion

#region Player

[System.Serializable]
public class SavedPlayer
{
    public float[] position;
    public float[] rotation;

    public SavedColony playerColony;

    public SavedPlayer(int side, float[] colonyColor, string colonyName, GameSettings settings)
    {
        playerColony = new SavedColony(side, colonyColor, colonyName, settings);

        position = new float[3] { 0, 25, -15 };
        rotation = new float[3];
    }

    public SavedPlayer(ColonyStats playerColony)
    {
        position = new float[3];
        position[0] = playerColony.transform.localPosition.x;
        position[1] = playerColony.transform.localPosition.y;
        position[2] = playerColony.transform.localPosition.z;

        rotation = new float[3];
        rotation[0] = playerColony.transform.rotation.x;
        rotation[1] = playerColony.transform.rotation.y;
        rotation[2] = playerColony.transform.rotation.z;

        this.playerColony = new SavedColony(playerColony);
    }
}

[System.Serializable]
public class SavedColony
{
    public float[] colonyColor;
    public string colonyName;
    public int side;

    public int colonist;

    public int money;
    public int energy;
    public float regolith;
    public float bioplastic;
    public float food;

    public int regolithSold;
    public int bioplasticSold;
    public int foodSold;

    public int regolithBought;
    public int bioplasticBought;
    public int foodBought;

    public SavedColony(int side, float[] colonyColor, string colonyName, GameSettings settings)
    {
        this.side = side;

        this.colonyName = colonyName;
        this.colonyColor = colonyColor;

        colonist = settings.gameDifficulty.colonist;
        money = settings.gameDifficulty.money;
        energy = settings.gameDifficulty.energy;
        regolith = settings.gameDifficulty.rigolyte;
        bioplastic = settings.gameDifficulty.bioPlastique;
        food = settings.gameDifficulty.food;
    }

    public SavedColony(ColonyStats playerColony)
    {
        colonyColor = new float[3];
        colonyColor[0] = playerColony.colony.colonyColor[0];
        colonyColor[1] = playerColony.colony.colonyColor[1];
        colonyColor[2] = playerColony.colony.colonyColor[2];

        colonyName = playerColony.colony.name;
        side = playerColony.colony.side;

        colonist = playerColony.colonist;
        money = playerColony.money;
        energy = playerColony.energy;
        regolith = playerColony.regolith;
        bioplastic = playerColony.bioPlastique;
        food = playerColony.food;

        regolithSold = playerColony.regolithSold;
        bioplasticSold = playerColony.bioplasticSold;
        foodSold = playerColony.foodSold;

        regolithBought = playerColony.regolithBought;
        bioplasticBought = playerColony.bioplasticBought;
        foodBought = playerColony.foodBought;
    }
}

#endregion

#region Entity

[System.Serializable]
public class SavedEntity
{
    public float health = 0;
    public float shield = 0;
    public float energy = 0;

    public float[] position;
    public float[] rotation;

    public int side = 0;
    public int groupId = -1;
    public int id = 1;
}

[System.Serializable]
public class SavedBuilding: SavedEntity
{
    public bool isEnable;

    public bool isFactory;
    public SavedFactory factory;

    public SavedBuilding(Building building, float[] position)
    {
        health = building.maxHealth;
        shield = building.maxShield;
        energy = building.maxEnergy;

        id = building.identity;

        this.position = position;
        rotation = new float[4];

        isEnable = true;
    }

    public SavedBuilding(Buildings motor)
    {
        shield = motor.shield;
        health = motor.health;

        isEnable = motor.isEnable;

        position = new float[3];
        position[0] = motor.transform.localPosition.x;
        position[1] = motor.transform.localPosition.y;
        position[2] = motor.transform.localPosition.z;

        rotation = new float[4];
        rotation[0] = motor.transform.rotation.x;
        rotation[1] = motor.transform.rotation.y;
        rotation[2] = motor.transform.rotation.z;
        rotation[3] = motor.transform.rotation.w;

        side = motor.side;
        groupId = motor.groupID;
        id = motor.id;

        if (motor.GetComponent<FactoryMotor>() != null)
        {
            isFactory = true;
            factory = new SavedFactory(motor.GetComponent<FactoryMotor>());
        }
    }
}

[System.Serializable]
public class SavedPreview: SavedEntity
{
    public int priority;

    public SavedPreview(Building building, float[] position)
    {
        id = building.identity;

        this.position = position;
        rotation = new float[4];
    }

    public SavedPreview(Preview motor)
    {
        shield = motor.shield;
        health = motor.health;
        priority = motor.priority;

        position = new float[3];
        position[0] = motor.transform.localPosition.x;
        position[1] = motor.transform.localPosition.y;
        position[2] = motor.transform.localPosition.z;

        rotation = new float[4];
        rotation[0] = motor.transform.rotation.x;
        rotation[1] = motor.transform.rotation.y;
        rotation[2] = motor.transform.rotation.z;
        rotation[3] = motor.transform.rotation.w;

        side = motor.side;
        groupId = motor.groupID;
        id = motor.id;
    }
}

[System.Serializable]
public class SavedUnit: SavedEntity
{
    public int level = 1;
    public float experience = 0;

    public int agressivity;
    public int formation;

    public List<Order> orders;

    public SavedUnit(Units unit, float[] position)
    {
        health = unit.health;
        shield = unit.shield;
        energy = unit.energy;

        id = unit.identity;
        agressivity = unit.baseAgressivity;

        this.position = position;
        rotation = new float[4];
    }

    public SavedUnit(Unit motor)
    {
        health = motor.health;
        shield = motor.shield;
        energy = motor.energy;

        experience = motor.experience;
        level = motor.level;

        position = new float[3];
        position[0] = motor.transform.localPosition.x;
        position[1] = motor.transform.localPosition.y;
        position[2] = motor.transform.localPosition.z;

        rotation = new float[4];
        rotation[0] = motor.transform.rotation.x;
        rotation[1] = motor.transform.rotation.y;
        rotation[2] = motor.transform.rotation.z;
        rotation[3] = motor.transform.rotation.w;

        agressivity = motor.agressivity;
        formation = motor.formation;

        orders = motor.orders;

        side = motor.side;
        id = motor.id;
        groupId = motor.groupID;
    }
}

[System.Serializable]
public class SavedFactory
{
    public List<int> queue = new List<int>();
    public float time = 0;

    public SavedFactory(FactoryMotor motor)
    {
        foreach (Units u in motor.queue)
        {
            queue.Add(u.identity);
        }

        time = motor.time;
    }
}

#endregion