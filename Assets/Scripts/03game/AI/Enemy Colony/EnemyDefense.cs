using UnityEngine;

public class EnemyDefense : MonoBehaviour
{
    private EnemyMotor motor;

    void Start()
    {
        GameObject[] enemies = FindObjectOfType<MoonManager>().FindTag(Tag.Enemy);

        foreach(GameObject go in enemies)
        {
            if(go.GetComponent<Buildings>().side == GetComponent<Buildings>().side)
            {
                motor = go.GetComponent<EnemyMotor>();
                break;
            }
        }

        AddDefense();
    }

    private void AddDefense()
    {
        motor.defenseNbr++;
    }

    private void RemoveDefense()
    {
        motor.defenseNbr--;
    }

    private void OnDestroy()
    {
        RemoveDefense();
    }
}
