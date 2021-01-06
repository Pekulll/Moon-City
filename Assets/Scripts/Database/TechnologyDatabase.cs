using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewTechDataBase", menuName = "Database/Technology/Technology")]
public class TechnologyDatabase : ScriptableObject
{
    public List<Technology> techs = new List<Technology>();

    public Technology GetTech(int id)
    {
        return techs.Find(tech => tech.identity == id);
    }

    public Technology GetTech(string name)
    {
        return techs.Find(tech => tech.name == name);
    }
}
