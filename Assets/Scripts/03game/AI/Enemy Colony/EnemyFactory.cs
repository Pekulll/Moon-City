using UnityEngine;

public class EnemyFactory : MonoBehaviour
{
    public EnemyMotor mtr;

    public void Init(EnemyMotor _mtr)
    {
        mtr = _mtr;
        Add();
    }

    public void Substract()
    {
        mtr.RemoveFactoryToList(gameObject.GetComponent<FactoryMotor>());
    }

    public void Add()
    {
        mtr.AddFactoryToList(gameObject.GetComponent<FactoryMotor>());
    }
}
