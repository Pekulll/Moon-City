using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EventSystem : MonoBehaviour
{
    [SerializeField] private Event[] eventsData;

    private GameObject eventUI;

    private Text eventTitle, dialogueText, locutorName;
    private Image locutorIcon;
    private Button btn_one, btn_two, btn_three;

    [HideInInspector] public int currentDialogIndex = 0;
    [HideInInspector] public int currentEventIdentity;
    [HideInInspector] public Event current;

    private MoonManager manager;
    private bool isInit;

    private void Start()
    {
        if (isInit) return;

        Init();
    }

    public void Init()
    {
        isInit = true;

        eventUI = GameObject.Find("EventUI");
        manager = GetComponent<MoonManager>();

        eventTitle = GameObject.Find("T_EventName").GetComponent<Text>();
        dialogueText = GameObject.Find("T_EventDescription").GetComponent<Text>();
        locutorName = GameObject.Find("T_EventLocutor").GetComponent<Text>();
        locutorIcon = GameObject.Find("I_EventLocutor").GetComponent<Image>();

        btn_one = GameObject.Find("Btn_Choice1").GetComponent<Button>();
        btn_two = GameObject.Find("Btn_Choice2").GetComponent<Button>();
        btn_three = GameObject.Find("Btn_Choice3").GetComponent<Button>();

        eventUI.SetActive(false);
    }

    public void InstantiateEvent(int identity)
    {
        foreach(Event evt in eventsData)
        {
            if(evt.identity == identity)
            {
                CreateEvent(evt);
                return;
            }
        }
    }

    public void InstantiateEvent(string name)
    {
        foreach (Event evt in eventsData)
        {
            if (evt.name == name)
            {
                CreateEvent(evt);
                return;
            }
        }
    }

    public void InstantiateEvent(Event evt)
    {
        CreateEvent(evt);
    }

    public void RandomizeEvent()
    {
        List<Event> randoms = new List<Event>();

        foreach (Event e in eventsData)
        {
            if (e.type == "Random")
            {
                randoms.Add(e);
            }
        }

        if (randoms.Count == 0) return;

        int randomIndex = UnityEngine.Random.Range(0, randoms.Count - 1);
        Event evt = randoms[randomIndex];

        CreateEvent(evt);
    }

    private void CreateEvent(Event _current)
    {
        if (_current.type == "Unshowable") return;

        current = _current;
        currentEventIdentity = current.identity;
        eventTitle.text = manager.Traduce(current.name);
        NextDialogue(0);
        //manager.ResetMiddle();
        manager.PauseGame(true, true);
        eventUI.SetActive(true);
    }

    public void NextDialogue(int index)
    {
        currentDialogIndex = index;

        dialogueText.text = manager.Traduce(current.dialogs[currentDialogIndex].text);
        locutorIcon.sprite = current.dialogs[currentDialogIndex].locutorIcon;
        locutorName.text = manager.Traduce(current.dialogs[currentDialogIndex].locutor);

        if (current.dialogs[currentDialogIndex].btns.Length == 1)
        {
            btn_two.gameObject.SetActive(false);
            btn_three.gameObject.SetActive(false);

            btn_one.GetComponent<TooltipCaller>().title = manager.Traduce(current.dialogs[currentDialogIndex].btns[0].btnName);
            btn_two.GetComponent<TooltipCaller>().title = manager.Traduce("None");
            btn_three.GetComponent<TooltipCaller>().title = manager.Traduce("None");

            btn_one.GetComponent<Image>().sprite = current.dialogs[currentDialogIndex].btns[0].icon;
            btn_two.GetComponent<Image>().sprite = null;
            btn_three.GetComponent<Image>().sprite = null;
        }
        else if (current.dialogs[currentDialogIndex].btns.Length == 2)
        {
            btn_two.gameObject.SetActive(true);
            btn_three.gameObject.SetActive(false);

            btn_one.GetComponent<TooltipCaller>().title = manager.Traduce(current.dialogs[currentDialogIndex].btns[0].btnName);
            btn_two.GetComponent<TooltipCaller>().title = manager.Traduce(current.dialogs[currentDialogIndex].btns[1].btnName);
            btn_three.GetComponent<TooltipCaller>().title = manager.Traduce("None");

            btn_one.GetComponent<Image>().sprite = current.dialogs[currentDialogIndex].btns[0].icon;
            btn_two.GetComponent<Image>().sprite = current.dialogs[currentDialogIndex].btns[1].icon;
            btn_three.GetComponent<Image>().sprite = null;
        }
        else if (current.dialogs[currentDialogIndex].btns.Length == 3)
        {
            btn_two.gameObject.SetActive(true);
            btn_three.gameObject.SetActive(true);

            btn_one.GetComponent<TooltipCaller>().title = manager.Traduce(current.dialogs[currentDialogIndex].btns[0].btnName);
            btn_two.GetComponent<TooltipCaller>().title = manager.Traduce(current.dialogs[currentDialogIndex].btns[1].btnName);
            btn_three.GetComponent<TooltipCaller>().title = manager.Traduce(current.dialogs[currentDialogIndex].btns[2].btnName);

            btn_one.GetComponent<Image>().sprite = current.dialogs[currentDialogIndex].btns[0].icon;
            btn_two.GetComponent<Image>().sprite = current.dialogs[currentDialogIndex].btns[1].icon;
            btn_three.GetComponent<Image>().sprite = current.dialogs[currentDialogIndex].btns[2].icon;
        }
        else
        {
            btn_one.gameObject.SetActive(false);
            btn_two.gameObject.SetActive(false);
            btn_three.gameObject.SetActive(false);

            btn_one.GetComponent<TooltipCaller>().title = manager.Traduce("None");
            btn_two.GetComponent<TooltipCaller>().title = manager.Traduce("None");
            btn_three.GetComponent<TooltipCaller>().title = manager.Traduce("None");

            btn_one.GetComponent<Image>().sprite = null;
            btn_two.GetComponent<Image>().sprite = null;
            btn_three.GetComponent<Image>().sprite = null;
        }
    }

    public void EndEvent()
    {
        eventUI.SetActive(false);
        current = null;
        currentDialogIndex = 0;
        manager.PauseGame(false, true);
        manager.ChangeOverUI(false);
    }

    public void HideEvent()
    {
        eventUI.SetActive(false);
    }

    public void DisplayEvent()
    {
        if (current == null || current.name == "") return;
        manager.PauseGame(true, true);
        eventUI.SetActive(true);
    }

    public void EventButton(int btnIndex)
    {
        current.dialogs[currentDialogIndex].btns[btnIndex].onExecute.Invoke();
    }
}
