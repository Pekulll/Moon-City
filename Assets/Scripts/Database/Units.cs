using UnityEngine;

[CreateAssetMenu(fileName = "LunarUnit", menuName = "Data/Unit")]
public class Units : ScriptableObject
{
    public int identity;

    [Header("Properties")]
    public Sprite unitIcon;
    public string name = "New unit";
    public string description = "-";
    public float speed = 5f;
    public GameObject model;
    public UnitType unitType = UnitType.Military;

    [Header("Statistics")]
    public float health = 10f;
    public float shield;
    public float energy = 10f;
    [Space(5)]
    public float range = 25f;
    public float cooldown = 1f;
    public float damage = 1f;
    public DamageType damageType = DamageType.Physic;
    [Space(5)]
    public int baseAgressivity = 1;

    [Header("Levels")]
    public int maxLevel;
    public float[] levelDamage;
    public float[] levelHealth;

    [Header("Direct cost")]
    public float time = 2f;
    public int place = 1;
    public int money = 10;
    public float food = 1f;

    [Header("Indirect cost")]
    public int moneyOut;
    public float foodOut;
}
