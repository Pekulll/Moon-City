using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Buildings))]
public class TrainingArea : MonoBehaviour
{
    public List<int> unitID;

    public int currentTrainingTime;
    public int currentUnitID;

    public List<int> queue;

    private MoonManager manager;
    private QuestManager questManager;
    private Buildings currentEntity;

    private Transform exitPoint;
    public Vector3 rallyPoint;

    private bool isInitialize;

    private void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        if (isInitialize) return;
        if (unitID.Count == 0) Destroy(this);
        isInitialize = true;

        manager = GameObject.Find("Manager").GetComponent<MoonManager>();
        questManager = GameObject.Find("Manager").GetComponent<QuestManager>();
        currentEntity = GetComponent<Buildings>();
        exitPoint = transform.Find("ExitPoint");

        queue = new List<int>();
        currentTrainingTime = 0;
        currentUnitID = -1;
        rallyPoint = exitPoint.position;
    }

    private IEnumerator Train()
    {
        WaitForSeconds delay = new WaitForSeconds(.1f);

        while(currentTrainingTime < manager.unitData[currentUnitID].time)
        {
            yield return delay;
            currentTrainingTime += 1;
            manager.UpdateFactoryQueue(currentEntity);
        }

        Units unit = manager.unitData[currentUnitID];

        if(unit.model != null)
        {
            GameObject u = Instantiate(unit.model, exitPoint.position, Quaternion.identity) as GameObject;
            Unit motor = u.GetComponent<Unit>();

            motor.side = currentEntity.side;
            motor.Initialize();
            motor.AddOrder(new Order(OrderType.Position, rallyPoint));

            questManager.CheckForBuildQuest(currentUnitID, currentEntity.side, EntityType.Unit);
        }

        Dequeue();
        yield return new WaitForEndOfFrame();

        if(queue.Count != 0)
        {
            InitializeTraining();
        }
    }

    public void InitializeTraining()
    {
        if (queue.Count == 0 || currentUnitID != -1) return;

        currentUnitID = queue[0];
        StartCoroutine(Train());
    }

    private void Dequeue()
    {
        queue.RemoveAt(0);
        currentTrainingTime = 0;
        currentUnitID = -1;
    }

    public void Enqueue(int id, bool isOnLoad)
    {
        if(queue.Count < 5)
        {
            queue.Add(id);

            if (!isOnLoad && !Enemy())
            {
                Units u = manager.unitData[id];

                manager.RemoveRessources(0, u.money, 0, 0, u.food);
                manager.AddOutput(0, u.moneyOut, 0, 0, u.foodOut, 0);
                manager.AddSettlers(u.place, 0);
            }
            else if (!isOnLoad)
            {
                currentEntity.RemoveOwnerResources(manager.unitData[id]);
            }

            InitializeTraining();
        }
        else
        {
            manager.Notify(manager.Traduce("03_notif_factory_queuefull"));
        }
    }

    public void Enqueue(int localID)
    {
        if (unitID.Count <= localID) return;

        Units u = manager.unitData[unitID[localID]];

        if (manager.HaveEnoughResource(u.place, 0, u.money, 0, 0, u.food))
        {
            Enqueue(unitID[localID], false);
        }
        else
        {
            manager.Notify(manager.Traduce("03_notif_factory_noresources"));
        }
    }

    public void SetRallyPoint(Vector3 position)
    {
        rallyPoint = position;
    }

    private bool Enemy() { return currentEntity.side != manager.side; }
}
