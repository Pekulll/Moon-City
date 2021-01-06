using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class N_SaveSystem {
    public static void SaveData (SceneData data, string fileName = "EMPTY_SAVE_FILE") {
        BinaryFormatter formatter = new BinaryFormatter ();

        if (!Directory.Exists (Application.persistentDataPath + "/Saves/")) {
            Directory.CreateDirectory (Application.persistentDataPath + "/Saves");
        }

        string path = Application.persistentDataPath + "/Saves/" + fileName + ".save";
        FileStream stream = new FileStream (path, FileMode.Create);

        formatter.Serialize (stream, data);
        stream.Close ();

        Debug.Log ("<b><color=#00BA0D>[INFO:SaveSystem] Game saved at: " + path + "</color></b>");
    }

    public static SceneData LoadData (string fileName = "EMPTY_SAVE_FILE") {
        if (!Directory.Exists (Application.persistentDataPath + "/Saves/")) {
            Directory.CreateDirectory (Application.persistentDataPath + "/Saves");
            return null;
        }

        string path = Application.persistentDataPath + "/Saves/" + fileName + ".save";

        if (File.Exists (path)) {
            BinaryFormatter formatter = new BinaryFormatter ();
            FileStream stream = new FileStream (path, FileMode.Open);

            SceneData data = formatter.Deserialize (stream) as SceneData;
            stream.Close ();

            Debug.Log ("[INFO:SceneData] File loaded ! (" + fileName + ")");
            return data;
        } else {
            Debug.Log ("<color=#FFB100>[WARN:SaveSystem] The file at: " + path + ", doesn't exist !</color>");
            return null;
        }
    }

    public static void DeleteData (string fileName = "EMPTY_SAVE_FILE") {
        if (!Directory.Exists (Application.persistentDataPath + "/Saves/")) {
            Directory.CreateDirectory (Application.persistentDataPath + "/Saves");
            return;
        }

        string path = Application.persistentDataPath + "/Saves/" + fileName + ".save";
        File.Delete (path);
        Debug.Log ("<b>[INFO:SaveSystem] Game deleted at: " + path + "</b>");
    }
}

#region Class pour la sauvegarde

[System.Serializable]
public class SceneData {
    public List<SavePreview> savedPreviews = new List<SavePreview> ();
    public List<SaveUnit> savedUnits = new List<SaveUnit> ();
    public List<SaveBuild> savedBuildings = new List<SaveBuild> ();
    //public List<SaveEnemy> savedEnemies = new List<SaveEnemy>();

    public SavePlayer savedPlayer;
    public SaveManager savedManager;
    public SaveDiplomacy savedDiplomacy;
    public SaveEvent savedEvent;
    public SaveNotificationQueue savedNotif;
    public SaveResearch savedResearch;
    public SaveWaveManager savedWaveManager;
    public SaveConfiguration savedConfiguration;

    public List<BuildQuest> savedBuildQuest;
    public List<KillQuest> savedKillQuest;

    public bool isNewSave;

    public SceneData (ColonyStats playerColony, MoonManager manager, NotificationSystem notif, Preview[] previews, Unit[] units, Buildings[] builds, QuestManager questManager, EnemyMotor[] enemies, SaveConfiguration config, bool newSave = false) {
        isNewSave = newSave;

        savedPlayer = new SavePlayer (playerColony, newSave);
        savedManager = new SaveManager (manager, newSave);
        savedConfiguration = config;

        if (!newSave) {
            savedDiplomacy = new SaveDiplomacy (manager.GetComponent<DiplomacySystem> ());
            savedEvent = new SaveEvent (manager.GetComponent<EventSystem> ());
            savedResearch = new SaveResearch (manager.GetComponent<ResearchSystem> ());
            savedWaveManager = new SaveWaveManager (manager.GetComponent<WaveSystem> ());

            if(questManager.buildingQuests != null) savedBuildQuest = questManager.buildingQuests;
            if(questManager.killingQuests != null) savedKillQuest = questManager.killingQuests;

            try {
                savedNotif = new SaveNotificationQueue(notif);                        //? ERREUR DE SERILIZATION normalement CORRIGÉE
            } catch {
                Debug.Log ("  [INFO:SceneData] Can't save notification !");
            }
        }

        foreach (Preview pr in previews) {
            if (pr.isEngaged) {
                if (pr.GetComponent<TagIdentifier>()._tags.Contains(Tag.Saved))
                    savedPreviews.Add (new SavePreview (pr));
            }
        }

        foreach (Unit e in units) {
            if (!isNewSave)
                if (e.GetComponent<TagIdentifier>()._tags.Contains(Tag.Saved))
                    savedUnits.Add (new SaveUnit (e, newSave));
            else
                savedUnits.Add(new SaveUnit(e, newSave));
        }

        foreach (Buildings t in builds) {
            if (!isNewSave) {
                if (t.GetComponent<TagIdentifier> ()._tags.Contains (Tag.Saved))
                    savedBuildings.Add (new SaveBuild (t, newSave));
            } else {
                savedBuildings.Add (new SaveBuild (t, newSave));
            }
        }

        /*foreach (EnemyMotor e in enemies)
        {
            savedEnemies.Add(new SaveEnemy(e));
        }*/
    }
}

[System.Serializable]
public class SavePlayer {
    public float[] m_position;

    public SaveColony m_playerColony;

    public SavePlayer (ColonyStats playerColony, bool newSave = false) {
        if (!newSave) {
            m_position = new float[3];
            m_position[0] = playerColony.transform.localPosition.x;
            m_position[1] = playerColony.transform.localPosition.y;
            m_position[2] = playerColony.transform.localPosition.z;
        } else {
            m_position = new float[3];
            m_position[0] = 0;
            m_position[1] = 25;
            m_position[2] = -15;
        }

        m_playerColony = new SaveColony (playerColony);
    }
}

[System.Serializable]
public class SaveManager {
    public int m_day;
    public int m_seconds;
    public int m_side;
    public float[] m_colonyColor;
    public string m_versionCode;
    public string m_history;

    public SaveManager (MoonManager manager, bool newSave = false) {
        m_day = manager.day;
        m_seconds = manager.seconds;
        m_side = manager.side;

        m_colonyColor = new float[3];
        m_colonyColor[0] = manager.colonyColor.r;
        m_colonyColor[1] = manager.colonyColor.g;
        m_colonyColor[2] = manager.colonyColor.b;

        m_versionCode = "Deleted";
        m_history = manager.notificationHistory;
    }
}

[System.Serializable]
public class SaveUnit {
    public float m_health;
    public float m_shield = 5f;
    public float m_battery = 0f;
    public int m_level = 1;
    public float m_experience = 0;

    public float[] m_position;
    public float[] m_rotations;

    public int m_formation;

    public List<Order> orders;

    public int m_agressivity = 0;
    public int m_side = 0;
    public int m_identity;
    public int m_groupId;

    public SaveUnit (Unit motor, bool newSave = false) {
        m_health = motor.health;
        m_shield = motor.shield;
        m_battery = motor.energy;
        m_experience = motor.experience;
        m_level = motor.level;

        if (!newSave) {
            m_position = new float[3];
            m_position[0] = motor.transform.localPosition.x;
            m_position[1] = motor.transform.localPosition.y;
            m_position[2] = motor.transform.localPosition.z;

            m_rotations = new float[4];
            m_rotations[0] = motor.transform.rotation.x;
            m_rotations[1] = motor.transform.rotation.y;
            m_rotations[2] = motor.transform.rotation.z;
            m_rotations[3] = motor.transform.rotation.w;

            m_groupId = motor.groupID;
        } else {
            m_position = new float[3];

            m_position[0] = -Random.Range (4, 7);
            m_position[1] = 0;
            m_position[2] = Random.Range (4, 7);

            m_rotations = new float[4];
        }

        m_formation = motor.formation;
        orders = motor.orders;

        m_agressivity = motor.agressivity;
        m_side = motor.side;
        m_identity = motor.id;
    }
}

[System.Serializable]
public class SaveBuild {
    public float[] m_position;
    public float[] m_rotations;

    public float m_health;
    public float m_shield;

    public int m_side = 0;
    public bool m_isPaused;

    public bool m_haveFactory;
    public SaveFactory m_factory;

    public int m_identity;
    public int m_groupId;

    public SaveBuild (Buildings motor, bool newSave = false) {
        if (!newSave) {
            m_position = new float[3];
            m_position[0] = motor.transform.localPosition.x;
            m_position[1] = motor.transform.localPosition.y;
            m_position[2] = motor.transform.localPosition.z;

            m_rotations = new float[4];
            m_rotations[0] = motor.transform.rotation.x;
            m_rotations[1] = motor.transform.rotation.y;
            m_rotations[2] = motor.transform.rotation.z;
            m_rotations[3] = motor.transform.rotation.w;

            m_groupId = motor.groupID;
        } else {
            m_position = new float[3];
            m_rotations = new float[4];
        }

        m_health = motor.health;
        m_shield = motor.shield;

        m_side = motor.side;
        m_isPaused = motor.isEnable;

        if (!newSave && motor.GetComponent<FactoryMotor> () != null) {
            m_haveFactory = true;
            m_factory = new SaveFactory (motor.GetComponent<FactoryMotor> ());
        }

        m_identity = motor.id;
    }
}

[System.Serializable]
public class SaveFactory {
    public List<int> m_queue = new List<int> ();
    public float m_time = 0;

    public SaveFactory (FactoryMotor motor) {
        foreach (Units u in motor.queue) {
            m_queue.Add (u.identity);
        }

        m_time = motor.time;
    }
}

[System.Serializable]
public class SavePreview {
    public int m_side = 0;
    public float[] m_positions;
    public float[] m_rotations;

    public float m_progress = 0;
    public int m_priority = 0;

    public int m_identity;
    public int m_groupId;

    public SavePreview (Preview motor) {
        m_positions = new float[3];
        m_positions[0] = motor.transform.localPosition.x;
        m_positions[1] = motor.transform.localPosition.y;
        m_positions[2] = motor.transform.localPosition.z;

        m_rotations = new float[4];
        m_rotations[0] = motor.transform.rotation.x;
        m_rotations[1] = motor.transform.rotation.y;
        m_rotations[2] = motor.transform.rotation.z;
        m_rotations[3] = motor.transform.rotation.w;

        m_priority = motor.priority;
        m_progress = motor.health;

        m_side = motor.side;
        m_identity = motor.id;

        m_groupId = motor.groupID;
    }
}

[System.Serializable]
public class SaveDiplomacy {
    public DiplomacyState[] m_statuts;

    public SaveDiplomacy (DiplomacySystem motor) {
        m_statuts = motor.m_diplomacyStates;
    }
}

[System.Serializable]
public class SaveEvent {
    public int m_currentDialogIndex;
    public int m_current;

    public SaveEvent (EventSystem motor) {
        if (motor.current != null && motor.current.name != "") {
            m_currentDialogIndex = motor.currentDialogIndex;
            m_current = motor.current.identity;
        }
    }
}

[System.Serializable]
public class SaveNotification {
    public string m_title;
    public string m_description;
    public float m_duration;
    public string m_iconName;
    public float[] m_color;
    public string m_command;

    public SaveNotification (Notification notif) {
        m_title = notif.title;
        m_description = notif.description;
        m_duration = notif.duration;
        m_iconName = notif.icon.name;
        m_color = new float[4] { notif.color.r, notif.color.g, notif.color.b, notif.color.a };
        m_command = notif.command;
    }
}

[System.Serializable]
public class SaveNotificationQueue{
    public List<SaveNotification> m_queue;

    public SaveNotificationQueue(NotificationSystem motor){
        m_queue = new List<SaveNotification>();

        foreach(Notification n in motor.queue){
            m_queue.Add(new SaveNotification(n));
        }
    }
}

[System.Serializable]
public class SaveEnemy {
    public float m_influenceRadius = 50f;
    public StrategyType m_strategy;

    public List<int> m_researchUnlock;
    public int m_currentTech = -1;
    public float m_techProgress;

    public string m_enemyName;

    public float m_scaleCritical = 1f;

    public SaveColony m_colony;

    public SaveEnemy (EnemyMotor motor) {
        m_influenceRadius = motor.influenceRadius;
        m_strategy = motor.strategy;
        m_researchUnlock = motor.researchUnlock;
        m_currentTech = motor.currentTech;
        m_techProgress = motor.techProgress;
        m_enemyName = motor.enemyName;
        m_scaleCritical = motor.scaleCritical;

        m_colony = new SaveColony (motor.GetComponent<ColonyStats> ());
    }
}

[System.Serializable]
public class SaveColony {
    public float[] m_colonyColor;
    public string colonyName;
    public int side;

    public int m_colonist;

    public int m_money;
    public int m_energy;
    public float m_regolith;
    public float m_bioPlastique;
    public float m_food;

    public SaveColony (ColonyStats playerColony) {
        m_colonyColor = new float[4];
        m_colonyColor[0] = playerColony.colony.colonyColor[0];
        m_colonyColor[1] = playerColony.colony.colonyColor[1];
        m_colonyColor[2] = playerColony.colony.colonyColor[2];
        m_colonyColor[3] = playerColony.colony.colonyColor[3];

        colonyName = playerColony.colony.name;
        side = playerColony.colony.side;

        m_colonist = playerColony.colonist;
        m_money = playerColony.money;
        m_energy = playerColony.energy;
        m_regolith = playerColony.regolith;
        m_bioPlastique = playerColony.bioPlastique;
        m_food = playerColony.food;
    }
}

[System.Serializable]
public class SaveResearch {
    public float m_progress;
    public int m_indentity;
    public List<int> m_techsUnlock;
    public List<int> m_queue;

    public SaveResearch (ResearchSystem motor) {
        m_progress = motor.progress;
        m_indentity = motor.currentTech;
        m_techsUnlock = motor.techUnlock;
        m_queue = motor.techQueue;
    }
}

[System.Serializable]
public class SaveWaveManager {
    public int m_currentWaveIndex;

    public SaveWaveManager (WaveSystem motor) {
        m_currentWaveIndex = motor.currentWaveIndex;
    }
}

[System.Serializable]
public class SaveConfiguration {
    public int m_seed;
    public int m_waveProbability;
    public int m_destroyCost;
    public int m_enemyNumber;

    public SaveConfiguration (int seed, int waveProbability, int destroyCost, int enemyNumber) {
        m_seed = seed;
        m_waveProbability = waveProbability;
        m_enemyNumber = enemyNumber;
        m_destroyCost = destroyCost;
    }
}

#endregion