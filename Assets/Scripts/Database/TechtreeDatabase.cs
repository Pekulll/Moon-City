using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewTechTree", menuName = "Database/Technology/Tree")]
public class TechtreeDatabase : ScriptableObject
{
    public List<TechTree> techTrees = new List<TechTree>();

    public Technology GetTech(int id)
    {
        foreach (TechTree tt in techTrees)
        {
            foreach (Technology t in tt.technologies)
            {
                if (t.identity == id)
                {
                    return t;
                }
            }
        }

        return null;
    }

    public Technology GetTech(string name)
    {
        foreach (TechTree tt in techTrees)
        {
            foreach (Technology t in tt.technologies)
            {
                if (t.name == name)
                {
                    return t;
                }
            }
        }

        return null;
    }

    public List<Technology> GetEveryTech()
    {
        List<Technology> everyTech = new List<Technology>();

        foreach (TechTree tt in techTrees)
        {
            everyTech.AddRange(tt.technologies);
        }

        return everyTech;
    }
}
