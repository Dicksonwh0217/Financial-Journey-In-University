using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StockMarket : Interactable
{
    [Header("Stock Market Settings")]
    public ItemContainer stockInventory; // Contains available stocks
    public float marketOpenTime = 9f; // 9 AM
    public float marketCloseTime = 17f; // 5 PM
    public bool isMarketOpen = true;

    [Header("Price Update Settings")]
    public float priceUpdateIntervalMinutes = 5f; // Update prices every 5 game minutes
    public float marketVolatilityMultiplier = 1f;

    private StockMarketManager stockMarketManager;
    private float lastPriceUpdateTime; // Store the last game time when prices were updated
    private int lastUpdateDay = -1; // Track which day we last updated prices

    private void Start()
    {
        stockMarketManager = FindFirstObjectByType<StockMarketManager>();
        if (stockMarketManager == null)
        {
            Debug.LogError("StockMarketManager not found! Please add one to the scene.");
        }

        // Check if DayTime singleton exists
        if (DayTime.Instance == null)
        {
            Debug.LogError("DayTime singleton not found! Make sure DayTime is loaded and has the singleton pattern.");
        }

        // Initialize stock prices if not already set
        InitializeStockPrices();

        // Set initial update time
        if (DayTime.Instance != null)
        {
            lastPriceUpdateTime = GetCurrentGameTimeInMinutes();
            lastUpdateDay = DayTime.Instance.days;
        }
    }

    private void Update()
    {
        if (DayTime.Instance == null) return;

        // Check if it's a new day - reset update tracking
        if (DayTime.Instance.days != lastUpdateDay)
        {
            lastUpdateDay = DayTime.Instance.days;
            lastPriceUpdateTime = GetCurrentGameTimeInMinutes();
        }

        // Check if enough game time has passed for price update
        float currentGameTimeInMinutes = GetCurrentGameTimeInMinutes();
        float timeSinceLastUpdate = currentGameTimeInMinutes - lastPriceUpdateTime;

        // Handle day rollover case
        if (timeSinceLastUpdate < 0)
        {
            timeSinceLastUpdate += 24 * 60; // Add 24 hours worth of minutes
        }

        if (timeSinceLastUpdate >= priceUpdateIntervalMinutes)
        {
            UpdateStockPrices();
            lastPriceUpdateTime = currentGameTimeInMinutes;
        }
    }

    private float GetCurrentGameTimeInMinutes()
    {
        if (DayTime.Instance == null) return 0f;

        // Convert game time to total minutes from start of day
        float totalMinutes = (DayTime.Instance.Hours * 60f) + DayTime.Instance.Minutes;
        return totalMinutes;
    }

    private void InitializeStockPrices()
    {
        foreach (var slot in stockInventory.slots)
        {
            if (slot.item != null && slot.item is Stock stock)
            {
                if (stock.currentPrice == 0)
                {
                    stock.currentPrice = stock.basePrice;
                    stock.previousDayPrice = stock.basePrice;
                }

                // Register stock with the manager
                if (stockMarketManager != null)
                {
                    stockMarketManager.RegisterStock(stock);
                }
            }
        }
    }

    private void UpdateStockPrices()
    {
        if (!isMarketOpen)
        {
            Debug.Log("Market is closed - skipping price update");
            return;
        }

        if (DayTime.Instance != null)
        {
            Debug.Log($"Updating stock prices at game time: {DayTime.Instance.GetTimeString()}");
        }

        foreach (var slot in stockInventory.slots)
        {
            if (slot.item != null && slot.item is Stock stock && stock.isActive)
            {
                // Simple price simulation with volatility
                float randomChange = Random.Range(-stock.volatility, stock.volatility);
                float newPrice = stock.currentPrice * (1 + randomChange * marketVolatilityMultiplier);

                // Prevent price from going below 1
                newPrice = Mathf.Max(newPrice, 1f);

                Debug.Log($"Stock {stock.name}: {stock.currentPrice:F2} -> {newPrice:F2} (Change: {randomChange:P2})");

                stock.UpdatePrice(newPrice);

                // Update the item's price for trading system
                stock.price = Mathf.RoundToInt(stock.currentPrice);
            }
        }
    }

    public override void Interact(Character character)
    {
        StockTrading stockTrading = character.GetComponent<StockTrading>();
        stockTrading.BeginStockTrading(this);
    }

    public bool IsMarketOpen()
    {
        return isMarketOpen;
    }

    public void SetMarketStatus(bool open)
    {
        isMarketOpen = open;
        string timeString = DayTime.Instance != null ? DayTime.Instance.GetTimeString() : "Unknown";
        Debug.Log($"Stock Market status changed to: {(open ? "OPEN" : "CLOSED")} at {timeString}");

        // If market just opened, do an immediate price update
        if (open && DayTime.Instance != null)
        {
            UpdateStockPrices();
        }
    }

    // Method to manually trigger price update for testing
    [ContextMenu("Force Price Update")]
    public void ForcePriceUpdate()
    {
        UpdateStockPrices();
        if (DayTime.Instance != null)
        {
            lastPriceUpdateTime = GetCurrentGameTimeInMinutes();
        }
    }

    [ContextMenu("Print Market Status")]
    public void PrintMarketStatus()
    {
        if (DayTime.Instance != null)
        {
            Debug.Log($"Market Status: {(isMarketOpen ? "OPEN" : "CLOSED")}");
            Debug.Log($"Current Game Time: {DayTime.Instance.GetTimeString()}");
            Debug.Log($"Current Day: {DayTime.Instance.days}");
            Debug.Log($"Day of Week: {DayTime.Instance.GetDayOfWeek()}");
            Debug.Log($"Last Price Update: {lastPriceUpdateTime:F1} minutes into day");
            Debug.Log($"Next Update In: {(priceUpdateIntervalMinutes - (GetCurrentGameTimeInMinutes() - lastPriceUpdateTime)):F1} minutes");
        }
        else
        {
            Debug.LogError("DayTime.Instance is null! Make sure DayTime singleton is properly set up.");
        }
    }
}