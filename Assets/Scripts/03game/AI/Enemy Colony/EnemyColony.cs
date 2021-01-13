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
    public int unitNbr;
    public int defenseNbr;

    [Header("- Others")]
    [SerializeField] private Building[] buildData;
    [SerializeField] private Units[] unitData;
    [SerializeField] private TechnologyDatabase techData;
    [SerializeField] private GameObject physicalPrefab;

    [HideInInspector] public List<int> researchUnlock = new List<int>();
    [HideInInspector] public int currentTech = -1;
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
            if (disableEnemy) break;

            if (DoSomething())
            {
                currentAction++;
            }

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

    private bool Construct(int[] ids)
    {
        if (previewNbr >= maxPreviews || buildNbr >= maxBuilding) return false;

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

    private bool CreateUnit()
    {
        if(factories.Count == 0 || QueuesAreFull())
        {
            return Construct(new int[2] { 8, 7 });
        }
        else
        {
            return AddUnitToQueue();
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

    private bool AddUnitToQueue()
    {
        unitNbr++;
        return true;
    }

    #region Resource checker

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