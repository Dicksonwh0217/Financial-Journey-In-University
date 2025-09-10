// EndingManager.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndingManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Character character;
    [SerializeField] private ScreenTint screenTint;
    [SerializeField] private EndingDialogueUI dialogueUI;
    [SerializeField] private Currency currency;
    [SerializeField] private BillPanel billPanel;

    [Header("Ending Configurations")]
    [SerializeField] private EndingConditions conditions;
    [SerializeField] private List<EndingData> endingDataList = new List<EndingData>();

    [Header("Game Progress")]
    [SerializeField] private int currentDay = 1;
    [SerializeField] private int maxDays = 90;

    [Header("Scene Management")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = false;

    [Header("Timing Settings")]
    [SerializeField] private float delayBeforeUntint = 0.5f;
    [SerializeField] private float delayAfterUntint = 0.2f; // Reduced delay

    private bool endingTriggered = false;
    private Dictionary<EndingType, EndingData> endingDataDict;
    private DayTime dayTimeSystem;

    private void Awake()
    {
        // Create dictionary for quick lookup
        endingDataDict = new Dictionary<EndingType, EndingData>();
        foreach (var endingData in endingDataList)
        {
            if (!endingDataDict.ContainsKey(endingData.endingType))
            {
                endingDataDict.Add(endingData.endingType, endingData);
            }
        }

        // Set up dialogue completion callback
        if (dialogueUI != null)
        {
            dialogueUI.OnDialogueComplete = OnEndingDialogueComplete;
        }

        // Find required components if not assigned
        if (currency == null)
            currency = FindFirstObjectByType<Currency>();
        if (billPanel == null)
            billPanel = FindFirstObjectByType<BillPanel>();

        dayTimeSystem = FindFirstObjectByType<DayTime>();
    }

    private void Start()
    {
        // Subscribe to character death if available
        if (character != null)
        {
            // We'll check for death in Update since the Character script doesn't have events
        }
    }

    private void Update()
    {
        if (!endingTriggered && character != null)
        {
            CheckForEndings();
        }

        // Check for examination deadline failures
        if (!endingTriggered)
        {
            CheckExaminationDeadlines();
        }
    }

    public void IncrementDay()
    {
        currentDay++;
        if (enableDebugLogs)
            Debug.Log($"Day incremented to: {currentDay}");

        if (currentDay >= maxDays && !endingTriggered)
        {
            Check90DayEndings();
        }
    }

    public void SetCurrentDay(int day)
    {
        currentDay = day;
    }

    private void CheckForEndings()
    {
        // Check death conditions first (highest priority)
        if (character.isDead)
        {
            if (character.Health.IsEmpty())
            {
                TriggerEnding(EndingType.StarvingDeath);
            }
            else if (character.Happiness.IsEmpty())
            {
                TriggerEnding(EndingType.MentalIllnessDeath);
            }
            return;
        }

        // Check for expired bills (Sleep on Street condition)
        if (conditions.checkExpiredBills)
        {
            CheckExpiredBillsCondition();
        }
    }

    private void CheckExpiredBillsCondition()
    {
        if (billPanel != null)
        {
            // Check if there are any critical expired bills (more than 7 days overdue)
            bool hasExpiredBills = billPanel.HasExpiredBills();

            if (hasExpiredBills)
            {
                if (enableDebugLogs)
                    Debug.Log("Player has expired bills - triggering Sleep on Street ending");
                TriggerEnding(EndingType.SleepOnStreet);
            }
        }
    }

    private bool HasExpiredBills()
    {
        // This method is no longer needed since we use billPanel.HasExpiredBills() directly
        if (billPanel != null)
        {
            return billPanel.HasExpiredBills();
        }
        return false;
    }

    private void CheckExaminationDeadlines()
    {
        if (!conditions.checkMissedExaminations) return;

        // Get current day and hour from DayTime system
        int currentDay = dayTimeSystem != null ? dayTimeSystem.days + 1 : this.currentDay;
        float currentHour = dayTimeSystem != null ? dayTimeSystem.Hours : 0f;

        // Check midterm deadline (Day 45, 10 AM)
        bool midtermMissed = CheckMidtermDeadline(currentDay, currentHour);

        // Check final exam deadline (Day 90, 3 PM)  
        bool finalMissed = CheckFinalDeadline(currentDay, currentHour);

        if (midtermMissed || finalMissed)
        {
            if (enableDebugLogs)
                Debug.Log($"Examination deadline missed - Midterm: {midtermMissed}, Final: {finalMissed}");
            TriggerEnding(EndingType.FastFoodWorker);
        }
    }

    private bool CheckMidtermDeadline(int currentDay, float currentHour)
    {
        // Check if we've passed the midterm deadline
        if (currentDay > conditions.midtermDeadlineDay)
        {
            return !PlayerPrefs.HasKey(QuizUtility.MidtermCompletedPrefKey) ||
                   PlayerPrefs.GetInt(QuizUtility.MidtermCompletedPrefKey, 0) == 0;
        }
        else if (currentDay == conditions.midtermDeadlineDay && currentHour >= conditions.midtermDeadlineHour)
        {
            return !PlayerPrefs.HasKey(QuizUtility.MidtermCompletedPrefKey) ||
                   PlayerPrefs.GetInt(QuizUtility.MidtermCompletedPrefKey, 0) == 0;
        }
        return false;
    }

    private bool CheckFinalDeadline(int currentDay, float currentHour)
    {
        // Check if we've passed the final exam deadline
        if (currentDay > conditions.finalDeadlineDay)
        {
            return !PlayerPrefs.HasKey(QuizUtility.FinalCompletedPrefKey) ||
                   PlayerPrefs.GetInt(QuizUtility.FinalCompletedPrefKey, 0) == 0;
        }
        else if (currentDay == conditions.finalDeadlineDay && currentHour >= conditions.finalDeadlineHour)
        {
            return !PlayerPrefs.HasKey(QuizUtility.FinalCompletedPrefKey) ||
                   PlayerPrefs.GetInt(QuizUtility.FinalCompletedPrefKey, 0) == 0;
        }
        return false;
    }

    private void Check90DayEndings()
    {
        if (enableDebugLogs)
            Debug.Log("Checking 90-day endings...");

        // Get currency information
        float currentMoney = currency != null ? currency.GetAmount() : 0f;
        float totalEarnings = currency != null ? currency.GetTotalEarnings() : 0f;

        // Get final grade
        string finalGrade = GetFinalGrade();

        if (enableDebugLogs)
        {
            Debug.Log($"Current Money: {currentMoney}, Total Earnings: {totalEarnings}, Grade: {finalGrade}");
        }

        // Check for Successful Person ending (highest priority)
        if (currentMoney >= conditions.successfulPersonMinCurrentMoney &&
            totalEarnings >= conditions.successfulPersonMinTotalEarnings &&
            finalGrade == conditions.successfulPersonRequiredGrade)
        {
            TriggerEnding(EndingType.SuccessfulPerson);
            return;
        }

        // Check for Normal Officer ending
        if (currentMoney < conditions.normalOfficerMaxCurrentMoney &&
            totalEarnings >= conditions.normalOfficerMinTotalEarnings &&
            IsGradeAtLeast(finalGrade, conditions.normalOfficerMinGrade))
        {
            TriggerEnding(EndingType.NormalOfficer);
            return;
        }

        // Default to Fast Food Worker ending if no other conditions are met
        TriggerEnding(EndingType.FastFoodWorker);
    }

    private string GetFinalGrade()
    {
        // Check if both exams are completed
        bool midtermCompleted = PlayerPrefs.GetInt(QuizUtility.MidtermCompletedPrefKey, 0) == 1;
        bool finalCompleted = PlayerPrefs.GetInt(QuizUtility.FinalCompletedPrefKey, 0) == 1;

        if (!midtermCompleted || !finalCompleted)
        {
            return "F"; // Failed if didn't complete both exams
        }

        // Get saved total grade or calculate it
        if (PlayerPrefs.HasKey(QuizUtility.TotalGradePrefKey))
        {
            float totalGrade = PlayerPrefs.GetFloat(QuizUtility.TotalGradePrefKey);
            return QuizUtility.GetLetterGrade(totalGrade);
        }
        else
        {
            // Calculate from individual scores if total grade not saved
            float midtermScore = PlayerPrefs.GetFloat(QuizUtility.MidtermScorePrefKey, 0f);
            float finalScore = PlayerPrefs.GetFloat(QuizUtility.FinalScorePrefKey, 0f);

            // Assuming max scores (you might want to make these configurable)
            float midtermMax = 100f;
            float finalMax = 100f;

            float totalGrade = QuizUtility.CalculateFinalGrade(midtermScore, finalScore, midtermMax, finalMax);

            // Save the calculated grade
            PlayerPrefs.SetFloat(QuizUtility.TotalGradePrefKey, totalGrade);
            PlayerPrefs.Save();

            return QuizUtility.GetLetterGrade(totalGrade);
        }
    }

    private bool IsGradeAtLeast(string actualGrade, string minimumGrade)
    {
        // Convert grades to numeric values for comparison
        int actualValue = GradeToNumeric(actualGrade);
        int minimumValue = GradeToNumeric(minimumGrade);

        return actualValue >= minimumValue;
    }

    private int GradeToNumeric(string grade)
    {
        switch (grade.ToUpper())
        {
            case "A": return 4;
            case "B": return 3;
            case "C": return 2;
            case "D": return 1;
            case "F": return 0;
            default: return 0;
        }
    }

    public void TriggerEnding(EndingType endingType)
    {
        if (endingTriggered) return;

        endingTriggered = true;

        if (enableDebugLogs)
            Debug.Log($"Triggering ending: {endingType}");

        if (!endingDataDict.ContainsKey(endingType))
        {
            Debug.LogError($"Ending data not found for {endingType}");
            return;
        }

        StartCoroutine(PlayEndingSequence(endingDataDict[endingType]));
    }

    private IEnumerator PlayEndingSequence(EndingData endingData)
    {
        if (enableDebugLogs)
            Debug.Log($"Playing ending sequence for: {endingData.endingType}");

        // First, tint the screen
        if (screenTint != null)
        {
            yield return StartCoroutine(screenTint.TintCoroutine());
        }

        // Ensure dialogue UI is ready
        if (dialogueUI != null)
        {
            dialogueUI.gameObject.SetActive(true);
        }

        yield return new WaitForSeconds(delayBeforeUntint);

        // Start showing the ending dialogue BEFORE untinting
        // This way the dialogue appears as soon as the screen is visible
        if (dialogueUI != null)
        {
            // Prepare the dialogue but don't show it yet
            dialogueUI.PrepareEndingDialogue(endingData);
        }

        // Untint the screen
        if (screenTint != null)
        {
            yield return StartCoroutine(screenTint.UnTintCoroutine());
        }

        // Now show the prepared dialogue
        if (dialogueUI != null)
        {
            dialogueUI.ShowPreparedDialogue();
        }
    }

    private void OnEndingDialogueComplete()
    {
        if (enableDebugLogs)
            Debug.Log("Ending dialogue completed, returning to main menu");

        StartCoroutine(ReturnToMainMenu());
    }

    private IEnumerator ReturnToMainMenu()
    {

        if (screenTint != null)
        {
            yield return StartCoroutine(screenTint.TintCoroutine());
        }

        CleanupDontDestroyOnLoadObjects();

        // Load main menu scene
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void CleanupDontDestroyOnLoadObjects()
    {
        if (enableDebugLogs)
            Debug.Log("Cleaning up DontDestroyOnLoad objects before returning to main menu");

        // Get all GameObjects in DontDestroyOnLoad scene
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        int destroyedCount = 0;

        foreach (GameObject obj in allObjects)
        {
            if (obj.scene.name == "DontDestroyOnLoad" && obj.transform.parent == null)
            {
                if (enableDebugLogs)
                    Debug.Log($"Destroying DontDestroyOnLoad object: {obj.name}");

                Destroy(obj);
                destroyedCount++;
            }
        }

        if (enableDebugLogs)
            Debug.Log($"Destroyed {destroyedCount} DontDestroyOnLoad objects");
    }

    // Public methods for external triggers
    public void ForceEnding(EndingType endingType)
    {
        TriggerEnding(endingType);
    }

    public bool IsEndingTriggered()
    {
        return endingTriggered;
    }

    // Debug methods for testing endings
    [ContextMenu("Test Starving Death")]
    public void TestStarvingDeath()
    {
        ForceEnding(EndingType.StarvingDeath);
    }

    [ContextMenu("Test Mental Illness Death")]
    public void TestMentalIllnessDeath()
    {
        ForceEnding(EndingType.MentalIllnessDeath);
    }

    [ContextMenu("Test Sleep on Street")]
    public void TestSleepOnStreet()
    {
        ForceEnding(EndingType.SleepOnStreet);
    }

    [ContextMenu("Test Fast Food Worker")]
    public void TestFastFoodWorker()
    {
        ForceEnding(EndingType.FastFoodWorker);
    }

    [ContextMenu("Test Normal Officer")]
    public void TestNormalOfficer()
    {
        ForceEnding(EndingType.NormalOfficer);
    }

    [ContextMenu("Test Successful Person")]
    public void TestSuccessfulPerson()
    {
        ForceEnding(EndingType.SuccessfulPerson);
    }

    [ContextMenu("Debug Current Status")]
    public void DebugCurrentStatus()
    {
        if (currency != null)
        {
            Debug.Log($"Current Money: {currency.GetAmount():F2}");
            Debug.Log($"Total Earnings: {currency.GetTotalEarnings():F2}");
        }

        int currentDay = dayTimeSystem != null ? dayTimeSystem.days + 1 : this.currentDay;
        float currentHour = dayTimeSystem != null ? dayTimeSystem.Hours : 0f;
        Debug.Log($"Current Day: {currentDay}, Hour: {currentHour:F2}");

        string grade = GetFinalGrade();
        Debug.Log($"Current Grade: {grade}");

        bool midtermCompleted = PlayerPrefs.GetInt(QuizUtility.MidtermCompletedPrefKey, 0) == 1;
        bool finalCompleted = PlayerPrefs.GetInt(QuizUtility.FinalCompletedPrefKey, 0) == 1;
        Debug.Log($"Midterm Completed: {midtermCompleted}, Final Completed: {finalCompleted}");
    }
}