using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class GroupedAttack
{

}

[System.Serializable]
public class Event
{
    [Header("Properties")]
    public string name;
    public int identity;
    public string type = "Random";
    [Header("Event dialogs")]
    public Dialog[] dialogs;
}

[System.Serializable]
public class Dialog
{
    [Header("Properties")]
    public string locutor = "DEFAULT_LOCUTOR";
    [TextArea] public string text = "DEFAULT_BLABLA";
    public Sprite locutorIcon;
    [Header("Choices")]
    public ChoiceButton[] btns;
}

[System.Serializable]
public struct ChoiceButton
{
    public string btnName;
    public Sprite icon;
    public UnityEvent onExecute;
}

[System.Serializable]
public class Build
{
    [Header("Parameters")]
    public string name;
    [TextArea] public string description;
    public int[] techsNeeded;
    public Vector2 heightLimit;
    public int identity;
    [Space(5)]
    public Sprite icon;
    [Header("Costs")]
    public int colonist;
    public int energy;
    public int money;
    [Space(3)]
    public float rigolyte;
    public float bioPlastique;
    public float food;
    [Space(5)]
    public int profit;
    public int maxColonist;
    public float rigolyteOutput, bioPlastiqueOutput, foodOutput, research;
    [Space(5)]
    public int energyStorage;
    public float rigolyteStock;
    public float bioPlasticStock, foodStock;
    [Header("Authorizations")]
    public bool canLink;
    public bool canBeLink;
    public bool canBePause;
    public bool canControlSector;
    [Header("Others")]
    public int maxProgress;
    public float experience;
    [Header("GameObjects")]
    public GameObject preview;
    public GameObject building;
}

[System.Serializable]
public class EnemieArmies
{
    public string name;
    public int difficultyIndex;
    public Event waveEvent;
    [Space(5)]
    public List<EnemieUnits> units = new List<EnemieUnits>();
    public int maxUnitPerLigne = 20;
    public int spacing = 8;
    public int interligneSpacing = 10;
    [Space(5)]
    public int identity;
}

[System.Serializable]
public class EnemieUnits
{
    public string name;
    [Range(0, 1000)] public int numberOfUnit;
    public int identityOfUnit;
}

/*[System.Serializable]
public class Unit
{
    [Header("Properties")]
    public string name = "";
    public Sprite unitIcon;
    public float time = 2f;
    [Space(5)]
    public int identity;
    [Header("Levels")]
    public int maxLevel;
    public float[] levelDamage;
    public float[] levelHealth;
    [Header("Direct cost")]
    public int place = 1;
    public int money = 10;
    public float food = 1f;
    [Header("Indirect cost")]
    public int moneyOut;
    public float foodOut;
    [Space(10)]
    public GameObject model;
}*/

[System.Serializable]
public class MoonColony
{
    public string name = "DEFAULT_COLONY";
    public float[] colonyColor = new float[4];
    public int side = 1;
    public int state = 1;
    public int puissance = 0;
}

[System.Serializable]
public class DiplomatieStatut
{
    public MoonColony pointOfView;
    public List<MoonColony> statutWithOther = new List<MoonColony>();
}

[System.Serializable]
public class ObjectGroup
{
    public int objectsNumber = 0;
    public EntityType groupType;
    public int groupSide;
    public List<Entity> objectsInGroup = new List<Entity>();
}

[System.Serializable]
public class Language
{
    public string langName = "en-EN";
    public List<Sentence> sentences;
}

[System.Serializable]
public class Sentence
{
    [TextArea] public string baseSentence = "DEFAULT";
    [TextArea] public string sentence = "DEFAULT";
}

[System.Serializable]
public class Notification
{
    public string title = "DEFAULT_NOTIF";
    public string description = "DEFAULT_DESCRIPTION";
    public float duration = 2f;
    public Sprite icon;
    public Color color = new Color(0, 0, 0);
    public string command = "";
}

[System.Serializable]
public struct TerrainType
{
    public string name;
    public float height;
    public Color color;
}

[System.Serializable]
public class Music
{
    public string musicName;
    public AudioClip audio;
    [Range(0,1)] public float volume = 1f;
}

[System.Serializable]
public class PonctualEvent : Event
{
    [Header("Date")]
    public int time = 720;
    public int day;
}

[System.Serializable]
public class Categorie
{
    public string name;
    public Sprite icon;
    public int[] builsId;
}

[System.Serializable]
public class Technology
{
    public string name;
    public string description;
    public Sprite icon;
    public float cost;
    public int[] neededTech;
    public int identity;
}

[System.Serializable]
public struct TechTree
{
    public string name;
    public List<Technology> technologies;

    [Space(10)]
    public int identifier;
}

public class PointHeightData
{
    private Vector2 pointPositions;
    private float pointHeight;

    public PointHeightData(int x, int z, float y)
    {
        pointPositions = new Vector2(x, z);
        pointHeight = y;
    }

    public bool CheckPointPosition(Vector2 pos)
    {
        if (pos == pointPositions) return true;
        else return false;
    }

    public float GetPointHeight()
    {
        return pointHeight;
    }
}

[System.Serializable]
public class GameKey
{
    public string keyName;
    public string keyTouch;
    public string basicKey;
}

[System.Serializable]
public class Difficulty
{
    public string name;
    public int colonist, maxColonist, rigolyte, food, bioPlastique, energy, money;
    public int enemyWaveRate;
    public int enemiesCount;
    public int destroyedCost;
}

[System.Serializable]
public class GameSettings
{
    public int seed;
    public Color colonyColor;
    public int flagId;
    public Difficulty gameDifficulty;
}

[System.Serializable]
public class Configuration
{
    public List<GameKey> keys;

    public Configuration(List<GameKey> _keys)
    {
        keys = _keys;
    }
}

[System.Serializable]
public enum DamageType { Physic, Energy, Impulsion, None }

[System.Serializable]
public enum UnitType { Worker, Medic, Security, Military }

[System.Serializable]
public enum StrategyType { Passif, Defensif, Aggressif }

[System.Serializable]
public enum EntityType { None, Unit, Building, Preview }