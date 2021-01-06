using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ResearchItem : MonoBehaviour
{
    [SerializeField] private int techId;
    private Technology current;

    private MoonManager manager;
    private ResearchSystem researchSystem;
    private ColorManager colorManager;
    
    public void Initialize(ResearchSystem researchSystem, MoonManager manager, Technology tech, int techID)
    {
        this.manager = manager;
        this.researchSystem = researchSystem;
        colorManager = FindObjectOfType<ColorManager>();

        techId = techID;
        current = tech;

        transform.Find("T_ResearchName").GetComponent<Text>().text = this.manager.Traduce(current.name);
        transform.Find("T_ResearchDescription").GetComponent<Text>().text = this.manager.Traduce(current.description); //? Temporary
        transform.Find("E_ResearchCost/T_ResearchCost").GetComponent<Text>().text = current.cost.ToString();
        transform.Find("I_ResearchIcon").GetComponent<Image>().sprite = current.icon; //? Temporary
    }

    private void Start()
    {
        Check();
    }

    public void Check()
    {
        if (researchSystem.CheckTechIsUnlock(techId))
        {
            GetComponent<Button>().interactable = false;
            GetComponent<Outline>().effectColor = colorManager.finished;
            return;
        }

        if (current.neededTech.Length != 0)
        {
            foreach (int id in current.neededTech)
            {
                if (!researchSystem.CheckTechIsUnlock(id))
                {
                    GetComponent<Outline>().effectColor = colorManager.unavailable;
                    return;
                }
            }
        }

        if (researchSystem.CheckTechIsQueued(techId))
        {
            GetComponent<Outline>().effectColor = colorManager.inProgress;
            return;
        }

        GetComponent<Outline>().effectColor = colorManager.backgroundBorder;
    }

    public void Research()
    {
        bool canBeSearch = true;
        int neededTech = -1;

        if(current.neededTech.Length != 0)
        {
            foreach (int id in current.neededTech)
            {
                if (!researchSystem.CheckTechIsUnlock(id))
                {
                    canBeSearch = false;
                    neededTech = id;
                    break;
                }
            }
        }

        if (canBeSearch)
        {
            if (researchSystem.CheckTechIsQueued(techId))
            {
                researchSystem.RemoveFromQueue(techId);
                GetComponent<Outline>().effectColor = colorManager.backgroundBorder;
            }
            else
            {
                if (researchSystem.Enqueue(techId))
                {
                    GetComponent<Outline>().effectColor = colorManager.inProgress;
                }
            }
        }
        else
        {
            manager.Notify("Action impossible!", manager.Traduce("You need to discover another technology to do this: ") + manager.Traduce(manager.techData.GetTech(neededTech).name), null, new Color(1, .655f, 0, 1), 3.5f);
        }
    }
}
