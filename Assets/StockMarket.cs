using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StockMarket : Interactable
{
    [Header("Stock Market Settings")]
    public ItemContainer stockInventory; // Contains available stocks
    public float marketOpenTime = 9f; // 9 AM
    public float marketCloseTime = 17f; // 5 PM
    public bool isMarketOpen = false; // Changed default to false

    [Header("Price Update Settings")]
    public float priceUpdateIntervalMinutes = 5f; // Update prices every 5 game minutes
    public float marketVolatilityMultiplier = 1f;
    public float afterHoursVolatilityMultiplier = 0.5f; // Reduced volatility when market is closed

    private StockMarketManager stockMarketManager;
    private float lastPriceUpdateTime; // Store the last game time when prices were updated
    private int lastUpdateDay = -1; // Track which day we last updated prices

    private void Start()
    {
        // Wait for StockMarketManager to initialize first
        StartCoroutine(InitializeAfterManager());
    }

    private IEnumerator InitializeAfterManager()
    {
        // Wait until StockMarketManager instance is available
        while (StockMarketManager.Instance == null)
        {
            yield return null;
        }

        stockMarketManager = StockMarketManager.Instance;

        // Check if DayTime singleton exists
        if (DayTime.Instance == null)
        {
            Debug.LogError("DayTime singleton not found! Make sure DayTime is loaded and has the singleton pattern.");
            yield break;
        }

        // Sync with manager's current market status
        isMarketOpen = stockMarketManager.IsMarketOpen();
        Debug.Log($"StockMarket initialized - synced market status: {(isMarketOpen ? "OPEN" : "CLOSED")}");

        // Initialize stock prices if not already set
        InitializeStockPrices();

        // Set initial update time
        lastPriceUpdateTime = GetCurrentGameTimeInMinutes();
        lastUpdateDay = DayTime.Instance.days;
    }

    private void Update()
    {
        if (DayTime.Instance == null || stockMarketManager == null) return;

        // Ensure we're always synced with the manager
        bool managerMarketStatus = stockMarketManager.IsMarketOpen();
        if (isMarketOpen != managerMarketStatus)
        {
            Debug.Log($"Market status mismatch detected. Syncing to manager status: {(managerMarketStatus ? "OPEN" : "CLOSED")}");
            isMarketOpen = managerMarketStatus;
        }

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
        if (DayTime.Instance != null)
        {
            string marketStatus = isMarketOpen ? "OPEN" : "CLOSED";
            Debug.Log($"Updating stock prices at game time: {DayTime.Instance.GetTimeString()} (Market: {marketStatus})");
        }

        foreach (var slot in stockInventory.slots)
        {
            if (slot.item != null && slot.item is Stock stock && stock.isActive)
            {
                // Adjust volatility based on market status
                float volatilityMultiplier = isMarketOpen ? marketVolatilityMultiplier : afterHoursVolatilityMultiplier;

                // Calculate trend component (with small random variation)
                float trendVariation = Random.Range(-stock.trendVolatility, stock.trendVolatility);
                float trendComponent = stock.trendDirection + trendVariation;

                // Calculate volatility component (random fluctuation)
                float volatilityComponent = Random.Range(-stock.volatility, stock.volatility) * volatilityMultiplier;

                // Combine trend and volatility for total change
                float totalChange = trendComponent + volatilityComponent;

                // Apply the change
                float newPrice = stock.currentPrice * (1 + totalChange);

                // Prevent price from going below 1
                newPrice = Mathf.Max(newPrice, 1f);

                string marketStatus = isMarketOpen ? "OPEN" : "CLOSED";
                Debug.Log($"Stock {stock.name} ({marketStatus}): {stock.currentPrice:F2} -> {newPrice:F2} " +
                         $"(Trend: {trendComponent:P3}, Volatility: {volatilityComponent:P3}, Total: {totalChange:P3})");

                stock.UpdatePrice(newPrice);
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
        // Always check with the manager for the most up-to-date status
        if (stockMarketManager != null)
        {
            return stockMarketManager.IsMarketOpen();
        }
        return isMarketOpen;
    }

    public void SetMarketStatus(bool open)
    {
        bool previousStatus = isMarketOpen;
        isMarketOpen = open;

        string timeString = DayTime.Instance != null ? DayTime.Instance.GetTimeString() : "Unknown";
        Debug.Log($"Stock Market status changed from {(previousStatus ? "OPEN" : "CLOSED")} to: {(open ? "OPEN" : "CLOSED")} at {timeString}");

        // If market just opened, do an immediate price update
        if (open && !previousStatus && DayTime.Instance != null)
        {
            Debug.Log("Market just opened - triggering immediate price update");
            UpdateStockPrices();
            lastPriceUpdateTime = GetCurrentGameTimeInMinutes();
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
            Debug.Log($"Local Market Status: {(isMarketOpen ? "OPEN" : "CLOSED")}");
            if (stockMarketManager != null)
            {
                Debug.Log($"Manager Market Status: {(stockMarketManager.IsMarketOpen() ? "OPEN" : "CLOSED")}");
            }
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

    [ContextMenu("Force Sync With Manager")]
    public void ForceSyncWithManager()
    {
        if (stockMarketManager != null)
        {
            bool managerStatus = stockMarketManager.IsMarketOpen();
            Debug.Log($"Forcing sync: Local={isMarketOpen}, Manager={managerStatus}");
            SetMarketStatus(managerStatus);
        }
        else
        {
            Debug.LogError("StockMarketManager not found!");
        }
    }
}