using UnityEngine;

public class NotificationSystem : MonoBehaviour
{
    [SerializeField] private GameObject item;
    private Transform contener;

    private void Start()
    {
        contener = transform;
    }

    public void Notify(string notification, float duration, int priority = 0, string cmd = "")
    {
        GameObject go = Instantiate(item, contener) as GameObject;
        go.GetComponent<NotificationItem>().Initialize(notification, 4f, priority);
    }
}
