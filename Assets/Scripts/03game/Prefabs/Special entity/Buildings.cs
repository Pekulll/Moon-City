﻿using System.Collections.Generic;
using UnityEngine;

public class Buildings : Entity
{
    [HideInInspector] public Building building;

    private RessourceData resources;

    private bool isInitialize;
    private bool isEnemy;

    [HideInInspector] public bool isEnable;
    private bool haveOutput;

    private SpriteRenderer status;

    private void Start()
    {
        Initialize();
        VerifySectorControl();
    }

    public void Initialize(bool onSave = false)
    {
        if (isInitialize) return;
        isInitialize = true;

        manager = GameObject.Find("Manager").GetComponent<MoonManager>();
        resources = GameObject.Find("Manager").GetComponent<RessourceData>();
        status = transform.Find("Sprt").GetComponent<SpriteRenderer>();

        building = manager.buildData[id];
        isEnemy = side != manager.side;

        haveOutput = false;
        entityType = EntityType.Building;

        LoadStats(onSave);
        AddOutput();
        SuperInitialization();
        
        UpdateEditor();
    }

    private void UpdateEditor()
    {
        entityName = building.name;
        gameObject.name = "[" + side + "] " + entityName;
        transform.SetParent(GameObject.Find("BuildingParent").transform);
    }

    public void LoadStats(bool onSave)
    {
        Building b = manager.buildData[id];

        maxHealth = b.maxHealth;
        maxShield = b.maxShield;
        maxEnergy = b.maxEnergy;
        weakness = b.weakness;

        description = b.description;

        if (onSave) return;

        health = b.maxHealth;
        shield = b.maxShield;
        isEnable = true;
    }

    private void VerifySectorControl()
    {
        if (building.canControlSector)
        {
            manager.ControlSector(transform.position, side);
        }
    }

    #region Statut

    public void Pause()
    {
        if (!building.canBePause) return;

        if (!isEnable)
        {
            List<int> ints = manager.HaveResources(id);

            if (ints.Contains(0))
            {
                manager.Notify(manager.Traduce("03_notif_building_activation_noworker"), priority: 2);
            }
            else if (ints.Contains(1))
            {
                manager.Notify(manager.Traduce("03_notif_building_activation_noenergy"), priority: 2);
            }
            else
            {
                isEnable = true;
                AddOutput();
                ChangeSprite(false);
            }
        }
        else
        {
            isEnable = false;
            RemoveOutput();
            ChangeSprite(true, resources.pause, new Color(1, .655f, 0, 1), .5f);
        }
    }

    private void ChangeSprite(bool enable, Sprite sprite = null, Color sprtColor = new Color(), float scale = 1)
    {
        status.gameObject.SetActive(enable);

        if (!enable) return;

        status.sprite = sprite;
        status.color = sprtColor;

        status.gameObject.transform.localScale = new Vector3(.5f, .5f, .5f) * scale;
    }

    #endregion

    #region Output

    private void AddOutput()
    {
        if (isEnable && !haveOutput)
        {
            if (!isEnemy)
            {
                manager.AddOutput(
                    building.energy, building.profit,
                    building.rigolyteOutput, building.metalOutput,
                    building.polymerOutput, building.foodOutput,
                    building.research
                );

                manager.AddSettlers(building.colonist, building.maxColonist);

                manager.ManageStorage(
                    building.energyStorage, building.rigolyteStock, building.metalStock, building.polymerStock, building.foodStock
                );

                haveOutput = true;
            }
        }
    }

    public void RemoveOutput()
    {
        if (!isEnable || haveOutput)
        {
            if (!isEnemy)
            {
                manager.RemoveOutput(
                    building.energy, building.profit,
                    building.rigolyteOutput, building.metalOutput,
                    building.polymerOutput, building.foodOutput,
                    building.research
                );

                manager.RemoveSettlers(building.colonist, building.maxColonist);

                manager.ManageStorage(
                    -building.energyStorage, -building.rigolyteStock, -building.metalStock, -building.polymerStock, -building.foodStock
                );

                haveOutput = false;
            }
        }
    }

    #endregion

    #region OnDeath

    private void OnDestroy()
    {
        RestoreCosts();
        manager.DeleteGameObjectOfTagList(gameObject);
    }

    private void RestoreCosts()
    {
        if(side == manager.side)
        {
            float percentage = 0.5f;

            manager.AddResources(0, (int)(building.money * percentage), (int)(building.regolith * percentage), (int)(building.metal * percentage), (int)(building.polymer * percentage), (int)(building.food * percentage));
            manager.RemoveSettlers(building.colonist, building.maxColonist);
        }
    }

    #endregion
}
