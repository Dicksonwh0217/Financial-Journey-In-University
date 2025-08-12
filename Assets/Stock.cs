using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Stock")]
public class Stock : Item
{
    [Header("Stock Specific Properties")]
    public string stockSymbol;
    public float basePrice = 100f; // Starting price in RM
    public float currentPrice;
    public float previousDayPrice;
    public float volatility = 0.05f;
    public bool isActive = true;

    [Header("Price History")]
    public List<float> priceHistory = new List<float>();
    public int maxHistoryLength = 30;

    public System.Action<Stock> OnPriceChanged;

    private void OnEnable()
    {
        itemType = ItemType.Stock;

        if (currentPrice == 0)
        {
            currentPrice = basePrice;
            previousDayPrice = basePrice;
        }
    }

    public void UpdatePrice(float newPrice)
    {
        previousDayPrice = currentPrice;
        currentPrice = newPrice;

        priceHistory.Add(currentPrice);
        if (priceHistory.Count > maxHistoryLength)
        {
            priceHistory.RemoveAt(0);
        }

        // Trigger event when price changes
        OnPriceChanged?.Invoke(this);
    }

    public float GetDailyChangePercent()
    {
        if (previousDayPrice == 0) return 0f;
        return ((currentPrice - previousDayPrice) / previousDayPrice) * 100f;
    }

    public bool IsUpToday()
    {
        return currentPrice > previousDayPrice;
    }
}
