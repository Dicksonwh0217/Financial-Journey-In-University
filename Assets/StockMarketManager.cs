using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StockMarketManager : MonoBehaviour
{
    [Header("Market Settings")]
    public List<Stock> availableStocks = new List<Stock>();
    public bool autoUpdatePrices = true;

    [Header("Market Hours")]
    public float marketOpenHour = 9f; // 9 AM
    public float marketCloseHour = 17f; // 5 PM

    private bool isMarketOpen = false;
    private int lastCheckedDay = -1;
    private bool hasOpenedToday = false;
    private bool hasClosedToday = false;

    public static StockMarketManager Instance { get; private set; }

    private void Awake()
    {
        // Singleton pattern with proper cleanup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Check if DayTime singleton exists
        if (DayTime.Instance == null)
        {
            Debug.LogError("StockMarketManager: DayTime singleton not found! Make sure DayTime is loaded first.");
            return;
        }

        // Initialize market state
        CheckMarketStatus();

        Debug.Log($"StockMarketManager initialized. Current game time: {GetCurrentTimeString()}");
    }

    private void Update()
    {
        if (DayTime.Instance == null) return;

        // Check if it's a new day
        if (DayTime.Instance.days != lastCheckedDay)
        {
            NewTradingDay();
            lastCheckedDay = DayTime.Instance.days;
            hasOpenedToday = false;
            hasClosedToday = false;
        }

        // Continuously check market status
        CheckMarketStatus();
    }

    private void CheckMarketStatus()
    {
        if (DayTime.Instance == null) return;

        float currentHour = DayTime.Instance.Hours;
        bool shouldBeOpen = (currentHour >= marketOpenHour && currentHour < marketCloseHour);

        // Only open if market should be open and hasn't opened today yet
        if (shouldBeOpen && !isMarketOpen && !hasOpenedToday)
        {
            OpenMarket();
            hasOpenedToday = true;
        }
        // Only close if market should be closed and hasn't closed today yet
        else if (!shouldBeOpen && isMarketOpen && !hasClosedToday)
        {
            CloseMarket();
            hasClosedToday = true;
        }
    }

    private void OpenMarket()
    {
        isMarketOpen = true;
        Debug.Log($"🔔 STOCK MARKET OPENED - Day {DayTime.Instance.days + 1} at {GetCurrentTimeString()}");

        // Update all stock market instances
        UpdateAllStockMarkets();
    }

    private void CloseMarket()
    {
        isMarketOpen = false;
        Debug.Log($"🔔 STOCK MARKET CLOSED - Day {DayTime.Instance.days + 1} at {GetCurrentTimeString()}");

        // Update all stock market instances
        UpdateAllStockMarkets();

        // End of day price updates
        EndOfDayUpdate();
    }

    private void UpdateAllStockMarkets()
    {
        // Find all StockMarket instances and update their status
        StockMarket[] stockMarkets = FindObjectsOfType<StockMarket>();
        Debug.Log($"Found {stockMarkets.Length} StockMarket instances to update");

        foreach (var market in stockMarkets)
        {
            if (market != null)
            {
                market.SetMarketStatus(isMarketOpen);
            }
        }
    }

    private void NewTradingDay()
    {
        Debug.Log($"=== NEW TRADING DAY: {DayTime.Instance.days + 1} ===");

        // Reset daily changes
        foreach (var stock in availableStocks)
        {
            if (stock != null)
            {
                stock.previousDayPrice = stock.currentPrice;
            }
        }

        // Generate overnight price changes (only if market was closed)
        if (!isMarketOpen)
        {
            GenerateOvernightPriceChanges();
        }
    }

    private void EndOfDayUpdate()
    {
        Debug.Log("Performing end-of-day price updates...");
        // This method is called when market closes, you can add any end-of-day logic here
        // But don't update prices here since they should update overnight
    }

    private void GenerateOvernightPriceChanges()
    {
        Debug.Log("Generating overnight price changes...");

        // Simulate overnight price changes
        foreach (var stock in availableStocks)
        {
            if (stock != null && stock.isActive)
            {
                float overnightChange = Random.Range(-stock.volatility * 0.3f, stock.volatility * 0.3f);
                float newPrice = stock.currentPrice * (1 + overnightChange);
                newPrice = Mathf.Max(newPrice, 1f);

                Debug.Log($"Stock {stock.name}: {stock.currentPrice:F2} -> {newPrice:F2} (Change: {overnightChange:P2})");

                stock.UpdatePrice(newPrice);
                stock.price = Mathf.RoundToInt(stock.currentPrice);
            }
        }
    }

    public bool IsMarketOpen()
    {
        return isMarketOpen;
    }

    public float GetCurrentTime()
    {
        return DayTime.Instance != null ? DayTime.Instance.Hours : 0f;
    }

    public int GetCurrentDay()
    {
        return DayTime.Instance != null ? DayTime.Instance.days + 1 : 1;
    }

    public string GetCurrentTimeString()
    {
        if (DayTime.Instance == null) return "00:00";
        return DayTime.Instance.GetTimeString();
    }

    public void RegisterStock(Stock stock)
    {
        if (stock != null && !availableStocks.Contains(stock))
        {
            availableStocks.Add(stock);
            Debug.Log($"Registered stock: {stock.name}");
        }
    }

    // Method to manually trigger market state for testing
    [ContextMenu("Force Open Market")]
    public void ForceOpenMarket()
    {
        OpenMarket();
        hasOpenedToday = true;
    }

    [ContextMenu("Force Close Market")]
    public void ForceCloseMarket()
    {
        CloseMarket();
        hasClosedToday = true;
    }

    [ContextMenu("Print Current Status")]
    public void PrintCurrentStatus()
    {
        Debug.Log($"Current Time: {GetCurrentTimeString()}, Day: {GetCurrentDay()}, Market Open: {isMarketOpen}");
        Debug.Log($"Available Stocks: {availableStocks.Count}");
        Debug.Log($"Has Opened Today: {hasOpenedToday}, Has Closed Today: {hasClosedToday}");
    }

    [ContextMenu("Skip to Market Open")]
    public void SkipToMarketOpen()
    {
        if (DayTime.Instance != null)
        {
            float currentHour = DayTime.Instance.Hours;
            if (currentHour < marketOpenHour)
            {
                // Skip to market open time today
                DayTime.Instance.SkipTime(hours: marketOpenHour - currentHour);
            }
            else
            {
                // Skip to market open time tomorrow
                DayTime.Instance.SkipTime(hours: (24 - currentHour) + marketOpenHour);
            }
        }
    }
}