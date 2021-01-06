using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class KeyBindSystem : MonoBehaviour
{
    private bool captureKey;
    private int keyIndex;
    private KeyCode keyToBind = KeyCode.None;

    private GameObject bindUI;
    private Text timeTxt, infoTxt, keysName;

    [SerializeField] private GameKey[] gameKeys;
    private TraduceSystem trad;

    private void Start()
    {
        bindUI = GameObject.Find("BindKeyUI");
        timeTxt = GameObject.Find("BindTime").GetComponent<Text>();
        infoTxt = GameObject.Find("BindInfo").GetComponent<Text>();
        keysName = GameObject.Find("KeysNameText").GetComponent<Text>();
        trad = FindObjectOfType<TraduceSystem>();

        bindUI.SetActive(false);

        foreach (GameKey key in gameKeys)
        {
            if (PlayerPrefs.HasKey(key.keyName))
            {
                key.keyTouch = PlayerPrefs.GetString(key.keyName);
            }
            else
            {
                key.keyTouch = key.basicKey;
                PlayerPrefs.SetString(key.keyName, key.basicKey.ToString().ToLower());
                Debug.Log("  [INFO:KeyBindSystem] Key saved : " + PlayerPrefs.GetString(key.keyName));
            }
        }

        UpdateKeysName();
    }

    private void UpdateKeysName()
    {
        keysName.text = trad.GetTraduction("Key set at <b>") + gameKeys[0].keyTouch + "</b>  [" + trad.GetTraduction("default") + ":W]\n" +
            trad.GetTraduction("Key set at <b>") + gameKeys[1].keyTouch + "</b>  [" + trad.GetTraduction("default") + ":S]\n" +
            trad.GetTraduction("Key set at <b>") + gameKeys[2].keyTouch + "</b>  [" + trad.GetTraduction("default") + ":A]\n" +
            trad.GetTraduction("Key set at <b>") + gameKeys[3].keyTouch + "</b>  [" + trad.GetTraduction("default") + ":D]\n" +
            trad.GetTraduction("Key set at <b>") + gameKeys[4].keyTouch + "</b>  [" + trad.GetTraduction("default") + ":Q]\n" +
            trad.GetTraduction("Key set at <b>") + gameKeys[5].keyTouch + "</b>  [" + trad.GetTraduction("default") + ":E]\n" +
            trad.GetTraduction("Key set at <b>") + gameKeys[6].keyTouch + "</b>  [" + trad.GetTraduction("default") + ":TAB]\n"
            /*"<color=#5B5B5B>" + trad.GetTraduction("Key set at <b>") + "NONE</b>  [" + trad.GetTraduction("default") + ":NONE]\n" +
            trad.GetTraduction("Key set at <b>") + "NONE</b>  [" + trad.GetTraduction("default") + ":NONE]\n" +
            trad.GetTraduction("Key set at <b>") + "NONE</b>  [" + trad.GetTraduction("default") + ":NONE]</color>"*/;
    }

    public void Btn_BindKey(int _keyIndex)
    {
        keyIndex = _keyIndex;
        infoTxt.text = "<i>" + trad.GetTraduction("Key to bind:") + " " + gameKeys[keyIndex].keyName + "</i>";
        timeTxt.text = "Wait 2.0 seconds...";
        bindUI.SetActive(true);
        StartCoroutine(BindKey());
    }

    public void Btn_CancelBind()
    {
        StopAllCoroutines();
        keyToBind = KeyCode.None;
        captureKey = false;
        keyIndex = -1;
        bindUI.SetActive(false);
    }

    private IEnumerator BindKey()
    {
        float delay = .2f;
        timeTxt.text = trad.GetTraduction("Wait") + " " + delay.ToString("0.0") + " seconds...";

        while (delay > 0)
        {
            yield return new WaitForSeconds(0.1f);
            delay -= 0.1f;
            if(delay > 0) timeTxt.text = trad.GetTraduction("Wait") + " " + delay.ToString("0.0") + " "  + trad.GetTraduction("seconds") + "...";
            else timeTxt.text = trad.GetTraduction("Wait") + " 0.0 " + trad.GetTraduction("second") + "...";
        }

        captureKey = true;
    }

    private void Update()
    {
        if (!captureKey) return;

        if (keyToBind != KeyCode.None)
        {
            captureKey = false;
            PlayerPrefs.SetString(gameKeys[keyIndex].keyName, keyToBind.ToString().ToLower());
            gameKeys[keyIndex].keyTouch = PlayerPrefs.GetString(gameKeys[keyIndex].keyName);
            Debug.Log("<b><color=#00AC0C>  [INFO] Key bind at " + keyToBind.ToString() + " [" + keyIndex + "]</color></b>");
            keyToBind = KeyCode.None;
            bindUI.SetActive(false);
            UpdateKeysName();
            return;
        }

        keyToBind = FetchKey(false);
    }

    public static KeyCode FetchKey(bool detectMouse)
    {
        foreach (KeyCode vKey in System.Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(vKey))
            {
                if (vKey.ToString().Contains("JoystickButton"))
                {
                    continue;
                }

                if (!detectMouse)
                {
                    if (vKey == KeyCode.Mouse0 || vKey == KeyCode.Mouse1 || vKey == KeyCode.Mouse2 || vKey == KeyCode.Mouse3 || vKey == KeyCode.Mouse4 || vKey == KeyCode.Mouse5 || vKey == KeyCode.Mouse6)
                    {
                        return KeyCode.None;
                    }
                }

                if (vKey == KeyCode.Escape) return KeyCode.None;

                return vKey;
            }
        }

        return KeyCode.None;
    }
}
