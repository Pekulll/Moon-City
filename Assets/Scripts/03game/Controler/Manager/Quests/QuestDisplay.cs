using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class QuestDisplay : MonoBehaviour, IPointerClickHandler
{
    private QuestType questType;
    private Text title, objectifTracker;

    private int id;

    private QuestManager manager;
    private MoonManager moonManager;

    private void Start()
    {
        ColorManager cm = FindObjectOfType<ColorManager>();

        manager = FindObjectOfType<QuestManager>();
        moonManager = FindObjectOfType<MoonManager>();

        title = transform.Find("B_Title/T_Title").GetComponent<Text>();
        objectifTracker = transform.Find("B_Title/BC_ObjectifTracker/T_ObjectifTracker").GetComponent<Text>();

        title.color = cm.text;
        objectifTracker.color = cm.text;
        transform.Find("B_Title").GetComponent<Image>().color = cm.background;
        transform.Find("B_Title/BC_ObjectifTracker").GetComponent<Image>().color = cm.forground;

        UpdateInterface();
    }

    public void InitKill(int id)
    {
        this.id = id;
        questType = QuestType.Kill;
    }

    public void InitBuild(int id)
    {
        this.id = id;
        questType = QuestType.Build;
    }

    public void InitEvent(int id)
    {
        this.id = id;
        questType = QuestType.Event;
    }

    public void UpdateInterface()
    {
        if (questType == QuestType.Kill)
        {
            UpdateKillingQuest();
        }
        else if (questType == QuestType.Build)
        {
            UpdateBuildingQuest();
        }
        else if (questType == QuestType.Event)
        {
            UpdateEventQuest();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void UpdateKillingQuest()
    {
        if (manager.killingQuests.Count - 1 < id)
        {
            Destroy(gameObject);
            return;
        }

        KillQuest quest = manager.killingQuests[id];
        title.text = moonManager.Traduce(quest.questName);

        int objectifComplete = GetCompletedObjectif(quest.currentObjectifCount);
        int objectifNumber = GetObjectifNumber(quest.objectifCount);

        objectifTracker.text = objectifComplete + " / " + objectifNumber;
    }

    private void UpdateBuildingQuest()
    {
        if (manager.buildingQuests.Count - 1 < id)
        {
            Destroy(gameObject);
            return;
        }

        BuildQuest quest = manager.buildingQuests[id];
        title.text = moonManager.Traduce(quest.questName);
        
        int objectifComplete = GetCompletedObjectif(quest.currentObjectifCount);
        int objectifNumber = GetObjectifNumber(quest.objectifCount);

        objectifTracker.text = objectifComplete + " / " + objectifNumber;
    }

    private void UpdateEventQuest()
    {
        if (manager.eventQuests.Count - 1 < id)
        {
            Destroy(gameObject);
            return;
        }

        EventQuest quest = manager.eventQuests[id];
        title.text = moonManager.Traduce(quest.questName);

        int objectifComplete = quest.currentObjectifCount;
        int objectifNumber = quest.objectifCount;

        objectifTracker.text = objectifComplete + " / " + objectifNumber;
    }

    private int GetCompletedObjectif(int[] objs)
    {
        int count = 0;

        foreach(int n in objs)
        {
            count += n;
        }

        return count;
    }

    private int GetObjectifNumber(int[] objs)
    {
        int count = 0;

        foreach (int n in objs)
        {
            count += n;
        }

        return count;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        manager.OpenTab(id, questType);
    }
}
