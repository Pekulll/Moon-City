using UnityEngine;
using System;

public class ShortcutManager : MonoBehaviour
{
    private SpeedManager speedManager;
    private GameObject wholeInterface;
    private Transform player;

    private void Start()
    {
        speedManager = GetComponent<SpeedManager>();
        wholeInterface = GameObject.Find("E_Interface");
        player = GameObject.Find("Player").transform;
    }

    void Update()
    {
        CheckForScreenshot(); // F6
        GameSpeedShortcut(); // Space, +, -
        CheckForInterfaceHiding(); // F7
        CheckForMouseHiding(); // F8
        ResetCamera(); // R
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

    private void CheckForInterfaceHiding()
    {
        if (Input.GetKeyDown(KeyCode.F7))
        {
            wholeInterface.SetActive(!wholeInterface.activeSelf);
        }
    }

    private void CheckForMouseHiding()
    {
        if (Input.GetKeyDown(KeyCode.F8))
        {
            Cursor.visible = !Cursor.visible;
        }
    }

    private void ResetCamera()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            player.GetComponent<CameraMotor>().ResetCamera();
        }
    }
}
