using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SleepArea : MonoBehaviour
{
    [Header("Achievement Settings")]
    [SerializeField] private int sweetDreamAchievementId = 2; // "Sweet Dream" achievement ID

    private AchievementManager achievementManager;

    private void Start()
    {
        // Find the achievement manager in the scene
        achievementManager = GameManager.instance.achievementManager;
        if (achievementManager == null)
        {
            Debug.LogWarning("[SLEEP AREA] AchievementManager not found in scene!");
        }
    }

    private void Update()
    {
        // Check for T key press to reset achievements (for debugging)
        if (Input.GetKeyDown(KeyCode.T))
        {
            ResetSweetDreamAchievement();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Sleep sleep = other.GetComponent<Sleep>();
        if (sleep != null)
        {
            // Trigger the sleep action
            sleep.DoSleep();

            // Check and unlock "Sweet Dream" achievement when player enters sleep area
            CheckSweetDreamAchievement();
        }
    }

    private void CheckSweetDreamAchievement()
    {
        if (achievementManager != null)
        {
            // Check if the achievement hasn't been completed yet
            if (!achievementManager.IsAchievementCompleted(sweetDreamAchievementId))
            {
                achievementManager.CompleteAchievementWithPopup(sweetDreamAchievementId);
            }

        }
    }

    private void ResetSweetDreamAchievement()
    {
        if (achievementManager != null)
        {
            achievementManager.ResetAchievement(sweetDreamAchievementId);
        }
    }

    // Method to test the achievement (can be called from inspector)
    [ContextMenu("Test Sweet Dream Achievement")]
    public void TestSweetDreamAchievement()
    {
        CheckSweetDreamAchievement();
    }

    // Method to manually reset the achievement (can be called from inspector)
    [ContextMenu("Reset Sweet Dream Achievement")]
    public void ManualResetSweetDreamAchievement()
    {
        ResetSweetDreamAchievement();
    }
}