using UnityEngine;
using UnityEngine.UI;

public class DiplomacySystem : MonoBehaviour
{
    [Header("Icons")]
    [SerializeField] private Sprite m_war;
    [SerializeField] private Sprite m_neutral;
    [SerializeField] private Sprite m_ally;

    public DiplomacyState[] m_diplomacyStates;

    private Text m_diploColonyName, m_themText, m_youText, m_reputationText;

    private Button m_peaceBtn, m_warBtn, m_tradeBtn, m_tradingBtn;
    private Image m_diploSatut;

    private int m_tradeValueThem, m_tradeValueYou;
    private Text tradeTextThem, tradeTextYou;

    private Slider[] m_themSld = new Slider[4];
    private Slider[] m_youSld = new Slider[4];

    private Text[] m_themValue = new Text[4];
    private Text[] m_youValue = new Text[4];

    private GameObject m_diploInfo, m_tradeUI;

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
        m_tradeUI = GameObject.Find("B_DiploTrade");

        m_diploColonyName = GameObject.Find("T_ColonyName").GetComponent<Text>();
        m_reputationText = GameObject.Find("T_Reputation").GetComponent<Text>();
        m_themText = GameObject.Find("T_TradeThem").GetComponent<Text>();
        m_youText = GameObject.Find("T_TradeYou").GetComponent<Text>();

        m_diploSatut = GameObject.Find("I_DiploStatut").GetComponent<Image>();

        m_peaceBtn = GameObject.Find("Btn_Peace").GetComponent<Button>();
        m_warBtn = GameObject.Find("Btn_War").GetComponent<Button>();
        m_tradeBtn = GameObject.Find("Btn_Trade").GetComponent<Button>();
        m_tradingBtn = GameObject.Find("Btn_Trading").GetComponent<Button>();

        m_themSld[0] = GameObject.Find("Sld_ThemMoney").GetComponent<Slider>();
        m_themSld[1] = GameObject.Find("Sld_ThemRigolyte").GetComponent<Slider>();
        m_themSld[2] = GameObject.Find("Sld_ThemBioplastic").GetComponent<Slider>();
        m_themSld[3] = GameObject.Find("Sld_ThemFood").GetComponent<Slider>();

        m_youSld[0] = GameObject.Find("Sld_YouMoney").GetComponent<Slider>();
        m_youSld[1] = GameObject.Find("Sld_YouRigolyte").GetComponent<Slider>();
        m_youSld[2] = GameObject.Find("Sld_YouBioplastic").GetComponent<Slider>();
        m_youSld[3] = GameObject.Find("Sld_YouFood").GetComponent<Slider>();

        m_themValue[0] = GameObject.Find("T_ThemMoney").GetComponent<Text>();
        m_themValue[1] = GameObject.Find("T_ThemRigolyte").GetComponent<Text>();
        m_themValue[2] = GameObject.Find("T_ThemBioplastic").GetComponent<Text>();
        m_themValue[3] = GameObject.Find("T_ThemFood").GetComponent<Text>();

        m_youValue[0] = GameObject.Find("T_YouMoney").GetComponent<Text>();
        m_youValue[1] = GameObject.Find("T_YouRigolyte").GetComponent<Text>();
        m_youValue[2] = GameObject.Find("T_YouBioplastic").GetComponent<Text>();
        m_youValue[3] = GameObject.Find("T_YouFood").GetComponent<Text>();

        tradeTextThem = GameObject.Find("T_ValueThem").GetComponent<Text>();
        tradeTextYou = GameObject.Find("T_ValueYou").GetComponent<Text>();
    }

    #endregion

    #region UI managment

    public void Btn_HideDiplomacy()
    {
        m_diploInfo.SetActive(false);
        HideTrade();
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
            m_tradeUI.SetActive(false);
            m_diploColonyName.text = "DIPLOMATIE_COLONY_NAME";
        }
        else
        {
            m_currentColony = current;
            m_currentStats = cur;
            m_tradeValueThem = 0;
            m_tradeValueYou = 0;
            m_diploColonyName.text = current.name;

            Vector2Int reputations = GetDiplomacyReputation(m_manager.side, m_currentColony.side);
            m_reputationText.text = (reputations.x + reputations.y).ToString();
            m_reputationText.transform.parent.gameObject.GetComponent<TooltipCaller>().info = m_manager.Traduce("Act:") + " " + reputations.x + "\n" + m_manager.Traduce("Trading:") + " " + reputations.y;
            m_reputationText.transform.parent.gameObject.GetComponent<TooltipCaller>().title = m_manager.Traduce("Reputation");

            UpdateButton();
            m_tradeUI.SetActive(false);
            m_diploInfo.SetActive(true);
        }
    }

    public void Btn_Trade()
    {
        m_currentStats.money += (int)m_youSld[0].value;
        m_currentStats.regolith += m_youSld[1].value;
        m_currentStats.bioPlastique += m_youSld[2].value;
        m_currentStats.food += m_youSld[3].value;
        m_manager.RemoveRessources(0, (int)m_youSld[0].value, m_youSld[1].value, m_youSld[2].value, m_youSld[3].value);

        m_currentStats.money -= (int)m_themSld[0].value;
        m_currentStats.regolith -= m_themSld[1].value;
        m_currentStats.bioPlastique -= m_themSld[2].value;
        m_currentStats.food -= m_themSld[3].value;
        m_manager.AddRessources(0, (int)m_themSld[0].value, m_themSld[1].value, m_themSld[2].value, m_themSld[3].value);

        ChangeDiplomacyReputation(m_manager.side, m_currentColony.side, 1, Mathf.Clamp(m_tradeValueYou - m_tradeValueThem, 1, 3));
        m_manager.Notify("Trade request", m_currentColony.name + m_manager.Traduce(" had accepted your trade!"), m_neutral, new Color(0, .8f, 0, 1f), 3f);
        UpdateSlider();
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
        m_tradingBtn.interactable = false;

        int diplo = GetDiplomacyState(m_manager.colonyStats.colony.side, m_currentColony.side);

        if (diplo == 0) { m_peaceBtn.interactable = false; m_tradingBtn.interactable = true; }
        else if (diplo == 2) { m_warBtn.interactable = false; HideTrade(); }

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

    public void UpdateTradingValue()
    {
        for (int i = 0; i < 4; i++)
        {
            m_themValue[i].text = m_themSld[i].value.ToString();
            m_youValue[i].text = m_youSld[i].value.ToString();
        }

        m_tradeValueThem = (int)(m_themSld[0].value + m_themSld[1].value * 2 + (int)(m_themSld[2].value * 2.5f) + m_themSld[3].value * 2);
        m_tradeValueYou = (int)(m_youSld[0].value + m_youSld[1].value * 2 + (int)(m_youSld[2].value * 2.5f) + m_youSld[3].value * 2);

        tradeTextYou.text = m_tradeValueYou.ToString();
        tradeTextThem.text = m_tradeValueThem.ToString();

        if (m_tradeValueYou == m_tradeValueThem && m_tradeValueYou == 0)
        {
            m_tradeBtn.interactable = false;
            m_themText.color = Color.red;
            m_youText.color = Color.green;
        }
        else if (m_tradeValueYou >= m_tradeValueThem)
        {
            m_tradeBtn.interactable = true;
            m_youText.color = Color.green;
            m_themText.color = Color.green;
        }
        else
        {
            m_tradeBtn.interactable = false;
            m_themText.color = Color.red;
        }
    }

    public void OpenTrading()
    {
        if (m_currentStats == null) return;

        if (m_tradeUI.activeSelf)
        {
            HideTrade();
        }
        else
        {
            m_tradeBtn.interactable = false;

            UpdateSlider();
            UpdateTradingValue();

            m_tradeUI.SetActive(true);
        }

    }

    private void UpdateSlider()
    {
        m_themSld[0].maxValue = m_currentStats.money - 100;
        m_themSld[1].maxValue = m_currentStats.regolith - 10;
        m_themSld[2].maxValue = m_currentStats.bioPlastique - 10;
        m_themSld[3].maxValue = m_currentStats.food - 15;

        m_youSld[0].maxValue = m_manager.colonyStats.money - 100;
        m_youSld[1].maxValue = m_manager.colonyStats.regolith - 10;
        m_youSld[2].maxValue = m_manager.colonyStats.bioPlastique - 10;
        m_youSld[3].maxValue = m_manager.colonyStats.food - 15;

        for (int i = 0; i < 4; i++)
        {
            if (m_themSld[i].maxValue < 0)
            {
                m_themSld[i].maxValue = 0;
            }
            if (m_youSld[i].maxValue < 0)
            {
                m_youSld[i].maxValue = 0;
            }
        }
    }

    public void HideTrade()
    {
        for (int i = 0; i < 4; i++)
        {
            m_themSld[i].value = m_themSld[i].minValue;
            m_youSld[i].value = m_youSld[i].minValue;
        }

        m_youText.color = Color.green;
        m_themText.color = Color.red;

        m_tradeBtn.interactable = false;
        m_tradeUI.SetActive(false);
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