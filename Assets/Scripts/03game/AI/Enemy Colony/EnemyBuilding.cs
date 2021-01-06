using UnityEngine;

public class EnemyBuilding : MonoBehaviour
{
    public EnemyMotor mtr;

    public void Init(EnemyMotor _mtr)
    {
        mtr = _mtr;
        Add();
    }

    public void Substract()
    {
        mtr.buildNbr--;
    }

    public void Add()
    {
        mtr.buildNbr++;
        mtr.previewNbr--;
    }
}
