using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StockMarketManager : MonoBehaviour
{
    [Header("Market Settings")]
    public List<Stock> availableStocks = new List<Stock>();
    public bool autoUpdatePrices = true;
    public bool allowAfterHoursTrading = true; // Allow price updates when market is closed

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
            Debug.Log("StockMarketManager singleton created");
        }
        else
        {
            Debug.Log("StockMarketManager singleton already exists, destroying duplicate");
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        StartCoroutine(InitializeManager());
    }

    private IEnumerator InitializeManager()
    {
        // Wait for DayTime to be ready
        int attempts = 0;
        while (DayTime.Instance == null && attempts < 100)
        {
            yield return new WaitForSeconds(0.1f);
            attempts++;
        }

        if (DayTime.Instance == null)
        {
            Debug.LogError("StockMarketManager: DayTime singleton not found after waiting! Make sure DayTime is loaded first.");
            yield break;
        }

        // Initialize market state
        CheckMarketStatus();
        lastCheckedDay = DayTime.Instance.days;

        Debug.Log($"StockMarketManager initialized successfully!");
        Debug.Log($"Current game time: {GetCurrentTimeString()}");
        Debug.Log($"Market should be {(ShouldMarketBeOpen() ? "OPEN" : "CLOSED")} at this time");

        // Force initial market status update
        UpdateAllStockMarkets();
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

    private bool ShouldMarketBeOpen()
    {
        if (DayTime.Instance == null) return false;
        float currentHour = DayTime.Instance.Hours;
        return (currentHour >= marketOpenHour && currentHour < marketCloseHour);
    }

    private void CheckMarketStatus()
    {
        if (DayTime.Instance == null) return;

        bool shouldBeOpen = ShouldMarketBeOpen();

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
        Debug.Log($"Updating {stockMarkets.Length} StockMarket instances with market status: {(isMarketOpen ? "OPEN" : "CLOSED")}");

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
        if (!allowAfterHoursTrading)
        {
            Debug.Log("After-hours trading disabled - skipping overnight price changes");
            return;
        }

        Debug.Log("Generating overnight price changes...");

        foreach (var stock in availableStocks)
        {
            if (stock != null && stock.isActive)
            {
                // Reduced volatility for after-hours trading
                float overnightVolatility = Random.Range(-stock.volatility * 0.3f, stock.volatility * 0.3f);

                // Reduced trend for overnight (since it's a longer period, we don't want too much change)
                float overnightTrend = stock.trendDirection * 0.5f; // Half the normal trend
                float trendVariation = Random.Range(-stock.trendVolatility * 0.5f, stock.trendVolatility * 0.5f);

                float totalOvernightChange = overnightTrend + trendVariation + overnightVolatility;
                float newPrice = stock.currentPrice * (1 + totalOvernightChange);
                newPrice = Mathf.Max(newPrice, 1f);

                Debug.Log($"Overnight Stock {stock.name}: {stock.currentPrice:F2} -> {newPrice:F2} " +
                         $"(Trend: {(overnightTrend + trendVariation):P3}, Volatility: {overnightVolatility:P3})");

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
        Debug.Log($"=== STOCK MARKET MANAGER STATUS ===");
        Debug.Log($"Current Time: {GetCurrentTimeString()}, Day: {GetCurrentDay()}");
        Debug.Log($"Market Status: {(isMarketOpen ? "OPEN" : "CLOSED")}");
        Debug.Log($"Should Market Be Open: {ShouldMarketBeOpen()}");
        Debug.Log($"Market Hours: {marketOpenHour:00}:00 - {marketCloseHour:00}:00");
        Debug.Log($"Available Stocks: {availableStocks.Count}");
        Debug.Log($"Has Opened Today: {hasOpenedToday}, Has Closed Today: {hasClosedToday}");
        Debug.Log($"Last Checked Day: {lastCheckedDay}");

        // Also check all StockMarket instances
        StockMarket[] stockMarkets = FindObjectsOfType<StockMarket>();
        Debug.Log($"Found {stockMarkets.Length} StockMarket instances in scene");
    }

    [ContextMenu("Force Update All Markets")]
    public void ForceUpdateAllMarkets()
    {
        Debug.Log("Forcing update of all StockMarket instances...");
        UpdateAllStockMarkets();
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