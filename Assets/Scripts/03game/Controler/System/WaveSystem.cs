using System.Collections.Generic;
using UnityEngine;

public class WaveSystem : MonoBehaviour {

    [Header("Enemie wave")]
    [SerializeField] private EnemieArmies[] waveArmies;
    [Space(5)]
    public int probablity;
    public GameObject waveParent;

    [Header("Wave spawns")]
    public Transform[] spawns;

    private MoonManager manager;
    private QuestManager quest;

    private System.Random randomizer;
    private System.Random randomizerBase;

    public int currentWaveIndex;

    private bool randomIsInit;
    private Units[] unitsData;

    private void Awake()
    {
        manager = GetComponent<MoonManager>();
        quest = GetComponent<QuestManager>();

        unitsData = manager.unitData;

        InitRandom();
        currentWaveIndex = -1;
    }

    private void InitRandom()
    {
        if (randomIsInit) return;

        randomIsInit = true;

        randomizer = new System.Random(manager.seed);
        randomizerBase = randomizer;
    }

    public void EngageWave()
    {
        if (probablity == 0) return;

        if(manager.day == manager.dateNextWave.x && manager.seconds / 60 == manager.dateNextWave.y)
        {
            try
            {
                AchieveWave();
            }
            catch
            {
                Debug.Log("  [INFO:WaveSystem] Can't achieve wave!");
            }
        }
        else
        {
            Debug.Log("  [INFO:WaveSystem] Wave not ready!");
        }
    }

    private void AchieveWave()
    {
        if (probablity == 0) return;

        currentWaveIndex++;
        EnemieArmies currentArmies = waveArmies[currentWaveIndex];
        Vector3 wavePosition = spawns[Random.Range(0, spawns.Length - 1)].position;

        int unitNumber = 0;

        foreach(EnemieUnits e in currentArmies.units)
        {
            unitNumber += e.numberOfUnit;
        }

        quest.NewKillQuest(
            new KillQuest(
                "Defeat wave",
                "We're being invaded! Repel this enemy invasion.",
                EntityType.Unit,
                GetUnitID(currentArmies.units),
                GetUnitCount(currentArmies.units),
                new QuestReward[0] { }
                )
            );

        foreach (EnemieUnits unit in currentArmies.units)
        {
            Vector3 unitPosition = new Vector3(wavePosition.x - Mathf.Min(unit.numberOfUnit, currentArmies.maxUnitPerLigne) / 2 * currentArmies.spacing, wavePosition.y, wavePosition.z);
            int unitSpawned = 0;

            for(int y = 0; y < unit.numberOfUnit; y++)
            {
                for (int x = 0; x < currentArmies.maxUnitPerLigne; x++)
                {
                    GameObject go = Instantiate(unitsData[unit.identityOfUnit].model);
                    Unit um = go.GetComponent<Unit>();

                    go.transform.position = unitPosition;

                    um.side = 999;
                    um.agressivity = 3;

                    unitPosition.x += currentArmies.spacing;
                    unitSpawned++;

                    if (unitSpawned >= unit.numberOfUnit) break;
                }

                unitPosition.x = wavePosition.x - Mathf.Min(unit.numberOfUnit, currentArmies.maxUnitPerLigne) / 2 * currentArmies.spacing;
                unitPosition.z -= currentArmies.interligneSpacing;

                if (unitSpawned >= unit.numberOfUnit) break;
            }
        }

        if (currentArmies.waveEvent.name != "") manager.InstantiateEvent(currentArmies.waveEvent);

        manager.Notify(manager.Traduce("03_notif_wave"), priority: 3);
        Debug.Log("  <b>[INFO] Enemy wave successfully spawned!</b>");
        UpdateDateNextWave();
    }

    public void NewWave(int _waveIndex, bool forced=false)
    {
        if (probablity == 0 && !forced) return;

        EnemieArmies currentArmies = waveArmies[_waveIndex];
        Vector3 wavePosition = spawns[randomizer.Next(0, spawns.Length)].position;
        currentWaveIndex++;

        foreach (EnemieUnits unit in currentArmies.units)
        {
            Vector3 unitPosition = new Vector3(wavePosition.x - Mathf.Min(unit.numberOfUnit, currentArmies.maxUnitPerLigne) / 2 * currentArmies.spacing, wavePosition.y, wavePosition.z);
            int unitSpawned = 0;

            for (int y = 0; y < unit.numberOfUnit; y++)
            {
                for (int x = 0; x < currentArmies.maxUnitPerLigne; x++)
                {
                    GameObject go = Instantiate(unitsData[unit.identityOfUnit].model);
                    Unit um = go.GetComponent<Unit>();

                    go.transform.position = unitPosition;

                    um.side = 999;
                    um.agressivity = 3;

                    unitPosition.x += currentArmies.spacing;
                    unitSpawned++;

                    if (unitSpawned >= unit.numberOfUnit) break;
                }

                unitPosition.x = wavePosition.x - Mathf.Min(unit.numberOfUnit, currentArmies.maxUnitPerLigne) / 2 * currentArmies.spacing;
                unitPosition.z -= currentArmies.interligneSpacing;

                if (unitSpawned >= unit.numberOfUnit) break;
            }
        }

        if (currentArmies.waveEvent.name != "") manager.InstantiateEvent(currentArmies.waveEvent);

        manager.Notify(manager.Traduce("03_notif_wave"), priority: 3);
        Debug.Log("  <b>[INFO] Enemy wave successfully spawned!</b>");
    }

    public void ForcedNewWave(EnemieArmies currentArmies)
    {
        Vector3 wavePosition = spawns[randomizer.Next(0, spawns.Length)].position;
        currentWaveIndex++;

        foreach (EnemieUnits unit in currentArmies.units)
        {
            Vector3 unitPosition = new Vector3(wavePosition.x - Mathf.Min(unit.numberOfUnit, currentArmies.maxUnitPerLigne) / 2 * currentArmies.spacing, wavePosition.y, wavePosition.z);
            int unitSpawned = 0;

            for (int y = 0; y < unit.numberOfUnit; y++)
            {
                for (int x = 0; x < currentArmies.maxUnitPerLigne; x++)
                {
                    GameObject go = Instantiate(unitsData[unit.identityOfUnit].model);
                    Unit um = go.GetComponent<Unit>();

                    go.transform.position = unitPosition;

                    um.side = 999;
                    um.agressivity = 3;

                    unitPosition.x += currentArmies.spacing;
                    unitSpawned++;

                    if (unitSpawned >= unit.numberOfUnit) break;
                }

                unitPosition.x = wavePosition.x - Mathf.Min(unit.numberOfUnit, currentArmies.maxUnitPerLigne) / 2 * currentArmies.spacing;
                unitPosition.z -= currentArmies.interligneSpacing;

                if (unitSpawned >= unit.numberOfUnit) break;
            }
        }

        if (currentArmies.waveEvent != null && currentArmies.waveEvent.name != "") manager.InstantiateEvent(currentArmies.waveEvent);

        manager.Notify(manager.Traduce("03_notif_wave"), priority: 3);
        Debug.Log("  <b>[INFO] Enemy wave successfully spawned!</b>");
    }

    public void UpdateDateNextWave()
    {
        if (probablity == 0) return;

        InitRandom();

        int seconds;
        int wave = -1;
        System.Random random = randomizerBase;

        for (int i = 1800; true; i += 600) //? 1800 <=> 30 heures //? 600 <=> 10 heures
        {
            if (random.Next(0, 100) <= probablity)
            {
                wave++;

                if (wave == currentWaveIndex + 1)
                {
                    seconds = i;
                    break;
                }
            }
        }

        int sol = seconds / 20160 - manager.day;
        int hours = (seconds - (sol * 20160)) / 60;

        Vector2 date = new Vector2(sol, hours);

        manager.dateNextWave = date;
        manager.UpdateTimeUntilNextWave();
    }

    private int[] GetUnitID(List<EnemieUnits> units)
    {
        int[] ids = new int[units.Count];

        for (int i = 0; i < units.Count; i++)
        {
            ids[i] = units[i].identityOfUnit;
        }

        return ids;
    }

    private int[] GetUnitCount(List<EnemieUnits> units)
    {
        int[] count = new int[units.Count];

        for (int i = 0; i < units.Count; i++)
        {
            count[i] = units[i].numberOfUnit;
        }

        return count;
    }
}
