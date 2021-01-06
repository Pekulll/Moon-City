using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NotificationSystem : MonoBehaviour
{
    [Header("Properties")]
    [SerializeField] private Sprite baseIcon;
    [SerializeField] private AudioClip m_notificationSound;

    [Header("Others")]
    [SerializeField] private List<Notification> notifPrefabs = new List<Notification>();

    public List<Notification> queue = new List<Notification>();
    private Notification current;

    private Text description;
    private Image icon;

    private Image notifTextArea;
    private Image notificationIcon;

    private GameObject notification;

    private TraduceSystem traduce;
    private AudioSource player;

    private bool isInit;
    
    private void Start()
    {
        if (!isInit) Init();

        if(queue.Count != 0)
        {
            StartSequence();
        }
    }

    public void Init()
    {
        isInit = true;
        AssignAll();
    }

    private void AssignAll()
    {
        notification = GameObject.Find("Notification"); // Contours + animations
        description = GameObject.Find("NotificationText").GetComponent<Text>();
        notificationIcon = GameObject.Find("IconContener").GetComponent<Image>(); // Contours
        icon = GameObject.Find("NotifIcon").GetComponent<Image>(); // Icône

        player = FindObjectOfType<AudioSource>();
        traduce = FindObjectOfType<TraduceSystem>();
    }

    public void Poke(int index)
    {
        if (notifPrefabs[index] == null) return;
        StopAllCoroutines();

        Notification cur = notifPrefabs[index];

        if(FindObjectOfType<MoonManager>() != null)
        {
            MoonManager manager = FindObjectOfType<MoonManager>();
            manager.AddToHistory("<b>" + manager.Traduce(cur.title) + "</b>;" + manager.Traduce(cur.description) + ";");
        }

        AddToQueue(cur);
    }

    public void NewNotification(string _title, string _description, Sprite _icon, Color _color, float _duration, string cmd = "")
    {
        StopAllCoroutines();

        Notification cur = new Notification();

        cur.title = _title;
        cur.description = _description;

        if(_icon == null)
        {
            cur.icon = baseIcon;
        }
        else
        {
            cur.icon = _icon;
        }

        cur.color = _color;
        cur.duration = _duration;
        cur.command = cmd;

        AddToQueue(cur);
    }

    private void AddToQueue(Notification ntf)
    {
        foreach(Notification n in queue)
        {
            if(n.title == ntf.title)
            {
                if(n.description == ntf.description)
                {
                    return;
                }
            }
        }

        queue.Add(ntf);

        if (queue.Count == 1) StartSequence();
    }

    public void StartSequence()
    {
        current = queue[0];
        Show();
        Invoke("Hide", current.duration + 1);
    }

    private void Show()
    {
        description.text = "<b><size=18>" + traduce.GetTraduction(current.title) + "</size></b>\n" + traduce.GetTraduction(current.description);

        if (current.icon != null)
            icon.sprite = current.icon;
        else
            icon.sprite = baseIcon;

        notification.GetComponent<Image>().color = current.color;
        notificationIcon.color = current.color;
        try { player.PlayOneShot(m_notificationSound); } catch { }
        
        if (notification.GetComponent<Animator>() != null)
        {
            notification.GetComponent<Animator>().Play("Show");
        }
    }

    private void Hide()
    {
        if (notification.GetComponent<Animator>() != null)
        {
            notification.GetComponent<Animator>().Play("Hide");
        }
        
        queue.Remove(current);
        Invoke("DisactiveNotification", 1f);
    }

    private void DisactiveNotification()
    {
        if (queue.Count != 0) StartSequence();
    }

    public void CallCommand()
    {
        try {
            FindObjectOfType<MoonManager>().SendCommand(current.command);
        } catch (Exception e) {
            Debug.Log("  <b>[WARN:Notification] Can't execute command... " + e.ToString());
        }
    }
}
