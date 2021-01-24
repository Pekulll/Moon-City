using UnityEngine;
using UnityEngine.UI;

public class NotificationItem : MonoBehaviour
{
    public void Initialize(string notificationText, float duration, int priority)
    {
        if (duration < 1) duration = 1;
        ColorManager manager = GameObject.Find("Manager").GetComponent<ColorManager>();

        GetComponentInChildren<Text>().text = notificationText;
        GetComponentInChildren<Text>().color = manager.text;

        if(priority == 0) GetComponent<Image>().color = manager.background;
        else if(priority == 1) GetComponent<Image>().color = manager.finished;
        else if(priority == 2) GetComponent<Image>().color = manager.importantColor;
        else if(priority == 3) GetComponent<Image>().color = manager.veryImportantColor;

        Invoke("Hide", duration + .5f);
        Destroy(gameObject, duration + 1);
    }

    private void Hide()
    {
        GetComponent<Animator>().Play("hide");
    }
}
