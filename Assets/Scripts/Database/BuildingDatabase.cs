using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewBuildDataBase", menuName = "Database/Build")]
public class BuildingDatabase : ScriptableObject
{
    public List<Build> builds = new List<Build>();

    public Build GetBuild(int id)
    {
        return builds.Find(build => build.identity == id);
    }

    public Build GetBuild(string name)
    {
        return builds.Find(build => build.name == name);
    }
}
