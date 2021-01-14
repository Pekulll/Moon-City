using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyColony : ColonyStats
{
    [Header("Enemy")]
    [SerializeField] private bool disableEnemy;

    [Header("- Actions")]
    [SerializeField] private int currentAction = 0;
    [SerializeField] [Range(.01f, 10f)] private float actionPerSeconds = 0.3f;
    [SerializeField] [Range(0, 1000)] private int maxAction = 10;
    [SerializeField] [Range(1, 50)] private int maxSubAction = 2;
    [Range(0, 100)] public int maxBuilding = 20;
    [Range(0, 10)] public int maxPreviews = 2;

    [Header("- Construction")]
    [Range(0, 5)] [SerializeField] private int buildSpacing = 0;
    public float influenceRadius = 50f;
    private PhysicalPointMotor physicalPoint;

    [Header("- Strategy")]
    [SerializeField] private StrategyType strategy = StrategyType.Defensive;
    [SerializeField] private Profile[] profiles;
    private Profile currentProfile;
    private float criticalityScale = 1f;

    [Header("- Statistics")]
    public int previewNbr;
    public int buildNbr;
    public int defenseNbr;
    public int unitNbr;
    public int constructorNbr;

    [Header("- Others")]
    [SerializeField] private Building[] buildData;
    [SerializeField] private Units[] unitData;
    [SerializeField] private TechtreeDatabase techData;
    [SerializeField] private GameObject physicalPrefab;

    [HideInInspector] public List<int> techsUnlocked = new List<int>();
    [HideInInspector] public Vector3 currentTech = new Vector3(-1, -1, -1);
    [HideInInspector] public float techProgress;

    [HideInInspector] public string enemyName;

    private List<Unit> units;
    private List<FactoryMotor> factories;

    private WaitForSeconds delay;

    private MoonManager manager;

    private int errorPropagation;

    private int pre_profit;
    private float pre_regolith;
    private float pre_bioplastic;
    private float pre_food;
    private float pre_research;
    private int pre_colonist;
    private int pre_maxColonist;

    private int pre_unit;

    private bool isInitialize;

    //? Règle universelle : 9 <=> false et 10 <=> true

    private void Start()
    {
        if (disableEnemy) return;

        Initialize();
        StartCoroutine(MainLoop());
    }

    #region Initialization

    public void Initialize()
    {
        if (isInitialize || disableEnemy) return;
        isInitialize = true;

        currentProfile = GetProfile();
        criticalityScale = currentProfile.criticalityScale;

        units = new List<Unit>();
        factories = new List<FactoryMotor>();

        delay = new WaitForSeconds(1 / actionPerSeconds);

        CreatePhysicalPointer();
        buildNbr++; // remove that when enemy buildings will be comptabilized
    }

    private Profile GetProfile()
    {
        foreach(Profile p in profiles)
        {
            if(p.type == strategy)
            {
                return p;
            }
        }

        throw new System.Exception("Profile for strategy \'" + strategy + "\' doesn't exists!");
    }

    private void CreatePhysicalPointer()
    {
        GameObject point = Instantiate(physicalPrefab) as GameObject;
        point.name = "{Physical:" + colony.side + "}";
        physicalPoint = point.GetComponent<PhysicalPointMotor>();
        physicalPoint.Initialize(buildSpacing);
    }

    #endregion

    private IEnumerator MainLoop()
    {
        while (true)
        {
            if (disableEnemy)
                break;

            if (DoSomething())
                currentAction++;

            if (currentAction >= maxAction)
                break;

            errorPropagation = 0;
            yield return delay;
        }
    }

    private bool DoSomething()
    {
        if(unitNbr / buildNbr < currentProfile.unitRatio)
        {
            return CreateUnit();
        }
        else if(defenseNbr / buildNbr < currentProfile.defenseRatio)
        {
            return Construct(new int[3] { 34, 15, 33 });
        }
        else
        {
            return Construct(new int[1] { currentProfile.priority });
        }
    }

    #region Construction

    private bool Construct(int[] ids)
    {
        if (previewNbr >= maxPreviews || buildNbr + previewNbr >= maxBuilding) return false;

        errorPropagation++;

        if(errorPropagation <= maxSubAction)
        {
            for (int i = ids.Length - 1; i >= 0; i--)
            {
                List<int> resourceNeeded = ResourceNeeded(buildData[ids[i]]);

                if (resourceNeeded.Count == 0)
                {
                    return Construct(ids[i]);
                }
                else
                {
                    if (resourceNeeded.Contains(0)) return Construct(new int[3] { 3, 2, 1 }); // Money (appart)
                    if (resourceNeeded.Contains(1)) return Construct(new int[2] { 29, 28 }); // Energy (nuclear reactor)
                    if (resourceNeeded.Contains(2)) return Construct(new int[3] { 3, 2, 1 }); // Colonist (appart)
                    if (resourceNeeded.Contains(3)) return Construct(new int[2] { 10, 9 }); // Regolith (excavator)
                    if (resourceNeeded.Contains(4)) return Construct(new int[1] { 16 }); // Bioplastic (biofactory)
                    if (resourceNeeded.Contains(5)) return Construct(new int[3] { 6, 5, 4 }); // Food (farm)
                }
            }
        }

        return false;
    }

    private bool Construct(int id)
    {
        Building current = buildData[id];

        Vector3 position = transform.position
            + new Vector3(Random.Range(-influenceRadius, influenceRadius), 0, Random.Range(-influenceRadius, influenceRadius));

        for (int lim = 0; lim < maxSubAction; lim++)
        {
            if (physicalPoint.isValid(current.preview, position))
            {
                GameObject go = Instantiate(current.preview, position, Quaternion.identity);
                Preview preview = go.GetComponent<Preview>();
                preview.side = colony.side;

                influenceRadius++;
                previewNbr++;

                RemoveRessources(0, current.money, current.regolith, current.bioPlastique, current.food);
                return true;
            }
            else
            {
                position = transform.position
                    + new Vector3(Random.Range(-influenceRadius, influenceRadius), 0, Random.Range(-influenceRadius, influenceRadius));
            }
        }

        Debug.Log("[INFO:EnemyColony] Can't find a correct position.");
        return false;
    }

    #endregion

    #region Unit creation

    private bool CreateUnit()
    {
        if(factories.Count == 0 || QueuesAreFull())
        {
            return Construct(new int[2] { 8, 7 }); // Build more barracks / military factories
        }
        else
        {
            return ChooseUnit(); // Add a unit to the emptiest queue
        }
    }

    private bool QueuesAreFull()
    {
        foreach(FactoryMotor f in factories)
        {
            if (f.queue.Count < 5) return false;
        }

        return true;
    }

    private bool ChooseUnit()
    {
        int[] unitIDs;

        if(constructorNbr < buildNbr / 2)
        {
            unitIDs = new int[1] { 6 };
        }
        else
        {
            unitIDs = new int[6] { 5, 4, 2, 3, 1, 0 };
        }

        foreach(int id in unitIDs)
        {
            List<int> resourceNeeded = ResourceNeeded(unitData[id]);
            if (resourceNeeded.Count != 0) continue;

            return AddUnitToQueue(id);
        }

        return false;
    }

    private bool AddUnitToQueue(int unitID)
    {
        int emptiestNumber = 6;
        int index = -1;

        for(int i = 0; i < factories.Count; i++)
        {
            if (factories[i].queue.Count < emptiestNumber
                && factories[i].queue.Count < 5
                && factories[i].unitsAvailable.Contains(unitID))
                index = i;
        }

        if(index != -1)
        {
            factories[index].AddQueue(unitID, false);
            pre_unit++;
            RemoveRessources(0, unitData[unitID].money, 0, 0, unitData[unitID].food);
            AddSettlers(unitData[unitID].place, 0);
            return true;
        }
        else
        {
            List<int> barracksUnit = new List<int>() { 7, 6, 2, 1, 0 };
            List<int> factoryUnit = new List<int>() { 5, 4, 3 };

            if (barracksUnit.Contains(unitID))
                return Construct(new int[1] { 7 });
            else if (factoryUnit.Contains(unitID))
                return Construct(new int[1] { 8 });
            else
                return false;
        }
    }

    #endregion

    #region Research

    private bool Research()
    {
        if(currentTech == new Vector3(-1, -1, -1))
        {
            // Do the next research (by id order)
            currentTech = GetTechById(techsUnlocked.Count);

            if (currentTech == new Vector3(-1, -1, -1))
                currentTech = new Vector3(-2, -2, -2);

            return true;
        }

        return false;
    }

    private Vector3 GetTechById(int id)
    {
        for(int i = 0; i < techData.techTrees.Count; i++)
        {
            for(int j = 0; j < techData.techTrees[i].technologies.Count; j++)
            {
                if(techData.techTrees[i].technologies[j].identity == id)
                {
                    return new Vector3(i, j, id);
                }
            }
        }

        return new Vector3(-1, -1, -1);
    }

    private bool OwnedTechs(int[] techs)
    {
        foreach(int id in techs)
        {
            if (!techsUnlocked.Contains(id))
            {
                return false;
            }
        }

        return true;
    }

    public void Progress()
    {
        if (currentTech == new Vector3(-1, -1, -1)
            || currentTech == new Vector3(-2, -2, -2))
            return;

        techProgress += research;

        if(techProgress >= techData.techTrees[(int)currentTech.x].technologies[(int)currentTech.y].cost)
        {
            techsUnlocked.Add((int)currentTech.z);
            currentTech = new Vector3(-1, -1, -1);
            techProgress = 0;
        }
    }

    #endregion

    #region Resources

    #region Checkers

    private List<int> ResourceNeeded(Building b)
    {
        List<int> resourceNeeded = new List<int>();

        if (colonist + pre_colonist + b.colonist > maxColonist) resourceNeeded.Add(0);
        if (energyOutput + anticipatedEnergy + b.energy < 0) resourceNeeded.Add(1);
        if (profit + pre_profit < b.money) resourceNeeded.Add(2);
        if (regolithOutput + pre_regolith < b.regolith) resourceNeeded.Add(3);
        if (bioPlastiqueOutput + pre_bioplastic < b.bioPlastique) resourceNeeded.Add(4);
        if (foodOutput + pre_food < b.food) resourceNeeded.Add(5);

        return resourceNeeded;
    }

    private List<int> ResourceNeeded(Units u)
    {
        List<int> resourceNeeded = new List<int>();

        if (colonist + pre_colonist + u.place > maxColonist) resourceNeeded.Add(0);
        if (profit + pre_profit < u.money) resourceNeeded.Add(2);
        if (foodOutput + pre_food < u.food) resourceNeeded.Add(5);

        return resourceNeeded;
    }

    #endregion

    #region Add / remove output

    public void AddOutput(int _energy, int _money, float _regolith, float _bioplastic, float _food, float _research)
    {
        Debug.Log("[INFO:MoonManager] Adding output...");

        if (_energy > 0) energyGain += Mathf.Abs(_energy);
        else energyLoss += Mathf.Abs(_energy);

        if (_money > 0) moneyGain += Mathf.Abs(_money);
        else moneyLoss += Mathf.Abs(_money);

        if (_regolith > 0) regolithGain += Mathf.Abs(_regolith);
        else regolithLoss += Mathf.Abs(_regolith);

        if (_bioplastic > 0) bioPlastiqueGain += Mathf.Abs(_bioplastic);
        else bioPlastiqueLoss += Mathf.Abs(_bioplastic);

        if (_food > 0) foodGain += Mathf.Abs(_food);
        else foodLoss += Mathf.Abs(_food);

        research += _research;
        CalculateOutput();
    }

    public void RemoveOutput(int _energy, int _money, float _regolith, float _bioplastic, float _food, float _research)
    {
        if (_energy > 0) energyGain -= Mathf.Abs(_energy);
        else energyLoss -= Mathf.Abs(_energy);

        if (_money > 0) moneyGain -= Mathf.Abs(_money);
        else moneyLoss -= Mathf.Abs(_money);

        if (_regolith > 0) regolithGain -= Mathf.Abs(_regolith);
        else regolithLoss -= Mathf.Abs(_regolith);

        if (_bioplastic > 0) bioPlastiqueGain -= Mathf.Abs(_bioplastic);
        else bioPlastiqueLoss -= Mathf.Abs(_bioplastic);

        if (_food > 0) foodGain -= Mathf.Abs(_food);
        else foodLoss -= Mathf.Abs(_food);

        research -= _research;
        CalculateOutput();
    }

    #endregion

    #region Add / remove ressources, workers and storage

    public void AddRessources(int _energy, int _money, float _regolith, float _bioplastic, float _food)
    {
        energy += _energy;
        money += _money;
        regolith += _regolith;
        bioPlastique += _bioplastic;
        food += _food;
    }

    public void RemoveRessources(int _energy, int _money, float _regolith, float _bioplastic, float _food)
    {
        energy -= _energy;
        money -= _money;
        regolith -= _regolith;
        bioPlastique -= _bioplastic;
        food -= _food;
    }

    public void AddSettlers(int _workers, int _colonist)
    {
        colonist += _workers;
        maxColonist += _colonist;
    }

    public void RemoveSettlers(int _workers, int _colonist)
    {
        colonist -= _workers;
        maxColonist -= _colonist;
    }

    public void ManageStorage(int _energy, float _regolith, float _bioplastic, float _food)
    {
        energyStorage += _energy;
        regolithStock += _regolith;
        bioPlasticStock += _bioplastic;
        foodStock += _food;
    }

    #endregion

    #region Anticipate

    public void AnticipateRessources(int energy)
    {
        anticipatedEnergy += energy;
    }

    public void RemoveAnticipateRessources(int energy)
    {
        anticipatedEnergy -= energy;
    }

    #endregion

    #endregion

    #region Counters

    public void AddEntity(Entity entity)
    {
        if (entity.entityType == EntityType.Building)
        {
            AddBuilding(entity.GetComponent<Buildings>());
        }
        else if (entity.entityType == EntityType.Preview)
        {
            AnticipateRessources(buildData[entity.GetComponent<Preview>().id].energy);
            previewNbr++;
        }
        else if (entity.entityType == EntityType.Unit)
        {
            unitNbr++;
            pre_unit--;
        }
    }

    public void RemoveEntity(Entity entity)
    {
        if (entity.entityType == EntityType.Building)
        {
            RemoveBuilding(entity.GetComponent<Buildings>());
        }
        else if (entity.entityType == EntityType.Preview)
        {
            RemoveAnticipateRessources(buildData[entity.GetComponent<Preview>().id].energy);
            previewNbr--;
        }
        else if (entity.entityType == EntityType.Unit)
        {
            unitNbr--;
        }
    }

    private void AddBuilding(Buildings b)
    {
        AddOutput(b.building.energy, b.building.profit, b.building.rigolyteOutput, b.building.bioPlastiqueOutput, b.building.foodOutput, b.building.research);
        ManageStorage(b.building.energyStorage, b.building.rigolyteStock, b.building.bioPlasticStock, b.building.foodStock);
        AddSettlers(b.building.colonist, b.building.maxColonist);
        buildNbr++;
    }

    private void RemoveBuilding(Buildings b)
    {
        RemoveOutput(b.building.energy, b.building.profit, b.building.rigolyteOutput, b.building.bioPlastiqueOutput, b.building.foodOutput, b.building.research);
        ManageStorage(-b.building.energyStorage, -b.building.rigolyteStock, -b.building.bioPlasticStock, -b.building.foodStock);
        RemoveSettlers(b.building.colonist, b.building.maxColonist);
        buildNbr--;
    }

    #endregion
}

[System.Serializable]
public enum StrategyType { Passive, Defensive, Agressive }

[System.Serializable]
public struct Profile
{
    public StrategyType type;

    public float criticalityScale;
    public float unitRatio;
    public float defenseRatio;

    public int priority;
}