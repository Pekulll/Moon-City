using UnityEngine;

public class QuestObjectif : MonoBehaviour
{
    [SerializeField] private QuestType questType;
    [SerializeField] private int questId;

    private QuestManager manager;

    public void Initialization(QuestType questType, int id)
    {
        if(questType == QuestType.Move)
        {
            manager = FindObjectOfType<QuestManager>();

            this.questType = questType;
            this.questId = id;

            Entity entity = GetComponent<Entity>();

            if (entity == null)
            {
                InitializePoint();
            }
            else
            {
                Debug.Log("[WARN:QuestObjectif] This objectif can't be assimilated to a point!");
            }
        }
        else
        {
            Debug.Log("[WARN:QuestObjectif] QuestType not compatible with QuestObjectif!");
        }        
    }

    private void InitializePoint()
    {
        BoxCollider collider = gameObject.AddComponent<BoxCollider>();
        collider.size = new Vector3(1.5f, 1.5f, 1.5f);
        collider.center = new Vector3(0, .75f, 0);
        collider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        manager.MoveProgression(questType, questId);
        Destroy(gameObject);
    }
}
