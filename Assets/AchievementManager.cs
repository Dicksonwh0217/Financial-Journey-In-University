using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class AchievementManager : MonoBehaviour
{
    [Header("UI References")]
    public Transform achievementListParent;
    public GameObject achievementItemPrefab;
    public AchievementDetailPanel detailPanel;

    [Header("Popup System")]
    public AchievementPopUpPanel achievementPopup; // Reference to the popup panel (can be inactive)

    [Header("Achievement Data")]
    public List<Achievement> achievements = new List<Achievement>();

    private List<AchievementItem> achievementItems = new List<AchievementItem>();
    private Achievement currentSelectedAchievement;
    private AchievementItem currentSelectedItem;

    private void Start()
    {
        // Find popup even if it's inactive
        if (achievementPopup == null)
        {
            achievementPopup = FindFirstObjectByType<AchievementPopUpPanel>(FindObjectsInactive.Include);
            if (achievementPopup != null)
            {
            }
        }

        PopulateAchievementList();
        LoadAchievements(); // Load saved achievement states
    }

    private void PopulateAchievementList()
    {
        // Clear existing items
        foreach (Transform child in achievementListParent)
        {
            Destroy(child.gameObject);
        }
        achievementItems.Clear();

        // Create achievement items
        foreach (Achievement achievement in achievements)
        {
            GameObject itemObj = Instantiate(achievementItemPrefab, achievementListParent);

            // Ensure the GameObject and all components are active
            itemObj.SetActive(true);

            // Force enable all components that might have been disabled
            ForceEnableComponents(itemObj);

            AchievementItem item = itemObj.GetComponent<AchievementItem>();

            if (item != null)
            {
                // Enable the component if it's disabled
                item.enabled = true;

                item.Initialize(achievement, this);
                achievementItems.Add(item);
            }
            else
            {
            }
        }

        // Force layout rebuild if using layout groups
        StartCoroutine(RefreshLayoutNextFrame());
    }

    // Force enable components that commonly get disabled
    private void ForceEnableComponents(GameObject itemObj)
    {
        // Enable Button component
        Button button = itemObj.GetComponent<Button>();
        if (button != null)
        {
            button.enabled = true;
            if (button.image != null)
                button.image.enabled = true;
        }

        // Enable all Image components in children
        Image[] images = itemObj.GetComponentsInChildren<Image>();
        foreach (Image img in images)
        {
            img.enabled = true;
        }

        // Enable all Text components in children
        TMPro.TextMeshProUGUI[] texts = itemObj.GetComponentsInChildren<TMPro.TextMeshProUGUI>();
        foreach (TMPro.TextMeshProUGUI text in texts)
        {
            text.enabled = true;
        }

        // Enable the AchievementItem component itself
        AchievementItem achievementItem = itemObj.GetComponent<AchievementItem>();
        if (achievementItem != null)
        {
            achievementItem.enabled = true;
        }
    }

    private System.Collections.IEnumerator RefreshLayoutNextFrame()
    {
        yield return new WaitForEndOfFrame();

        // Force layout rebuild
        LayoutRebuilder.ForceRebuildLayoutImmediate(achievementListParent.GetComponent<RectTransform>());

        // Additional frame delay to ensure everything is properly initialized
        yield return new WaitForEndOfFrame();

        // Double-check component states after layout rebuild
        foreach (AchievementItem item in achievementItems)
        {
            if (item != null)
            {
                ForceEnableComponents(item.gameObject);
            }
        }
    }

    public void SelectAchievement(Achievement achievement, AchievementItem selectedItem)
    {
        // Deselect previously selected item
        if (currentSelectedItem != null)
        {
            currentSelectedItem.SetSelected(false);
        }

        // Select new item
        currentSelectedAchievement = achievement;
        currentSelectedItem = selectedItem;
        selectedItem.SetSelected(true);

        // Update detail panel
        detailPanel.ShowAchievementDetails(achievement);
    }

    // Methods for unlocking/completing achievements
    public void UnlockAchievement(int achievementId)
    {
        Achievement achievement = achievements.Find(a => a.id == achievementId);
        if (achievement != null && !achievement.isUnlocked)
        {
            achievement.isUnlocked = true;
            RefreshAchievementDisplay(achievement);

            // Show popup for newly unlocked achievement
            ShowAchievementPopup(achievement);

            // Save the achievement state
            SaveAchievements();
        }
    }

    public void CompleteAchievement(int achievementId)
    {
        Achievement achievement = achievements.Find(a => a.id == achievementId);
        if (achievement != null)
        {
            bool wasAlreadyCompleted = achievement.isCompleted;

            achievement.isUnlocked = true;
            achievement.isCompleted = true;

            RefreshAchievementDisplay(achievement);

            // Show popup only if it wasn't already completed
            if (!wasAlreadyCompleted)
            {
                ShowAchievementPopup(achievement);
            }
            else
            {
            }

            // Update detail panel if this achievement is currently selected
            if (currentSelectedAchievement == achievement)
            {
                detailPanel.ShowAchievementDetails(achievement);
            }

            // Save the achievement state
            SaveAchievements();
        }
        else
        {
        }
    }

    // Show achievement popup - works with inactive popup
    private void ShowAchievementPopup(Achievement achievement)
    {
        // Try direct reference first
        if (achievementPopup != null)
        {
            achievementPopup.ShowAchievementPopup(achievement);
            return;
        }

        // Try to find popup in scene (including inactive objects)
        achievementPopup = FindFirstObjectByType<AchievementPopUpPanel>(FindObjectsInactive.Include);
        if (achievementPopup != null)
        {
            achievementPopup.ShowAchievementPopup(achievement);
        }
        else
        {
            AchievementPopUpPanel.ShowAchievement(achievement);
        }
    }

    private void RefreshAchievementDisplay(Achievement achievement)
    {
        AchievementItem item = null;

        foreach (AchievementItem achievementItem in achievementItems)
        {
            if (achievementItem != null && achievementItem.name.Contains($"Achievement_{achievement.id}"))
            {
                item = achievementItem;
                break;
            }
        }

        // If not found by name, try finding by checking the achievement data
        if (item == null)
        {
            foreach (AchievementItem achievementItem in achievementItems)
            {
                if (achievementItem != null)
                {
                    // Use the index as a fallback
                    int itemIndex = achievementItems.IndexOf(achievementItem);
                    if (itemIndex < achievements.Count && achievements[itemIndex].id == achievement.id)
                    {
                        item = achievementItem;
                        break;
                    }
                }
            }
        }

        if (item != null)
        {
            // Force enable components before refreshing display
            ForceEnableComponents(item.gameObject);

            // Refresh the display
            item.RefreshDisplay();
        }
        else
        {
        }
    }

    // Add method to reset achievements
    public void ResetAchievement(int achievementId)
    {
        Achievement achievement = achievements.Find(a => a.id == achievementId);
        if (achievement != null)
        {
            achievement.isUnlocked = true;
            achievement.isCompleted = false;

            Debug.Log($"[ACHIEVEMENT] Reset: {achievement.title} (ID: {achievementId})");

            RefreshAchievementDisplay(achievement);

            // Update detail panel if this achievement is currently selected
            if (currentSelectedAchievement == achievement)
            {
                detailPanel.ShowAchievementDetails(achievement);
            }

            // Also clear from PlayerPrefs
            PlayerPrefs.DeleteKey($"Achievement_{achievement.id}_Unlocked");
            PlayerPrefs.DeleteKey($"Achievement_{achievement.id}_Completed");
            PlayerPrefs.Save();
        }
    }

    // Save/Load methods
    public void SaveAchievements()
    {
        for (int i = 0; i < achievements.Count; i++)
        {
            Achievement achievement = achievements[i];
        }
        PlayerPrefs.Save();
    }

    public void LoadAchievements()
    {
        for (int i = 0; i < achievements.Count; i++)
        {
            Achievement achievement = achievements[i];
        }

        // Refresh all displays after loading
        foreach (Achievement achievement in achievements)
        {
            RefreshAchievementDisplay(achievement);
        }
    }

    // Public method to check if an achievement is unlocked
    public bool IsAchievementUnlocked(int achievementId)
    {
        Achievement achievement = achievements.Find(a => a.id == achievementId);
        return achievement != null && achievement.isUnlocked;
    }

    public bool IsAchievementCompleted(int achievementId)
    {
        Achievement achievement = achievements.Find(a => a.id == achievementId);
        return achievement != null && achievement.isCompleted;
    }

    // Public methods for other scripts to call
    public void CompleteAchievementWithPopup(int achievementId)
    {
        CompleteAchievement(achievementId);
    }

    public void UnlockAchievementWithPopup(int achievementId)
    {
        UnlockAchievement(achievementId);
    }

    [ContextMenu("Force Refresh All Achievement Displays")]
    public void ForceRefreshAllDisplays()
    {
        foreach (Achievement achievement in achievements)
        {
            RefreshAchievementDisplay(achievement);
        }
    }
}