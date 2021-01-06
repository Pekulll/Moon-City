using UnityEngine;

public class ColonyStats : MonoBehaviour
{
    public MoonColony colony;

    [Header("Properties")]
    public int colonist;
    public int maxColonist = 20;
    public int energy;
    public int anticipatedEnergy;

    [Header("Ressources")]
    public int money;
    public float regolith;
    public float bioPlastique;
    public float food;

    [Header("Storage")]
    public int energyStorage;
    public float regolithStock;
    public float bioPlasticStock;
    public float foodStock;

    [Header("Profit")]
    public int profit;
    public int energyOutput;
    public float regolithOutput;
    public float bioPlastiqueOutput;
    public float foodOutput;
    public float research;

    [Header("Stats - Loss")]
    public int moneyLoss;
    public int energyLoss;
    public float regolithLoss;
    public float bioPlastiqueLoss;
    public float foodLoss;

    [Header("Stats - Gain")]
    public int moneyGain;
    public int energyGain;
    public float regolithGain;
    public float bioPlastiqueGain;
    public float foodGain;

    [Header("Trade - Sold")]
    public int regolithSold;
    public int bioplasticSold;
    public int foodSold;

    [Header("Trade - Bought")]
    public int regolithBought;
    public int bioplasticBought;
    public int foodBought;

    public void CalculateOutput()
    {
        profit = moneyGain - moneyLoss;
        energyOutput = energyGain - energyLoss;
        regolithOutput = regolithGain - regolithLoss;
        bioPlastiqueOutput = bioPlastiqueGain - bioPlastiqueLoss;
        foodOutput = foodGain - foodLoss;
    }
}
