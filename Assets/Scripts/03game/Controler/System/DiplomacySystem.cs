using UnityEngine;
using UnityEngine.UI;

public class DiplomacySystem : MonoBehaviour
{
    [Header("Icons")]
    [SerializeField] private Sprite m_war;
    [SerializeField] private Sprite m_neutral;
    [SerializeField] private Sprite m_ally;

    public DiplomacyState[] m_diplomacyStates;

    private Text m_diploColonyName, m_reputationText;

    private Button m_peaceBtn, m_warBtn;
    private Image m_diploSatut;

    private GameObject m_diploInfo;

    private MoonColony m_currentColony;
    private ColonyStats m_currentStats;
    private MoonManager m_manager;

    private bool haveSave;
    private bool isAlreadyInitialized;

    #region Initialisation

    private void Start()
    {
        AssignAll();
        Initialize();
        Btn_HideDiplomacy();
    }

    public void Initialize(bool isOnSave = false)
    {
        if (!isAlreadyInitialized)
        {
            isAlreadyInitialized = true;
            haveSave = isOnSave;
            GenerateDiplomacy();
        }
    }

    private void AssignAll()
    {
        m_manager = GameObject.Find("Manager").GetComponent<MoonManager>();

        m_diploInfo = GameObject.Find("BC_Diplomacy");

        m_diploColonyName = GameObject.Find("T_ColonyName").GetComponent<Text>();
        m_reputationText = GameObject.Find("T_Reputation").GetComponent<Text>();

        m_diploSatut = GameObject.Find("I_DiploStatut").GetComponent<Image>();

        m_peaceBtn = GameObject.Find("Btn_Peace").GetComponent<Button>();
        m_warBtn = GameObject.Find("Btn_War").GetComponent<Button>();
    }

    #endregion

    #region UI managment

    public void Btn_HideDiplomacy()
    {
        m_diploInfo.SetActive(false);
        m_currentColony = null;
        m_currentStats = null;
    }

    public void Btn_OpenDiplomacyInfo(GameObject currentGo)
    {
        bool isActive = m_diploInfo.activeSelf;
        ColonyStats cur = currentGo.GetComponent<ColonyStats>();
        MoonColony current = cur.colony;

        if (isActive && current == m_currentColony)
        {
            m_diploInfo.SetActive(false);
            m_currentColony = null;
            m_currentStats = null;
            m_diploColonyName.text = "DIPLOMATIE_COLONY_NAME";
        }
        else
        {
            m_currentColony = current;
            m_currentStats = cur;
            m_diploColonyName.text = current.name;

            Vector2Int reputations = GetDiplomacyReputation(m_manager.side, m_currentColony.side);
            m_reputationText.text = (reputations.x + reputations.y).ToString();
            m_reputationText.transform.parent.gameObject.GetComponent<TooltipCaller>().info = m_manager.Traduce("Act:") + " " + reputations.x + "\n" + m_manager.Traduce("Trading:") + " " + reputations.y;
            m_reputationText.transform.parent.gameObject.GetComponent<TooltipCaller>().title = m_manager.Traduce("Reputation");

            UpdateButton();
            m_diploInfo.SetActive(true);
        }
    }

    public void Btn_DeclareWar()
    {
        ChangeDiplomacyState(m_manager.side, m_currentColony.side, 2);
        ChangeDiplomacyReputation(m_manager.side, m_currentColony.side, 0, -50);
        m_manager.Notify("Diplomacy request", m_currentColony.name + m_manager.Traduce(" is disappointed in you... You are now in war with them!"), m_war, new Color(.8f, 0, 0, 1f), 5f);
        UpdateInterface();
    }

    public void Btn_DeclarePeace()
    {
        if (GetDiplomacyState(m_manager.side, m_currentColony.side) == 0) return;

        Vector2Int reputations = GetDiplomacyReputation(m_manager.side, m_currentColony.side);
        int reputation = reputations.x + reputations.y;

        if(reputation >= 50 && reputations.x >= 25)
        {
            ChangeDiplomacyState(m_manager.side, m_currentColony.side, 0);
            ChangeDiplomacyReputation(m_manager.side, m_currentColony.side, 0, 50);
            m_manager.Notify("Diplomacy request", m_manager.Traduce("Your diplomatie request has been accepted! You're now in alliance with ") + m_currentColony.name + "!", m_ally, new Color(0, .8f, 0, 1f), 5f);
        }
        else
        {
            ChangeDiplomacyReputation(m_manager.side, m_currentColony.side, 0, UnityEngine.Random.Range(-3, -1));
            m_manager.Notify("Diplomacy request", "Your diplomatie request has been declined!", m_ally, new Color(.8f, 0, 0, 1f), 5f);
        }

        UpdateInterface();
    }

    public void Btn_Denounce()
    {
        int change = UnityEngine.Random.Range(1, 4);
        ChangeDiplomacyReputation(m_manager.side, m_currentColony.side, 0, -change);
        m_manager.Notify("Denunciation", m_manager.Traduce("You ratted out one of your competitors.") + " (-" + change + ")", m_ally, new Color(.8f, 0f, 0, 1f), 4f);
    }

    public void Btn_Flatter()
    {
        int reputation = GetDiplomacyReputation(m_manager.side, m_currentColony.side).x;

        if (reputation < 15)
        {
            if(Random.Range(0, 16) > reputation)
            {
                int change = Random.Range(0, 4);
                ChangeDiplomacyReputation(m_manager.side, m_currentColony.side, 0, change);
                m_manager.Notify("Flattery", m_manager.Traduce("You flattered one of your competitors.") + " (+" + change + ")", m_ally, new Color(0f, 0.8f, 0, 1f), 4f);
            }
            else
            {
                int change = Random.Range(0, 4);
                ChangeDiplomacyReputation(m_manager.side, m_currentColony.side, 0, -change);
                m_manager.Notify("Flattery", m_manager.Traduce("This competitor isn't naive, he saw right through you.") + " (-" + change + ")", m_ally, new Color(0.8f, 0f, 0, 1f), 4f);
            }
            
        }
        else
        {
            m_manager.Notify("Flattery", m_manager.Traduce("Your flattery didn't work.") + " (+0)", m_ally, new Color(.8f, 0f, 0, 1f), 4f);
        }
    }

    private void UpdateButton()
    {
        m_peaceBtn.interactable = true;
        m_warBtn.interactable = true;

        int diplo = GetDiplomacyState(m_manager.colonyStats.colony.side, m_currentColony.side);
        if (diplo == 2) { m_warBtn.interactable = false; }

        Vector2Int reputations = GetDiplomacyReputation(m_manager.side, m_currentColony.side);
        m_reputationText.text = (reputations.x + reputations.y).ToString();
        m_reputationText.transform.parent.gameObject.GetComponent<TooltipCaller>().info = m_manager.Traduce("Act:") + " " + reputations.x + "\n" + m_manager.Traduce("Trading:") + " " + reputations.y;
        m_reputationText.transform.parent.gameObject.GetComponent<TooltipCaller>().title = m_manager.Traduce("Reputation");

        UpdateIcon();
    }

    private void UpdateIcon()
    {
        int diplo = GetDiplomacyState(m_manager.colonyStats.colony.side, m_currentColony.side);

        if (diplo == 0) m_diploSatut.sprite = m_ally;
        else if (diplo == 1)m_diploSatut.sprite = m_neutral;
        else if (diplo == 2) m_diploSatut.sprite = m_war;
    }

    private void UpdateInterface()
    {
        UpdateIcon();
        UpdateButton();
    }

    private void UpdateReputation()
    {
        Vector2Int reputations = GetDiplomacyReputation(m_manager.side, m_currentColony.side);
        m_reputationText.text = (reputations.x + reputations.y).ToString();
        m_reputationText.transform.parent.gameObject.GetComponent<TooltipCaller>().info = m_manager.Traduce("Act:") + " " + reputations.x + "\n" + m_manager.Traduce("Trading:") + " " + reputations.y;
        m_reputationText.transform.parent.gameObject.GetComponent<TooltipCaller>().title = m_manager.Traduce("Reputation");
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
            ColonyStats[] colonies = FindObjectsOfType<ColonyStats>();
            m_diplomacyStates = new DiplomacyState[colonies.Length + 1];

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

                diplomacies[colonies.Length - 1] = new DiplomacyColony(999, 2, -999, -999);

                m_diplomacyStates[i] = new DiplomacyState(colonies[i].colony.side, diplomacies);
            }

            DiplomacyColony[] waveColony = new DiplomacyColony[colonies.Length];

            for(int i = 0; i < waveColony.Length; i++)
            {
                if(colonies[i].colony.side == m_manager.side)
                {
                    waveColony[i] = new DiplomacyColony(colonies[i].colony.side, 2, -999, -999);
                }
                else
                {
                    waveColony[i] = new DiplomacyColony(colonies[i].colony.side, 0, 50, 50);
                }
            }

            m_diplomacyStates[m_diplomacyStates.Length - 1] = new DiplomacyState(999, waveColony);

            Debug.Log("[INFO:DiplomacySystem] Generation of the diplomacy done.");
        }
        else
        {
            Debug.Log("[INFO:DiplomacySystem] The generation of the diplomacy has been canceled by the save.");
        }
    }

    private Vector2Int GetDiplomacyReputation(int povSide = 0, int side = 1)
    {
        foreach (DiplomacyState d in m_diplomacyStates)
        {
            if (d.m_povSide == povSide)
            {
                foreach (DiplomacyColony dc in d.m_states)
                {
                    if (dc.m_side == side)
                    {
                        return new Vector2Int(dc.m_actReputation, dc.m_commercialReputation);
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
            foreach (DiplomacyState d in m_diplomacyStates)
            {
                if (d.m_povSide == povSide)
                {
                    foreach (DiplomacyColony dc in d.m_states)
                    {
                        if (dc.m_side == side)
                        {
                            dc.m_actReputation = Mathf.Clamp(dc.m_actReputation + change, -50, 50);
                        }
                    }
                }
            }

            foreach (DiplomacyState d in m_diplomacyStates)
            {
                if (d.m_povSide == side)
                {
                    foreach (DiplomacyColony dc in d.m_states)
                    {
                        if (dc.m_side == povSide)
                        {
                            dc.m_actReputation = Mathf.Clamp(dc.m_actReputation + change, -50, 50);
                        }
                    }
                }
            }
        }
        else if (rep == 1)
        {
            foreach (DiplomacyState d in m_diplomacyStates)
            {
                if (d.m_povSide == povSide)
                {
                    foreach (DiplomacyColony dc in d.m_states)
                    {
                        if (dc.m_side == side)
                        {
                            dc.m_commercialReputation = Mathf.Clamp(dc.m_commercialReputation + change, -50, 50);
                        }
                    }
                }
            }

            foreach (DiplomacyState d in m_diplomacyStates)
            {
                if (d.m_povSide == side)
                {
                    foreach (DiplomacyColony dc in d.m_states)
                    {
                        if (dc.m_side == povSide)
                        {
                            dc.m_commercialReputation = Mathf.Clamp(dc.m_commercialReputation + change, -50, 50);
                        }
                    }
                }
            }
        }

        UpdateReputation();
    }

    private int GetDiplomacyState(int povSide = 0, int side = 1)
    {
        foreach(DiplomacyState d in m_diplomacyStates)
        {
            if(d.m_povSide == povSide)
            {
                foreach(DiplomacyColony dc in d.m_states)
                {
                    if(dc.m_side == side)
                    {
                        return dc.m_state;
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

        foreach (DiplomacyState d in m_diplomacyStates)
        {
            if (d.m_povSide == povSide)
            {
                foreach (DiplomacyColony dc in d.m_states)
                {
                    if (dc.m_side == side)
                    {
                        s = dc.m_state;
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
        foreach (DiplomacyState d in m_diplomacyStates)
        {
            if (d.m_povSide == povSide)
            {
                foreach (DiplomacyColony dc in d.m_states)
                {
                    if (dc.m_side == side)
                    {
                        dc.m_state = state;
                        Debug.Log("[INFO:DiplomacySystem] Diplomacy state has been changed. [" + povSide + ", " + side + ", " + state + "]");
                    }
                }
            }
        }

        foreach (DiplomacyState d in m_diplomacyStates)
        {
            if (d.m_povSide == side)
            {
                foreach (DiplomacyColony dc in d.m_states)
                {
                    if (dc.m_side == povSide)
                    {
                        dc.m_state = state;
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
    public int m_povSide;
    public DiplomacyColony[] m_states;

    public DiplomacyState(int povSide, DiplomacyColony[] states)
    {
        m_povSide = povSide;
        m_states = states;
    }
}

[System.Serializable]
public class DiplomacyColony
{
    public int m_side;
    public int m_state;
    public int m_commercialReputation; // allant de -50 à +50
    public int m_actReputation; // allant de -50 à +50

    public DiplomacyColony(int side, int state, int commercialReputation, int actReputation)
    {
        m_side = side;
        m_state = state;
        m_commercialReputation = commercialReputation;
        m_actReputation = actReputation;
    }
}

#endregion