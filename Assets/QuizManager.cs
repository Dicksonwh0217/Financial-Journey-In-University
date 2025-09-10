using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Collections;
using TMPro;

public class QuizManager : MonoBehaviour
{
    Question[] _questions = null;
    public Question[] Questions { get { return _questions; } }

    [SerializeField] QuizEvents events = null;

    [Header("Examination Settings")]
    [SerializeField] ExaminationType currentExaminationType = ExaminationType.Midterm;
    [SerializeField] string examSubject = "General";

    [Header("UI References")]
    [SerializeField] Animator timerAnimator = null;
    [SerializeField] TextMeshProUGUI timerText = null;
    [SerializeField] TextMeshProUGUI examTypeText = null;
    [SerializeField] Color timerHalfWayOutColor = Color.yellow;
    [SerializeField] Color timerAlmostOutColor = Color.red;
    private Color timerDefaultColor = Color.white;

    [Header("Countdown References")]
    [SerializeField] ScreenTint screenTint = null;
    [SerializeField] TextMeshProUGUI countdownText = null;
    [SerializeField] GameObject countdownPanel = null;
    [SerializeField] float countdownDuration = 1f; // Duration for each countdown number

    // ADD THIS: Reference to DisableControls component
    [Header("Player Control References")]
    [SerializeField] DisableControls playerControls = null;

    private List<AnswerData> PickedAnswers = new List<AnswerData>();
    private List<int> FinishedQuestions = new List<int>();
    private int currentQuestion = 0;
    private int maxPossibleScore = 0;

    private int timerStateParaHash = 0;

    private IEnumerator IE_WaitTillNextRound = null;
    private IEnumerator IE_StartTimer = null;

    private bool quizStarted = false;
    private Question[] filteredQuestions;

    private bool IsFinished
    {
        get
        {
            return (FinishedQuestions.Count >= filteredQuestions.Length);
        }
    }

    void OnEnable()
    {
        events.UpdateQuestionAnswer += UpdateAnswers;
    }

    void OnDisable()
    {
        events.UpdateQuestionAnswer -= UpdateAnswers;
    }

    void Awake()
    {
        events.CurrentFinalScore = 0;
        events.CurrentExamType = currentExaminationType;

        // ADD THIS: Auto-find DisableControls if not assigned
        if (playerControls == null)
        {
            playerControls = FindObjectOfType<DisableControls>();
            if (playerControls == null)
            {
                Debug.LogWarning("DisableControls component not found! Player controls won't be disabled during exams.");
            }
        }
    }

    private void Start()
    {
        timerDefaultColor = timerText.color;
        LoadQuestions();
        FilterQuestionsByExamType();
        CalculateMaxScore();

        timerStateParaHash = Animator.StringToHash("TimerState");

        var seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        UnityEngine.Random.InitState(seed);

        UpdateExamTypeUI();

        // Initialize countdown UI
        if (countdownPanel != null)
        {
            countdownPanel.SetActive(false);
        }
    }

    [Header("Quiz Panel")]
    [SerializeField] GameObject quizPanel;

    public void StartMidtermExam()
    {
        // Check if midterm is already completed
        if (PlayerPrefs.GetInt(QuizUtility.MidtermCompletedPrefKey, 0) == 1)
        {
            Debug.Log("Midterm exam already completed!");
            // You might want to show a message to the player
            return;
        }

        // ADD THIS: Disable player controls at the start
        if (playerControls != null)
        {
            playerControls.DisableControl();
            Debug.Log("Player controls disabled for Midterm exam");
        }

        if (MusicManager.instance != null)
        {
            MusicManager.instance.StartMidtermExam();
        }

        currentExaminationType = ExaminationType.Midterm;
        StartCoroutine(StartExamWithCountdown());
    }

    public void StartFinalExam()
    {
        // Check if midterm is completed before allowing final exam
        if (PlayerPrefs.GetInt(QuizUtility.MidtermCompletedPrefKey, 0) == 0)
        {
            Debug.Log("Complete midterm exam first!");
            // You might want to show a message to the player
            return;
        }

        // Check if final is already completed
        if (PlayerPrefs.GetInt(QuizUtility.FinalCompletedPrefKey, 0) == 1)
        {
            Debug.Log("Final exam already completed!");
            return;
        }

        // ADD THIS: Disable player controls at the start
        if (playerControls != null)
        {
            playerControls.DisableControl();
            Debug.Log("Player controls disabled for Final exam");
        }

        if (MusicManager.instance != null)
        {
            MusicManager.instance.StartFinalExam();
        }

        currentExaminationType = ExaminationType.Final;
        StartCoroutine(StartExamWithCountdown());
    }

    private IEnumerator StartExamWithCountdown()
    {
        // Tint the screen
        if (screenTint != null)
        {
            yield return StartCoroutine(screenTint.TintCoroutine());
        }

        // Wait a moment after tinting
        yield return new WaitForSeconds(0.5f);

        // Show countdown panel
        if (countdownPanel != null)
        {
            countdownPanel.SetActive(true);
        }

        // Untint the screen
        if (screenTint != null)
        {
            yield return StartCoroutine(screenTint.UnTintCoroutine());
        }

        // Countdown from 3 to 1
        for (int i = 3; i >= 1; i--)
        {
            if (countdownText != null)
            {
                countdownText.text = i.ToString();
                countdownText.fontSize = 148f; // Make it big

                // Optional: Add a scaling effect
                if (countdownText.transform != null)
                {
                    StartCoroutine(ScaleCountdownNumber(countdownText.transform));
                }
            }

            yield return new WaitForSeconds(countdownDuration);
        }

        // Show "START!" or "GO!" message
        if (countdownText != null)
        {
            countdownText.text = "START!";
            countdownText.fontSize = 148f;

            if (countdownText.transform != null)
            {
                StartCoroutine(ScaleCountdownNumber(countdownText.transform));
            }
        }

        yield return new WaitForSeconds(countdownDuration);

        // Hide countdown panel
        if (countdownPanel != null)
        {
            countdownPanel.SetActive(false);
        }

        // Actually start the exam
        StartExam();
    }

    private IEnumerator ScaleCountdownNumber(Transform textTransform)
    {
        Vector3 originalScale = textTransform.localScale;
        Vector3 targetScale = originalScale * 1.2f;
        float scaleTime = 0.2f;

        // Scale up
        float elapsed = 0f;
        while (elapsed < scaleTime)
        {
            textTransform.localScale = Vector3.Lerp(originalScale, targetScale, elapsed / scaleTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Scale back down
        elapsed = 0f;
        while (elapsed < scaleTime)
        {
            textTransform.localScale = Vector3.Lerp(targetScale, originalScale, elapsed / scaleTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        textTransform.localScale = originalScale;
    }

    private void StartExam()
    {
        if (quizStarted) return;

        quizStarted = true;
        events.CurrentExamType = currentExaminationType;

        if (GameManager.instance != null && GameManager.instance.dialogueActionHandler != null)
        {
            GameManager.instance.dialogueActionHandler.OnQuizStarted();
        }

        if (quizPanel != null)
        {
            quizPanel.SetActive(true);
        }

        // Reset quiz state for new exam
        events.CurrentFinalScore = 0;
        FinishedQuestions.Clear();
        EraseAnswers();
        currentQuestion = 0;

        // Filter questions and calculate max score for current exam type
        FilterQuestionsByExamType();
        CalculateMaxScore();
        UpdateExamTypeUI();

        if (events.ScoreUpdated != null)
        {
            events.ScoreUpdated();
        }

        Display();
    }

    private void FilterQuestionsByExamType()
    {
        if (_questions == null || _questions.Length == 0) return;

        filteredQuestions = _questions.Where(q => q.ExaminationType == currentExaminationType).ToArray();
        Debug.Log($"Filtered {filteredQuestions.Length} questions for {currentExaminationType} exam");
    }

    private void CalculateMaxScore()
    {
        maxPossibleScore = 0;
        foreach (var question in filteredQuestions)
        {
            maxPossibleScore += question.AddScore;
        }
        events.CurrentExamMaxScore = maxPossibleScore;
        Debug.Log($"Max possible score for {currentExaminationType}: {maxPossibleScore}");
    }

    private void UpdateExamTypeUI()
    {
        if (examTypeText != null)
        {
            examTypeText.text = $"{currentExaminationType} Examination";
        }
    }

    public void UpdateAnswers(AnswerData newAnswer)
    {
        if (!quizStarted) return;

        if (filteredQuestions[currentQuestion].GetAnswerType == Question.AnswerType.Single)
        {
            foreach (var answer in PickedAnswers)
            {
                if (answer != newAnswer)
                {
                    answer.Reset();
                }
            }
            PickedAnswers.Clear();
            PickedAnswers.Add(newAnswer);
        }
        else
        {
            bool alreadyPicked = PickedAnswers.Exists(x => x == newAnswer);
            if (alreadyPicked)
            {
                PickedAnswers.Remove(newAnswer);
            }
            else
            {
                PickedAnswers.Add(newAnswer);
            }
        }
    }

    public void EraseAnswers()
    {
        foreach (var answer in PickedAnswers)
        {
            if (answer != null)
            {
                answer.Reset();
            }
        }
        PickedAnswers.Clear();
    }

    void Display()
    {
        if (!quizStarted) return;

        EraseAnswers();
        var question = GetRandomQuestion();

        if (question == null)
        {
            Debug.LogError("No question available to display!");
            return;
        }

        if (events.UpdateQuestionUI != null)
        {
            events.UpdateQuestionUI(question);
        }

        if (question.UseTimer)
        {
            UpdateTimer(question.UseTimer);
        }
    }

    public void Accept()
    {
        if (!quizStarted) return;

        UpdateTimer(false);
        bool isCorrect = CheckAnswers();
        FinishedQuestions.Add(currentQuestion);

        if (isCorrect)
        {
            UpdateScore(filteredQuestions[currentQuestion].AddScore);
        }

        if (IsFinished)
        {
            CompleteExam();
            quizStarted = false;
        }

        var type = (IsFinished) ? QuizUI.ResultScreenType.Finish : (isCorrect) ? QuizUI.ResultScreenType.Correct : QuizUI.ResultScreenType.Incorrect;

        if (events.DisplayResultScreen != null)
        {
            events.DisplayResultScreen(type, filteredQuestions[currentQuestion].AddScore);
        }

        if (!IsFinished)
        {
            if (IE_WaitTillNextRound != null)
            {
                StopCoroutine(IE_WaitTillNextRound);
            }
            IE_WaitTillNextRound = WaitTillNextRound();
            StartCoroutine(IE_WaitTillNextRound);
        }
    }

    private void CompleteExam()
    {
        string scoreKey = "";
        string completedKey = "";

        switch (currentExaminationType)
        {
            case ExaminationType.Midterm:
                scoreKey = QuizUtility.MidtermScorePrefKey;
                completedKey = QuizUtility.MidtermCompletedPrefKey;
                break;
            case ExaminationType.Final:
                scoreKey = QuizUtility.FinalScorePrefKey;
                completedKey = QuizUtility.FinalCompletedPrefKey;
                break;
        }

        // Save exam score and mark as completed
        PlayerPrefs.SetInt(scoreKey, events.CurrentFinalScore);
        PlayerPrefs.SetInt(completedKey, 1);

        // Calculate and save final grade if both exams are completed
        if (PlayerPrefs.GetInt(QuizUtility.MidtermCompletedPrefKey, 0) == 1 &&
            PlayerPrefs.GetInt(QuizUtility.FinalCompletedPrefKey, 0) == 1)
        {
            CalculateAndSaveFinalGrade();
        }

        PlayerPrefs.Save();

        if (MusicManager.instance != null)
        {
            MusicManager.instance.EndExam();
        }

        // ADD THIS: Re-enable player controls when exam is complete
        if (playerControls != null)
        {
            playerControls.EnableControl();
            Debug.Log($"Player controls re-enabled after {currentExaminationType} exam completion");
        }

        // Trigger exam completed event
        if (events.ExamCompleted != null)
        {
            events.ExamCompleted(currentExaminationType, events.CurrentFinalScore, maxPossibleScore);
        }

        Debug.Log($"{currentExaminationType} exam completed! Score: {events.CurrentFinalScore}/{maxPossibleScore}");
    }

    private void CalculateAndSaveFinalGrade()
    {
        int midtermScore = PlayerPrefs.GetInt(QuizUtility.MidtermScorePrefKey, 0);
        int finalScore = PlayerPrefs.GetInt(QuizUtility.FinalScorePrefKey, 0);

        // Calculate max scores for each exam type
        int midtermMaxScore = _questions.Where(q => q.ExaminationType == ExaminationType.Midterm).Sum(q => q.AddScore);
        int finalMaxScore = _questions.Where(q => q.ExaminationType == ExaminationType.Final).Sum(q => q.AddScore);

        float finalGrade = QuizUtility.CalculateFinalGrade(midtermScore, finalScore, midtermMaxScore, finalMaxScore);

        PlayerPrefs.SetFloat(QuizUtility.TotalGradePrefKey, finalGrade);
        PlayerPrefs.Save();

        Debug.Log($"Final Grade Calculated: {finalGrade:F2}% ({QuizUtility.GetLetterGrade(finalGrade)})");
    }

    // ADD THIS: Method to force re-enable controls (for emergency situations)
    public void ForceEnableControls()
    {
        if (playerControls != null)
        {
            playerControls.EnableControl();
            Debug.Log("Player controls force re-enabled");
        }
    }

    // Rest of the methods remain similar but use filteredQuestions instead of Questions
    void UpdateTimer(bool state)
    {
        if (!quizStarted) return;

        switch (state)
        {
            case true:
                if (IE_StartTimer != null)
                {
                    StopCoroutine(IE_StartTimer);
                }
                IE_StartTimer = StartTimer();
                StartCoroutine(IE_StartTimer);
                timerAnimator.SetInteger(timerStateParaHash, 2);
                break;
            case false:
                if (IE_StartTimer != null)
                {
                    StopCoroutine(IE_StartTimer);
                    IE_StartTimer = null;
                }
                timerAnimator.SetInteger(timerStateParaHash, 1);
                break;
        }
    }

    IEnumerator StartTimer()
    {
        var totalTime = filteredQuestions[currentQuestion].Timer;
        var timeLeft = totalTime;

        timerText.color = timerDefaultColor;
        while (timeLeft > 0 && quizStarted)
        {
            timeLeft--;

            if (timeLeft < totalTime / 2 && timeLeft > totalTime / 4)
            {
                timerText.color = timerHalfWayOutColor;
            }
            if (timeLeft < totalTime / 4)
            {
                timerText.color = timerAlmostOutColor;
            }

            timerText.text = timeLeft.ToString();
            yield return new WaitForSeconds(1.0f);
        }

        if (quizStarted)
        {
            Accept();
        }
    }

    IEnumerator WaitTillNextRound()
    {
        yield return new WaitForSeconds(QuizUtility.ResultDelayTime);
        if (!IsFinished && quizStarted)
        {
            Display();
        }
    }

    Question GetRandomQuestion()
    {
        if (IsFinished)
        {
            return null;
        }

        var randomIndex = GetRandomQuestionIndex();
        currentQuestion = randomIndex;

        return filteredQuestions[currentQuestion];
    }

    int GetRandomQuestionIndex()
    {
        var random = 0;
        if (FinishedQuestions.Count < filteredQuestions.Length)
        {
            do
            {
                random = UnityEngine.Random.Range(0, filteredQuestions.Length);
            } while (FinishedQuestions.Contains(random));
        }
        return random;
    }

    bool CheckAnswers()
    {
        return CompareAnswers();
    }

    bool CompareAnswers()
    {
        if (PickedAnswers.Count > 0)
        {
            List<int> correctAnswers = filteredQuestions[currentQuestion].GetCorrectAnswers();
            List<int> pickedAnswers = PickedAnswers.Select(x => x.AnswerIndex).ToList();

            var missingCorrect = correctAnswers.Except(pickedAnswers).ToList();
            var extraPicked = pickedAnswers.Except(correctAnswers).ToList();

            bool isCorrect = !missingCorrect.Any() && !extraPicked.Any();
            return isCorrect;
        }
        return false;
    }

    void LoadQuestions()
    {
        Object[] objs = Resources.LoadAll("Questions", typeof(Question));
        _questions = new Question[objs.Length];
        for (int i = 0; i < objs.Length; i++)
        {
            _questions[i] = (Question)objs[i];
        }

        Debug.Log($"Loaded {_questions.Length} questions");
    }

    private void UpdateScore(int add)
    {
        events.CurrentFinalScore += add;
        if (events.ScoreUpdated != null)
        {
            events.ScoreUpdated();
        }
    }

    // Public methods to check exam status
    public bool IsMidtermCompleted()
    {
        return PlayerPrefs.GetInt(QuizUtility.MidtermCompletedPrefKey, 0) == 1;
    }

    public bool IsFinalCompleted()
    {
        return PlayerPrefs.GetInt(QuizUtility.FinalCompletedPrefKey, 0) == 1;
    }

    public float GetFinalGrade()
    {
        return PlayerPrefs.GetFloat(QuizUtility.TotalGradePrefKey, 0f);
    }

    public void ResetAllExams()
    {
        PlayerPrefs.DeleteKey(QuizUtility.MidtermScorePrefKey);
        PlayerPrefs.DeleteKey(QuizUtility.FinalScorePrefKey);
        PlayerPrefs.DeleteKey(QuizUtility.MidtermCompletedPrefKey);
        PlayerPrefs.DeleteKey(QuizUtility.FinalCompletedPrefKey);
        PlayerPrefs.DeleteKey(QuizUtility.TotalGradePrefKey);
        PlayerPrefs.Save();
        Debug.Log("All exam progress reset!");

        // ADD THIS: Re-enable controls when resetting exams (in case they were disabled)
        if (playerControls != null)
        {
            playerControls.EnableControl();
        }
    }

    public static class EndGameGradeHelper
    {
        public static ExamResults GetExamResults()
        {
            ExamResults results = new ExamResults();

            results.midtermCompleted = PlayerPrefs.GetInt(QuizUtility.MidtermCompletedPrefKey, 0) == 1;
            results.finalCompleted = PlayerPrefs.GetInt(QuizUtility.FinalCompletedPrefKey, 0) == 1;

            if (results.midtermCompleted)
            {
                results.midtermScore = PlayerPrefs.GetInt(QuizUtility.MidtermScorePrefKey, 0);
            }

            if (results.finalCompleted)
            {
                results.finalScore = PlayerPrefs.GetInt(QuizUtility.FinalScorePrefKey, 0);
            }

            if (results.midtermCompleted && results.finalCompleted)
            {
                results.finalGrade = PlayerPrefs.GetFloat(QuizUtility.TotalGradePrefKey, 0f);
                results.letterGrade = QuizUtility.GetLetterGrade(results.finalGrade);
                results.bothExamsCompleted = true;
            }

            return results;
        }

        public static string GetGradeReport()
        {
            ExamResults results = GetExamResults();
            System.Text.StringBuilder report = new System.Text.StringBuilder();

            report.AppendLine("=== EXAMINATION RESULTS ===");
            report.AppendLine();

            // Midterm results
            if (results.midtermCompleted)
            {
                report.AppendLine($"Midterm Exam: {results.midtermScore} points ✓");
            }
            else
            {
                report.AppendLine("Midterm Exam: Not Completed ✗");
            }

            // Final results
            if (results.finalCompleted)
            {
                report.AppendLine($"Final Exam: {results.finalScore} points ✓");
            }
            else
            {
                report.AppendLine("Final Exam: Not Completed ✗");
            }

            report.AppendLine();

            // Final grade
            if (results.bothExamsCompleted)
            {
                report.AppendLine($"Final Grade: {results.finalGrade:F1}%");
                report.AppendLine($"Letter Grade: {results.letterGrade}");

                // Add grade interpretation
                if (results.finalGrade >= QuizUtility.GradeA)
                    report.AppendLine("Excellent work! 🏆");
                else if (results.finalGrade >= QuizUtility.GradeB)
                    report.AppendLine("Good job! 👍");
                else if (results.finalGrade >= QuizUtility.GradeC)
                    report.AppendLine("Satisfactory performance.");
                else if (results.finalGrade >= QuizUtility.GradeD)
                    report.AppendLine("You passed, but there's room for improvement.");
                else
                    report.AppendLine("You'll need to retake the examinations.");
            }
            else
            {
                report.AppendLine("Final Grade: Incomplete");
                report.AppendLine("Complete both examinations to receive your final grade.");
            }

            return report.ToString();
        }
    }

    [System.Serializable]
    public struct ExamResults
    {
        public bool midtermCompleted;
        public bool finalCompleted;
        public bool bothExamsCompleted;
        public int midtermScore;
        public int finalScore;
        public float finalGrade;
        public string letterGrade;
    }
}