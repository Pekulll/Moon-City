using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DefaultWaves", menuName = "Database/Wave")]
public class EnemieWave : ScriptableObject
{
    public List<EnemieArmies> army = new List<EnemieArmies>();

    public EnemieArmies GetArmy(int id)
    {
        return army.Find(build => build.identity == id);
    }

    public EnemieArmies GetArmy(string name)
    {
        return army.Find(build => build.name == name);
    }
}
