using UnityEngine;

public class EnemyPreview : MonoBehaviour
{
    private EnemyMotor mtr;

    public void Init(EnemyMotor _mtr)
    {
        mtr = _mtr;
        InvokeRepeating("Progress", 1f, 1f);
        Add();
    }

    private void Progress()
    {
        GetComponent<Preview>().Progress(1);
    }

    public void Substract()
    {
        mtr.previewNbr--;
        mtr.RemovePreview(GetComponent<Preview>().id);
    }

    public void Add()
    {
        mtr.previewNbr++;
        mtr.AddPreview(GetComponent<Preview>().id);
    }
}
