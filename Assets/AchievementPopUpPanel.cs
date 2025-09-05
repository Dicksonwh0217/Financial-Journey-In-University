using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;

public class AchievementPopUpPanel : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private RectTransform popupPanel;          // The main popup panel RectTransform
    [SerializeField] private Image achievementImage;            // Achievement icon/image
    [SerializeField] private TextMeshProUGUI achievementText;   // Achievement title text
    [SerializeField] private TextMeshProUGUI unlockedText;      // "Achievement Unlocked" text

    [Header("Animation Settings")]
    [SerializeField] private float animationDuration = 0.5f;   // Duration for slide animations
    [SerializeField] private float displayDuration = 3f;       // How long to show the popup
    [SerializeField] private Ease slideEase = Ease.OutBack;    // Easing for slide animation
    [SerializeField] private float slideDistance = 200f;       // Distance to slide (will be calculated from panel height)

    [Header("Sound Effects (Optional)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip achievementSound;

    private Vector3 hiddenPosition;
    private Vector3 visiblePosition;
    private bool isAnimating = false;
    private Queue<Achievement> achievementQueue = new Queue<Achievement>();
    private bool positionsCalculated = false;

    // Static reference to handle calls when inactive
    private static AchievementPopUpPanel instance;

    private void Awake()
    {
        // Set static reference
        instance = this;

        // Auto-find components if not assigned
        if (popupPanel == null)
            popupPanel = GetComponent<RectTransform>();

        if (achievementImage == null)
            achievementImage = GetComponentInChildren<Image>();

        if (achievementText == null || unlockedText == null)
        {
            TextMeshProUGUI[] texts = GetComponentsInChildren<TextMeshProUGUI>();
            if (texts.Length >= 2)
            {
                // Assume first text is achievement title, second is "unlocked" text
                achievementText = texts[0];
                unlockedText = texts[1];
            }
        }

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    private void CalculatePositions()
    {
        if (popupPanel == null || positionsCalculated) return;

        // Get canvas for calculations
        Canvas canvas = GetComponentInParent<Canvas>();
        float canvasHeight = canvas != null ? canvas.GetComponent<RectTransform>().rect.height : Screen.height;

        // Visible position (slightly above bottom of screen) - use current X position
        visiblePosition = new Vector3(popupPanel.anchoredPosition.x, 100f, 0f);

        // Hidden position (below screen)
        float panelHeight = popupPanel.rect.height;
        if (panelHeight <= 0) panelHeight = 150f; // Default height if not calculated yet

        hiddenPosition = new Vector3(popupPanel.anchoredPosition.x, -panelHeight - 50f, 0f);
        positionsCalculated = true;
    }

    public void ShowAchievementPopup(Achievement achievement)
    {
        if (achievement == null)
        {
            Debug.LogError("Achievement is null!");
            return;
        }

        // FIRST: Activate the GameObject so we can run coroutines
        if (!gameObject.activeInHierarchy)
        {
            gameObject.SetActive(true);
        }

        // Calculate positions now that we're active
        if (!positionsCalculated)
        {
            CalculatePositions();
        }

        // Add to queue if currently animating
        if (isAnimating)
        {
            achievementQueue.Enqueue(achievement);
            return;
        }

        StartCoroutine(ShowPopupCoroutine(achievement));
    }

    private IEnumerator ShowPopupCoroutine(Achievement achievement)
    {
        isAnimating = true;

        // Update UI with achievement data
        UpdatePopupContent(achievement);

        // Set initial position (hidden)
        popupPanel.anchoredPosition = hiddenPosition;

        // Play sound effect
        PlayAchievementSound();

        // Wait one frame to ensure UI is updated
        yield return new WaitForEndOfFrame();

        popupPanel.DOAnchorPos(visiblePosition, animationDuration)
            .SetEase(slideEase);

        // Wait for slide up to complete
        yield return new WaitForSeconds(animationDuration);

        // Display for specified duration
        yield return new WaitForSeconds(displayDuration);

        // Slide down animation
        popupPanel.DOAnchorPos(hiddenPosition, animationDuration)
            .SetEase(Ease.InBack);

        // Wait for slide down to complete
        yield return new WaitForSeconds(animationDuration);

        // Deactivate the GameObject
        gameObject.SetActive(false);


        isAnimating = false;

        // Process queue if any achievements are waiting
        if (achievementQueue.Count > 0)
        {
            Achievement nextAchievement = achievementQueue.Dequeue();

            // Reactivate and show next achievement
            gameObject.SetActive(true);
            StartCoroutine(ShowPopupCoroutine(nextAchievement));
        }
    }

    private void UpdatePopupContent(Achievement achievement)
    {
        // Set achievement title
        if (achievementText != null)
        {
            achievementText.text = achievement.title;
        }

        // Set achievement icon
        if (achievementImage != null && achievement.icon != null)
        {
            achievementImage.sprite = achievement.icon;
        }

        // Set unlocked text
        if (unlockedText != null)
        {
            unlockedText.text = "Achievement Unlocked!";
        }
    }

    private void PlayAchievementSound()
    {
        if (audioSource != null && achievementSound != null)
        {
            audioSource.PlayOneShot(achievementSound);
        }
    }

    // Static method that works even when popup is inactive
    public static void ShowAchievement(Achievement achievement)
    {
        if (instance != null)
        {
            instance.ShowAchievementPopup(achievement);
        }
        else
        {
            // Try to find the popup in scene (including inactive objects)
            AchievementPopUpPanel popup = FindFirstObjectByType<AchievementPopUpPanel>(FindObjectsInactive.Include);
            if (popup != null)
            {
                popup.ShowAchievementPopup(achievement);
            }
            else
            {
                Debug.LogError("AchievementPopUpPanel not found in scene!");
            }
        }
    }

    public void HidePopupInstant()
    {
        if (popupPanel != null)
        {
            if (!positionsCalculated) CalculatePositions();
            popupPanel.anchoredPosition = hiddenPosition;
            gameObject.SetActive(false);
        }
    }

    public void HidePopupAnimated()
    {
        if (!gameObject.activeInHierarchy || isAnimating) return;

        if (!positionsCalculated) CalculatePositions();

        isAnimating = true;
        popupPanel.DOAnchorPos(hiddenPosition, animationDuration)
            .SetEase(Ease.InBack)
            .OnComplete(() => {
                gameObject.SetActive(false);
                isAnimating = false;
            });
    }

    // Method to clear the queue
    public void ClearQueue()
    {
        achievementQueue.Clear();
        if (!isAnimating)
        {
            gameObject.SetActive(false);
        }
    }

    // Public method to recalculate positions
    [ContextMenu("Recalculate Positions")]
    public void RecalculatePositions()
    {
        positionsCalculated = false;
        bool wasActive = gameObject.activeInHierarchy;

        if (!wasActive) gameObject.SetActive(true);
        CalculatePositions();
        if (!wasActive) gameObject.SetActive(false);
    }

    public bool IsPopupActive()
    {
        return gameObject.activeInHierarchy && isAnimating;
    }

    private void OnDestroy()
    {
        // Kill any running DOTween animations
        if (popupPanel != null)
        {
            popupPanel.DOKill();
        }

        // Clear static reference
        if (instance == this)
        {
            instance = null;
        }
    }

    private void OnDisable()
    {
        // Reset animation state when disabled
        isAnimating = false;
    }
}