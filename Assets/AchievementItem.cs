using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Achievement
{
    public int id;
    public string title;
    public string description;
    public Sprite icon;
    public bool isUnlocked;
    public bool isCompleted;

    public Achievement(int id, string title, string description, Sprite icon)
    {
        this.id = id;
        this.title = title;
        this.description = description;
        this.icon = icon;
        this.isUnlocked = false;
        this.isCompleted = false;
    }
}

public class AchievementItem : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image achievementImage;    // Manually assign Frame/Image
    [SerializeField] private TextMeshProUGUI titleText; // Manually assign Title
    [SerializeField] private Button button;             // Manually assign Button
    [SerializeField] private GameObject completedBadge; // Manually assign CompletedBadge

    [Header("Visual States - Colors")]
    public Color completedTitleColor = new Color32(43, 184, 35, 255); // #2BB823
    public Color unlockedTitleColor = Color.white;
    public Color lockedTitleColor = Color.gray;

    public Color completedImageColor = Color.white;
    public Color unlockedImageColor = Color.white;
    public Color lockedImageColor = Color.gray;

    [Header("Button Sprites")]
    public Sprite normalSprite;
    public Sprite selectedSprite;

    private Achievement achievementData;
    private AchievementManager achievementManager;
    private bool isSelected = false;

    private void Awake()
    {
        // Force enable this component on awake
        this.enabled = true;

        // Validate and force enable components early
        ForceValidateAndEnableComponents();
    }

    public void Initialize(Achievement achievement, AchievementManager manager)
    {
        achievementData = achievement;
        achievementManager = manager;

        // Force validate components again during initialization
        ForceValidateAndEnableComponents();

        // Store the normal sprite if not assigned
        if (normalSprite == null && button != null && button.image != null)
            normalSprite = button.image.sprite;

        UpdateUI();

        // Set up button click event
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => {
                manager.SelectAchievement(achievement, this);
            });
            button.interactable = true;
        }
    }

    private void ForceValidateAndEnableComponents()
    {
        // Auto-find and force enable Button component
        if (button == null)
        {
            button = GetComponent<Button>();
        }

        if (button != null)
        {
            button.enabled = true;
            if (button.image != null)
            {
                button.image.enabled = true;
            }
        }
        else
        {
            Debug.LogError($"Button component missing on {gameObject.name}");
        }

        // Auto-find and force enable achievement image
        if (achievementImage == null)
        {
            // Try multiple common paths
            string[] imagePaths = { "Frame/Image", "Image", "Icon", "Frame/Icon" };

            foreach (string path in imagePaths)
            {
                Transform imageTransform = transform.Find(path);
                if (imageTransform != null)
                {
                    achievementImage = imageTransform.GetComponent<Image>();
                    if (achievementImage != null)
                    {
                        Debug.Log($"Found achievement image at: {path}");
                        break;
                    }
                }
            }

            // If still not found, try getting any Image component in children
            if (achievementImage == null)
            {
                Image[] childImages = GetComponentsInChildren<Image>();
                if (childImages.Length > 0)
                {
                    // Skip the button's image, find the first non-button image
                    foreach (Image img in childImages)
                    {
                        if (button == null || img != button.image)
                        {
                            achievementImage = img;
                            break;
                        }
                    }
                }
            }
        }

        if (achievementImage != null)
        {
            achievementImage.enabled = true;
        }
        else
        {
            Debug.LogWarning($"Achievement Image component not found on {gameObject.name}");
        }

        // Auto-find and force enable title text
        if (titleText == null)
        {
            // Try multiple common paths
            string[] textPaths = { "Title", "Text", "Label", "TitleText" };

            foreach (string path in textPaths)
            {
                Transform textTransform = transform.Find(path);
                if (textTransform != null)
                {
                    titleText = textTransform.GetComponent<TextMeshProUGUI>();
                    if (titleText != null)
                    {
                        break;
                    }
                }
            }

            // If still not found, try getting any TextMeshProUGUI component in children
            if (titleText == null)
            {
                titleText = GetComponentInChildren<TextMeshProUGUI>();
                if (titleText != null)
                {
                }
            }
        }

        if (titleText != null)
        {
            titleText.enabled = true;
        }
        else
        {
        }

        // Auto-find completed badge if not assigned
        if (completedBadge == null)
        {
            // Try multiple common paths for completed badge
            string[] badgePaths = { "CompletedBadge", "Badge", "CompletedIcon", "CheckMark", "CompleteIcon" };

            foreach (string path in badgePaths)
            {
                Transform badgeTransform = transform.Find(path);
                if (badgeTransform != null)
                {
                    completedBadge = badgeTransform.gameObject;
                    break;
                }
            }
        }
    }

    private void UpdateUI()
    {
        // Force enable components before updating UI
        ForceEnableAllComponents();

        if (achievementData != null)
        {
            if (titleText != null)
                titleText.text = achievementData.title;

            if (achievementImage != null)
            {
                achievementImage.sprite = achievementData.icon;
            }

            // Set colors and badge based on achievement state
            if (achievementData.isCompleted)
            {
                // Completed state - green title, normal image
                if (titleText != null)
                    titleText.color = completedTitleColor;
                if (achievementImage != null)
                    achievementImage.color = completedImageColor;
                if (completedBadge != null)
                    completedBadge.SetActive(true);
            }
            else if (achievementData.isUnlocked)
            {
                // Unlocked but not completed - normal colors
                if (titleText != null)
                    titleText.color = unlockedTitleColor;
                if (achievementImage != null)
                    achievementImage.color = unlockedImageColor;
                if (completedBadge != null)
                    completedBadge.SetActive(false);
            }
            else
            {
                // Locked state - gray colors
                if (titleText != null)
                    titleText.color = lockedTitleColor;
                if (achievementImage != null)
                    achievementImage.color = lockedImageColor;
                if (completedBadge != null)
                    completedBadge.SetActive(false);
            }
        }
    }

    private void ForceEnableAllComponents()
    {
        // Force enable all components
        if (button != null)
        {
            button.enabled = true;
            if (button.image != null)
                button.image.enabled = true;
        }

        if (achievementImage != null)
            achievementImage.enabled = true;

        if (titleText != null)
            titleText.enabled = true;

        // Don't force enable completed badge - it should be controlled by achievement state
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;

        // Force enable components before changing selection
        ForceEnableAllComponents();

        // Update button sprite based on selection state
        if (selectedSprite != null && normalSprite != null && button != null && button.image != null)
        {
            button.image.sprite = selected ? selectedSprite : normalSprite;
        }
    }

    public bool IsSelected()
    {
        return isSelected;
    }

    public void RefreshDisplay()
    {
        // Force validate and enable components before refreshing
        ForceValidateAndEnableComponents();
        UpdateUI();
    }
}