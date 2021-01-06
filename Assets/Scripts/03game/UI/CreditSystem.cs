using UnityEngine;
using UnityEngine.UI;

public class CreditSystem : MonoBehaviour
{
    private Text creditsText;
    [SerializeField] private CreditsCategory[] credits;

    private void Start()
    {
        creditsText = GameObject.Find("T_Credits").GetComponent<Text>();

        foreach(CreditsCategory cc in credits)
        {
            creditsText.text += "<b><size=" + (creditsText.fontSize + 5) + ">" + cc.title + "</size></b>\n\n";

            foreach(string p in cc.persons)
            {
                creditsText.text += p + "\n";
            }

            creditsText.text += "\n";
        }
    }
}

[System.Serializable]
public class CreditsCategory
{
    public string title;
    public string[] persons;
}