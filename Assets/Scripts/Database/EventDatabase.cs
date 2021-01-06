using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEventDatabase", menuName = "Database/Event")]
public class EventDatabase : ScriptableObject
{
    public List<Event> events = new List<Event>();

    public Event GetEvent(int id)
    {
        return events.Find(build => build.identity == id);
    }

    public Event GetEvent(string name)
    {
        return events.Find(build => build.name == name);
    }
}
