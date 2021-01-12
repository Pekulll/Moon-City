using System.Collections.Generic;
using UnityEngine;

public class Buildings : Entity
{
    [HideInInspector] public Building building;

    private EnemyBuilding enemy = null;
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
        InitializeEnemy();
    }

    public void Initialize(bool onSave = false)
    {
        if (isInitialize) return;
        isInitialize = true;

        manager = GameObject.Find("Manager").GetComponent<MoonManager>();
        resources = GameObject.Find("Manager").GetComponent<RessourceData>();
        status = GetComponentInChildren<SpriteRenderer>();

        building = manager.buildData[id];
        isEnemy = side != manager.side;

        haveOutput = false;

        SuperInitialization();
        LoadStats(onSave);
        
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

        if (onSave) return;

        health = b.maxHealth;
        shield = b.maxShield;
        isEnable = true;
    }

    private void InitializeEnemy()
    {
        if (isEnemy)
        {
            if (GetComponent<EnemyBuilding>() == null)
                enemy = gameObject.AddComponent<EnemyBuilding>();
            else
                enemy = GetComponent<EnemyBuilding>();

            EnemyMotor[] enemies = FindObjectsOfType<EnemyMotor>();

            foreach (EnemyMotor e in enemies)
            {
                if (e.GetComponent<ColonyStats>().colony.side == side)
                {
                    enemy.Init(e);
                    return;
                }
            }
        }
        else
        {
            AddOutput();
        }
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
            List<int> ints = manager.HaveRessources(id);

            if (ints.Contains(0)) { manager.Notify(21); }
            else if (ints.Contains(1)) { manager.Notify(22); }
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

    public void AddOutput()
    {
        if (isEnable && !haveOutput)
        {
            if (!isEnemy)
            {
                manager.AddOutput(
                    building.energy, building.profit,
                    building.rigolyteOutput, building.bioPlastiqueOutput, building.foodOutput,
                    building.research
                );

                manager.AddSettlers(building.colonist, building.maxColonist);

                manager.ManageStorage(
                    building.energyStorage, building.rigolyteStock, building.bioPlasticStock, building.foodStock
                );
            }
            else
            {
                EnemyMotor mtr = enemy.mtr;

                mtr.AddOutput(
                    building.energy, building.profit,
                    building.rigolyteOutput, building.bioPlastiqueOutput, building.foodOutput,
                    building.research
                );

                mtr.AddSettlers(building.colonist, building.maxColonist);

                mtr.ManageStorage(
                    building.energyStorage, building.rigolyteStock, building.bioPlasticStock, building.foodStock
                );
            }

            haveOutput = true;
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
                    building.rigolyteOutput, building.bioPlastiqueOutput, building.foodOutput,
                    building.research
                );

                manager.RemoveSettlers(building.colonist, building.maxColonist);

                manager.ManageStorage(
                    -building.energyStorage, -building.rigolyteStock, -building.bioPlasticStock, -building.foodStock
                );
            }
            else
            {
                EnemyMotor mtr = enemy.mtr;

                mtr.RemoveRessources(
                    building.energy, building.profit,
                    building.rigolyteOutput, building.bioPlastiqueOutput, building.foodOutput
                );

                mtr.RemoveSettlers(building.colonist, building.maxColonist);

                mtr.ManageStorage(
                    -building.energyStorage, -building.rigolyteStock, -building.bioPlasticStock, -building.foodStock
                );
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
        float percentage = 0.5f;

        manager.AddRessources(0, (int)(building.money * percentage), (int)(building.regolith * percentage), (int)(building.bioPlastique * percentage), (int)(building.food * percentage));
        manager.RemoveSettlers(building.colonist, 0);
    }

    #endregion
}
