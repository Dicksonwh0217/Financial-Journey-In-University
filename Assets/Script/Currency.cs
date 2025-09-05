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

    [Header("Testing Controls")]
    [SerializeField] private bool enableTestingMode = false; // Toggle this in inspector during testing
    [SerializeField] private KeyCode addMoneyKey = KeyCode.M;

    // Achievement constants
    private const int FIRST_STEP_BILLIONAIRE_ID = 0;
    private const float BILLIONAIRE_TARGET = 10000f;

    private void Start()
    {
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
    }

    private void Update()
    {
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
        if (amount < 0)
        {
            amount = 0;
        }
        UpdateText();

        // Note: We don't subtract from totalEarnings when spending money
        // because the achievement tracks total earned, not current balance
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
        if (amount < 0)
        {
            amount = 0;
        }
        UpdateText();
    }

    // Method to set total earnings directly (useful for loading save data)
    public void SetTotalEarnings(float earnings)
    {
        totalEarnings = earnings;
        if (totalEarnings < 0)
        {
            totalEarnings = 0;
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
}