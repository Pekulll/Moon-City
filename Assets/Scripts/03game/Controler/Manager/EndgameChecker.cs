using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndgameChecker : MonoBehaviour
{
    private ResearchSystem researchSystem;
    private MoonManager manager;
    private SpeedManager speedManager;

    public List<EndgameType> validType = new List<EndgameType>();

    private GameObject endgame;
    private GameObject scorePanel;
    private GameObject statsPanel;

    private Text score, scoreTitle;
    private Text result;

    private void Start()
    {
        manager = GetComponent<MoonManager>();
        speedManager = GetComponent<SpeedManager>();
        researchSystem = GetComponent<ResearchSystem>();

        endgame = GameObject.Find("C_Endgame");
        scorePanel = GameObject.Find("I_Score");
        statsPanel = GameObject.Find("I_Statistics");

        score = GameObject.Find("T_StatsSummary").GetComponent<Text>();
        scoreTitle = GameObject.Find("T_StatsSummaryTitle").GetComponent<Text>();
        result = GameObject.Find("T_GameEndState").GetComponent<Text>();

        //TODO: Save integration
        //Without save
        validType = new List<EndgameType>() { EndgameType.Economic, EndgameType.Monument, EndgameType.Cheat };

        StartCoroutine(VictoryChecker());
        StartCoroutine(DefeatChecker());
        StartCoroutine(CommonChecker());

        ResetInterafce();
    }

    #region Victory

    private IEnumerator VictoryChecker()
    {
        WaitForSeconds wait = new WaitForSeconds(2f);
        yield return new WaitForSeconds(2f);

        while (true)
        {
            bool military = EnemyColonyExists();

            if (!military)
            {
                DeclareVictory(EndgameType.Military);
                break;
            }

            yield return wait;
        }
    }

    private bool EnemyColonyExists()
    {
        return manager.FindTags(new Tag[2] { Tag.Core, Tag.Enemy }).Length != 0;
    }

    #endregion

    #region Defeat

    private IEnumerator DefeatChecker()
    {
        WaitForSeconds wait = new WaitForSeconds(2f);
        yield return new WaitForSeconds(2f);

        while (true)
        {
            bool military = PlayerColonyExists();

            if (!military)
            {
                DeclareDefeat(EndgameType.Military);
                break;
            }

            yield return wait;
        }
    }

    private bool PlayerColonyExists()
    {
        GameObject[] buildings = manager.FindTag(Tag.Core);

        foreach(GameObject b in buildings)
        {
            Entity e = b.GetComponent<Entity>();

            if(e.side == manager.side)
            {
                return true;
            }
        }

        return false;
    }

    #endregion

    #region Common

    private IEnumerator CommonChecker()
    {
        WaitForSeconds wait = new WaitForSeconds(2f);
        yield return new WaitForSeconds(2f);

        while (true)
        {
            if (MonumentExists()) break;
            yield return wait;
        }
    }

    private bool MonumentExists()
    {
        GameObject[] buildings = manager.FindTag(Tag.Building);

        foreach (GameObject b in buildings)
        {
            Entity e = b.GetComponent<Entity>();

            if (e.id == 44 && e.side == 0)
            {
                DeclareVictory(EndgameType.Monument);
                return true;
            }
            else if (e.id == 44)
            {
                DeclareDefeat(EndgameType.Monument);
                return true;
            }
        }

        return false;
    }

    #endregion

    #region Interface

    private void ResetInterafce()
    {
        endgame.SetActive(false);
        scorePanel.SetActive(false);
        statsPanel.SetActive(false);
    }

    public void DeclareVictory(EndgameType type)
    {
        if (!validType.Contains(type)) return;

        Debug.Log("[INFO] Victory! " + type);
        result.text = manager.Traduce("03_ui_eg_victory");
        DisplayEndgameScreen(type);
    }

    public void DeclareDefeat(EndgameType type)
    {
        if (!validType.Contains(type)) return;

        Debug.Log("[INFO] Defeat! " + type);
        result.text = manager.Traduce("03_ui_eg_defeat");
        DisplayEndgameScreen(type);
    }

    private void DisplayEndgameScreen(EndgameType type)
    {
        ResetInterafce();
        DisplayScore(type);

        speedManager.ChangeSpeed(0);
        endgame.SetActive(true);
    }

    private void DisplayScore(EndgameType type)
    {
        int[] calculatedScore = CalculateScore();

        scoreTitle.text = "<size=25><b>" + manager.Traduce("03_ui_eg_score") + "</b></size>\n"
            + manager.Traduce("03_ui_eg_buildings") + "\n"
            + manager.Traduce("03_ui_eg_units") + "\n"
            + manager.Traduce("03_ui_eg_techs") + "\n"
            + manager.Traduce("03_ui_eg_trading") + "\n"
            + manager.Traduce("03_ui_eg_eco");

        if (type != EndgameType.Cheat)
            score.text = "<size=25><b>" + calculatedScore[0].ToString() + "</b></size>\n"
                + calculatedScore[1].ToString() + "\n"
                + calculatedScore[2].ToString() + "\n"
                + calculatedScore[3].ToString() + "\n"
                + calculatedScore[4].ToString() + "\n"
                + calculatedScore[5].ToString();
        else
            score.text = "<size=25><b> </b>Cheating</size>\n0\n0\n0\n0\n0";

        scorePanel.SetActive(true);
    }

    private int[] CalculateScore()
    {
        int[] calculatedScore = new int[6];

        GameObject[] buildings = manager.FindTag(Tag.Building);

        foreach(GameObject b in buildings)
        {
            Entity e = b.GetComponent<Entity>();

            if(e.side == manager.side)
            {
                calculatedScore[1] += (int)((e.health + e.shield + e.energy) / (e.maxHealth + e.maxShield + e.maxEnergy) * (e.maxHealth + e.maxShield));
            }
        }

        GameObject[] units = manager.FindTag(Tag.Unit);

        foreach (GameObject u in units)
        {
            Unit e = u.GetComponent<Unit>();

            if (e.side == manager.side)
            {
                calculatedScore[2] += (int)(((e.health + e.shield + e.energy) / (e.maxHealth + e.maxShield + e.maxEnergy)) * e.level * (e.killCount + 1));
            }
        }

        calculatedScore[3] = researchSystem.techUnlock.Count * 10;

        calculatedScore[4] = (Mathf.Abs(manager.colonyStats.regolithBought - manager.colonyStats.regolithSold)
            + Mathf.Abs(manager.colonyStats.metalBought - manager.colonyStats.metalSold) * 2
            + Mathf.Abs(manager.colonyStats.polymerBought - manager.colonyStats.polymerSold) * 2
            + Mathf.Abs(manager.colonyStats.foodBought - manager.colonyStats.foodSold)) * 10;

        calculatedScore[5] = (int)(manager.colonyStats.money + manager.colonyStats.regolith * 1.5f
            + (manager.colonyStats.metal + manager.colonyStats.polymer) * 2 
            + manager.colonyStats.food * 3);

        for(int i = 1; i < calculatedScore.Length; i++)
        {
            calculatedScore[0] += calculatedScore[i];
        }

        return calculatedScore;
    }

    #endregion
}

[System.Serializable]
public enum EndgameType { Economic, Military, Monument, Cheat }
