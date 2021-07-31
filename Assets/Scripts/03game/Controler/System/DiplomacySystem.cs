using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class DiplomacySystem : MonoBehaviour
{
    [Header("Icons")]
    [FormerlySerializedAs("m_war")] [SerializeField] private Sprite war;
    [FormerlySerializedAs("m_neutral")] [SerializeField] private Sprite neutral;
    [FormerlySerializedAs("m_ally")] [SerializeField] private Sprite ally;
    
    [Header("Interface")]
    [SerializeField] private GameObject button;

    [FormerlySerializedAs("m_diplomacyStates")] public DiplomacyState[] diplomacyStates;

    private Text diploColonyName, reputationText;

    private Button peaceBtn, warBtn;
    private Image diploSatut;

    private GameObject diploInfo;

    private MoonColony currentColony;
    private MoonManager manager;

    private RectTransform content;

    private bool haveSave;
    private bool isAlreadyInitialized;

    private ColonyStats[] colonies;

    #region Initialisation

    private void Start()
    {
        Initialize();
    }

    public void Initialize(bool isOnSave = false)
    {
        if (!isAlreadyInitialized)
        {
            AssignAll();
            isAlreadyInitialized = true;
            haveSave = isOnSave;
            GenerateDiplomacy();
            CreateButtons();
            Btn_HideDiplomacy();
        }
    }

    private void AssignAll()
    {
        manager = GameObject.Find("Manager").GetComponent<MoonManager>();

        diploInfo = GameObject.Find("BC_Diplomacy");

        diploColonyName = GameObject.Find("T_ColonyName").GetComponent<Text>();
        reputationText = GameObject.Find("T_Reputation").GetComponent<Text>();

        diploSatut = GameObject.Find("I_DiploStatut").GetComponent<Image>();

        peaceBtn = GameObject.Find("Btn_Peace").GetComponent<Button>();
        warBtn = GameObject.Find("Btn_War").GetComponent<Button>();

        content = GameObject.Find("E_Diplomacy").GetComponent<RectTransform>();
        colonies = FindObjectsOfType<ColonyStats>();
    }

    #endregion

    #region UI managment

    private void CreateButtons()
    {
        for (int i = 0; i < colonies.Length; i++)
        {
            if (colonies[i].colony.side == manager.side)
                continue;
            
            GameObject but = Instantiate(button, content);
            Outline outline = but.GetComponent<Outline>();
            Image icon = but.transform.Find("Image").GetComponent<Image>();
            Button b = but.GetComponent<Button>();
            
            outline.effectColor = new Color(colonies[i].colony.colonyColor[0], colonies[i].colony.colonyColor[1], colonies[i].colony.colonyColor[2], 1);
            icon.sprite = neutral;

            int temp = i;
            b.onClick.AddListener(delegate { Btn_OpenDiplomacyInfo(colonies[temp].colony.side); });
        }
    }
    
    public void Btn_HideDiplomacy()
    {
        diploInfo.SetActive(false);
        currentColony = null;
    }

    public void Btn_OpenDiplomacyInfo(int side)
    {
        bool isActive = diploInfo.activeSelf;
        MoonColony current = GetColony(side);

        if (isActive && current == currentColony)
        {
            diploInfo.SetActive(false);
            currentColony = null;
            diploColonyName.text = "DIPLOMATIE_COLONY_NAME";
        }
        else
        {
            currentColony = current;
            diploColonyName.text = current.name;

            Vector2Int reputations = GetDiplomacyReputation(manager.side, currentColony.side);
            reputationText.text = (reputations.x + reputations.y).ToString();
            
            TooltipCaller tooltip = reputationText.transform.parent.gameObject.GetComponent<TooltipCaller>();
            tooltip.info = manager.Traduce("Act:") + " " + reputations.x + "\n" + manager.Traduce("Trading:") + " " + reputations.y;
            tooltip.title = manager.Traduce("Reputation");

            UpdateButton();
            diploInfo.SetActive(true);
        }
    }

    private MoonColony GetColony(int side)
    {
        foreach (ColonyStats c in colonies)
        {
            if (c.colony.side == side)
                return c.colony;
        }

        return null;
    }

    public void Btn_DeclareWar()
    {
        ChangeDiplomacyState(manager.side, currentColony.side, 2);
        ChangeDiplomacyReputation(manager.side, currentColony.side, 0, -50);
        manager.Notify(string.Format(manager.Traduce("03_notif_war"), currentColony.name), priority: 3);
        UpdateInterface();
    }

    public void Btn_DeclarePeace()
    {
        if (GetDiplomacyState(manager.side, currentColony.side) == 0) return;

        Vector2Int reputations = GetDiplomacyReputation(manager.side, currentColony.side);
        int reputation = reputations.x + reputations.y;

        if(reputation >= 50 && reputations.x >= 25)
        {
            ChangeDiplomacyState(manager.side, currentColony.side, 0);
            ChangeDiplomacyReputation(manager.side, currentColony.side, 0, 50);
            manager.Notify(string.Format(manager.Traduce("03_notif_peace"), currentColony.name), priority: 1);
        }
        else
        {
            ChangeDiplomacyReputation(manager.side, currentColony.side, 0, UnityEngine.Random.Range(-3, -1));
            manager.Notify(manager.Traduce("03_notif_declined"));
        }

        UpdateInterface();
    }

    public void Btn_Denounce()
    {
        int change = UnityEngine.Random.Range(1, 4);
        ChangeDiplomacyReputation(manager.side, currentColony.side, 0, -change);
        manager.Notify(string.Format(manager.Traduce("03_notif_denounce"), change), priority: 2);
    }

    public void Btn_Flatter()
    {
        int reputation = GetDiplomacyReputation(manager.side, currentColony.side).x;

        if (reputation < 15)
        {
            if(Random.Range(0, 16) > reputation)
            {
                int change = Random.Range(0, 4);
                ChangeDiplomacyReputation(manager.side, currentColony.side, 0, change);
                manager.Notify(string.Format(manager.Traduce("03_notif_flatter_success"), change), priority: 1);
            }
            else
            {
                int change = Random.Range(0, 4);
                ChangeDiplomacyReputation(manager.side, currentColony.side, 0, -change);
                manager.Notify(string.Format(manager.Traduce("03_notif_flatter_fail"), change), priority: 2);
            }
        }
        else
        {
            manager.Notify(string.Format(manager.Traduce("03_notif_flatter_fail")));
        }
    }

    private void UpdateButton()
    {
        peaceBtn.interactable = true;
        warBtn.interactable = true;

        int diplo = GetDiplomacyState(manager.colonyStats.colony.side, currentColony.side);
        if (diplo == 2) { warBtn.interactable = false; }

        Vector2Int reputations = GetDiplomacyReputation(manager.side, currentColony.side);
        reputationText.text = (reputations.x + reputations.y).ToString();
        reputationText.transform.parent.gameObject.GetComponent<TooltipCaller>().info = manager.Traduce("Act:") + " " + reputations.x + "\n" + manager.Traduce("Trading:") + " " + reputations.y;
        reputationText.transform.parent.gameObject.GetComponent<TooltipCaller>().title = manager.Traduce("Reputation");

        UpdateIcon();
    }

    private void UpdateIcon()
    {
        int diplo = GetDiplomacyState(manager.colonyStats.colony.side, currentColony.side);

        if (diplo == 0) diploSatut.sprite = ally;
        else if (diplo == 1)diploSatut.sprite = neutral;
        else if (diplo == 2) diploSatut.sprite = war;
    }

    private void UpdateInterface()
    {
        UpdateIcon();
        UpdateButton();
    }

    private void UpdateReputation()
    {
        Vector2Int reputations = GetDiplomacyReputation(manager.side, currentColony.side);
        reputationText.text = (reputations.x + reputations.y).ToString();
        reputationText.transform.parent.gameObject.GetComponent<TooltipCaller>().info = manager.Traduce("Act:") + " " + reputations.x + "\n" + manager.Traduce("Trading:") + " " + reputations.y;
        reputationText.transform.parent.gameObject.GetComponent<TooltipCaller>().title = manager.Traduce("Reputation");
    }

    #endregion

    #region Diplomacy act

    public void RegenerateDiplimacy()
    {
        haveSave = false;
        GenerateDiplomacy();
        haveSave = true;
    }

    private void GenerateDiplomacy()
    {
        if (!haveSave)
        {
            diplomacyStates = new DiplomacyState[colonies.Length + 1];

            for(int i = 0; i < colonies.Length; i++)
            {
                DiplomacyColony[] diplomacies = new DiplomacyColony[colonies.Length];
                int index = 0;

                for(int o = 0; o < colonies.Length; o++)
                {
                    if (colonies[o] != colonies[i])
                    {
                        diplomacies[index] = new DiplomacyColony(colonies[o].colony.side, 1, 0, 0);
                        index++;
                    }
                }

                diplomacies[colonies.Length - 1] = new DiplomacyColony(999, 2, -99999, -99999);
                diplomacyStates[i] = new DiplomacyState(colonies[i].colony.side, diplomacies);
            }

            DiplomacyColony[] waveColony = new DiplomacyColony[colonies.Length];

            for(int i = 0; i < waveColony.Length; i++)
            {
                if(colonies[i].colony.side == manager.side)
                {
                    waveColony[i] = new DiplomacyColony(colonies[i].colony.side, 2, -99999, -99999);
                }
                else
                {
                    waveColony[i] = new DiplomacyColony(colonies[i].colony.side, 0, 50, 50);
                }
            }

            diplomacyStates[diplomacyStates.Length - 1] = new DiplomacyState(999, waveColony);

            Debug.Log("[INFO:DiplomacySystem] Generation of the diplomacy done.");
        }
        else
        {
            Debug.Log("[INFO:DiplomacySystem] The generation of the diplomacy has been canceled by the save.");
        }
    }

    private Vector2Int GetDiplomacyReputation(int povSide = 0, int side = 1)
    {
        foreach (DiplomacyState d in diplomacyStates)
        {
            if (d.povSide == povSide)
            {
                foreach (DiplomacyColony dc in d.states)
                {
                    if (dc.side == side)
                    {
                        return new Vector2Int(dc.actReputation, dc.commercialReputation);
                    }
                }
            }
        }

        Debug.Log("[INFO:DiplomacySystem] Can't find diplomacy reputation! (" + povSide + ", " + side + ")");
        return new Vector2Int();
    }

    private void ChangeDiplomacyReputation(int povSide = 0, int side = 1, int rep = 0, int change = 0)
    {
        if (rep == 0)
        {
            foreach (DiplomacyState d in diplomacyStates)
            {
                if (d.povSide == povSide)
                {
                    foreach (DiplomacyColony dc in d.states)
                    {
                        if (dc.side == side)
                        {
                            dc.actReputation = Mathf.Clamp(dc.actReputation + change, -50, 50);
                        }
                    }
                }
            }

            foreach (DiplomacyState d in diplomacyStates)
            {
                if (d.povSide == side)
                {
                    foreach (DiplomacyColony dc in d.states)
                    {
                        if (dc.side == povSide)
                        {
                            dc.actReputation = Mathf.Clamp(dc.actReputation + change, -50, 50);
                        }
                    }
                }
            }
        }
        else if (rep == 1)
        {
            foreach (DiplomacyState d in diplomacyStates)
            {
                if (d.povSide == povSide)
                {
                    foreach (DiplomacyColony dc in d.states)
                    {
                        if (dc.side == side)
                        {
                            dc.commercialReputation = Mathf.Clamp(dc.commercialReputation + change, -50, 50);
                        }
                    }
                }
            }

            foreach (DiplomacyState d in diplomacyStates)
            {
                if (d.povSide == side)
                {
                    foreach (DiplomacyColony dc in d.states)
                    {
                        if (dc.side == povSide)
                        {
                            dc.commercialReputation = Mathf.Clamp(dc.commercialReputation + change, -50, 50);
                        }
                    }
                }
            }
        }

        UpdateReputation();
    }

    private int GetDiplomacyState(int povSide = 0, int side = 1)
    {
        foreach(DiplomacyState d in diplomacyStates)
        {
            if(d.povSide == povSide)
            {
                foreach(DiplomacyColony dc in d.states)
                {
                    if(dc.side == side)
                    {
                        return dc.state;
                    }
                }
            }
        }

        Debug.Log("[WARN:DiplomacySystem] Can't find the diplomacy state. [" + povSide + ", " + side + "]");
        return 1;
    }

    public bool IsInWar(int povSide = 0, int side = 1)
    {
        int s = 1;

        foreach (DiplomacyState d in diplomacyStates)
        {
            if (d.povSide == povSide)
            {
                foreach (DiplomacyColony dc in d.states)
                {
                    if (dc.side == side)
                    {
                        s = dc.state;
                        //Debug.Log("POVSide=" + povSide + " / side=" + side + " :: s=" + s);
                    }
                }
            }
        }

        if (s == 2) return true;
        else return false;
    }

    private void ChangeDiplomacyState(int povSide = 0, int side = 1, int state = 1)
    {
        foreach (DiplomacyState d in diplomacyStates)
        {
            if (d.povSide == povSide)
            {
                foreach (DiplomacyColony dc in d.states)
                {
                    if (dc.side == side)
                    {
                        dc.state = state;
                        Debug.Log("[INFO:DiplomacySystem] Diplomacy state has been changed. [" + povSide + ", " + side + ", " + state + "]");
                    }
                }
            }
        }

        foreach (DiplomacyState d in diplomacyStates)
        {
            if (d.povSide == side)
            {
                foreach (DiplomacyColony dc in d.states)
                {
                    if (dc.side == povSide)
                    {
                        dc.state = state;
                        Debug.Log("[INFO:DiplomacySystem] Diplomacy state has been changed. [" + side + ", " + povSide + ", " + state + "]");
                        return;
                    }
                }
            }
        }

        Debug.Log("[WARN:DiplomacySystem] Can't find the diplomacy state. [" + povSide + ", " + side + "]");
    }

    #endregion
}

#region Class

[System.Serializable]
public class DiplomacyState
{
    [FormerlySerializedAs("m_povSide")] public int povSide;
    [FormerlySerializedAs("m_states")] public DiplomacyColony[] states;

    public DiplomacyState(int povSide, DiplomacyColony[] states)
    {
        this.povSide = povSide;
        this.states = states;
    }
}

[System.Serializable]
public class DiplomacyColony
{
    [FormerlySerializedAs("m_side")] public int side;
    [FormerlySerializedAs("m_state")] public int state;
    [FormerlySerializedAs("m_commercialReputation")] public int commercialReputation; // from -50 to +50
    [FormerlySerializedAs("m_actReputation")] public int actReputation; // from -50 to +50

    public DiplomacyColony(int side, int state, int commercialReputation, int actReputation)
    {
        this.side = side;
        this.state = state;
        this.commercialReputation = commercialReputation;
        this.actReputation = actReputation;
    }
}

#endregion