using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class KeyBindSystem : MonoBehaviour
{
    private bool captureKey;
    private int keyIndex;
    private KeyCode keyToBind = KeyCode.None;

    private GameObject bindUI;
    private Text timeTxt, infoTxt;

    private RectTransform content;
    [SerializeField] private GameObject keyButton;

    private TraduceSystem trad;

    private void Start()
    {
        bindUI = GameObject.Find("BindKeyUI");
        timeTxt = GameObject.Find("BindTime").GetComponent<Text>();
        infoTxt = GameObject.Find("BindInfo").GetComponent<Text>();
        content = GameObject.Find("E_KeyLayout").GetComponent<RectTransform>();
        trad = FindObjectOfType<TraduceSystem>();

        bindUI.SetActive(false);
        UpdateKeysName();
    }

    private void UpdateKeysName()
    {
        foreach (RectTransform child in content)
        {
            Destroy(child.gameObject);
        }

        int i = 0;
        
        foreach (SettingsData.PlayerInput key in SettingsData.instance.settings.playerInputs)
        {
            GameObject go = Instantiate(keyButton, content) as GameObject;
            go.GetComponentInChildren<Text>().text = string.Format(trad.GetTraduction("01_key_assign_to"),
                key.inputLabel, key.inputName.ToUpper(), key.inputDefault.ToUpper());
                
            int temp = i;
            go.GetComponent<Button>().onClick.AddListener(delegate { Btn_BindKey(temp); });
            
            i++;
        }
    }

    public void Btn_BindKey(int _keyIndex)
    {
        keyIndex = _keyIndex;
        infoTxt.text = trad.GetTraduction("01_key_to_bind") + trad.GetTraduction(SettingsData.instance.settings.playerInputs[keyIndex].inputLabel);
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
        
        timeTxt.text = string.Format(trad.GetTraduction("01_key_wait"), delay.ToString("0.0"));

        while (delay > 0)
        {
            yield return new WaitForSeconds(0.1f);
            delay -= 0.1f;
            
            if(delay > 0) timeTxt.text = string.Format(trad.GetTraduction("01_key_wait"), delay.ToString("0.0"));
            else timeTxt.text = trad.GetTraduction("01_key_press");
        }

        captureKey = true;
    }

    private void Update()
    {
        if (!captureKey) return;

        if (keyToBind != KeyCode.None)
        {
            captureKey = false;
            
            SettingsData.instance.settings.playerInputs[keyIndex].inputName = keyToBind.ToString().ToLower();
            SettingsData.instance.SaveSettings();
            
            Debug.Log("<b><color=#00AC0C>  [INFO] Key bind at " + keyToBind.ToString() + " [" + keyIndex + "]</color></b>");
            keyToBind = KeyCode.None;
            bindUI.SetActive(false);
            
            UpdateKeysName();
            SettingsData.instance.SaveSettings();
            
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
