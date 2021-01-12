using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TagIdentifier))]
public class Entity : MonoBehaviour
{
    [Header("Properties")]
    public string entityName = "Default_Entity";
    public int side;
    public int groupID = -1;
    public int id;

    [HideInInspector] public float health;
    [HideInInspector] public float maxHealth;

    [HideInInspector] public float shield;
    [HideInInspector] public float maxShield;

    [HideInInspector] public float energy;
    [HideInInspector] public float maxEnergy;

    [HideInInspector] public DamageType weakness = DamageType.None;
    [HideInInspector] public EntityType entityType;

    [HideInInspector] public MoonManager manager;

    private List<Unit> calledUnits;

    private GameObject marker;
    private SpriteRenderer eventSprite;

    private void Awake()
    {
        try
        {
            eventSprite = transform.Find("Sprt").GetComponent<SpriteRenderer>();
            eventSprite.gameObject.SetActive(false);
        }
        catch { }
    }

    #region Super initialization

    public void SuperInitialization()
    {
        FindMarker();
        UpdateTag();
    }

    private void FindMarker()
    {
        if (transform.Find("Marker") != null)
        {
            marker = transform.Find("Marker").gameObject;

            SpriteRenderer sprite = marker.GetComponent<SpriteRenderer>();

            if (manager == null) manager = GameObject.Find("Manager").GetComponent<MoonManager>();

            if (side == manager.side)
            {
                sprite.color = manager.colonyColor;
            }
            else
            {
                sprite.color = new Color(0.8f, 0, 0, 1);
            }

            /*if (isTurret)
            {
                range = transform.Find("Range").gameObject;
                Light lgtr = range.GetComponent<Light>();
                lgtr.renderMode = LightRenderMode.ForcePixel;
            }*/

            Marker(false);
        }
    }

    public void Marker(bool enable)
    {
        if (marker == null) return;

        marker.SetActive(enable);
    }

    private void UpdateTag()
    {
        if (side != manager.side)
        {
            TagIdentifier tagIdentifier = GetComponent<TagIdentifier>();

            if (!tagIdentifier._tags.Contains(Tag.Enemy))
            {
                tagIdentifier._tags.Add(Tag.Enemy);
            }

            // remove this when the enemies should be saved
            if (tagIdentifier._tags.Contains(Tag.Saved))
            {
                tagIdentifier._tags.Remove(Tag.Saved);
            }
        }
    }

    #endregion

    #region Damage / healing

    public bool ApplyDamage(float amount, DamageType type)
    {
        return false;
    }

    public bool ApplyHeal(float amount)
    {
        health += amount;

        if(health >= maxHealth)
        {
            health = maxHealth;
            shield = maxShield;
            return true;
        }

        return false;
    }

    #endregion

    #region Engage / disengage

    public void EngageUnits(UnitType typeOfUnit)
    {
        GameObject[] units = manager.FindTag(Tag.Unit);
        calledUnits = new List<Unit>();

        foreach(GameObject g in units)
        {
            Unit e = g.GetComponent<Unit>();

            if(e.side == side && e.entityType == EntityType.Unit)
            {
                Unit u = e.GetComponent<Unit>();
                
                if(u.unitType == typeOfUnit)
                {
                    calledUnits.Add(u);
                    u.Engage(this);
                }
            }
        }
    }

    public void DisengageUnits(UnitType typeOfUnit)
    {
        List<Unit> units = new List<Unit>();

        foreach(Unit u in calledUnits)
        {
            if(u.unitType == typeOfUnit)
            {
                u.Disengage(this);
            }
            else
            {
                units.Add(u);
            }
        }

        calledUnits = units;
    }

    #endregion

    #region Sectors

    public void Abandon()
    {
        if (entityType == EntityType.Preview)
        {
            Destroy(gameObject);
        }
        else if (entityType == EntityType.Building)
        {
            side = -1; // -1 is neutral player's side
            groupID = -1;
            Marker(false);
        }
    }

    public void Restore(int side)
    {
        if (entityType == EntityType.Building)
        {
            this.side = side;
        }
    }

    #endregion

    private void OnDestroy()
    {
        manager.DeleteGameObjectOfTagList(gameObject);
    }
}