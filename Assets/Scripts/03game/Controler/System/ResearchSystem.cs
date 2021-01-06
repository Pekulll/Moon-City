using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResearchSystem : MonoBehaviour
{
    [Header("Properties")]
    public int currentTech;
    public float progress;

    [Header("Unlocked & queued")]
    public List<int> techUnlock = new List<int>();
    public List<int> techQueue = new List<int>();

    [Header("Others")]
    [SerializeField] private Image techPrefab;

    private const int maxTechQueued = 4;

    private MoonManager manager;

    private List<GameObject> techDisplayed;
    private GameObject researchUI;

    private Image progressBar;
    private Text point, queueText;

    private RectTransform content;

    private void Start()
    {
        manager = GetComponent<MoonManager>();

        researchUI = GameObject.Find("BC_ResearchMenu");
        content = GameObject.Find("E_AvailableTech").GetComponent<RectTransform>();

        progressBar = GameObject.Find("I_ResearchProgress").GetComponent<Image>();
        point = GameObject.Find("T_ResearchPoint").GetComponent<Text>();
        queueText = GameObject.Find("T_ResearchQueue").GetComponent<Text>();

        point.text = "+" + manager.colonyStats.research.ToString("0.0");
        queueText.text = "";

        techDisplayed = new List<GameObject>();

        progressBar.fillAmount = 0;
        Invoke("UpdateUI", 0.01f);
        HideUI();
    }

    private void UpdateUI()
    {
        point.text = "+" + manager.colonyStats.research.ToString("0.0");
    }

    public void UpdateButton()
    {
        if (techDisplayed.Count == 0) return;

        foreach (GameObject btn in techDisplayed)
        {
            btn.GetComponent<ResearchItem>().Check();
        }
    }

    #region Display buttons

    private void DestroyItem()
    {
        if (techDisplayed.Count == 0) return;

        foreach (GameObject item in techDisplayed)
        {
            Destroy(item);
        }
    }

    private void DisplayButtons()
    {
        List<int> techAvailable = GetAvailableTech();
        float count = techAvailable.Count;

        DestroyItem();

        if (count == 0) return;

        foreach (int id in techAvailable)
        {
            Image current = Instantiate(techPrefab, content) as Image;
            Technology cur = manager.techData.GetTech(id);

            techDisplayed.Add(current.gameObject);

            current.GetComponent<ResearchItem>().Initialize(this, manager, cur, id);
        }
    }

    private List<int> GetAvailableTech()
    {
        List<Technology> technologies = manager.techData.GetEveryTech();
        List<int> availableTech = new List<int>();

        foreach(Technology tech in technologies)
        {
            if (!CheckTechIsUnlock(tech.identity))
            {
                bool canResearch = true;

                foreach (int id in tech.neededTech)
                {
                    if (!CheckTechIsUnlock(id))
                    {
                        canResearch = false;
                        break;
                    }
                }

                if (canResearch)
                {
                    availableTech.Add(tech.identity);
                }
            }
        }

        return availableTech;
    }

    #endregion

    public void IncreaseResearch(float value)
    {
        if (currentTech == -1) return;

        progress += value;
        progressBar.fillAmount = progress / manager.techData.GetTech(currentTech).cost;
        UpdateUI();

        if(progress >= manager.techData.GetTech(currentTech).cost)
        {
            progress = 0;
            techUnlock.Add(currentTech);
            RemoveFromQueue(currentTech);
            currentTech = -1;

            try { UpdateButton(); } catch { }

            manager.Notify(6);

            if(techQueue.Count > 0)
            {
                Dequeue();
            }
            else
            {
                progressBar.fillAmount = 0;
                point.text = "+" + manager.colonyStats.research;
            }

            UpdateQueueList();
        }
    }

    public void Btn_Research()
    {
        manager.ResetLeft();
        DisplayButtons();
        researchUI.SetActive(true);
    }

    public void Btn_QuitResearch()
    {
        manager.ResetInterface();
    }

    #region Queue

    public void UpdateQueueList()
    {
        string researchQueue = "";

        foreach(int id in techQueue)
        {
            researchQueue += manager.Traduce(manager.techData.GetTech(id).name) + "\n";
        }

        queueText.text = researchQueue;
    }

    public bool Enqueue(int techId)
    {
        if(techQueue.Count >= maxTechQueued)
        {
            manager.Notify(19);
            return false;
        }
        else
        {
            techQueue.Add(techId);

            if(techQueue.Count == 1)
            {
                Dequeue();
            }

            UpdateQueueList();

            return true;
        }
    }

    public void Dequeue()
    {
        if(techQueue.Count > 0)
        {
            currentTech = techQueue[0];
            Technology tech = manager.techData.GetTech(currentTech);

            progress = 0;
            progressBar.fillAmount = progress / tech.cost;
            point.text = "+" + manager.colonyStats.research.ToString("0.0");

            UpdateQueueList();
        }
        else
        {
            progress = 0;
            currentTech = -1;
            progressBar.fillAmount = 0;
        }
    }

    public void RemoveFromQueue(int id)
    {
        if (!techQueue.Contains(id))
        {
            return;
        }

        if(techQueue.Count == 1)
        {
            techQueue.Clear();
        }
        else
        {
            techQueue.Remove(id);

            /*List<int> temp = new List<int>();

            foreach (int identity in techQueue)
            {
                if(identity != id)
                {
                    temp.Add(identity);
                }
            }

            techQueue = temp;*/
        }

        UpdateQueueList();
    }

    public bool CheckTechIsQueued(int id)
    {
        return techQueue.Contains(id);
    }

    #endregion

    public void HideUI()
    {
        researchUI.SetActive(false);
    }

    public bool CheckTechIsUnlock(int id)
    {
        return techUnlock.Contains(id);
    }
}
