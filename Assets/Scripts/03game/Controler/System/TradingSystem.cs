using UnityEngine;
using UnityEngine.UI;

public class TradingSystem : MonoBehaviour
{
    private MoonManager manager;

    private GameObject tradeInterface;
    private Text amountText;
    private Text regolithValue;
    private Text bioplasticValue;
    private Text foodValue;

    public StockMarket market;
    private CelestialBody currentTarget;

    private int amount;
    
    public void Initialize(StockMarket market)
    {
        manager = GetComponent<MoonManager>();

        tradeInterface = GameObject.Find("BC_Trade");
        amountText = GameObject.Find("T_TradingAmount").GetComponent<Text>();
        regolithValue = GameObject.Find("T_MarketValueRegolith").GetComponent<Text>();
        bioplasticValue = GameObject.Find("T_MarketValueBioplastic").GetComponent<Text>();
        foodValue = GameObject.Find("T_MarketValueFood").GetComponent<Text>();

        this.market = market;
        ResetInterface();
    }

    private void Update()
    {
        if (!tradeInterface.activeSelf) return;
        UpdateAmount();
    }

    #region Interface and action buttons

    private void ResetInterface()
    {
        tradeInterface.SetActive(false);
        currentTarget = CelestialBody.Moon;
    }

    public void Btn_Trade()
    {
        bool active = tradeInterface.activeSelf;
        ResetInterface();

        tradeInterface.SetActive(!active);
    }

    private void UpdateAmount()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            amount = 100;
        }
        else if (Input.GetKey(KeyCode.LeftControl))
        {
            amount = 1;
        }
        else
        {
            amount = 10;
        }

        amountText.text = amount.ToString("0");
        regolithValue.text = (amount * market.regolithValue).ToString("00.0") + manager.Traduce("currency");
        bioplasticValue.text = (amount * market.bioplasticValue).ToString("00.0") + manager.Traduce("currency");
        foodValue.text = (amount * market.foodValue).ToString("00.0") + manager.Traduce("currency");
    }

    public void Btn_ChooseTarget(CelestialBody tg)
    {
        currentTarget = tg;
    }

    public void Btn_Regolith(bool sell)
    {
        if (sell)
        {
            if (manager.HaveEnoughResource(0, 0, 0, amount, 0, 0))
            {
                manager.RemoveResources(0, 0, amount, 0, 0, 0);
                manager.AddResources(0, (int)(market.regolithValue * amount), 0, 0, 0, 0);
                market.FluctuateRegolithValue(-1f);
                manager.colonyStats.regolithSold += amount;
            }
        }
        else
        {
            if (manager.HaveEnoughResource(0, 0, (int)(market.regolithValue * amount), 0, 0, 0))
            {
                manager.AddResources(0, 0, amount, 0, 0, 0);
                manager.RemoveResources(0, (int)(market.regolithValue * amount), 0, 0, 0, 0);
                market.FluctuateRegolithValue(1f);
                manager.colonyStats.regolithBought += amount;
            }
        }
    }

    public void Btn_Bioplastic(bool sell)
    {
        if (sell)
        {
            if (manager.HaveEnoughResource(0, 0, 0, 0, amount, 0))
            {
                manager.RemoveResources(0, 0, 0, 0, amount, 0);
                manager.AddResources(0, (int)(market.bioplasticValue * amount), 0, 0, 0, 0);
                market.FluctuateBioplasticValue(-1f);
                manager.colonyStats.polymerSold += amount;
            }
        }
        else
        {
            if (manager.HaveEnoughResource(0, 0, (int)(market.bioplasticValue * amount), 0, 0, 0))
            {
                manager.AddResources(0, 0, 0, 0, amount, 0);
                manager.RemoveResources(0, (int)(market.bioplasticValue * amount), 0, 0, 0, 0);
                market.FluctuateBioplasticValue(1f);
                manager.colonyStats.polymerBought += amount;
            }
        }
    }

    public void Btn_Food(bool sell)
    {
        if (sell)
        {
            if (manager.HaveEnoughResource(0, 0, 0, 0, 0, amount))
            {
                manager.RemoveResources(0, 0, 0, 0, 0, amount);
                manager.AddResources(0, (int)(market.foodValue * amount), 0, 0, 0, 0);
                market.FluctuateFoodValue(-1f);
                manager.colonyStats.foodSold += amount;
            }
        }
        else
        {
            if(manager.HaveEnoughResource(0, 0, (int)(market.foodValue * amount), 0, 0, 0))
            {
                manager.AddResources(0, 0, 0, 0, 0, amount);
                manager.RemoveResources(0, (int)(market.foodValue * amount), 0, 0, 0, 0);
                market.FluctuateFoodValue(1f);
                manager.colonyStats.foodBought += amount;
            }
        }
    }

    public void UpdateStockMarket()
    {
        market.Fluctuate(3f);
    }

    #endregion

    #region Update methods

    public void UpdateResources()
    {
        UpdateMarket();
    }

    private void UpdatePlayerResources()
    {

    }

    private void UpdateMarket()
    {

    }

    #endregion
}

[System.Serializable]
public enum CelestialBody { Venus, Earth, Moon, Ceres, Mars, Europe, Titan }

[System.Serializable]
public class StockMarket
{
    public float regolithValue;
    public float bioplasticValue;
    public float foodValue;

    public StockMarket(float rValue, float bValue, float fValue)
    {
        regolithValue = rValue;
        bioplasticValue = bValue;
        foodValue = fValue;
    }

    public void Fluctuate(float factor)
    {
        regolithValue += Random.Range(-100, 101) * factor / 100;
        bioplasticValue += Random.Range(-100, 101) * factor / 100;
        foodValue += Random.Range(-100, 101) * factor / 100;
    }

    public void FluctuateRegolithValue(float factor)
    {
        regolithValue += Random.Range(0, 101) * factor / 100;
    }

    public void FluctuateBioplasticValue(float factor)
    {
        bioplasticValue += Random.Range(0, 101) * factor / 100;
    }

    public void FluctuateFoodValue(float factor)
    {
        foodValue += Random.Range(0, 101) * factor / 100;
    }
}