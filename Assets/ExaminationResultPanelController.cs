using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ExaminationResultsPanelController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject resultsPanel;
    [SerializeField] private TextMeshProUGUI midtermResultText;
    [SerializeField] private TextMeshProUGUI finalResultText;
    [SerializeField] private Button closeButton;

    [Header("Examination Schedule")]
    [SerializeField] private int midtermDay = 45;
    [SerializeField] private string midtermTime = "8:00 a.m. - 10:00 a.m.";
    [SerializeField] private int finalDay = 90;
    [SerializeField] private string finalTime = "12:00 p.m. - 3:00 p.m.";

    [Header("Colors")]
    [SerializeField] private Color completedColor = Color.green;
    [SerializeField] private Color pendingColor = Color.yellow;
    [SerializeField] private Color notAvailableColor = Color.gray;

    private bool isPanelOpen = false;

    private void Start()
    {
        // Ensure panel starts closed
        if (resultsPanel != null)
        {
            resultsPanel.SetActive(false);
        }

        // Setup close button
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(ClosePanel);
        }
    }

    private void Update()
    {
        // Check for Q key press to toggle panel
        if (Input.GetKeyDown(KeyCode.Q))
        {
            TogglePanel();
        }
    }

    public void TogglePanel()
    {
        if (isPanelOpen)
        {
            ClosePanel();
        }
        else
        {
            OpenPanel();
        }
    }

    public void OpenPanel()
    {
        if (resultsPanel != null)
        {
            resultsPanel.SetActive(true);
            isPanelOpen = true;
            UpdatePanelContent();
        }
    }

    public void ClosePanel()
    {
        if (resultsPanel != null)
        {
            resultsPanel.SetActive(false);
            isPanelOpen = false;
        }
    }

    private void UpdatePanelContent()
    {
        UpdateMidtermDisplay();
        UpdateFinalDisplay();
    }

    private void UpdateMidtermDisplay()
    {
        bool midtermCompleted = PlayerPrefs.GetInt(QuizUtility.MidtermCompletedPrefKey, 0) == 1;

        if (midtermCompleted)
        {
            // Show completed midterm result
            int midtermScore = PlayerPrefs.GetInt(QuizUtility.MidtermScorePrefKey, 0);
            int midtermMaxScore = GetMaxScoreForExamType(ExaminationType.Midterm);
            float percentage = midtermMaxScore > 0 ? (float)midtermScore / midtermMaxScore * 100f : 0f;

            midtermResultText.text =$"Score: {midtermScore} / {midtermMaxScore}\n" +
                                   $"Percentage: {percentage:F1}%\n" +
                                   $"Status: COMPLETED";

            midtermResultText.color = completedColor;
        }
        else
        {
            // Show scheduled midterm time
            midtermResultText.text =$"Scheduled: Day {midtermDay}\n" +
                                   $"Time: {midtermTime}\n" +
                                   $"Status: NOT TAKEN";

            midtermResultText.color = pendingColor;
        }
    }

    private void UpdateFinalDisplay()
    {
        bool midtermCompleted = PlayerPrefs.GetInt(QuizUtility.MidtermCompletedPrefKey, 0) == 1;
        bool finalCompleted = PlayerPrefs.GetInt(QuizUtility.FinalCompletedPrefKey, 0) == 1;

        if (finalCompleted)
        {
            // Show completed final result
            int finalScore = PlayerPrefs.GetInt(QuizUtility.FinalScorePrefKey, 0);
            int finalMaxScore = GetMaxScoreForExamType(ExaminationType.Final);
            float percentage = finalMaxScore > 0 ? (float)finalScore / finalMaxScore * 100f : 0f;

            finalResultText.text = $"Score: {finalScore} / {finalMaxScore}\n" +
                                 $"Percentage: {percentage:F1}%\n";

            finalResultText.color = completedColor;
        }
        else if (midtermCompleted)
        {
            // Show scheduled final time (available after midterm)
            finalResultText.text = $"Scheduled: Day {finalDay}\n" +
                                 $"Time: {finalTime}\n" +
                                 $"Status: NOT TAKEN";

            finalResultText.color = pendingColor;
        }
        else
        {
            // Final not available yet (midterm not completed)
            finalResultText.text = $"Scheduled: Day {finalDay}\n" +
                                 $"Time: {finalTime}\n" +
                                 $"Status: NOT AVAILABLE\n" +
                                 $"(Complete Midterm First)";

            finalResultText.color = notAvailableColor;
        }
    }

    private int GetMaxScoreForExamType(ExaminationType examType)
    {
        // This method calculates max score based on questions in Resources folder
        Object[] questionObjects = Resources.LoadAll("Questions", typeof(Question));
        int maxScore = 0;

        foreach (Object obj in questionObjects)
        {
            Question question = obj as Question;
            if (question != null && question.ExaminationType == examType)
            {
                maxScore += question.AddScore;
            }
        }

        return maxScore;
    }

    private Color GetGradeColor(string letterGrade)
    {
        switch (letterGrade)
        {
            case "A":
                return Color.green;
            case "B":
                return Color.cyan;
            case "C":
                return Color.yellow;
            case "D":
                return new Color(1f, 0.5f, 0f); // Orange
            case "F":
                return Color.red;
            default:
                return Color.white;
        }
    }

    // Public methods for external access
    public bool IsPanelOpen => isPanelOpen;

    public void SetMidtermSchedule(int day, string time)
    {
        midtermDay = day;
        midtermTime = time;
        if (isPanelOpen)
        {
            UpdatePanelContent();
        }
    }

    public void SetFinalSchedule(int day, string time)
    {
        finalDay = day;
        finalTime = time;
        if (isPanelOpen)
        {
            UpdatePanelContent();
        }
    }

    // Method to refresh panel content (useful when called from other scripts)
    public void RefreshPanelContent()
    {
        if (isPanelOpen)
        {
            UpdatePanelContent();
        }
    }
}