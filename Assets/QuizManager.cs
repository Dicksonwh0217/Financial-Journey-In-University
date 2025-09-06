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

    [SerializeField] Animator timerAnimator = null;
    [SerializeField] TextMeshProUGUI timerText = null;
    [SerializeField] Color timerHalfWayOutColor = Color.yellow;
    [SerializeField] Color timerAlmostOutColor = Color.red;
    private Color timerDefaultColor = Color.white;

    private List<AnswerData> PickedAnswers = new List<AnswerData>();
    private List<int> FinishedQuestions = new List<int>();
    private int currentQuestion = 0;

    private int timerStateParaHash = 0;

    private IEnumerator IE_WaitTillNextRound = null;
    private IEnumerator IE_StartTimer = null;

    private bool quizStarted = false; // Added to control quiz start

    private bool IsFinished
    {
        get
        {
            return (FinishedQuestions.Count >= Questions.Length);
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
        // Clear the total score when quiz system starts
        PlayerPrefs.SetInt(QuizUtility.SavePrefKey, 0);
        PlayerPrefs.Save(); // Force save to ensure it's written immediately
    }

    private void Start()
    {
        events.StartupTotalScore = PlayerPrefs.GetInt(QuizUtility.SavePrefKey);

        timerDefaultColor = timerText.color;
        LoadQuestions();

        timerStateParaHash = Animator.StringToHash("TimerState");

        var seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        UnityEngine.Random.InitState(seed);

        // Don't start the quiz automatically anymore
        // Display();
    }

    [Header("Quiz Panel")]
    [SerializeField] GameObject quizPanel; // Assign your quiz panel GameObject

    /// <summary>
    /// Call this function from a button to start the quiz
    /// </summary>
    public void StartQuiz()
    {
        if (quizStarted) return; // Prevent starting multiple times

        quizStarted = true;

        if (GameManager.instance.dialogueActionHandler != null)
        {
            GameManager.instance.dialogueActionHandler.OnQuizStarted();
        }

        // Activate the quiz panel
        if (quizPanel != null)
        {
            quizPanel.SetActive(true);
        }

        // Reset quiz state for new game
        events.CurrentFinalScore = 0;
        FinishedQuestions.Clear();
        EraseAnswers();
        currentQuestion = 0;

        // Update score UI
        if (events.ScoreUpdated != null)
        {
            events.ScoreUpdated();
        }

        // Start the first question
        Display();
    }

    public void UpdateAnswers(AnswerData newAnswer)
    {
        if (!quizStarted) return; // Only allow answers when quiz is started

        if (Questions[currentQuestion].GetAnswerType == Question.AnswerType.Single)
        {
            // Fixed: Reset other answers first, then clear and add new answer
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
        // Reset all picked answers before clearing
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
        if (!quizStarted) return; // Only display when quiz is started

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
        else
        {
            Debug.LogWarning("Error while displaying new Question UI Data, GameEvents.UpdateQuestionUI is null");
        }

        if (question.UseTimer)
        {
            UpdateTimer(question.UseTimer);
        }
    }

    public void Accept()
    {
        if (!quizStarted) return; // Only allow accept when quiz is started

        UpdateTimer(false);
        bool isCorrect = CheckAnswers();
        FinishedQuestions.Add(currentQuestion);

        // Only add points for correct answers, no deduction for wrong answers
        if (isCorrect)
        {
            UpdateScore(Questions[currentQuestion].AddScore);
        }
        // No score change for incorrect answers

        if (IsFinished)
        {
            SetTotalScore();
            quizStarted = false; // Reset quiz state when finished
        }

        var type = (IsFinished) ? QuizUI.ResultScreenType.Finish : (isCorrect) ? QuizUI.ResultScreenType.Correct : QuizUI.ResultScreenType.Incorrect;

        if (events.DisplayResultScreen != null)
        {
            events.DisplayResultScreen(type, Questions[currentQuestion].AddScore);
        }

        // Only continue to next round if not finished
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

    void UpdateTimer(bool state)
    {
        if (!quizStarted) return; // Only update timer when quiz is started

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
        var totalTime = Questions[currentQuestion].Timer;
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

        return Questions[currentQuestion];
    }

    int GetRandomQuestionIndex()
    {
        var random = 0;
        if (FinishedQuestions.Count < Questions.Length)
        {
            do
            {
                random = UnityEngine.Random.Range(0, Questions.Length);
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
            List<int> correctAnswers = Questions[currentQuestion].GetCorrectAnswers();
            List<int> pickedAnswers = PickedAnswers.Select(x => x.AnswerIndex).ToList();

            // Debug logging to help troubleshoot
            Debug.Log($"Question: {Questions[currentQuestion].Info}");
            Debug.Log($"Correct answers: [{string.Join(", ", correctAnswers)}]");
            Debug.Log($"Picked answers: [{string.Join(", ", pickedAnswers)}]");

            var missingCorrect = correctAnswers.Except(pickedAnswers).ToList();
            var extraPicked = pickedAnswers.Except(correctAnswers).ToList();

            bool isCorrect = !missingCorrect.Any() && !extraPicked.Any();
            Debug.Log($"Answer is correct: {isCorrect}");

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

    private void SetTotalScore()
    {
        var totalscore = PlayerPrefs.GetInt(QuizUtility.SavePrefKey);
        // Add current session score to total score (cumulative)
        totalscore += events.CurrentFinalScore;
        PlayerPrefs.SetInt(QuizUtility.SavePrefKey, totalscore);
        Debug.Log($"Total Score updated: Previous + Current Session = {PlayerPrefs.GetInt(QuizUtility.SavePrefKey)} + {events.CurrentFinalScore} = {totalscore}");
    }

    private void UpdateScore(int add)
    {
        events.CurrentFinalScore += add;
        if (events.ScoreUpdated != null)
        {
            events.ScoreUpdated();
        }
    }
}