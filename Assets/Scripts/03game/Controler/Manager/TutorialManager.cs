using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour
{
    private int tutorialStep;
    private int tutorialPart;

    [SerializeField] private List<TutorialPart> tutorials;

    private GameObject endScreen;

    private QuestManager quest;
    private WaveSystem wave;

    private int questAdvancement;

    private MoonManager manager;

    public void InitTutorial()
    {
        manager = FindObjectOfType<MoonManager>();
        quest = FindObjectOfType<QuestManager>();
        wave = FindObjectOfType<WaveSystem>();
        endScreen = GameObject.Find("EndScreenTutorial");

        endScreen.SetActive(false);
        ResetTutorials();
        
        if (manager.saveName == "Tutorial" && manager.data.iteration == 0)
        {
            tutorials[tutorialPart].background.SetActive(true);
            tutorials[0].steps[0].SetActive(true);
        }
        else
        {
            Destroy(GameObject.Find("TutorialUI"));
        }
    }

    public void NextStep()
    {
        ResetStep(tutorialPart);
        tutorialStep++;
        tutorials[tutorialPart].steps[tutorialStep].SetActive(true);
    }

    public void NextPart()
    {
        ResetTutorials();
        tutorialPart++;
        tutorialStep = 0;

        tutorials[tutorialPart].background.SetActive(true);
        tutorials[tutorialPart].steps[tutorialStep].SetActive(true);
    }

    private void ResetTutorials()
    {
        ResetPart();
    }

    private void ResetStep(int part)
    {
        foreach (GameObject go in tutorials[part].steps)
        {
            go.SetActive(false);
        }
    }

    private void ResetPart()
    {
        for(int i = 0; i < tutorials.Count; i++)
        {
            ResetStep(i);
            tutorials[i].background.SetActive(false);
        }
    }

    public void TutorialEnd()
    {
        ResetTutorials();
        AddQuests();
    }

    public void AddQuests()
    {
        if (quest == null) return;

        if (questAdvancement == 0)
        {
            quest.NewBuildQuest(
                new BuildQuest(
                    "Essential buildings",
                    "Every colony needs survival building. Build an apartment and a farm, via the build menu.",
                    EntityType.Building,
                    new int[2] { 1, 4 },
                    new int[2] { 1, 1 },
                    new QuestReward[1] { new QuestReward(RewardType.Food, 10) },
                    "/tutonextquest"
                    )
                );
            questAdvancement++;
        }
        else if(questAdvancement == 1)
        {
            quest.NewBuildQuest(
                new BuildQuest(
                    "Self-sufficient",
                    "It is important to be self-sufficient. Build an excavator and a biofactory, via the construction menu.",
                    EntityType.Building, new int[2] { 9, 16 },
                    new int[2] { 1, 1 },
                    new QuestReward[2] { new QuestReward(RewardType.Regolith, 15), new QuestReward(RewardType.Bioplastic, 10) },
                    "/tutonextquest"
                )
            );

            questAdvancement++;
        }
        else if(questAdvancement == 2)
        {
            quest.NewBuildQuest(
                new BuildQuest(
                    "Prepare your defenses",
                    "It's time to prepare our colony for a possible enemy attack. Recruit gravitational soldiers in a barracks.",
                    EntityType.Unit,
                    new int[1] { 1 },
                    new int[1] { 4 },
                    new QuestReward[1] { new QuestReward(RewardType.Food, 10) },
                    "/tutonextquest"
                    )
                );
            questAdvancement++;
        }
        else if(questAdvancement == 3)
        {
            manager.SendCommand("/wave 0");
            quest.NewKillQuest(
                new KillQuest(
                    "Invasion",
                    "We're being invaded! Defend our colony by eliminating all enemy units. They're coming from the east!",
                    EntityType.Unit,
                    new int[1] { 1 },
                    new int[1] { 3 },
                    new QuestReward[0],
                    "/tutoend"
                    )
                );
            questAdvancement++;
        }
    }

    public void SpawnWave()
    {
        EnemieArmies current = new EnemieArmies();
        current.difficultyIndex = 1;
        current.interligneSpacing = 5;
        current.maxUnitPerLigne = 2;
        current.name = "Tutorial";
        current.spacing = 4;

        EnemieUnits units = new EnemieUnits();
        units.identityOfUnit = 1;
        units.name = "Gravity soldier";
        units.numberOfUnit = 3;

        current.units.Add(units);
        current.waveEvent = null;

        wave.ForcedNewWave(current);
    }

    public void EndTutorial()
    {
        endScreen.SetActive(true);
    }

    public void ReturnToMain()
    {
        PlayerPrefs.DeleteKey("CurrentSave");

        for (int i = 0; PlayerPrefs.HasKey("Save" + i); i++)
        {
            if (PlayerPrefs.GetString("Save" + i) == "Tutorial")
            {
                PlayerPrefs.DeleteKey("Save" + i);
                break;
            }
        }

        N_SaveSystem.DeleteData("Tutorial");
        SceneManager.LoadScene("00loading");
    }

    public void SkipTutorial()
    {
        ResetTutorials();
        EndTutorial();
    }
}

[System.Serializable]
public struct TutorialPart
{
    public GameObject[] steps;
    public GameObject background;
}
