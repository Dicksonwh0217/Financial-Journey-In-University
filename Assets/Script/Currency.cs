using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Currency : MonoBehaviour
{
    [SerializeField] float amount;
    [SerializeField] TMPro.TextMeshProUGUI InventoryText;
    [SerializeField] TMPro.TextMeshProUGUI BillText;

    [Header("Achievement Tracking")]
    [SerializeField] private float totalEarnings; // Track total earnings separately
    [SerializeField] private AchievementManager achievementManager; // Reference to achievement manager

    [Header("Daily Bonuses")]
    [SerializeField] private float dailyBonusAmount = 2500f; // Amount to add on special days
    [SerializeField] private int[] bonusDays = { 1, 31, 61 }; // Days to receive bonuses
    [SerializeField] private HashSet<int> processedBonusDays; // Track which bonus days have been processed
    private int lastCheckedDay = -1; // Track the last day we checked for bonuses

    [Header("Testing Controls")]
    [SerializeField] private bool enableTestingMode = false; // Toggle this in inspector during testing
    [SerializeField] private KeyCode addMoneyKey = KeyCode.M;

    // Achievement constants
    private const int FIRST_STEP_BILLIONAIRE_ID = 0;
    private const float BILLIONAIRE_TARGET = 10000f;

    private void Awake()
    {
        // Initialize the processed bonus days set
        processedBonusDays = new HashSet<int>();
        LoadProcessedBonusDays();
    }

    private void Start()
    {
        // Initialize starting currency for new games
        InitializeStartingCurrency();

        // Load saved total earnings first
        LoadTotalEarnings();

        // Find AchievementManager if not assigned
        if (achievementManager == null)
        {
            achievementManager = FindFirstObjectByType<AchievementManager>();
        }

        UpdateText();

        // Check if achievement should already be unlocked
        CheckBillionaireAchievement();

        // Check for daily bonus when starting
        CheckDailyBonus();
    }

    private void InitializeStartingCurrency()
    {
        // Check if this is a new game with starting currency set
        if (PlayerPrefs.HasKey("StartingCurrency"))
        {
            float startingAmount = PlayerPrefs.GetFloat("StartingCurrency", 0f);

            // Set the starting amount
            amount = startingAmount;

            // Remove the key so it doesn't interfere with future loads
            PlayerPrefs.DeleteKey("StartingCurrency");
            PlayerPrefs.Save();

            Debug.Log($"New game started with {startingAmount} currency");
        }
        else
        {
            // Load existing amount from save data if available
            // You can add your save/load logic here if you have one
            // For now, we'll just keep the serialized amount from the inspector
        }
    }

    private void Update()
    {
        // Check for daily bonuses
        CheckDailyBonus();

        // Only enable testing controls if testing mode is active
        if (enableTestingMode)
        {
            // Check for money cheat key
            if (Input.GetKeyDown(addMoneyKey))
            {
                Add(1000f);
            }
        }
    }

    private void CheckDailyBonus()
    {
        // Get current day from DayTime singleton
        if (DayTime.Instance != null)
        {
            int currentDay = DayTime.Instance.days + 1; // Add 1 because days is 0-indexed

            // Debug logging for testing
            if (enableTestingMode)
            {
                Debug.Log($"[TESTING] Checking Day {currentDay}, Last Checked: {lastCheckedDay}");
                Debug.Log($"[TESTING] Processed Bonus Days: [{string.Join(", ", processedBonusDays)}]");
            }

            // Only check if we haven't checked this day yet
            if (currentDay != lastCheckedDay)
            {
                lastCheckedDay = currentDay;

                // Check if current day is a bonus day and hasn't been processed yet
                for (int i = 0; i < bonusDays.Length; i++)
                {
                    if (currentDay == bonusDays[i])
                    {
                        if (!processedBonusDays.Contains(currentDay))
                        {
                            // Add the bonus
                            Add(dailyBonusAmount);

                            // Mark this day as processed
                            processedBonusDays.Add(currentDay);
                            SaveProcessedBonusDays();

                            string logPrefix = enableTestingMode ? "[TESTING] " : "";
                            Debug.Log($"{logPrefix}Daily bonus of {dailyBonusAmount} added for Day {currentDay}!");
                        }
                        else
                        {
                            // Already processed this bonus day
                            if (enableTestingMode)
                            {
                                Debug.Log($"[TESTING] Day {currentDay} bonus already processed - skipping");
                            }
                        }
                        break; // Exit loop once we find the bonus day (processed or not)
                    }
                }
            }
        }
        else
        {
            if (enableTestingMode)
            {
                Debug.LogWarning("[TESTING] DayTime.Instance is null - cannot check daily bonus");
            }
        }
    }

    private void UpdateText()
    {
        InventoryText.text = amount.ToString("F2"); // Format to 2 decimal places
        BillText.text = amount.ToString("F2");
    }

    internal void Add(float moneyGain)
    {
        if (moneyGain > 0) // Only count positive gains towards total earnings
        {
            amount += moneyGain;
            totalEarnings += moneyGain; // Add to cumulative earnings

            // Save total earnings
            SaveTotalEarnings();

            // Check achievement after earning money
            CheckBillionaireAchievement();

            string logPrefix = enableTestingMode ? "[TESTING] " : "";
        }

        UpdateText();
    }

    internal bool Check(float totalPrice)
    {
        return amount >= totalPrice;
    }

    internal void Decrease(float totalPrice)
    {
        amount -= totalPrice;
        if (amount < 0f)
        {
            amount = 0f;
        }
        UpdateText();
    }

    // Additional method for spending with validation
    public bool TrySpend(float cost)
    {
        if (Check(cost))
        {
            Decrease(cost);
            return true;
        }
        return false;
    }

    // Method to spend money with optional validation
    public void Spend(float cost, bool forceSpend = false)
    {
        if (forceSpend || Check(cost))
        {
            Decrease(cost);
        }
        else
        {
            Debug.LogWarning($"Insufficient funds to spend {cost:F2}. Current amount: {amount:F2}");
        }
    }

    // Additional getter for current amount
    public float GetAmount()
    {
        return amount;
    }

    // Getter for total earnings
    public float GetTotalEarnings()
    {
        return totalEarnings;
    }

    // Method to set amount directly (useful for testing or loading save data)
    public void SetAmount(float newAmount)
    {
        amount = newAmount;
        if (amount < 0f)
        {
            amount = 0f;
        }
        UpdateText();
    }

    // Method to set total earnings directly (useful for loading save data)
    public void SetTotalEarnings(float earnings)
    {
        totalEarnings = earnings;
        if (totalEarnings < 0f)
        {
            totalEarnings = 0f;
        }

        SaveTotalEarnings();
        CheckBillionaireAchievement();
    }

    // Check and unlock the billionaire achievement
    private void CheckBillionaireAchievement()
    {
        if (achievementManager != null && totalEarnings >= BILLIONAIRE_TARGET)
        {
            // Only complete if not already completed (prevents duplicate popups)
            if (!achievementManager.IsAchievementCompleted(FIRST_STEP_BILLIONAIRE_ID))
            {
                achievementManager.CompleteAchievement(FIRST_STEP_BILLIONAIRE_ID);
                string logPrefix = enableTestingMode ? "[TESTING] " : "";
            }
        }
    }

    // Save total earnings to PlayerPrefs
    private void SaveTotalEarnings()
    {
        PlayerPrefs.SetFloat("TotalEarnings", totalEarnings);
        PlayerPrefs.Save();
    }

    // Load total earnings from PlayerPrefs
    private void LoadTotalEarnings()
    {
        totalEarnings = PlayerPrefs.GetFloat("TotalEarnings", 0f);
    }

    // Save processed bonus days to PlayerPrefs
    private void SaveProcessedBonusDays()
    {
        string processedDaysString = string.Join(",", processedBonusDays);
        PlayerPrefs.SetString("ProcessedBonusDays", processedDaysString);
        PlayerPrefs.Save();
    }

    // Load processed bonus days from PlayerPrefs
    private void LoadProcessedBonusDays()
    {
        string processedDaysString = PlayerPrefs.GetString("ProcessedBonusDays", "");
        processedBonusDays.Clear();

        if (!string.IsNullOrEmpty(processedDaysString))
        {
            string[] dayStrings = processedDaysString.Split(',');
            foreach (string dayString in dayStrings)
            {
                if (int.TryParse(dayString, out int day))
                {
                    processedBonusDays.Add(day);
                }
            }
        }
    }

    // UPDATED: Reset testing progress AND the achievement (but keep it unlocked)
    private void ResetTestingProgress()
    {
        if (!enableTestingMode)
        {
            return;
        }

        // Reset the earnings
        totalEarnings = 0f;
        SaveTotalEarnings();

        // Reset processed bonus days
        processedBonusDays.Clear();
        SaveProcessedBonusDays();
        lastCheckedDay = -1;

        // Reset the achievement but keep it unlocked (so it can be completed again for testing)
        if (achievementManager != null)
        {
            // Find the achievement and set it to unlocked but not completed
            var achievement = achievementManager.achievements.Find(a => a.id == FIRST_STEP_BILLIONAIRE_ID);
            if (achievement != null)
            {
                achievement.isUnlocked = true;
                achievement.isCompleted = false;

                // Save the new state
                achievementManager.SaveAchievements();

                // Refresh the display
                achievementManager.ForceRefreshAllDisplays();
            }
        }
        else
        {
            Debug.LogWarning("[TESTING] AchievementManager not found - only reset earnings");
        }
    }

    // Context menu methods for testing in editor
    [ContextMenu("Add Test Earnings (1000)")]
    public void AddTestEarnings()
    {
        Add(1000f);
    }

    [ContextMenu("Check Achievement Progress")]
    public void CheckAchievementProgress()
    {
        float progress = (totalEarnings / BILLIONAIRE_TARGET) * 100f;
        bool isCompleted = achievementManager != null ? achievementManager.IsAchievementCompleted(FIRST_STEP_BILLIONAIRE_ID) : false;
        Debug.Log($"Billionaire Achievement Progress: {totalEarnings:F2} / {BILLIONAIRE_TARGET:F2} ({progress:F1}%) - Completed: {isCompleted}");
    }

    [ContextMenu("Reset Total Earnings (Testing Only)")]
    public void ResetTotalEarnings()
    {
        if (enableTestingMode || Application.isEditor)
        {
            ResetTestingProgress();
        }
        else
        {
            Debug.LogWarning("Cannot reset earnings - testing mode is disabled and not in editor");
        }
    }

    [ContextMenu("Force Reset Achievement (Keep Unlocked)")]
    public void ForceResetAchievementKeepUnlocked()
    {
        if (achievementManager != null)
        {
            var achievement = achievementManager.achievements.Find(a => a.id == FIRST_STEP_BILLIONAIRE_ID);
            if (achievement != null)
            {
                achievement.isUnlocked = true;
                achievement.isCompleted = false;

                // Save the new state
                achievementManager.SaveAchievements();

                // Refresh the display
                achievementManager.ForceRefreshAllDisplays();

                Debug.Log("Reset billionaire achievement to unlocked but not completed");
            }
        }
        else
        {
            Debug.LogWarning("AchievementManager not found");
        }
    }

    [ContextMenu("Toggle Testing Mode")]
    public void ToggleTestingMode()
    {
        enableTestingMode = !enableTestingMode;
        Debug.Log($"Testing mode {(enableTestingMode ? "ENABLED" : "DISABLED")}");
    }

    [ContextMenu("Force Check Daily Bonus")]
    public void ForceCheckDailyBonus()
    {
        lastCheckedDay = -1; // Reset last checked day to force a check
        CheckDailyBonus();
    }

    [ContextMenu("Reset Bonus Days Progress")]
    public void ResetBonusDaysProgress()
    {
        if (enableTestingMode || Application.isEditor)
        {
            processedBonusDays.Clear();
            SaveProcessedBonusDays();
            lastCheckedDay = -1;
            Debug.Log("Bonus days progress reset - bonuses can now be received again");
        }
        else
        {
            Debug.LogWarning("Cannot reset bonus progress - testing mode is disabled and not in editor");
        }
    }

    [ContextMenu("Show Bonus Days Status")]
    public void ShowBonusDaysStatus()
    {
        int currentDay = DayTime.Instance != null ? DayTime.Instance.days + 1 : 0;
        Debug.Log($"=== BONUS DAYS STATUS ===");
        Debug.Log($"Current Day: {currentDay}");
        Debug.Log($"Processed Bonus Days: [{string.Join(", ", processedBonusDays)}]");

        List<int> remainingDays = new List<int>();
        foreach (int day in bonusDays)
        {
            if (!processedBonusDays.Contains(day))
            {
                remainingDays.Add(day);
            }
        }
        Debug.Log($"Remaining Bonus Days: [{string.Join(", ", remainingDays)}]");
        Debug.Log($"Last Checked Day: {lastCheckedDay}");
    }

    [ContextMenu("Clear All PlayerPrefs (DANGER)")]
    public void ClearAllPlayerPrefs()
    {
        if (enableTestingMode || Application.isEditor)
        {
            if (Application.isEditor)
            {
                Debug.LogWarning("CLEARING ALL PLAYERPREFS - This will reset ALL game progress!");
                PlayerPrefs.DeleteAll();
                PlayerPrefs.Save();

                // Reset local variables
                processedBonusDays.Clear();
                totalEarnings = 0f;
                lastCheckedDay = -1;

                Debug.Log("All PlayerPrefs cleared. Restart the game to test fresh bonuses.");
            }
        }
        else
        {
            Debug.LogWarning("Cannot clear PlayerPrefs - testing mode is disabled and not in editor");
        }
    }

    [ContextMenu("Simulate Fresh Game Start")]
    public void SimulateFreshGameStart()
    {
        if (enableTestingMode || Application.isEditor)
        {
            // Clear bonus progress
            processedBonusDays.Clear();
            SaveProcessedBonusDays();

            // Reset earnings (optional)
            // totalEarnings = 0f;
            // SaveTotalEarnings();

            // Reset day tracking
            lastCheckedDay = -1;

            Debug.Log("Simulated fresh game start - bonus days reset, ready to test bonuses again");
        }
        else
        {
            Debug.LogWarning("Cannot simulate fresh start - testing mode is disabled and not in editor");
        }
    }
}