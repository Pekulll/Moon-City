using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class FeedbackSender : MonoBehaviour
{
    private GameObject feedbackPanel;
    private GameObject feedbackPanelCompleteCover;
    private InputField textFeedback;
    private Text textCharCount;
    private Toggle toggleGeneralSelected;
    private Toggle toggleFeatureSelected;
    private Toggle toggleIssueSelected;

    private const string kGFormBaseURL = "https://docs.google.com/forms/d/e/1FAIpQLSfrslXAcyVzG0obEwstcwJV4sAmG-8TSvrFGhqdnwvTK7aLjw/";
    private const string generalGFormEntryID = "entry.27578610";
    private const string featureGFormEntryID = "entry.1577395210";
    private const string issueGFormEntryID = "entry.807771510";

    private SpeedManager speedManager;

    void Start()
    {
        speedManager = GameObject.Find("Manager").GetComponent<SpeedManager>();

        feedbackPanel = GameObject.Find("BC_Feedback");
        feedbackPanelCompleteCover = GameObject.Find("I_FeedbackCover");

        textFeedback = GameObject.Find("IF_Feedback").GetComponent<InputField>();
        textCharCount = GameObject.Find("T_Caracters").GetComponent<Text>();

        toggleGeneralSelected = GameObject.Find("TG_General").GetComponent<Toggle>();
        toggleFeatureSelected = GameObject.Find("TG_Suggestion").GetComponent<Toggle>();
        toggleIssueSelected = GameObject.Find("TG_Bug").GetComponent<Toggle>();

        textCharCount.text = textFeedback.text.Length + "/" + textFeedback.characterLimit;
        textFeedback.onValueChanged.AddListener(delegate (string text)
        {
            textCharCount.text = text.Length + "/" + textFeedback.characterLimit;
        });

        feedbackPanel.SetActive(false);
        feedbackPanelCompleteCover.SetActive(false);
    }

    public void Panel()
    {
        feedbackPanelCompleteCover.SetActive(false);

        bool isActive = !feedbackPanel.activeSelf;
        feedbackPanel.SetActive(isActive);

        if (isActive) speedManager.ChangeSpeed(0);
        else speedManager.ChangeSpeed(1);

        textFeedback.text = "";
    }

    public void Submit()
    {
        if (textFeedback.text.Trim() == "") return;

        if (toggleGeneralSelected.isOn)
        {
            StartCoroutine(SendGFormData(textFeedback.text, generalGFormEntryID));
        }
        else if (toggleFeatureSelected.isOn)
        {
            StartCoroutine(SendGFormData(textFeedback.text, featureGFormEntryID));
        }
        else if (toggleIssueSelected.isOn)
        {
            StartCoroutine(SendGFormData(textFeedback.text, issueGFormEntryID));
        }

        feedbackPanelCompleteCover.SetActive(true);
    }

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.F3)) return;
        Panel();
    }

    private static IEnumerator SendGFormData<T>(T dataContainer, string entryId)
    {
        bool isString = dataContainer is string;
        string jsonData = isString ? dataContainer.ToString() : JsonUtility.ToJson(dataContainer);

        WWWForm form = new WWWForm();
        form.AddField(entryId, jsonData);
        string urlGFormResponse = kGFormBaseURL + "formResponse";
        UnityWebRequest www = UnityWebRequest.Post(urlGFormResponse, form);
        yield return www.SendWebRequest();
    }
}