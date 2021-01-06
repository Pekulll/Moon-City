using UnityEngine;
using System;

public class ShortcutManager : MonoBehaviour
{
    private SpeedManager speedManager;

    private void Start()
    {
        speedManager = GetComponent<SpeedManager>();
    }

    void Update()
    {
        CheckForScreenshot();
        GameSpeedShortcut();
    }

    private void CheckForScreenshot()
    {
        if (Input.GetKeyDown(KeyCode.F6))
        {
            DateTime dateTime = DateTime.Now;
            string date = dateTime.Day + "-" + dateTime.Month + "-" + dateTime.Year + "_";
            string hour = dateTime.Hour + "-" + dateTime.Minute + "-" + dateTime.Second + "-" + dateTime.Millisecond;

            ScreenCapture.CaptureScreenshot(Application.dataPath + "/MoonCity_" + date + hour + ".png");
            Debug.Log("  [INFO:Screenshot] Screen registred at: " + Application.dataPath + "/MoonCity_" + date + hour + ".png");
        }
    }

    private void GameSpeedShortcut()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            speedManager.Pause();
        }
        else if (Input.GetKeyDown(KeyCode.Plus) || Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            speedManager.IncreaseSpeed();
        }
        else if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            speedManager.DecreaseSpeed();
        }
    }
}
