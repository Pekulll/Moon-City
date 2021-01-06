using UnityEngine;

public class EnemyUnit : MonoBehaviour
{
    public EnemyMotor mtr;

    public void Init(EnemyMotor _mtr)
    {
        mtr = _mtr;
        Add();
    }

    public void Substract()
    {
        mtr.unitNbr--;
    }

    public void Add()
    {
        mtr.unitNbr++;
    }
}
