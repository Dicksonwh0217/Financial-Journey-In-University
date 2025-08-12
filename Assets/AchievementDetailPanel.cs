using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AchievementDetailPanel : MonoBehaviour
{
    [Header("UI References")]
    public Image detailImage;
    public TextMeshProUGUI detailTitle;
    public TextMeshProUGUI detailDescription;
    public GameObject completedBadge;

    [Header("Default State")]
    public string defaultTitle = "Select an Achievement";
    public string defaultDescription = "Click on an achievement from the list to view details.";
    public Sprite defaultImage;

    [Header("Color Settings")]
    public Color completedTitleColor = new Color32(43, 184, 35, 255); // #2BB823
    public Color unlockedTitleColor = Color.white;
    public Color lockedTitleColor = Color.gray;

    public Color completedDescriptionColor = Color.white;
    public Color unlockedDescriptionColor = Color.white;
    public Color lockedDescriptionColor = Color.gray;

    public Color completedImageColor = Color.white;
    public Color unlockedImageColor = Color.white;
    public Color lockedImageColor = Color.gray;

    private void Start()
    {
        ShowDefault();
    }

    public void ShowAchievementDetails(Achievement achievement)
    {
        if (achievement != null)
        {
            // Set content
            detailImage.sprite = achievement.icon;
            detailTitle.text = achievement.title;
            detailDescription.text = achievement.description;

            // Set colors based on achievement state
            if (achievement.isCompleted)
            {
                // Completed state - green title
                detailTitle.color = completedTitleColor;
                detailDescription.color = completedDescriptionColor;
                detailImage.color = completedImageColor;
            }
            else if (achievement.isUnlocked)
            {
                // Unlocked but not completed
                detailTitle.color = unlockedTitleColor;
                detailDescription.color = unlockedDescriptionColor;
                detailImage.color = unlockedImageColor;
            }
            else
            {
                // Locked state
                detailTitle.color = lockedTitleColor;
                detailDescription.color = lockedDescriptionColor;
                detailImage.color = lockedImageColor;
            }

            // Show/hide completed badge
            if (completedBadge != null)
                completedBadge.SetActive(achievement.isCompleted);
        }
        else
        {
            ShowDefault();
        }
    }

    private void ShowDefault()
    {
        detailTitle.text = defaultTitle;
        detailDescription.text = defaultDescription;
        detailTitle.color = unlockedTitleColor;
        detailDescription.color = unlockedDescriptionColor;

        if (defaultImage != null)
        {
            detailImage.sprite = defaultImage;
            detailImage.color = unlockedImageColor;
        }

        if (completedBadge != null)
            completedBadge.SetActive(false);
    }
}