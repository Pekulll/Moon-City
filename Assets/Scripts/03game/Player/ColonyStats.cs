using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class ColonyStats : MonoBehaviour
{
    public MoonColony colony;

    [FormerlySerializedAs("colonist")] [Header("Properties")]
    public int workers;
    public int maxColonist = 20;
    public int energy;
    public int anticipatedEnergy;

    [Header("- Ressources")]
    public int money;
    public float regolith;
    public float metal;
    public float polymer;
    public float food;

    [Header("- Storage")]
    public int energyStorage;
    public float regolithStock;
    public float metalStock;
    public float polymerStock;
    public float foodStock;

    [Header("- Profit")]
    public int profit;
    public int energyOutput;
    public float regolithOutput;
    public float metalOutput;
    public float polymerOutput;
    public float foodOutput;
    public float research;

    [Header("- Loss")]
    [Header("Statistics")]
    public int moneyLoss;
    public int energyLoss;
    public float regolithLoss;
    public float metalLoss;
    public float polymerLoss;
    public float foodLoss;

    [Header("- Gain")]
    public int moneyGain;
    public int energyGain;
    public float regolithGain;
    public float metalGain;
    public float polymerGain;
    public float foodGain;

    [Header("- Sold")]
    [Header("Trading")]
    public int regolithSold;
    public int metalSold;
    public int polymerSold;
    public int foodSold;

    [Header("- Bought")]
    public int regolithBought;
    public int metalBought;
    public int polymerBought;
    public int foodBought;

    public void CalculateOutput()
    {
        profit = moneyGain - moneyLoss;
        energyOutput = energyGain - energyLoss;
        regolithOutput = regolithGain - regolithLoss;
        metalOutput = metalGain - metalLoss;
        polymerOutput = polymerGain - polymerLoss;
        foodOutput = foodGain - foodLoss;
    }

    public void GainOutput(bool haveEnergy = true)
    {
        energy += energyOutput;
        
        if (haveEnergy)
        {
            money += profit;
            regolith += regolithOutput;
            metal += metalOutput;
            polymer += polymerOutput;
            food += foodOutput;
        }
        else
        {
            money -= moneyLoss;
            regolith -= regolithLoss;
            metal -= metalLoss;
            polymer -= polymerLoss;
            food -= foodLoss;
        }

        VerifyStock();
    }

    private void VerifyStock()
    {
        energy = Mathf.Clamp(energy, -99999, energyStorage);
        regolith = Mathf.Clamp(regolith, -99999, regolithStock);
        metal = Mathf.Clamp(metal, -99999, metalStock);
        polymer = Mathf.Clamp(polymer, -99999, polymerStock);
        food = Mathf.Clamp(food, -99999, foodStock);
    }
    
    #region Output and checkers
    
    public bool HaveEnoughResource (int colonist, int energy, int money, float regolith, float metal, float polymer, float food) {
        if (this.maxColonist < this.workers + colonist) return false;
        if (this.energy + this.anticipatedEnergy < energy) return false;
        if (this.money < money) return false;
        if (this.regolith < regolith) return false;
        if (this.metal < metal) return false;
        if (this.polymer < polymer) return false;
        if (this.food < food) return false;

        return true;
    }

    public List<int> HaveResources (int colonist, int energy, int money, float regolith, float metal, float polymer, float food) {
        List<int> ints = new List<int> ();

        if (this.maxColonist < this.workers + colonist && colonist != 0) ints.Add (0);
        if (this.energyOutput + this.anticipatedEnergy + energy < 0 && energy < 0) ints.Add (1);
        if (this.money < money && money != 0) ints.Add (2);
        if (this.regolith < regolith && regolith != 0) ints.Add (3);
        if (this.polymer < polymer && polymer != 0) ints.Add (4);
        if (this.food < food && food != 0) ints.Add (5);
        if (this.metal < metal && metal != 0) ints.Add (6);

        return ints;
    }

    public List<int> HaveResources(Building b)
    {
        int colonist = b.colonist;
        int energy = b.energy;
        int money = b.money;
        float regolith = b.regolith;
        float polymer = b.polymer;
        float food = b.food;
        float metal = b.metal;

        return HaveResources(colonist, energy, money, regolith, metal, polymer, food);
    }

    #region Add / remove output

    public void AddOutput(int energy, int money, float regolith, float metal, float polymer, float food, float research)
    {
        if (energy > 0) energyGain += Mathf.Abs(energy);
        else energyLoss += Mathf.Abs(energy);

        if (money > 0) moneyGain += Mathf.Abs(money);
        else moneyLoss += Mathf.Abs(money);

        if (regolith > 0) regolithGain += Mathf.Abs(regolith);
        else regolithLoss += Mathf.Abs(regolith);
        
        if (metal > 0) metalGain += Mathf.Abs(metal);
        else metalLoss += Mathf.Abs(metal);

        if (polymer > 0) polymerGain += Mathf.Abs(polymer);
        else polymerLoss += Mathf.Abs(polymer);

        if (food > 0) foodGain += Mathf.Abs(food);
        else foodLoss += Mathf.Abs(food);

        this.research += research;
        CalculateOutput();
    }

    public void RemoveOutput(int energy, int money, float regolith, float metal, float polymer, float food, float research)
    {
        Debug.Log("[INFO:MoonManager] Removing output...");

        if (energy > 0) energyGain -= Mathf.Abs(energy);
        else energyLoss -= Mathf.Abs(energy);

        if (money > 0) moneyGain -= Mathf.Abs(money);
        else moneyLoss -= Mathf.Abs(money);

        if (regolith > 0) regolithGain -= Mathf.Abs(regolith);
        else regolithLoss -= Mathf.Abs(regolith);
        
        if (metal > 0) metalGain -= Mathf.Abs(metal);
        else metalLoss -= Mathf.Abs(metal);

        if (polymer > 0) polymerGain -= Mathf.Abs(polymer);
        else polymerLoss -= Mathf.Abs(polymer);

        if (food > 0) foodGain -= Mathf.Abs(food);
        else foodLoss -= Mathf.Abs(food);

        this.research -= research;
        CalculateOutput();
    }

    #endregion

    #region Add / remove resources, workers and storage

    public void AddResources(int energy, int money, float regolith, float metal, float polymer, float food)
    {
        this.energy += energy;
        this.money += money;
        this.regolith += regolith;
        this.metal += metal;
        this.polymer += polymer;
        this.food += food;
        VerifyStock();
    }

    public void RemoveResources(int energy, int money, float regolith, float metal, float polymer, float food)
    {
        this.energy -= energy;
        this.money -= money;
        this.regolith -= regolith;
        this.metal -= metal;
        this.polymer -= polymer;
        this.food -= food;
    }

    public void AddSettlers(int workers, int colonist)
    {
        this.workers += workers;
        maxColonist += colonist;
    }

    public void RemoveSettlers(int workers, int colonist)
    {
        this.workers -= workers;
        maxColonist -= colonist;
    }

    public void ManageStorage (int energy, float regolith, float metal, float polymer, float food) {
        energyStorage += energy;
        regolithStock += regolith;
        metalStock += metal;
        polymerStock += polymer;
        foodStock += food;
    }
    
    #endregion
    
    #region Anticipate
    
    public void AnticipateResources (int energy)
    {
        anticipatedEnergy += energy;
    }

    public void RemoveAnticipateResources (int energy)
    {
        anticipatedEnergy -= energy;
    }
    
    #endregion
    
    #endregion
}
