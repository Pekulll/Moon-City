﻿using System.Collections.Generic;
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

    private EnemyColony owner;

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
        ActualizeOwner();
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
        else
        {
            Debug.LogError("[ERROR:Entity] Can't find marker!");
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
            /*if (tagIdentifier._tags.Contains(Tag.Saved))
            {
                tagIdentifier._tags.Remove(Tag.Saved);
            }*/
        }
    }

    private void ActualizeOwner()
    {
        if (side == manager.side) return;

        owner = GetOwner();
        owner.AddEntity(this);
    }

    private EnemyColony GetOwner()
    {
        GameObject[] enemies = manager.FindTags(new Tag[2] { Tag.Enemy, Tag.Core });

        foreach (GameObject g in enemies)
        {
            EnemyColony e = g.GetComponent<EnemyColony>();

            if (e.colony.side == side)
            {
                return e;
            }
        }

        throw new System.Exception("Can't find the appropriate enemy for: " + side);
    }

    #endregion

    #region Damage / healing

    public bool ApplyDamage(float amount, DamageType type)
    {
        health -= (type == weakness) ? amount * 2 : amount;

        if(health <= 0)
        {
            DisengageAll();
            DestroyImmediate(gameObject);
        }

        return true;
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

    public void AddEngagedUnit(Unit unit)
    {
        calledUnits.Add(unit);
    }

    public void DisengageUnits(UnitType typeOfUnit)
    {
        List<Unit> units = new List<Unit>();

        if(calledUnits != null)
        {
            foreach (Unit u in calledUnits)
            {
                if (u.unitType == typeOfUnit)
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
    }

    public void DisengageAll()
    {
        if(calledUnits != null)
        {
            foreach (Unit u in calledUnits)
            {
                u.Disengage(this);
            }
        }
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

    #region Enemy

    public void RemoveOwnerResources(Units u)
    {
        owner.RemoveResources(0, u.money, 0, 0, 0, u.food);
        owner.AddOutput(0, u.moneyOut, 0, 0, 0, u.foodOut, 0);
        owner.AddSettlers(u.place, 0);
    }

    #endregion

    private void OnDestroy()
    {
        DisengageAll();
        manager.DeleteGameObjectOfTagList(gameObject);

        if (owner != null)
            owner.RemoveEntity(this);
        else if(entityType == EntityType.Unit)
        {
            manager.RemoveSettlers(manager.unitData[id].place, 0);
        }
    }
}