using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class TradingSystem : MonoBehaviour
{
    private MoonManager manager;

    private GameObject tradeInterface;
    private Text amountText;
    private Text regolithValue;
    private Text metalValue;
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
        metalValue = GameObject.Find("T_MarketValueMetal").GetComponent<Text>();
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

        amountText.text = amount.ToString();
        regolithValue.text = (amount * market.regolithValue).ToString("00.0") + manager.Traduce("currency");
        metalValue.text = (amount * market.metalValue).ToString("00.0") + manager.Traduce("currency");
        bioplasticValue.text = (amount * market.polymerValue).ToString("00.0") + manager.Traduce("currency");
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
            if (manager.HaveEnoughResource(0, 0, 0, amount, 0, 0, 0))
            {
                manager.RemoveResources(0, 0, amount, 0, 0, 0);
                manager.AddResources(0, (int)(market.regolithValue * amount), 0, 0, 0, 0);
                market.FluctuateRegolithValue(-1f);
                manager.colonyStats.regolithSold += amount;
                return;
            }
        }
        else
        {
            if (manager.HaveEnoughResource(0, 0, (int)(market.regolithValue * amount), 0, 0, 0, 0))
            {
                manager.AddResources(0, 0, amount, 0, 0, 0);
                manager.RemoveResources(0, (int)(market.regolithValue * amount), 0, 0, 0, 0);
                market.FluctuateRegolithValue(1f);
                manager.colonyStats.regolithBought += amount;
                return;
            }
        }
        
        manager.Notify(manager.Traduce("03_notif_factory_noresources"), priority: 2);
    }
    
    public void Btn_Metal(bool sell)
    {
        if (sell)
        {
            if (manager.HaveEnoughResource(0, 0, 0, 0, amount, 0, 0))
            {
                manager.RemoveResources(0, 0, 0, amount, 0, 0);
                manager.AddResources(0, (int)(market.metalValue * amount), 0, 0, 0, 0);
                market.FluctuateMetalValue(-1f);
                manager.colonyStats.metalSold += amount;
                return;
            }
        }
        else
        {
            if (manager.HaveEnoughResource(0, 0, (int)(market.metalValue * amount), 0, 0, 0, 0))
            {
                manager.AddResources(0, 0, 0, amount, 0, 0);
                manager.RemoveResources(0, (int)(market.metalValue * amount), 0, 0, 0, 0);
                market.FluctuateMetalValue(1f);
                manager.colonyStats.metalBought += amount;
                return;
            }
        }
        
        manager.Notify(manager.Traduce("03_notif_factory_noresources"), priority: 2);
    }

    public void Btn_Bioplastic(bool sell)
    {
        if (sell)
        {
            if (manager.HaveEnoughResource(0, 0, 0, 0, 0, amount, 0))
            {
                manager.RemoveResources(0, 0, 0, 0, amount, 0);
                manager.AddResources(0, (int)(market.polymerValue * amount), 0, 0, 0, 0);
                market.FluctuatePolymerValue(-1f);
                manager.colonyStats.polymerSold += amount;
                return;
            }
        }
        else
        {
            if (manager.HaveEnoughResource(0, 0, (int)(market.polymerValue * amount), 0, 0, 0, 0))
            {
                manager.AddResources(0, 0, 0, 0, amount, 0);
                manager.RemoveResources(0, (int)(market.polymerValue * amount), 0, 0, 0, 0);
                market.FluctuatePolymerValue(1f);
                manager.colonyStats.polymerBought += amount;
                return;
            }
        }

        manager.Notify(manager.Traduce("03_notif_factory_noresources"), priority: 2);
    }

    public void Btn_Food(bool sell)
    {
        if (sell)
        {
            if (manager.HaveEnoughResource(0, 0, 0, 0, 0, 0, amount))
            {
                manager.RemoveResources(0, 0, 0, 0, 0, amount);
                manager.AddResources(0, (int)(market.foodValue * amount), 0, 0, 0, 0);
                market.FluctuateFoodValue(-1f);
                manager.colonyStats.foodSold += amount;
                return;
            }
        }
        else
        {
            if(manager.HaveEnoughResource(0, 0, (int)(market.foodValue * amount), 0, 0, 0, 0))
            {
                manager.AddResources(0, 0, 0, 0, 0, amount);
                manager.RemoveResources(0, (int)(market.foodValue * amount), 0, 0, 0, 0);
                market.FluctuateFoodValue(1f);
                manager.colonyStats.foodBought += amount;
                return;
            }
        }
        
        manager.Notify(manager.Traduce("03_notif_factory_noresources"), priority: 2);
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
    public float metalValue;
    public float polymerValue;
    public float foodValue;

    public StockMarket(float rValue, float mValue, float pValue, float fValue)
    {
        regolithValue = rValue;
        metalValue = mValue;
        polymerValue = pValue;
        foodValue = fValue;
    }

    public void Fluctuate(float factor)
    {
        regolithValue += Random.Range(-100, 101) * factor / 100;
        metalValue += Random.Range(-100, 101) * factor / 100;
        polymerValue += Random.Range(-100, 101) * factor / 100;
        foodValue += Random.Range(-100, 101) * factor / 100;
    }

    public void FluctuateRegolithValue(float factor)
    {
        regolithValue += Random.Range(0, 101) * factor / 100;
    }
    
    public void FluctuateMetalValue(float factor)
    {
        metalValue += Random.Range(0, 101) * factor / 100;
    }

    public void FluctuatePolymerValue(float factor)
    {
        polymerValue += Random.Range(0, 101) * factor / 100;
    }

    public void FluctuateFoodValue(float factor)
    {
        foodValue += Random.Range(0, 101) * factor / 100;
    }
}