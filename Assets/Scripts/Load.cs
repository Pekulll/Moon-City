using UnityEngine;

public class Load : MonoBehaviour
{
    public void LoadScene()
    {
        FindObjectOfType<LoadingMotor>().ActiveScene();
    }
}
