using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OptionDialogueSystem : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] Text questionText;
    [SerializeField] Text nameText;
    [SerializeField] Image portrait;
    [SerializeField] Transform optionsContainer;
    [SerializeField] GameObject optionPrefab;

    [Header("Current Data")]
    OptionDialogueDefinition currentDialogue;
    List<Button> optionButtons = new List<Button>();

    // Back to original Initialize method - no need for trigger reference
    public void Initialize(OptionDialogueDefinition dialogue)
    {
        currentDialogue = dialogue;
        Show(true);
        UpdateUI();
        CreateOptions();
    }

    private void UpdateUI()
    {
        if (currentDialogue == null) return;

        // Update portrait
        if (portrait != null && currentDialogue.portrait != null)
        {
            portrait.sprite = currentDialogue.portrait;
        }

        // Update name
        if (nameText != null)
        {
            nameText.text = currentDialogue.characterName;
        }

        // Update question text
        if (questionText != null)
        {
            questionText.text = currentDialogue.questionText;
        }
    }

    private void CreateOptions()
    {
        // Clear existing options and wait for cleanup if needed
        ClearOptions();

        if (currentDialogue == null || currentDialogue.options == null) return;

        // Create new options
        for (int i = 0; i < currentDialogue.options.Count; i++)
        {
            GameObject optionGO = Instantiate(optionPrefab, optionsContainer);
            OptionButton optionButton = optionGO.GetComponent<OptionButton>();
            Button button = optionGO.GetComponent<Button>();

            if (optionButton != null)
            {
                // Setup option text and type
                optionButton.SetOptionText(currentDialogue.options[i].optionText);
                optionButton.SetOptionType(currentDialogue.options[i].optionType);

                // Check if option should be interactable (including class time check)
                bool canSelect = CanSelectOption(currentDialogue.options[i]);
                optionButton.SetButtonInteractable(canSelect);
            }

            // Setup click handler
            int optionIndex = i; // Capture for closure
            button.onClick.AddListener(() => OnOptionSelected(optionIndex));

            optionButtons.Add(button);
        }

        // Set first selectable option as selected for keyboard navigation
        SetFirstSelectableOption();
    }

    private void ClearOptions()
    {
        // Method 1: Clear through the optionButtons list
        foreach (Button btn in optionButtons)
        {
            if (btn != null && btn.gameObject != null)
            {
                // Remove the click listener to prevent memory leaks
                btn.onClick.RemoveAllListeners();
                // Use Destroy() instead of DestroyImmediate() for runtime
                Destroy(btn.gameObject);
            }
        }
        optionButtons.Clear();
    }

    // Alternative method using coroutine for immediate cleanup
    private void ClearOptionsImmediate()
    {
        StartCoroutine(ClearOptionsCoroutine());
    }

    private IEnumerator ClearOptionsCoroutine()
    {
        // Clear existing options
        foreach (Button btn in optionButtons)
        {
            if (btn != null && btn.gameObject != null)
            {
                btn.onClick.RemoveAllListeners();
                Destroy(btn.gameObject);
            }
        }
        optionButtons.Clear();

        // Wait one frame to ensure cleanup is complete
        yield return null;
    }

    private bool CanSelectOption(DialogueOption option)
    {
        // First check regular requirements
        if (!option.hasRequirements)
        {
            // If no requirements, check if this option needs a class to be active
            return CheckClassTimeRequirement(option);
        }

        // Check requirements (you can implement this based on your game's needs)
        foreach (var requirement in option.requirements)
        {
            if (!CheckRequirement(requirement))
            {
                return false;
            }
        }

        // If all requirements passed, check class time requirement
        return CheckClassTimeRequirement(option);
    }

    private bool CheckRequirement(OptionRequirement requirement)
    {
        // Implement your requirement checking logic here
        // For example, check player stats, inventory, etc.
        // This is just a placeholder
        return true;
    }

    private bool CheckClassTimeRequirement(DialogueOption option)
    {
        // Check if this option has a class time requirement
        // You can add a field to DialogueOption for this, or use a naming convention
        // For now, I'll assume options that contain "Attend" or "Class" need active classes

        string optionTextLower = option.optionText.ToLower();

        // Check if this option is class-related
        bool isClassRelated = optionTextLower.Contains("attend") ||
                             optionTextLower.Contains("class") ||
                             optionTextLower.Contains("lesson") ||
                             optionTextLower.Contains("lecture");

        if (!isClassRelated)
        {
            return true; // Non-class options are always available
        }

        // If it's class-related, check if there's an active class
        return IsClassInSession();
    }

    private bool IsClassInSession()
    {
        // Check if required components exist
        if (DayTime.Instance == null || TimetableManager.Instance == null)
            return false;

        DayOfWeek currentDay = DayTime.Instance.GetDayOfWeek();
        float currentTime = DayTime.Instance.Hours;

        List<Class> weeklyClasses = TimetableManager.Instance.GetWeeklyClasses();

        foreach (Class classItem in weeklyClasses)
        {
            // Check if this class is happening today
            if (classItem.dayOfWeek == currentDay)
            {
                // Check if current time is within class duration
                // Assuming classes have a duration (you might need to adjust this)
                float classEndTime = classItem.startTime + 1f; // Assuming 1 hour duration

                if (currentTime >= classItem.startTime && currentTime <= classEndTime)
                {
                    return true; // Found an active class
                }
            }
        }

        return false; // No active class found
    }

    // Optional: Get the current active class name
    public string GetCurrentClassName()
    {
        if (DayTime.Instance == null || TimetableManager.Instance == null)
            return "";

        DayOfWeek currentDay = DayTime.Instance.GetDayOfWeek();
        float currentTime = DayTime.Instance.Hours;

        List<Class> weeklyClasses = TimetableManager.Instance.GetWeeklyClasses();

        foreach (Class classItem in weeklyClasses)
        {
            if (classItem.dayOfWeek == currentDay)
            {
                float classEndTime = classItem.startTime + 1f; // Assuming 1 hour duration

                if (currentTime >= classItem.startTime && currentTime <= classEndTime)
                {
                    return classItem.className;
                }
            }
        }

        return "";
    }

    private void SetFirstSelectableOption()
    {
        foreach (Button button in optionButtons)
        {
            if (button.interactable)
            {
                button.Select();
                break;
            }
        }
    }

    private void OnOptionSelected(int optionIndex)
    {
        if (currentDialogue == null || optionIndex >= currentDialogue.options.Count) return;

        DialogueOption selectedOption = currentDialogue.options[optionIndex];

        // Execute actions through GameManager's DialogueActionHandler
        if (!string.IsNullOrEmpty(currentDialogue.actionSetId) &&
            GameManager.instance != null &&
            GameManager.instance.dialogueActionHandler != null)
        {
            GameManager.instance.dialogueActionHandler.ExecuteOptionAction(currentDialogue.actionSetId, optionIndex);
        }

        // Close dialogue if specified
        if (selectedOption.closesDialogue)
        {
            Conclude();
        }
    }

    private void Show(bool show)
    {
        gameObject.SetActive(show);
    }

    private void Conclude()
    {
        Debug.Log("Option dialogue concluded.");
        ClearOptions();
        Show(false);
    }

    // Optional: Handle keyboard input for quick selection
    private void Update()
    {
        if (!gameObject.activeInHierarchy) return;

        // Handle number key selection (1, 2, 3, etc.)
        for (int i = 0; i < optionButtons.Count && i < 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                if (optionButtons[i].interactable)
                {
                    OnOptionSelected(i);
                }
            }
        }

        // Handle escape to close dialogue
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Conclude();
        }
    }
}