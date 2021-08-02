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

    [Header("- Research")]
    public Vector3 currentTech = new Vector3(-1, -1, -1);
    public float techProgress;
    public List<int> techsUnlocked = new List<int>();
    
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

    [HideInInspector] public string enemyName;

    private List<Unit> units;
    private List<TrainingArea> factories;

    private WaitForSeconds delay;

    //private MoonManager manager;
    private TradingSystem trade;

    private int errorPropagation;

    private int pre_profit;
    private float pre_regolith;
    private float pre_metal;
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

        Initialize(Random.Range(0, 2));
        StartCoroutine(MainLoop());
    }

    #region Initialization

    public void Initialize(int startegy)
    {
        if (isInitialize || disableEnemy) return;
        isInitialize = true;

        trade = GameObject.Find("Manager").GetComponent<TradingSystem>();
        
        this.strategy = strategy;
        currentProfile = GetProfile();
        criticalityScale = currentProfile.criticalityScale;

        units = new List<Unit>();
        factories = new List<TrainingArea>();

        delay = new WaitForSeconds(1 / actionPerSeconds);

        CreatePhysicalPointer();
    }

    private Profile GetProfile()
    {
        foreach(Profile p in profiles)
        {
            if(/*p.type == strategy*/p.type == StrategyType.Passive)
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
        yield return delay;

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
        if (!ConstructToSurvive())
        {
            if (unitNbr / buildNbr < currentProfile.unitRatio && buildNbr > 10)
            {
                //Debug.Log("[INFO:EnemyColony] Creating unit...");
                return CreateUnit();
            }
            else if ((defenseNbr + 1) / buildNbr < currentProfile.defenseRatio && buildNbr > 20)
            {
                Debug.Log("[INFO:EnemyColony] Creating defense...");
                return Construct(new int[3] { 34, 15, 33 });
            }
            else
            {
                //Debug.Log("[INFO:EnemyColony] Creating building...");
                return Construct(new int[1] { currentProfile.priority });
            }
        }
        else
        {
            return true;
        }
    }

    #region Construction

    private bool ConstructToSurvive()
    {
        List<int> resourceNeeded = ResourceNeeded();

        if(resourceNeeded.Count != 0)
        {
            if (resourceNeeded.Contains(0)) return Construct(new int[3] { 3, 2, 1 }); // Money (appart)
            if (resourceNeeded.Contains(1)) return Construct(new int[2] { 29, 28 }); // Energy (nuclear reactor)
            if (resourceNeeded.Contains(2)) return Construct(new int[3] { 3, 2, 1 }); // Colonist (appart)
            if (resourceNeeded.Contains(3)) return Construct(new int[2] { 10, 9 }); // Regolith (excavator)
            if (resourceNeeded.Contains(4)) return Construct(new int[1] { 16 }); // Bioplastic (biofactory)
            if (resourceNeeded.Contains(5)) return Construct(new int[3] { 6, 5, 4 }); // Food (farm)
        }

        return false;
    }

    private bool Construct(int[] ids)
    {
        if (previewNbr >= maxPreviews || buildNbr + previewNbr >= maxBuilding) return false;

        errorPropagation++;
        string reason = "Propagation";

        if(errorPropagation <= maxSubAction)
        {
            for (int i = 0; i < ids.Length; i++)
            {
                if (OwnTechs(buildData[ids[i]]))
                {
                    List<int> resourceNeeded = ResourceNeeded(buildData[ids[i]]);

                    if (resourceNeeded.Count == 0)
                    {
                        return Construct(ids[i]);
                    }
                    else if(i == ids.Length - 1)
                    {
                        if (errorPropagation == maxSubAction)
                            return BuyResources(resourceNeeded);

                        string missing = "Resources missing: " + ", ".Join(resourceNeeded)
                            + ". To build: " + ", ".Join(ids);
                        Debug.Log("[INFO:EnemyColony] " + missing);
                        
                        if (resourceNeeded.Contains(0)) return Construct(new int[3] { 3, 2, 1 }); // Money (appart)
                        if (resourceNeeded.Contains(1)) return Construct(new int[2] { 29, 28 }); // Energy (nuclear reactor)
                        if (resourceNeeded.Contains(2)) return Construct(new int[3] { 3, 2, 1 }); // Colonist (appart)
                        if (resourceNeeded.Contains(3)) return Construct(new int[2] { 10, 9 }); // Regolith (excavator)
                        if (resourceNeeded.Contains(4)) return Construct(new int[1] { 16 }); // Polymer (biofactory)
                        if (resourceNeeded.Contains(5)) return Construct(new int[3] { 6, 5, 4 }); // Food (farm)
                    }
                }
                else if(!Research(buildData[ids[i]]))
                {
                    // TODO: Research the missing tech if possible
                    reason = "Missing tech";
                    continue;
                }
            }
        }

        Debug.Log("[WARN:EnemyColony] Can't find anything to build! " + reason);
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

                RemoveResources(0, current.money, current.regolith, current.metal, current.polymer, current.food);
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
        foreach(TrainingArea f in factories)
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
                && factories[i].unitID.Contains(unitID))
                index = i;
        }

        if(index != -1)
        {
            factories[index].Enqueue(unitID, false);
            pre_unit++;
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

    public void Progress()
    {
        if (currentTech == new Vector3(-1, -1, -1)
            || currentTech == new Vector3(-2, -2, -2))
            return;

        techProgress += research * 100; // WARN: Need to remove the *100 for release

        if(techProgress >= techData.techTrees[(int)currentTech.x].technologies[(int)currentTech.y].cost)
        {
            techsUnlocked.Add((int)currentTech.z);
            currentTech = new Vector3(-1, -1, -1);
            techProgress = 0;
        }
    }

    private bool OwnTechs(Building b)
    {
        foreach(int id in b.techsNeeded)
        {
            if (!techsUnlocked.Contains(id))
            {
                return false;
            }
        }

        return true;
    }

    private bool Research(Building b)
    {
        foreach(int id in b.techsNeeded)
        {
            if (!techsUnlocked.Contains(id))
            {
                return Research(id);
            }
        }

        return false;
    }

    private bool Research(int techID)
    {
        if (currentTech == new Vector3(-1, -1, -1))
        {
            Vector3 targetedTech = GetTechById(techID);

            if (targetedTech.x < 0) return false;
            int[] neededTech = techData.techTrees[(int)targetedTech.x].technologies[(int)targetedTech.y].neededTech;

            if (neededTech.Length != 0)
            {
                foreach(int id in neededTech)
                {
                    if (!techsUnlocked.Contains(id))
                    {
                        return Research(id);
                    }
                }
            }

            if (currentTech == new Vector3(-1, -1, -1))
                currentTech = new Vector3(-2, -2, -2);

            currentTech = targetedTech;
            //Debug.Log("[INFO:EnemyColony] Search: " + currentTech);
            return true;
        }

        return false;
    }

    #endregion

    #region Resources

    #region Checkers

    private List<int> ResourceNeeded()
    {
        List<int> resourceNeeded = new List<int>();

        if (maxColonist - (workers + pre_colonist) <= 0) resourceNeeded.Add(0);
        if (energyOutput + anticipatedEnergy <= 0) resourceNeeded.Add(1);
        if (profit + pre_profit <= 0) resourceNeeded.Add(2);
        if (regolithOutput + pre_regolith <= 0) resourceNeeded.Add(3);
        if (polymerOutput + pre_bioplastic <= 0) resourceNeeded.Add(4);
        if (foodOutput + pre_food <= 0) resourceNeeded.Add(5);
        if (metalOutput + pre_metal <= 0) resourceNeeded.Add(6);

        return resourceNeeded;
    }

    private List<int> ResourceNeeded(Building b)
    {
        List<int> resourceNeeded = new List<int>();

        if (workers + pre_colonist + b.colonist > maxColonist && b.colonist > 0) resourceNeeded.Add(0);
        if (energyOutput + anticipatedEnergy + b.energy < 0 && b.energy < 0) resourceNeeded.Add(1);
        if (money < b.money && b.money > 0) resourceNeeded.Add(2);
        if (regolith < b.regolith && b.regolith > 0) resourceNeeded.Add(3);
        if (polymer < b.polymer && b.polymer > 0) resourceNeeded.Add(4);
        if (food < b.food && b.food > 0) resourceNeeded.Add(5);
        if (metal < b.metal && b.metal > 0) resourceNeeded.Add(5);

        return resourceNeeded;
    }

    private List<int> ResourceNeeded(Units u)
    {
        List<int> resourceNeeded = new List<int>();

        if (workers + pre_colonist + u.place > maxColonist && u.place > 0) resourceNeeded.Add(0);
        if (profit + pre_profit < u.money && u.money > 0) resourceNeeded.Add(2);
        if (foodOutput + pre_food < u.food && u.food > 0) resourceNeeded.Add(5);

        return resourceNeeded;
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

    public void Output()
    {
        this.GainOutput();
        Progress();
    }

    public void CheckCritical()
    {
        
    }

    private bool BuyResources(List<int> resourceNeeded)
    {
        // Money
        if (resourceNeeded.Contains(0))
        {
            Debug.Log("[INFO:EnemyColony] Money can't be buy...");
            return false;
        }

        bool hasBuy = false;
        
        // Regolith (excavator)
        if (resourceNeeded.Contains(3))
        {
            RemoveResources(0, (int)(trade.market.regolithValue * 10), 0, 0, 0, 0);
            AddResources(0, 0, 10, 0, 0, 0);
            trade.market.FluctuateRegolithValue(1f);
            Debug.Log("[INFO:EnemyColony] Regolith has been bought.");
            hasBuy = true;
        }
        
        // Polymer (biofactory)
        if (resourceNeeded.Contains(4))
        {
            RemoveResources(0, (int)(trade.market.polymerValue * 10), 0, 0, 0, 0);
            AddResources(0, 0, 0, 0, 10, 0);
            trade.market.FluctuatePolymerValue(1f);
            Debug.Log("[INFO:EnemyColony] Polymer has been bought.");
            hasBuy = true;
        }
        
        // Food (farm)
        if (resourceNeeded.Contains(5))
        {
            RemoveResources(0, (int)(trade.market.foodValue * 10), 0, 0, 0, 0);
            AddResources(0, 0, 0, 0, 0, 10);
            trade.market.FluctuateFoodValue(1f);
            Debug.Log("[INFO:EnemyColony] Food has been bought.");
            hasBuy = true;
        }
        
        return hasBuy;
    }
    
    #endregion

    #region Counters

    public void AddEntity(Entity entity)
    {
        if (entity.entityType == EntityType.Building)
        {
            AddBuilding((Buildings)entity);
        }
        else if (entity.entityType == EntityType.Preview)
        {
            AnticipateRessources(buildData[entity.id].energy);
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
            RemoveBuilding((Buildings)entity);
        }
        else if (entity.entityType == EntityType.Preview)
        {
            previewNbr -= 2;
            RemoveAnticipateRessources(buildData[entity.id].energy);
        }
        else if (entity.entityType == EntityType.Unit)
        {
            unitNbr--;
        }
    }

    private void AddBuilding(Buildings b)
    {
        AddOutput(b.building.energy, b.building.profit, b.building.rigolyteOutput, b.building.metalOutput, b.building.polymerOutput, b.building.foodOutput, b.building.research);
        ManageStorage(b.building.energyStorage, b.building.rigolyteStock, b.building.metalStock, b.building.polymerStock, b.building.foodStock);
        AddSettlers(b.building.colonist, b.building.maxColonist);
        buildNbr++;
    }

    private void RemoveBuilding(Buildings b)
    {
        RemoveOutput(b.building.energy, b.building.profit, b.building.rigolyteOutput, b.building.metalOutput, b.building.polymerOutput, b.building.foodOutput, b.building.research);
        ManageStorage(-b.building.energyStorage, -b.building.rigolyteStock, -b.building.metalStock, -b.building.polymerStock, -b.building.foodStock);
        RemoveSettlers(b.building.colonist, b.building.maxColonist);
        buildNbr--;
    }

    #endregion

    public void Failure()
    {

    }
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