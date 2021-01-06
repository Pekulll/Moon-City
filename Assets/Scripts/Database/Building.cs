using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "LunarBuilding", menuName = "Data/Building")]
public class Building : ScriptableObject
{
    public int identity;

    [Header("Properties")]
    public Sprite icon;
    public string name = "New building";
    public string description = "This description is empty";
    [Space(5)]
    public int[] techsNeeded;

    [Header("Statistics")]
    public float maxHealth = 10f;
    public float maxShield;
    public float maxEnergy = 1f;
    public DamageType weakness = DamageType.None;

    [Header("Costs")]
    public int colonist;
    public int energy;
    public int money;
    [FormerlySerializedAs("rigolyte")] public float regolith;
    public float bioPlastique;
    public float food;

    [Header("Output")]
    public int profit;
    public int maxColonist;
    public float rigolyteOutput, bioPlastiqueOutput, foodOutput, research;

    [Header("Storage")]
    public int energyStorage;
    public float rigolyteStock;
    public float bioPlasticStock, foodStock;

    [Header("GameObjects")]
    public GameObject preview;
    public GameObject building;

    [Header("Placement")]
    public Vector2 heightLimit = new Vector2(-0.1f, 0.5f);
    public int maxProgress = 10;
    public float experience = 2f;
    [Space(5)]
    public bool needSpecificCollision = false;
    public Tag tagToCollide = Tag.Untagged;
    [Space(5)]
    public bool needNearBuilding = false;
    public float distance = 10f;
    public int nearBuildingID = 0;

    [Header("Authorizations")]
    public bool canLink = true;
    public bool canBeLink = true;
    public bool canBePause = true;
    public bool canControlSector = false;
}
