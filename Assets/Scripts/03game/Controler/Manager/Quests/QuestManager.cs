using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestManager : MonoBehaviour
{
    public List<BuildQuest> buildingQuests = new List<BuildQuest>();
    public List<KillQuest> killingQuests = new List<KillQuest>();
    public List<EventQuest> eventQuests = new List<EventQuest>();

    private RectTransform content;
    private Image questPrefab;
    
    private List<QuestDisplay> displayed = new List<QuestDisplay>();
    private List<GameObject> displayedObject = new List<GameObject>();

    private MoonManager manager;

    private GameObject detailedQuestUI;
    private Text questName, questType, questObjectives, questDescription;

    private bool isInit;

    private void Start()
    {
        Init();
    }

    public void Init(List<KillQuest> killQuest = null, List<BuildQuest> buildQuest = null)
    {
        if (!isInit)
        {
            isInit = true;

            manager = GetComponent<MoonManager>();
            questPrefab = FindObjectOfType<RessourceData>().questPrefab;

            detailedQuestUI = GameObject.Find("BC_QuestDetails");
            questName = GameObject.Find("T_QuestDetailName").GetComponent<Text>();
            questType = GameObject.Find("T_QuestDetailType").GetComponent<Text>();
            questObjectives = GameObject.Find("T_QuestDetailProgress").GetComponent<Text>();
            questDescription = GameObject.Find("T_QuestDetailDescription").GetComponent<Text>();

            content = GameObject.Find("QuestContent").GetComponent<RectTransform>();

            if (killQuest != null && killQuest.Count != 0) killingQuests = killQuest;
            if (buildQuest != null && buildQuest.Count != 0) buildingQuests = buildQuest;

            CloseTab();
            ShowQuestList();
        }
        else
        {
            if (killQuest != null && killQuest.Count != 0) killingQuests = killQuest;
            if (buildQuest != null && buildQuest.Count != 0) buildingQuests = buildQuest;
            ShowQuestList();
        }
    }

    #region Adding new quest

    public int NewKillQuest(KillQuest quest)
    {
        killingQuests.Add(quest);
        ShowQuestList();
        return killingQuests.Count - 1;
    }

    public int NewBuildQuest(BuildQuest quest)
    {
        buildingQuests.Add(quest);
        ShowQuestList();
        return buildingQuests.Count - 1;
    }

    public int NewEventQuest(EventQuest quest)
    {
        eventQuests.Add(quest);
        ShowQuestList();
        return eventQuests.Count - 1;
    }

    #endregion
    
    #region Quest progression

    public void MoveProgression(QuestType questType, int questId)
    {
        
    }

    public void CheckForBuildQuest(int entityId, int entitySide, EntityType entityType)
    {
        if (entitySide != manager.side || buildingQuests.Count == 0) return;
        
        List<BuildQuest> toRemove = new List<BuildQuest>();

        for(int i = 0; i < buildingQuests.Count; i++)
        {
            if (i > buildingQuests.Count - 1) break;

            if (buildingQuests[i].objectifType == entityType)
            {
                buildingQuests[i].Progress(entityId);
                
                if (buildingQuests[i].IsCompleted())
                {
                    toRemove.Add(buildingQuests[i]);

                    foreach (QuestReward r in buildingQuests[i].rewards)
                    {
                        ObtainReward(r);
                    }

                    if (buildingQuests[i].callback != "")
                        manager.SendCommand(buildingQuests[i].callback);

                    manager.Notify(string.Format(manager.Traduce("03_notif_questcomplete"), buildingQuests[i].questName));
                    Debug.Log("[INFO:QuestManager] Quest (building) completed!");
                }
            }
        }

        if (toRemove.Count != 0)
        {
            for (int i = 0; i < toRemove.Count; i++)
            {
                buildingQuests.Remove(toRemove[i]);
            }

            ShowQuestList();
        }
        else
        {
            if(buildingQuests.Count != 0)
            {
                UpdateQuestList();
            }
            else
            {
                buildingQuests = new List<BuildQuest>();
            }
        }
    }

    public void CheckForEventQuest(ObjectifType objType, int entitySide, EntityType entityType)
    {
        if (entitySide != manager.side || eventQuests.Count == 0) return;

        List<EventQuest> toRemove = new List<EventQuest>();

        for(int i = 0; i < eventQuests.Count; i++)
        {
            if (i > eventQuests.Count + 1) break;
            
            if (eventQuests[i].objectifType == objType)
            {
                eventQuests[i].Progress();

                if (eventQuests[i].IsCompleted())
                {
                    toRemove.Add(eventQuests[i]);

                    foreach (QuestReward r in eventQuests[i].rewards)
                    {
                        ObtainReward(r);
                    }

                    if (eventQuests[i].callback != "")
                        manager.SendCommand(eventQuests[i].callback);

                    manager.Notify(string.Format(manager.Traduce("03_notif_questcomplete"), eventQuests[i].questName));
                    Debug.Log("[INFO:QuestManager] Quest (building) completed!");
                }
            }
        }

        if (toRemove.Count != 0)
        {
            for (int i = 0; i < toRemove.Count; i++)
            {
                eventQuests.Remove(toRemove[i]);
            }

            ShowQuestList();
        }
        else
        {
            if (eventQuests.Count != 0)
            {
                UpdateQuestList();
            }
            else
            {
                eventQuests = new List<EventQuest>();
            }
        }
    }

    public void CheckForKillQuest(int entitySide, EntityType entityType, int identity)
    {
        if (entitySide == manager.side || killingQuests.Count == 0) return;

        List<KillQuest> toRemove = new List<KillQuest>();

        for(int i = 0; i < killingQuests.Count; i++)
        {
            if (i > killingQuests.Count - 1) break;

            if (killingQuests[i].objectifType == entityType)
            {
                killingQuests[i].Progress(identity);

                if (killingQuests[i].IsCompleted())
                {
                    toRemove.Add(killingQuests[i]);

                    foreach (QuestReward r in killingQuests[i].rewards)
                    {
                        ObtainReward(r);
                    }

                    if (killingQuests[i].callback != "")
                        manager.SendCommand(killingQuests[i].callback);

                    manager.Notify(string.Format(manager.Traduce("03_notif_questcomplete"), killingQuests[i].questName));
                    Debug.Log("[INFO:QuestManager] Quest (killing) completed!");
                }
            }
        }

        if (toRemove.Count != 0)
        {
            for (int i = 0; i < toRemove.Count; i++)
            {
                killingQuests.Remove(toRemove[i]);
            }

            ShowQuestList();
        }
        else
        {
            if(killingQuests.Count != 0)
            {
                //try { UpdateQuestList(); } catch { Debug.Log("[WARN:QuestManager] Can't update the list!"); }
                UpdateQuestList();
            }
            else
            {
                killingQuests = new List<KillQuest>();
            }
        }
    }

    private void ObtainReward(QuestReward reward)
    {
        if(reward.rewardsType != RewardType.Unit)
        {
            manager.GiveRessources(reward.rewardsType, reward.rewardsAmount);
        }
        else if(reward.rewardsType == RewardType.Unit)
        {

        }
    }

    #endregion

    #region Interface update

    #region Quick display

    private void UpdateQuestList()
    {
        ShowQuestList();

        return;

        foreach(QuestDisplay q in displayed)
        {
            q.UpdateInterface();
        }
    }

    private void ShowQuestList()
    {
        ClearDisplay();
        DisplayQuest();
    }

    private void ClearDisplay()
    {
        for(int i = 0; i < displayedObject.Count; i++)
        {
            Destroy(displayedObject[i]);
        }
    }

    private void DisplayQuest()
    {
        DisplayBuild();
        DisplayEvent();
        DisplayKill();
    }

    private void DisplayKill()
    {
        float count = killingQuests.Count;
        List<int> toRemove = new List<int>();

        if (count == 0) return;

        for (int i = 0; i < count; i++)
        {
            if (!killingQuests[i].IsCompleted())
            {
                Image current = Instantiate(questPrefab, content) as Image;
                QuestDisplay display = current.GetComponent<QuestDisplay>();
                GameObject go = current.gameObject;

                displayed.Add(display);
                displayedObject.Add(go);
                display.InitKill(i);
            }
            else
            {
                toRemove.Add(i);
            }
        }
    }

    private void DisplayBuild()
    {
        float count = buildingQuests.Count;
        List<int> toRemove = new List<int>();

        if (count == 0) return;

        for (int i = 0; i < count; i++)
        {
            if (!buildingQuests[i].IsCompleted())
            {
                Image current = Instantiate(questPrefab, content) as Image;
                QuestDisplay display = current.GetComponent<QuestDisplay>();
                GameObject go = current.gameObject;

                displayed.Add(display);
                displayedObject.Add(go);
                display.InitBuild(i);
            }
            else
            {
                toRemove.Add(i);
            }
        }
    }

    private void DisplayEvent()
    {
        float count = eventQuests.Count;
        List<int> toRemove = new List<int>();

        if (count == 0) return;

        for (int i = 0; i < count; i++)
        {
            if (!eventQuests[i].IsCompleted())
            {
                Image current = Instantiate(questPrefab, content) as Image;
                QuestDisplay display = current.GetComponent<QuestDisplay>();
                GameObject go = current.gameObject;

                displayed.Add(display);
                displayedObject.Add(go);
                display.InitEvent(i);
            }
            else
            {
                toRemove.Add(i);
            }
        }

        /*foreach (int i in toRemove)
        {
            eventQuests.RemoveAt(i);
        }*/
    }

    #endregion

    #region Detailed display

    public void OpenTab(int questID, QuestType questType)
    {
        CloseTab();

        if(questType == QuestType.Kill)
        {
            if (killingQuests.Count - 1 < questID) return;

            questName.text = manager.Traduce(killingQuests[questID].questName);
            this.questType.text = manager.Traduce("Kill");

            int count = 0, maxCount = 0;
            
            for(int i = 0; i < killingQuests[questID].currentObjectifCount.Length; i++)
            {
                count += killingQuests[questID].currentObjectifCount[i];
                maxCount += killingQuests[questID].objectifCount[i];
            }

            questObjectives.text = count + "/" + maxCount;
            questDescription.text = manager.Traduce(killingQuests[questID].questDescritpion);
        }
        else if (questType == QuestType.Build)
        {
            if (buildingQuests.Count - 1 < questID) return;

            questName.text = manager.Traduce(buildingQuests[questID].questName);
            this.questType.text = manager.Traduce("Build");

            int count = 0, maxCount = 0;

            for (int i = 0; i < buildingQuests[questID].currentObjectifCount.Length; i++)
            {
                count += buildingQuests[questID].currentObjectifCount[i];
                maxCount += buildingQuests[questID].objectifCount[i];
            }

            questObjectives.text = count + "/" + maxCount;
            questDescription.text = manager.Traduce(buildingQuests[questID].questDescritpion);
        }
        else if (questType == QuestType.Event)
        {
            if (eventQuests.Count - 1 < questID) return;

            questName.text = manager.Traduce(eventQuests[questID].questName);
            this.questType.text = manager.Traduce("Event");
            questObjectives.text = eventQuests[questID].currentObjectifCount + "/" + eventQuests[questID].objectifCount;
            questDescription.text = manager.Traduce(eventQuests[questID].questDescritpion);
        }

        detailedQuestUI.SetActive(true);
    }

    public void CloseTab()
    {
        detailedQuestUI.SetActive(false);
    }

    #endregion

    #endregion
}

public enum ObjectifType { None, Unit, Building, Point, Epidemy, Riot }
public enum QuestType { None, Kill, Build, Move, Event }
public enum RewardType { None, Money, Energy, Regolith, Bioplastic, Food, Unit }

[System.Serializable]
public class KillQuest
{
    [Header("Properties")]
    public string questName;
    public string questDescritpion;
    public EntityType objectifType;

    [Header("Objectif")]
    public int[] objectId;
    public int[] currentObjectifCount;
    public int[] objectifCount;

    [Header("Rewards")]
    public QuestReward[] rewards;
    public string callback;

    public KillQuest(string name, string description, EntityType objType, int[] objId, int[] objectifCount, QuestReward[] rewards, string callback = "")
    {
        this.questName = name;
        questDescritpion = description;
        this.objectId = objId;
        this.objectifType = objType;
        this.rewards = rewards;
        this.objectifCount = objectifCount;
        this.callback = callback;

        VerifyArrayLength();

        this.currentObjectifCount = new int[this.objectId.Length];
    }

    private void VerifyArrayLength()
    {
        if (objectId.Length > objectifCount.Length)
        {
            List<int> newObjectID = new List<int>();

            for (int i = 0; i < objectifCount.Length; i++)
            {
                newObjectID.Add(objectId[i]);
            }

            objectId = newObjectID.ToArray();
        }
        else if (objectifCount.Length > objectId.Length)
        {
            List<int> newObjectID = new List<int>();

            for (int i = 0; i < objectId.Length; i++)
            {
                newObjectID.Add(objectifCount[i]);
            }

            objectifCount = newObjectID.ToArray();
        }
    }

    public bool IsCompleted()
    {
        bool completed = true;

        for(int i = 0; i < objectId.Length; i++)
        {
            if(currentObjectifCount[i] < objectifCount[i])
            {
                completed = false;
                break;
            }
        }

        return completed;
    }

    public void Progress(int objId)
    {
        for (int i = 0; i < objectId.Length; i++)
        {
            if (objectId[i] == objId)
            {
                currentObjectifCount[i]++;
            }
        }
    }
}

[System.Serializable]
public class BuildQuest
{
    [Header("Properties")]
    public string questName;
    public string questDescritpion;
    public EntityType objectifType;

    [Header("Objectif")]
    public int[] objectId;
    public int[] currentObjectifCount;
    public int[] objectifCount;

    [Header("Rewards")]
    public QuestReward[] rewards;
    public string callback;

    public BuildQuest(string name, string description, EntityType objType, int[] objId, int[] objCount, QuestReward[] rewards, string callback = "")
    {
        this.questName = name;
        questDescritpion = description;
        this.objectifType = objType;
        this.objectId = objId;
        this.objectifCount = objCount;
        this.rewards = rewards;
        this.callback = callback;

        VerifyArrayLength();

        this.currentObjectifCount = new int[this.objectId.Length];
    }

    private void VerifyArrayLength()
    {
        if (objectId.Length > objectifCount.Length)
        {
            List<int> newObjectID = new List<int>();

            for (int i = 0; i < objectifCount.Length; i++)
            {
                newObjectID.Add(objectId[i]);
            }

            objectId = newObjectID.ToArray();
        }
        else if (objectifCount.Length > objectId.Length)
        {
            List<int> newObjectID = new List<int>();

            for (int i = 0; i < objectId.Length; i++)
            {
                newObjectID.Add(objectifCount[i]);
            }

            objectifCount = newObjectID.ToArray();
        }
    }

    public bool IsCompleted()
    {
        float count = 0;
        float maxCount = 0;

        for (int i = 0; i < objectId.Length; i++)
        {
            count += currentObjectifCount[i];
            maxCount += objectifCount[i];
        }

        return count >= maxCount;
    }

    public void Progress(int objId)
    {
        for(int i = 0; i < objectId.Length; i++)
        {
            if(objectId[i] == objId)
            {
                currentObjectifCount[i]++;
            }
        }
    }
}

[System.Serializable]
public class QuestReward
{
    public RewardType rewardsType;
    public int rewardsAmount;
    public int rewardsId = -1;

    public QuestReward(RewardType kind, int amount = -1, int id = -1)
    {
        rewardsAmount = amount;
        rewardsId = id;
        rewardsType = kind;
    }
}

[System.Serializable]
public class EventQuest
{
    [Header("Properties")]
    public string questName;
    public string questDescritpion;
    public ObjectifType objectifType;

    [Header("Objectif")]
    public int currentObjectifCount;
    public int objectifCount;

    [Header("Rewards")]
    public QuestReward[] rewards;
    public string callback;

    public EventQuest(string name, string description, ObjectifType objType, int objCount, QuestReward[] rewards, string callback = "")
    {
        questName = name;
        questDescritpion = description;
        objectifType = objType;
        objectifCount = objCount;
        this.rewards = rewards;
        this.callback = callback;
    }

    public bool IsCompleted()
    {
        return currentObjectifCount == objectifCount;
    }

    public void Progress()
    {
        currentObjectifCount++;
    }
}
