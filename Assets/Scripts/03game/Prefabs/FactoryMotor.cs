using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Buildings))]
public class FactoryMotor : MonoBehaviour
{
    private MoonManager manager;
    public List<Units> queue = new List<Units>();
    public List<int> unitsAvailable;

    public float time = 0;
    public float maxTime = 0;

    private Units[] unitData;

    private bool isEnemyFactory;

    private void Start()
    {
        Init();
    }

    public void Init()
    {
        if (unitsAvailable.Count == 0) Destroy(this);

        manager = GameObject.Find("Manager").GetComponent<MoonManager>();
        unitData = manager.unitData;
        queue = new List<Units>(5);

        for(int i = 0; i < 5; i++)
        {
            if (queue.Count >= 5) break;
            else queue.Add(new Units());
        }
    }

    public void AddQueue(int index, bool isOnLoad, bool isEnemy = false)
    {
        if (queue[4].name != "")
        {
            manager.Notify(manager.Traduce("03_notif_factory_queuefull"));
            return;
        }

        if (manager == null) manager = GameObject.Find("Manager").GetComponent<MoonManager>();

        if (!isEnemy)
        {
            if (manager.HaveEnoughResource(unitData[index].place, 0, unitData[index].money, 0, 0, unitData[index].food) && !isOnLoad)
            {
                AddToQueue(index, false, isEnemy);
            }
            else if (isOnLoad)
            {
                AddToQueue(index, isOnLoad, isEnemy);
            }
            else
            {
                manager.Notify(manager.Traduce("03_notif_factory_noresources"));
                return;
            }
        }
        else
        {
            AddToQueue(index, false, isEnemy);
        }
        
    }

    private void AddToQueue(int index, bool isOnLoad, bool isEnemy)
    {
        for(int i = 0; i < queue.Count; i++)
        {
            if(queue[i].name == "")
            {
                queue[i] = unitData[index];

                if (!isOnLoad && !isEnemy)
                {
                    manager.RemoveRessources(0, queue[i].money, 0, 0, queue[i].food);
                    manager.AddOutput(0, queue[i].moneyOut, 0, 0, queue[i].foodOut, 0);
                    manager.AddSettlers(queue[i].place, 0);
                }

                manager.UpdateFactoryQueue(GetComponent<Entity>());

                if (i == 0) StartCoroutine(Fabric());

                return;
            }
        }

        manager.Notify(manager.Traduce("03_notif_factory_queuefull"));
    }

    private IEnumerator Fabric()
    {
        if (queue[0].name == "") yield return null;

        Units cur = queue[0];
        maxTime = cur.time;

        while (time <= maxTime)
        {
            yield return new WaitForSeconds(0.1f);
            time += 0.1f;
            manager.UpdateFactoryQueue(GetComponent<Entity>());
        }

        if(cur.model != null)
        {
            GameObject go = Instantiate(cur.model, gameObject.transform.position + new Vector3(0, 2, 10), Quaternion.identity) as GameObject;
            int side = GetComponent<Entity>().side;

            go.GetComponent<Unit>().side = side;

            FindObjectOfType<QuestManager>().CheckForBuildQuest(cur.identity, side, EntityType.Unit);
        }

        time = 0;
        UpdateQueue();
    }

    private void UpdateQueue()
    {
        List<Units> temp = new List<Units>();
        temp.AddRange(queue);

        for(int i = 0; i < queue.Count; i++)
        {
            try { queue[i] = temp[i + 1]; } catch { queue[i] = new Units(); }
        }

        manager.UpdateFactoryQueue(GetComponent<Entity>());

        if (queue[0].name == "") return;

        maxTime = queue[0].time;

        manager.UpdateFactoryQueue(GetComponent<Entity>());
        StartCoroutine(Fabric());
    }

    public void AddQueueBtn(int index)
    {
        AddQueue(unitsAvailable[index], false);
    }
}
