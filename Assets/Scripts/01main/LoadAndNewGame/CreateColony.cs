using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CreateColony : MonoBehaviour
{
    [Header("Difficulty")]
    [SerializeField] private Difficulty[] difficulties;
    [SerializeField] private Difficulty[] customs;
    [SerializeField] private Sprite[] flags;

    [Header("Blank save")]
    [SerializeField] private Building home;
    [SerializeField] private Units constructor;

    private int currentId = 0, currentCustomId = 1, currentFlagIndex = 0;
    private Color colonyColor;

    private InputField colonyName, seedInput;

    private Text errorTxt, textButonLeft, textButtonRight;
    private Text waveRate, enemiesCount;

    private Image createColony;
    private Outline easy, normal, hard, custom;
    private Image flagLeft, flagMiddle, flagRight;
    private Outline high, standard, defensive, low;

    private Slider waveSld, countSld;

    private GameSettings currentGameSettings;

    private GameObject customDifficulty, m_Gamemodes, m_GameSettingsInterface;

    private TraduceSystem traductor;
    private ColorManager colorManager;

    private void Awake()
    {
        createColony = GameObject.Find("CreateColony").GetComponent<Image>();
        colorManager = FindObjectOfType<ColorManager>();

        easy = GameObject.Find("Easy").GetComponentInChildren<Outline>();
        normal = GameObject.Find("Normal").GetComponentInChildren<Outline>();
        hard = GameObject.Find("Hard").GetComponentInChildren<Outline>();
        custom = GameObject.Find("Custom").GetComponentInChildren<Outline>();

        high = GameObject.Find("High").GetComponentInChildren<Outline>();
        standard = GameObject.Find("Standard").GetComponentInChildren<Outline>();
        defensive = GameObject.Find("Defensive").GetComponentInChildren<Outline>();
        low = GameObject.Find("Low").GetComponentInChildren<Outline>();

        flagRight = GameObject.Find("Flag1").GetComponent<Image>();
        flagMiddle = GameObject.Find("MainFlag").GetComponent<Image>();
        flagLeft = GameObject.Find("Flag-1").GetComponent<Image>();

        colonyName = GameObject.Find("ColonyName").GetComponent<InputField>();
        seedInput = GameObject.Find("IF_GameSeed").GetComponent<InputField>();

        errorTxt = GameObject.Find("ErrorText").GetComponent<Text>();
        textButonLeft = GameObject.Find("Text_ButtonLeft").GetComponent<Text>();
        textButtonRight = GameObject.Find("Text_ButtonRight").GetComponent<Text>();
        waveRate = GameObject.Find("Text_WaveRate").GetComponent<Text>();
        enemiesCount = GameObject.Find("Text_Count").GetComponent<Text>();

        waveSld = GameObject.Find("Sld_WaveRate").GetComponent<Slider>();
        countSld = GameObject.Find("Sld_Count").GetComponent<Slider>();

        currentGameSettings = new GameSettings();
        customDifficulty = GameObject.Find("CustomSettings");
        m_Gamemodes = GameObject.Find("Gamemode");
        m_GameSettingsInterface = GameObject.Find("ClassiqueSettings");

        m_GameSettingsInterface.SetActive(false);
        customDifficulty.SetActive(false);

        traductor = FindObjectOfType<TraduceSystem>();

        RandomizeSeed();

        SetColor(new Color(.5351f, 0, .8392f, 1));
        SetDifficulty(1);
        ChangeFlag(0);
        createColony.color = colonyColor;
    }

    #region Gamemode

    public void DefendAndConquest()
    {
        m_GameSettingsInterface.SetActive(true);
    }

    #endregion

    #region Custom difficulty

    private void Btn_ShowCustom()
    {
        if(currentGameSettings.gameDifficulty == difficulties[0] || currentGameSettings.gameDifficulty == difficulties[1] || currentGameSettings.gameDifficulty == difficulties[2])
        {
            SetCustomDifficulty(1);

            currentGameSettings.gameDifficulty.enemyWaveRate = 10;
            currentGameSettings.gameDifficulty.enemiesCount = 3;

            waveSld.value = 10;
            countSld.value = 3;

            waveRate.text = waveSld.value + "%";
            enemiesCount.text = countSld.value + " / 10";
        }

        customDifficulty.SetActive(true);
    }

    public void SetCustomDifficulty(int id)
    {
        currentGameSettings.gameDifficulty = customs[id];
        currentCustomId = id;

        UpdateCustomColor();
    }

    public void CustomWaveRate()
    {
        waveRate.text = waveSld.value + "%";
        currentGameSettings.gameDifficulty.enemyWaveRate = (int)waveSld.value;
    }

    public void CustomEnemyCount()
    {
        enemiesCount.text = countSld.value + " / 10";
        currentGameSettings.gameDifficulty.enemiesCount = (int)countSld.value;
    }

    public void CustomDone()
    {
        customDifficulty.SetActive(false);
    }

    #endregion

    #region Classic settings

    public void SetDifficulty(int id)
    {
        currentId = id;

        UpdateColor();

        if (id != 3)
        {
            currentGameSettings.gameDifficulty = difficulties[id];
        }
        else
        {
            Btn_ShowCustom();
        }
    }

    public void SetColor(Image img)
    {
        SetColor(img.color);
        UpdateColor();
        UpdateCustomColor();
    }

    private void SetColor(Color color)
    {
        colonyColor = color;
        currentGameSettings.colonyColor = color;

        createColony.color = colonyColor;
        flagMiddle.color = colonyColor;

        textButtonRight.color = colonyColor;
        textButonLeft.color = colonyColor;

        UpdateColor();
    }

    public void UpdateColor()
    {
        easy.effectColor = colorManager.backgroundBorder;
        normal.effectColor = colorManager.backgroundBorder;
        hard.effectColor = colorManager.backgroundBorder;
        custom.effectColor = colorManager.backgroundBorder;

        if (currentId == 0)
        {
            easy.effectColor = colonyColor;
        }
        else if (currentId == 1)
        {
            normal.effectColor = colonyColor;
        }
        else if (currentId == 2)
        {
            hard.effectColor = colonyColor;
        }
        else if (currentId == 3)
        {
            custom.effectColor = colonyColor;
        }
    }

    private void UpdateCustomColor()
    {
        high.effectColor = colorManager.backgroundBorder;
        standard.effectColor = colorManager.backgroundBorder;
        defensive.effectColor = colorManager.backgroundBorder;
        low.effectColor = colorManager.backgroundBorder;

        if (currentCustomId == 0)
        {
            high.effectColor = colonyColor;
        }
        else if (currentCustomId == 1)
        {
            standard.effectColor = colonyColor;
        }
        else if (currentCustomId == 2)
        {
            defensive.effectColor = colonyColor;
        }
        else if (currentCustomId == 3)
        {
            low.effectColor = colonyColor;
        }
    }

    public void ChangeFlag(int index)
    {
        currentFlagIndex += index;

        if (currentFlagIndex < 0)
        {
            currentFlagIndex = flags.Length - 1;
        }
        else if(currentFlagIndex > flags.Length - 1)
        {
            currentFlagIndex = 0;
        }

        flagMiddle.sprite = flags[currentFlagIndex];
        currentGameSettings.flagId = currentFlagIndex;

        if(currentFlagIndex + 1 > flags.Length - 1)
        {
            flagRight.sprite = flags[0];
        }
        else
        {
            flagRight.sprite = flags[currentFlagIndex + 1];
        }


        if (currentFlagIndex - 1 < 0)
        {
            flagLeft.sprite = flags[flags.Length - 1];
        }
        else
        {
            flagLeft.sprite = flags[currentFlagIndex - 1];
        }
    }

    public void ChangeSeed()
    {
        if(seedInput.text != "")
        {
            currentGameSettings.seed = int.Parse(seedInput.text);
        }
        else
        {
            seedInput.text = currentGameSettings.seed.ToString();
        }
    }

    public void RandomizeSeed()
    {
        currentGameSettings.seed = Random.Range(-999999, 1000000);
        seedInput.text = currentGameSettings.seed.ToString();
    }

    #endregion

    #region Create colony

    public void CreateNewColony()
    {
        if (colonyName.text.Length < 5) { errorTxt.text = "<color=#DA0209>" + traductor.GetTraduction("This colony name is invalid! (too short)") + "</color>"; return; }
        if (colonyName.text.Length > 20) { errorTxt.text = "<color=#DA0209>" + traductor.GetTraduction("This colony name is invalid! (too long)") + "</color>"; return; }

        CreateBlankSave();
        PlayerPrefs.SetString("CurrentSave", colonyName.text);
        SceneManager.LoadScene("02loading");
    }

    private void CreateBlankSave()
    {
        SavedManager manager = new SavedManager(0);

        SavedPlayer player = new SavedPlayer(
            0, new float[3] { colonyColor.r, colonyColor.g, colonyColor.b }, colonyName.text, currentGameSettings
        );

        SavedUnit[] units = new SavedUnit[2]
        {
            new SavedUnit(constructor, new float[3] { -5, 2, -25 } ),
            new SavedUnit(constructor, new float[3] { 5, 2, -25 } )
        };

        SavedBuilding[] buildings = new SavedBuilding[1]
        {
            new SavedBuilding(home, new float[3] { 0, 0, 0 })
        };

        SavedConfiguration config = new SavedConfiguration(currentGameSettings);

        SaveSystem.Save(colonyName.text + ".json", new SavedScene("EMPTY", manager, player, config, buildings, units));
    }

    #endregion

    private void OnDisable()
    {
        m_GameSettingsInterface.SetActive(false);
        customDifficulty.SetActive(false);
    }
}
