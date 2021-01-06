using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewUnitDataBase", menuName = "Database/Units")]
public class UnitsDatabase : ScriptableObject
{
    public List<Units> units = new List<Units>();

    public Units GetUnit(int id)
    {
        return units.Find(unit => unit.identity == id);
    }

    public Units GetUnit(string name)
    {
        return units.Find(unit => unit.name == name);
    }
}
