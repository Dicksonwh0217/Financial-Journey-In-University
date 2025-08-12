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

    [Header("Achievement Data")]
    public List<Achievement> achievements = new List<Achievement>();

    private List<AchievementItem> achievementItems = new List<AchievementItem>();
    private Achievement currentSelectedAchievement;
    private AchievementItem currentSelectedItem;

    private void Start()
    {
        InitializeAchievements();
        PopulateAchievementList();
    }

    private void InitializeAchievements()
    {
        // Example achievements - replace with your actual data
        if (achievements.Count == 0)
        {
            achievements.Add(new Achievement(1, "First Steps", "Complete your first level", null));
            achievements.Add(new Achievement(2, "Collector", "Collect 100 items", null));
            achievements.Add(new Achievement(3, "Speed Runner", "Complete a level in under 30 seconds", null));
            achievements.Add(new Achievement(4, "Explorer", "Discover all hidden areas", null));
        }
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
                Debug.LogError($"AchievementItem component not found on instantiated prefab: {itemObj.name}");
            }
        }

        // Force layout rebuild if using layout groups
        StartCoroutine(RefreshLayoutNextFrame());
    }

    // NEW METHOD: Force enable components that commonly get disabled
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

        Debug.Log($"Force enabled components on {itemObj.name}");
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

    private void HighlightSelectedItem(Achievement selectedAchievement)
    {
        // This method is now handled by SelectAchievement above
        // Keep it for backwards compatibility if needed
    }

    // Methods for unlocking/completing achievements
    public void UnlockAchievement(int achievementId)
    {
        Achievement achievement = achievements.Find(a => a.id == achievementId);
        if (achievement != null && !achievement.isUnlocked)
        {
            achievement.isUnlocked = true;
            RefreshAchievementDisplay(achievement);
        }
    }

    public void CompleteAchievement(int achievementId)
    {
        Achievement achievement = achievements.Find(a => a.id == achievementId);
        if (achievement != null)
        {
            achievement.isUnlocked = true;
            achievement.isCompleted = true;
            RefreshAchievementDisplay(achievement);

            // Update detail panel if this achievement is currently selected
            if (currentSelectedAchievement == achievement)
            {
                detailPanel.ShowAchievementDetails(achievement);
            }
        }
    }

    private void RefreshAchievementDisplay(Achievement achievement)
    {
        AchievementItem item = achievementItems.Find(i => i.name.Contains(achievement.id.ToString()));
        if (item != null)
        {
            // Force enable components before refreshing display
            ForceEnableComponents(item.gameObject);
            item.RefreshDisplay();
        }
    }

    // Save/Load methods (optional)
    public void SaveAchievements()
    {
        // Implement save logic using PlayerPrefs or file system
        for (int i = 0; i < achievements.Count; i++)
        {
            Achievement achievement = achievements[i];
            PlayerPrefs.SetInt($"Achievement_{achievement.id}_Unlocked", achievement.isUnlocked ? 1 : 0);
            PlayerPrefs.SetInt($"Achievement_{achievement.id}_Completed", achievement.isCompleted ? 1 : 0);
        }
        PlayerPrefs.Save();
    }

    public void LoadAchievements()
    {
        // Implement load logic
        for (int i = 0; i < achievements.Count; i++)
        {
            Achievement achievement = achievements[i];
            achievement.isUnlocked = PlayerPrefs.GetInt($"Achievement_{achievement.id}_Unlocked", 0) == 1;
            achievement.isCompleted = PlayerPrefs.GetInt($"Achievement_{achievement.id}_Completed", 0) == 1;
        }
    }

    // DEBUG METHOD: Call this to check component states
    [ContextMenu("Debug Component States")]
    public void DebugComponentStates()
    {
        Debug.Log("=== DEBUGGING COMPONENT STATES ===");
        foreach (AchievementItem item in achievementItems)
        {
            if (item != null)
            {
                Button btn = item.GetComponent<Button>();
                Image[] images = item.GetComponentsInChildren<Image>();

                Debug.Log($"Item: {item.name}");
                Debug.Log($"  AchievementItem enabled: {item.enabled}");
                Debug.Log($"  Button enabled: {(btn != null ? btn.enabled.ToString() : "NULL")}");
                Debug.Log($"  Images count: {images.Length}");

                for (int i = 0; i < images.Length; i++)
                {
                    Debug.Log($"    Image {i} ({images[i].name}) enabled: {images[i].enabled}");
                }
            }
        }
        Debug.Log("=== END DEBUG ===");
    }
}