using UnityEngine;
using UnityEngine.UI;

public class SpeedManager : MonoBehaviour
{
    private ColorManager colorManager;
    private Image pause, normal, fast, veryFast;

    private float currentSpeed;
    private float previousSpeed;

    private void Start()
    {
        colorManager = FindObjectOfType<ColorManager>();

        pause = GameObject.Find("Btn_0").GetComponent<Image>();
        normal = GameObject.Find("Btn_1").GetComponent<Image>();
        fast = GameObject.Find("Btn_2").GetComponent<Image>();
        veryFast = GameObject.Find("Btn_4").GetComponent<Image>();

        ChangeSpeed(1);
    }

    public void ChangeSpeed(float speed)
    {
        ResetColor();
        currentSpeed = speed;

        if(speed == 0)
        {
            pause.color = colorManager.background;
            Time.timeScale = 0f;
        }
        else if(speed == 1f)
        {
            normal.color = colorManager.background;
            Time.timeScale = 1f;
        }
        else if (speed == 2f)
        {
            fast.color = colorManager.background;
            Time.timeScale = 2f;
        }
        else if (speed == 4f)
        {
            veryFast.color = colorManager.background;
            Time.timeScale = 4f;
        }
    }

    public void Pause()
    {
        if(currentSpeed == 0)
        {
            currentSpeed = previousSpeed;
        }
        else
        {
            previousSpeed = currentSpeed;
            currentSpeed = 0;
        }

        ChangeSpeed(currentSpeed);
    }

    public void IncreaseSpeed()
    {
        currentSpeed++;

        if(currentSpeed >= 3)
        {
            currentSpeed = 4;
        }

        ChangeSpeed(currentSpeed);
    }

    public void DecreaseSpeed()
    {
        currentSpeed--;

        if(currentSpeed < 0)
        {
            currentSpeed = 0;
        }
        else if(currentSpeed == 3)
        {
            currentSpeed = 2;
        }

        ChangeSpeed(currentSpeed);
    }

    private void ResetColor()
    {
        pause.color = colorManager.forground;
        normal.color = colorManager.forground;
        fast.color = colorManager.forground;
        veryFast.color = colorManager.forground;
    }
}
