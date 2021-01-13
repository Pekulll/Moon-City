using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMotor : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private bool disableEnemy;
    [SerializeField] private float actionPerSeconds = 0.3f;
    [SerializeField] private int maxAction = 10;
    [SerializeField] [Range(1, 50)] private int maxSubAction = 2;
    public int maxBuilding = 20;
    public int maxPreviews = 2;
    [Range(0, 5)] [SerializeField] private int buildSpacing = 0;
    public float influenceRadius = 50f;
    public StrategyType strategy = StrategyType.Defensive;
    [Header("Statistics")]
    [SerializeField] private int currentAction = 0;
    public int previewNbr;
    public int buildNbr;
    public int unitNbr;
    public int defenseNbr;
    [Header("Others")]
    [SerializeField] private GameObject physicalPrefab;
    [SerializeField] private Building[] buildData;
    [SerializeField] private Units[] unitData;
    [SerializeField] private TechnologyDatabase techData;

    [HideInInspector] public List<int> researchUnlock = new List<int>();
    [HideInInspector] public int currentTech = -1;
    [HideInInspector] public float techProgress;

    [HideInInspector] public string enemyName;

    [HideInInspector] public float scaleCritical = 1f;

    private GameObject physicalPoint;

    private List<Unit> units;
    private List<FactoryMotor> factories;

    private WaitForSeconds delay;

    private ColonyStats colonyStats;
    private MoonManager manager;

    private int m_errorPropagation;

    private int pre_profit;
    private float pre_rigolyteOutput;
    private float pre_bioPlastiqueOutput;
    private float pre_foodOutput;
    private float pre_research;
    private int pre_colonist;
    private int pre_maxColonist;
    private int pre_energy;

    //? Règle universelle : 9 <=> false et 10 <=> true

    private void Awake()
    {
        units = new List<Unit>();
        factories = new List<FactoryMotor>();
    }

    private void Start()
    {
        colonyStats = GetComponent<ColonyStats>();
        manager = GameObject.Find("Manager").GetComponent<MoonManager>();
        manager.AddEnemyColony();
        enemyName = "Enemy N°" + colonyStats.colony.side;

        delay = new WaitForSeconds(1 / actionPerSeconds);

        if (disableEnemy)
        {
            Invoke("EnemyFailure", 0.1f);
            return;
        }

        physicalPoint = Instantiate(physicalPrefab) as GameObject;
        physicalPoint.name = "{Physical " + colonyStats.colony.side + "} Empty";

        if (strategy == StrategyType.Passive) { scaleCritical = 6; }
        else if (strategy == StrategyType.Defensive) { scaleCritical = 4; }
        else if (strategy == StrategyType.Agressive) { scaleCritical = 2; }

        foreach(Technology t in techData.techs)
        {
            researchUnlock.Add(t.identity);
        }

        CheckPoint();
        StartAction();
    }

    #region Main part

    private bool CheckPoint()
    {
        try
        {
            PhysicalPointMotor mtr = physicalPoint.GetComponent<PhysicalPointMotor>();

            if (mtr == null)
                mtr = physicalPoint.AddComponent<PhysicalPointMotor>();

            mtr.Initialize(buildSpacing);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private void StartAction()
    {
        StartCoroutine(DoAction());
    }

    private IEnumerator DoAction()
    {
        yield return delay;

        while (true)
        {
            if (currentAction < maxAction && buildNbr + previewNbr < maxBuilding)
            {
                if (currentTech == -1) //? currentTech = -2 si plus de recherche a effectué
                {
                    ResearchTech();
                }
                else if (previewNbr < maxPreviews)
                {
                    int crit = CheckCritical();  //? Identifie un potentiel problème de ressources

                    if (crit == 0)  //? Aucun problème de ressources
                    {
                        if (DevelopColony() == 10)
                        {
                            
                        }
                        else
                        {
                            
                        }
                    }
                    else if (crit == 1)  //? Un bâtiment a été construit pour remedier à un problème de ressources
                    {
                        
                    }
                    else if (crit == 2)  //? Impossible de régler le problème car trop peu de ressources
                    {
                        int critBalance = CheckCriticalBalance();  //? Check si la balance est réellement critique (<0)

                        if (critBalance == 0) //? Aucune anomalie, il suffit d'attendre
                        {
                            
                        }
                        else if (critBalance >= 2) //? Au moins deux ressources sont dans un état critique, fin de la partie #gameover
                        {
                            Debug.Log("  [INFO:EnemyMotor] The balance is really critical... critics=" + critBalance);
                            EnemyFailure();
                            break;
                        }
                        else //? Seulement une ressource est critique, on attend que ça se règle #wait
                        {
                            
                        }
                    }
                }

                yield return delay;
                currentAction++;
            }
            else
            {
                break;
            }
        }
    }

    #endregion

    #region Research

    private void ResearchTech()
    {
        foreach(Technology tech in techData.techs)
        {
            if (!researchUnlock.Contains(tech.identity))
            {
                currentTech = tech.identity;
                techProgress = 0;
            }
        }

        if(currentTech == -1) //? Si il n'y a plus de recherche a effectué le système se stoppe
        {
            currentTech = -2;
        }
    }

    private void Research(float point)
    {
        if(currentTech < 0) return;

        techProgress += point;

        if(techProgress >= techData.GetTech(currentTech).cost)
        {
            techProgress = 0;
            researchUnlock.Add(currentTech);
            currentTech = -1;
        }
    }

    #endregion

    #region Colony development

    private int CheckCritical()
    {
        if (CheckBalance() == 6) return 0;

        int doAction = 9;

        if (CheckBalance() == 4 && doAction == 9) doAction = ChooseBuild(new int[3] { 14, 13, 12 });
        if (CheckBalance() == 5 && doAction == 9) doAction = ChooseBuild(new int[2] { 2, 1 });
        if (CheckBalance() == 0 && doAction == 9) doAction = ChooseBuild(new int[3] { 6, 5, 4 });
        if (CheckBalance() == 1 && doAction == 9) doAction = ChooseBuild(new int[2] { 9, 10 });
        if (CheckBalance() == 2 && doAction == 9) doAction = ChooseBuild(new int[1] { 16 });
        if (CheckBalance() == 3 && doAction == 9) doAction = ChooseBuild(new int[2] { 2, 1 });

        if (doAction == 10) return 1;
        else return 2;
    }

    private int DevelopColony()
    {
        if(strategy == 0)
        {
            if(unitNbr * 10 + 10 < buildNbr + previewNbr)
            {
                return CreateUnit();
            }
            else
            {
                if(defenseNbr * 8 < buildNbr - defenseNbr)
                {
                    int haveBuildDefense = ChooseBuild(new int[3] { 34, 33, 15 });

                    if (haveBuildDefense == 9)
                        return ChooseBuild();
                    else
                        return 10;
                }
                else
                {
                    return ChooseBuild();
                }
            }
        }
        else if(strategy == StrategyType.Defensive)
        {
            if(unitNbr * 4 + 7 < buildNbr + previewNbr)
            {
                return CreateUnit();
            }
            else
            {
                if (defenseNbr * 4 < buildNbr - defenseNbr)
                {
                    int haveBuildDefense = ChooseBuild(new int[3] { 34, 33, 15 });

                    if (haveBuildDefense == 9)
                        return ChooseBuild();
                    else
                        return 10;
                }
                else
                {
                    return ChooseBuild();
                }
            }
        }
        else if(strategy == StrategyType.Agressive)
        {
            if(unitNbr * 2 + 3 < buildNbr + previewNbr)
            {
                Debug.Log("  [INFO:EnemyMotor] Trying to create units...");
                return CreateUnit();
            }
            else
            {
                if (defenseNbr * 6 < buildNbr - defenseNbr)
                {
                    int haveBuildDefense = ChooseBuild(new int[3] { 34, 33, 15 });

                    if (haveBuildDefense == 9)
                        return ChooseBuild();
                    else
                        return 10;
                }
                else
                {
                    if(unitNbr >= 10)
                    {
                        Debug.Log("  [INFO:EnemyMotor] Attack order sended.");
                        return AttackWithUnits();
                    }
                    else
                    {
                        return ChooseBuild();
                    }
                }
            }
        }
        else
        {
            return 10;
        }
    }

    #endregion

    #region Units

    private int CreateUnit()
    {
        try
        {
            foreach (FactoryMotor f in factories)
            {
                for (int i = f.unitsAvailable.Length - 1; i > 0; i--)
                {
                    int haveRessources = HaveEnoughRessource(unitData[f.unitsAvailable[i]]);

                    if (haveRessources == 3)
                    {
                        if (f.queue[4].name == "")
                        {
                            f.AddQueue(i, false, true);
                            Debug.Log("  [INFO:EnemyMotor] Unit has been qeueed. [" + i + "]");
                            return 10;
                        }
                    }
                }
            }

            Debug.Log("  [INFO:EnemyMotor] Can't make unit.");

            if(factories.Count >= 1)
            {
                return ChooseBuild();
            }
            else
            {
                return ChooseBuild(new int[2] { 8, 7 });
            }
        }
        catch
        {
            Debug.Log("  [WARN:EnemyMotor] Can't make another unit.");

            if (factories.Count >= 1)
            {
                return ChooseBuild();
            }
            else
            {
                return ChooseBuild(new int[2] { 8, 7 });
            }
        }
    }

    private int AttackWithUnits()
    {
        if(units.Count > 0)
        {
            foreach (Unit u in units)
            {
                u.agressivity = 2; //? Rend les unités agressives envers tout les ennemies de la colonie
            }

            return 10;
        }
        else
        {
            return 9;
        }        
    }

    public void AddUnitToList(Unit unit)
    {
        units.Add(unit);
    }

    public void RemoveUnitToList(Unit unit)
    {
        units.Remove(unit);
    }

    public void AddFactoryToList(FactoryMotor fact)
    {
        factories.Add(fact);
    }

    public void RemoveFactoryToList(FactoryMotor fact)
    {
        factories.Remove(fact);
    }

    #endregion

    #region Construction

    private int ChooseBuild()
    {
        bool haveFind = false;
        int currentId = 0;
        m_errorPropagation = 0;

        //Debug.Log("  [INFO:EnemyMotor] Choosing build... (" + enemyName + ") Error propagation : " + m_errorPropagation);

        if (strategy == 0)
        {
            int[] buildsId = new int[2] { 2, 1 };

            while (!haveFind)
            {
                foreach(int id in buildsId)
                {
                    bool haveTech = true;

                    foreach (int techId in buildData[id].techsNeeded)
                    {
                        if(!researchUnlock.Contains(techId))
                        {
                            haveTech = false;
                            break;
                        }
                    }

                    if (haveTech && HaveEnoughRessource(buildData[id]) == 6)
                    {
                        currentId = id;
                        haveFind = true;
                        break;
                    }
                }

                if (haveFind)
                {
                    return Build(buildData[currentId]);
                }
                else
                {
                    int res = HaveEnoughRessource(buildData[buildsId[buildsId.Length - 1]]);

                    if (res == 0) { return ChooseBuild(new int[3] { 7, 6, 5 }); }
                    else if (res == 1) { return ChooseBuild(new int[5] { 29, 28, 15, 14, 13 }); }
                    else if (res == 3) { return ChooseBuild(new int[1] { 10 }); }
                    else if (res == 4) { return ChooseBuild(new int[1] { 16 }); }
                }
            }
        }
        else if(strategy == StrategyType.Defensive)
        {
            int[] buildsId = new int[4] { 15, 17, 10, 9 };

            while(!haveFind)
            {
                foreach (int id in buildsId)
                {
                    bool haveTech = true;

                    foreach (int techId in buildData[id].techsNeeded)
                    {
                        if(!researchUnlock.Contains(techId))
                        {
                            haveTech = false;
                            break;
                        }
                    }

                    if(haveTech && HaveEnoughRessource(buildData[id]) == 6)
                    {
                        currentId = id;
                        haveFind = true;
                        break;
                    }
                }

                if (haveFind)
                {
                    return Build(buildData[currentId]);
                }
                else
                {
                    int res = HaveEnoughRessource(buildData[buildsId[buildsId.Length - 1]]);

                    if (res == 0) { return ChooseBuild(new int[3] { 7, 6, 5 }); }
                    else if (res == 1) { return ChooseBuild(new int[5] { 29, 28, 15, 14, 13 }); }
                    else if (res == 3) { return ChooseBuild(new int[1] { 10 }); }
                    else if (res == 4) { return ChooseBuild(new int[1] { 16 }); }
                }
            }
        }
        else if (strategy == StrategyType.Agressive)
        {
            int[] buildsId;

            if ((colonyStats.colonist / 3) * 2 < colonyStats.maxColonist)
                buildsId = new int[2] { 8, 7 };
            else
                buildsId = new int[2] { 15, 17 };

            while (!haveFind)
            {
                foreach (int id in buildsId)
                {
                    bool haveTech = true;

                    foreach (int techId in buildData[id].techsNeeded)
                    {
                        if (!researchUnlock.Contains(techId))
                        {
                            haveTech = false;
                            break;
                        }
                    }

                    if (haveTech && HaveEnoughRessource(buildData[id]) == 6)
                    {
                        currentId = id;
                        haveFind = true;
                        break;
                    }
                }

                if (haveFind)
                {
                    return Build(buildData[currentId]);
                }
                else
                {
                    int res = HaveEnoughRessource(buildData[buildsId[buildsId.Length - 1]]);

                    if (res == 0) { return ChooseBuild(new int[3] { 7, 6, 5 }); }
                    else if (res == 1) { return ChooseBuild(new int[5] { 29, 28, 15, 14, 13 }); }
                    else if (res == 3) { return ChooseBuild(new int[1] { 10 }); }
                    else if (res == 4) { return ChooseBuild(new int[1] { 16 }); }
                }
            }
        }

        return 9;
    }

    private int ChooseBuild(int[] buildsId)
    {
        bool haveFind = false;
        int currentId = 0;
        m_errorPropagation++;

        if(m_errorPropagation > 6)
        {
            string str = "<b>[INFO:EnemyMotor=" + colonyStats.colony.side + "] Buildings: </b> ";

            foreach (int i in buildsId)
            {
                str += i + " / ";
            }

           Debug.Log(str);

            m_errorPropagation = 0;
            Debug.Log("<b><color=#CD7F00>[WARN:EnemyMotor=" + colonyStats.colony.side + "] Enemy " + enemyName + " is in error. (infinite loop)</color></b>");
            return 10;
        }

        while (!haveFind)
        {
            foreach (int id in buildsId)
            {
                bool haveTech = true;
                Building current = buildData[id];
                int[] techs = current.techsNeeded;

                foreach(int tech in techs)
                {
                    if (!researchUnlock.Contains(tech)) haveTech = false;
                }

                haveTech = true; // A ENLEVER QUAND LE SYSTEME DE RECHERCHES POUR LES ENNEMIES AURA ETE REFAIT !!!!!!!

                if (haveTech)
                {
                    if (HaveEnoughRessource(current) == 6)
                    {
                        currentId = id;
                        haveFind = true;
                        break;
                    }
                }
            }

            if (haveFind)
            {
                if (Build(buildData[currentId]) == 10)
                {
                    return 10;
                }
                else
                {
                    return 9;
                }
            }
            else
            {
                int res = HaveEnoughRessource(buildData[buildsId[buildsId.Length - 1]]);
                int isFinished = 9;
                int ressID = -1;

                if (res == 0) { isFinished = ChooseBuild(new int[3] { 7, 6, 5 }); ressID = 0; }
                else if (res == 1) { isFinished = ChooseBuild(new int[5] { 29, 28, 15, 14, 13 }); ressID = 1; }
                else if (res == 2) { isFinished = ChooseBuild(new int[2] { 2, 1 }); ressID = 2; }
                else if (res == 3) { isFinished = ChooseBuild(new int[1] { 10 }); ressID = 3; }
                else if (res == 4) { isFinished = ChooseBuild(new int[1] { 16 }); ressID = 4; }
                else if (res == 5) { isFinished = ChooseBuild(new int[2] { 2, 1 }); ressID = 5; }

                if(isFinished == 10)
                {
                    return isFinished;
                }
                else if(ressID != -1)
                {
                    Debug.Log("<b><color=#CD7F00>[WARN:EnemyMotor=" + colonyStats.colony.side + "] Not enough ressource " + ressID + "</color></b>");
                    return ressID;
                }
                else
                {
                    return 10;
                }
            }
        }

        return 9;
    }

    private int Build(Building bld)
    {
        if (physicalPoint.GetComponent<Collider>() == null && !CheckPoint()) return 9;

        Preview prMtr = bld.preview.GetComponent<Preview>();

        Vector2 posTmp = new Vector2(Random.Range(-influenceRadius, influenceRadius), Random.Range(-influenceRadius, influenceRadius));
        Vector3 pos = new Vector3(posTmp.x, 0, posTmp.y) + transform.position;

        for(int lim = 0; lim < maxSubAction; lim++)
        {
            if(PositionIsValid(prMtr.gameObject, pos))
            {
                GameObject go = Instantiate(bld.preview, pos, Quaternion.identity);
                Preview mtr = go.GetComponent<Preview>(); //On récupère le script instancié et non celui de la prefab (pMtr)
                mtr.side = colonyStats.colony.side;

                influenceRadius++;
                previewNbr++;
                return 10;
            }
            else
            {
                posTmp = new Vector2(Random.Range(-influenceRadius, influenceRadius), Random.Range(-influenceRadius, influenceRadius));
                pos = new Vector3(posTmp.x, 0, posTmp.y) + transform.position;
            }
        }

        Debug.Log("[INFO:EnemyMotor] Can't find a correct position.");
        return 9;
    }

    private bool PositionIsValid(GameObject pr, Vector3 position)
    {
        return physicalPoint.GetComponent<PhysicalPointMotor>().isValid(pr, position);
    }

    #endregion

    #region Previsualisation des profits

    public void RemovePreview(int id)
    {
        Building cur = buildData[id];

        pre_colonist -= cur.colonist;
        pre_energy -= cur.energy;
        pre_foodOutput -= cur.foodOutput;
        pre_maxColonist -= cur.maxColonist;
        pre_profit -= cur.profit;
        pre_rigolyteOutput -= cur.rigolyteOutput;
        pre_bioPlastiqueOutput -= cur.bioPlastiqueOutput;
        pre_research -= cur.research;
    }

    public void AddPreview(int id)
    {
        Building cur = buildData[id];

        pre_colonist += cur.colonist;
        pre_energy += cur.energy;
        pre_foodOutput += cur.foodOutput;
        pre_maxColonist += cur.maxColonist;
        pre_profit += cur.profit;
        pre_rigolyteOutput += cur.rigolyteOutput;
        pre_bioPlastiqueOutput += cur.bioPlastiqueOutput;
        pre_research += cur.research;
    }

    #endregion

    #region Balance
    /*
    public void SubstractRessource(int _colonist, int _maxColonist, int _energy, int _money, int moneyOut, float _rigolyte, float rigolyteOut, float _bioPlastique, float bioPlastiqueOut, float _food, float foodOut, float _research)
    {
        colonyStats.colonist += _colonist;
        colonyStats.maxColonist -= _maxColonist;
        colonyStats.energy -= _energy;
        colonyStats.money -= _money;
        colonyStats.profit -= moneyOut;
        colonyStats.regolith -= _rigolyte;
        colonyStats.regolithOutput -= rigolyteOut;
        colonyStats.bioPlastique -= _bioPlastique;
        colonyStats.bioPlastiqueOutput -= bioPlastiqueOut;
        colonyStats.food -= _food;
        colonyStats.foodOutput -= foodOut;
        colonyStats.research -= _research;
    }

    public void AddRessource(int colonist, int maxColonist, int energy, int money, float regolith, float bioplastic, float food, float _research)
    {
        colonyStats.colonist -= colonist;
        colonyStats.maxColonist += maxColonist;
        colonyStats.energyGain += _energy;
        colonyStats.money += _money;
        colonyStats.profit += moneyOut;
        colonyStats.regolith += _rigolyte;
        colonyStats.regolithOutput += rigolyteOut;
        colonyStats.bioPlastique += _bioPlastique;
        colonyStats.bioPlastiqueOutput += bioPlastiqueOut;
        colonyStats.food += _food;
        colonyStats.foodOutput += foodOut;
        colonyStats.research += _research;
    }
    
    private void UpdateOutput()
    {

    }

    private void GiveRessources(int money, int energy, float regolith, float bioplastic, float food)
    {
        colonyStats.money += money;
        colonyStats.energy += energy;
        colonyStats.regolith += regolith;
        colonyStats.bioPlastique += bioplastic;
        colonyStats.food += food;
    }*/

    #endregion

    #region Ressources methods

    public void Output()
    {
        colonyStats.money += colonyStats.profit;
        colonyStats.regolith += colonyStats.regolithOutput;
        colonyStats.bioPlastique += colonyStats.bioPlastiqueOutput;
        colonyStats.food += colonyStats.foodOutput;
        colonyStats.energy += colonyStats.energyOutput;

        if (currentTech == -1 || currentTech == -2) return;

        techProgress += colonyStats.research;
    }

    #region Add / remove output

    public void AddOutput(int _energy, int _money, float _regolith, float _bioplastic, float _food, float _research)
    {
        if (_energy > 0) colonyStats.energyGain += _energy;
        else colonyStats.energyLoss -= _energy;

        if (_money > 0) colonyStats.moneyGain += _money;
        else colonyStats.moneyLoss -= _money;

        if (_regolith > 0) colonyStats.regolithGain += _regolith;
        else colonyStats.regolithLoss -= _regolith;

        if (_bioplastic > 0) colonyStats.bioPlastiqueGain += _bioplastic;
        else colonyStats.bioPlastiqueLoss -= _bioplastic;

        if (_food > 0) colonyStats.foodGain += _food;
        else colonyStats.foodLoss -= _food;

        colonyStats.research += _research;

        colonyStats.CalculateOutput();
    }

    public void RemoveOutput(int _energy, int _money, float _regolith, float _bioplastic, float _food, float _research)
    {
        if (_energy > 0) colonyStats.energyGain -= _energy;
        else colonyStats.energyLoss += _energy;

        if (_money > 0) colonyStats.moneyGain -= _money;
        else colonyStats.moneyLoss += _money;

        if (_regolith > 0) colonyStats.regolithGain -= _regolith;
        else colonyStats.regolithLoss += _regolith;

        if (_bioplastic > 0) colonyStats.bioPlastiqueGain -= _bioplastic;
        else colonyStats.bioPlastiqueLoss += _bioplastic;

        if (_food > 0) colonyStats.foodGain -= _food;
        else colonyStats.foodLoss += _food;

        colonyStats.research -= _research;

        colonyStats.CalculateOutput();
    }

    #endregion

    #region Add / remove ressources, workers and storage

    public void AddRessources(int _energy, int _money, float _regolith, float _bioplastic, float _food)
    {
        colonyStats.energy += _energy;
        colonyStats.money += _money;
        colonyStats.regolith += _regolith;
        colonyStats.bioPlastique += _bioplastic;
        colonyStats.food += _food;
    }

    public void RemoveRessources(int _energy, int _money, float _regolith, float _bioplastic, float _food)
    {
        colonyStats.energy -= _energy;
        colonyStats.money -= _money;
        colonyStats.regolith -= _regolith;
        colonyStats.bioPlastique -= _bioplastic;
        colonyStats.food -= _food;
    }

    public void AddSettlers(int _workers, int _colonist)
    {
        colonyStats.colonist += _workers;
        colonyStats.maxColonist += _colonist;
    }

    public void RemoveSettlers(int _workers, int _colonist)
    {
        colonyStats.colonist += _workers;
        colonyStats.maxColonist += _colonist;
    }

    public void ManageStorage(int _energy, float _regolith, float _bioplastic, float _food)
    {
        colonyStats.energyStorage += _energy;
        colonyStats.regolithStock += _regolith;
        colonyStats.bioPlasticStock += _bioplastic;
        colonyStats.foodStock += _food;
    }

    #endregion

    public void GiveRessources(string arg)
    {
        string[] args = arg.Split(' ');

        if (args[0] == "-m")
        {
            try
            {
                int amount = int.Parse(args[1]);
                colonyStats.money += amount;
            }
            catch
            {

            }
        }
    }

    public void GiveRessources(RewardType reward, int amount)
    {
        if (reward == RewardType.Money)
        {
            colonyStats.money += amount;
        }
        else if (reward == RewardType.Regolith)
        {
            colonyStats.regolith += amount;
        }
        else if (reward == RewardType.Bioplastic)
        {
            colonyStats.bioPlastique += amount;
        }
        else if (reward == RewardType.Food)
        {
            colonyStats.food += amount;
        }
        else if (reward == RewardType.Energy)
        {
            colonyStats.energy += amount;
        }
    }

    public void AnticipateRessources(int energy)
    {
        colonyStats.anticipatedEnergy += energy;
    }

    public void RemoveAnticipateRessources(int energy)
    {
        colonyStats.anticipatedEnergy -= energy;
    }

    #region Have ressources

    public bool haveEnoughRessource(int _colonist, int _energy, int _money, float _regolith, float _bioPlastique, float _food)
    {
        if (colonyStats.maxColonist < colonyStats.colonist + _colonist) return false;
        if (colonyStats.energy + colonyStats.anticipatedEnergy < _energy) return false;
        if (colonyStats.money < _money) return false;
        if (colonyStats.regolith < _regolith) return false;
        if (colonyStats.bioPlastique < _bioPlastique) return false;
        if (colonyStats.food < _food) return false;

        return true;
    }

    public List<int> HaveRessources(int _colonist, int _energy, int _money, float _regolith, float _bioPlastique, float _food)
    {
        List<int> ints = new List<int>();

        if (colonyStats.maxColonist < colonyStats.colonist + _colonist) ints.Add(0);
        if (colonyStats.energyOutput + colonyStats.anticipatedEnergy + _energy < 0) ints.Add(1);
        if (colonyStats.money < _money) ints.Add(2);
        if (colonyStats.regolith < _regolith) ints.Add(3);
        if (colonyStats.bioPlastique < _bioPlastique) ints.Add(4);
        if (colonyStats.food < _food) ints.Add(5);

        return ints;
    }

    private int HaveEnoughRessource(Units u)
    {
        if (u.place + colonyStats.colonist > colonyStats.maxColonist) return 2;
        if (u.money > colonyStats.money) return 1;
        if (u.food != 0) { if (u.food > colonyStats.food) return 0; }

        return 3;
    }

    private int HaveEnoughRessource(Building bld)
    {
        if (bld.colonist != 0) { if (bld.colonist + colonyStats.colonist > colonyStats.maxColonist) return 5; }

        if (bld.energy < 0)
        {
            if (bld.energy + colonyStats.energyOutput + colonyStats.anticipatedEnergy < 0)
                return 1;
        }

        if (bld.money < 0) { if (bld.money > colonyStats.money) return 2; }
        if (bld.regolith < 0) { if (bld.regolith > colonyStats.regolith) return 3; }
        if (bld.bioPlastique < 0) { if (bld.bioPlastique > colonyStats.bioPlastique) return 4; }
        if (bld.food < 0) { if (bld.food > colonyStats.food) return 0; }

        return 6;
    }

    #endregion

    #region Balance and critical

    private int CheckBalance()
    {
        if (colonyStats.energy + pre_energy + colonyStats.energyOutput <= 10) return 4;
        if (colonyStats.foodOutput + pre_foodOutput <= scaleCritical) return 0;
        if (colonyStats.regolithOutput + pre_rigolyteOutput <= scaleCritical) return 1;
        if (colonyStats.bioPlastiqueOutput + pre_bioPlastiqueOutput <= scaleCritical) return 2;
        if (colonyStats.profit + pre_profit <= 50) return 3;
        if (colonyStats.colonist + 15 + pre_colonist >= colonyStats.maxColonist + pre_maxColonist) return 5;

        return 6;
    }

    private int CheckCriticalBalance()
    {
        int critics = 0;
        if (colonyStats.foodOutput + pre_foodOutput < 0) critics++;
        if (colonyStats.regolithOutput + pre_rigolyteOutput < 0) critics++;
        if (colonyStats.bioPlastiqueOutput + pre_bioPlastiqueOutput < 0) critics++;
        if (colonyStats.profit + pre_profit < 0) critics++;
        if (colonyStats.energy + pre_energy + colonyStats.energyOutput <= 0) critics++;

        return critics;
    }

    #endregion

    #endregion

    #region Enemy failure

    public void EnemyFailure()
    {
        Debug.Log(" <b>[INFO:EnemyMotor] " + enemyName + " has been destroyed!</b>");

        manager.RemoveEnemyColony();
        DestroyColonyBuilding();

        manager.Notify("A colony is in failure !", manager.Traduce("The colony ") + enemyName + manager.Traduce(" has been destroyed by itself."), null, new Color(0, 0, 0, 1), 3.5f);

        manager.DeleteGameObjectOfTagList(gameObject);
        Destroy(gameObject);
    }

    public void DestroyEnemy()
    {
        if (disableEnemy) return;

        manager.Notify("A colony has been destroyed !", manager.Traduce("The colony ") + enemyName + manager.Traduce(" has been destroyed by another colony."), null, new Color(1, .66f, 0, 1), 3.5f);
        Debug.Log(" <b>[INFO:EnemyMotor] " + enemyName + " has been destroyed!</b>");

        manager.RemoveEnemyColony();
        DestroyColonyBuilding();

        manager.DeleteGameObjectOfTagList(gameObject);
        Destroy(gameObject);
    }

    private void DestroyColonyBuilding()
    {
        List<GameObject> allGoOfEnemy = new List<GameObject>();
        List<GameObject> allGo = new List<GameObject>();
        allGo.AddRange(manager.FindTagList(Tag.Building));
        allGo.AddRange(manager.FindTagList(Tag.Unit));
        allGo.AddRange(manager.FindTagList(Tag.Preview));

        try
        {
            foreach (Unit u in units)
            {
                allGoOfEnemy.Add(u.gameObject);
            }
        }
        catch { }

        foreach (GameObject go in allGo)
        {
            if (go != gameObject)
            {
                if (go.GetComponent<Entity>().side == colonyStats.colony.side)
                    allGoOfEnemy.Add(go);
            }
        }

        for (int i = 0; i < allGoOfEnemy.Count; i++)
        {
            manager.DeleteGameObjectOfTagList(allGoOfEnemy[i]);
            Destroy(allGoOfEnemy[i]);
        }
    }

    #endregion

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, new Vector3(influenceRadius * 2, 25, influenceRadius * 2));
    }
}
