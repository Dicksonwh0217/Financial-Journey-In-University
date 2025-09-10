using UnityEngine;
using System;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

[Serializable()]
public struct QuizUIParameters
{
    [Header("Answers Options")]
    [SerializeField] float margins;
    public float Margins { get { return margins; } }

    [Header("Result Screen Options")]
    [SerializeField] Color correctResultColor;
    public Color CorrectResultColor { get { return correctResultColor; } }
    [SerializeField] Color incorrectResultColor;
    public Color IncorrectResultColor { get { return incorrectResultColor; } }
    [SerializeField] Color finalResultColor;
    public Color FinalResultColor { get { return finalResultColor; } }

    [Header("Audio Options")]
    [SerializeField] AudioClip correctSFX;
    public AudioClip CorrectSFX { get { return correctSFX; } }
    [SerializeField] AudioClip incorrectSFX;
    public AudioClip IncorrectSFX { get { return incorrectSFX; } }
    [SerializeField] AudioClip finishSFX;
    public AudioClip FinishSFX { get { return finishSFX; } }
}

[Serializable()]
public struct UIElements
{
    [SerializeField] RectTransform answersArea;
    public RectTransform AnswersArea { get { return answersArea; } }

    [SerializeField] TextMeshProUGUI questionInfoTextObject;
    public TextMeshProUGUI QuestionInfoTextObject { get { return questionInfoTextObject; } }

    [SerializeField] TextMeshProUGUI scoreText;
    public TextMeshProUGUI ScoreText { get { return scoreText; } }

    [Space]

    [SerializeField] Animator resultPanelAnimator;
    public Animator ResultPanelAnimator { get { return resultPanelAnimator; } }

    [SerializeField] Image resultPanel;
    public Image ResultPanel { get { return resultPanel; } }

    [SerializeField] TextMeshProUGUI resultPanelInfoText;
    public TextMeshProUGUI RresultPanelInfoText { get { return resultPanelInfoText; } }

    [SerializeField] TextMeshProUGUI resultPanelScoreText;
    public TextMeshProUGUI ResultPanelScoreText { get { return resultPanelScoreText; } }

    [Space]

    [SerializeField] TextMeshProUGUI totalScoreText;
    public TextMeshProUGUI TotalScoreText { get { return totalScoreText; } }

    [SerializeField] CanvasGroup mainCanvasGroup;
    public CanvasGroup MainCanvasGroup { get { return mainCanvasGroup; } }

    [SerializeField] RectTransform finishUIElements;
    public RectTransform FinishUIElements { get { return finishUIElements; } }
}

public class QuizUI : MonoBehaviour
{
    public enum ResultScreenType { Correct, Incorrect, Finish }

    [Header("References")]
    [SerializeField] QuizEvents events;

    [Header("UI Elements (Prefab)")]
    [SerializeField] AnswerData answerPrefab;

    [Header("Audio")]
    [SerializeField] AudioSource audioSource;

    [SerializeField] UIElements uIElements;

    [Space]
    [SerializeField] QuizUIParameters parameters;

    List<AnswerData> currentAnswer = new List<AnswerData>();
    private int resStateParaHash = 0;

    private IEnumerator IE_DIsplayTimedResult;

    void OnEnable()
    {
        events.UpdateQuestionUI += UpdateQuestionUI;
        events.DisplayResultScreen += DisplayResult;
        events.ScoreUpdated += UpdateScoreUI;
    }

    void OnDisable()
    {
        events.UpdateQuestionUI -= UpdateQuestionUI;
        events.DisplayResultScreen -= DisplayResult;
        events.ScoreUpdated -= UpdateScoreUI;
    }

    private void Start()
    {
        UpdateScoreUI();
        resStateParaHash = Animator.StringToHash("ScreenState");
    }

    void UpdateQuestionUI(Question question)
    {
        uIElements.QuestionInfoTextObject.text = question.Info;
        CreateAnswers(question);
    }

    void DisplayResult(ResultScreenType type, int score)
    {
        UpdateResUI(type, score);
        PlayResultSound(type); // Play sound effect based on result type
        uIElements.ResultPanelAnimator.SetInteger(resStateParaHash, 2);
        uIElements.MainCanvasGroup.blocksRaycasts = false;

        if (type != ResultScreenType.Finish)
        {
            if (IE_DIsplayTimedResult != null)
            {
                StopCoroutine(IE_DIsplayTimedResult);
            }
            IE_DIsplayTimedResult = DisplayTimedResult();
            StartCoroutine(IE_DIsplayTimedResult);
        }
    }

    IEnumerator DisplayTimedResult()
    {
        yield return new WaitForSeconds(QuizUtility.ResultDelayTime);
        uIElements.ResultPanelAnimator.SetInteger(resStateParaHash, 1);
        uIElements.MainCanvasGroup.blocksRaycasts = true;
    }

    void UpdateResUI(ResultScreenType type, int score)
    {
        switch (type)
        {
            case ResultScreenType.Correct:
                uIElements.ResultPanel.color = parameters.CorrectResultColor;
                uIElements.RresultPanelInfoText.text = "CORRECT!";
                uIElements.ResultPanelScoreText.text = "+" + score;
                break;
            case ResultScreenType.Incorrect:
                uIElements.ResultPanel.color = parameters.IncorrectResultColor;
                uIElements.RresultPanelInfoText.text = "WRONG!";
                uIElements.ResultPanelScoreText.text = "0"; // No points deducted, show 0
                break;
            case ResultScreenType.Finish:
                uIElements.ResultPanel.color = parameters.FinalResultColor;
                uIElements.RresultPanelInfoText.text = "FINAL SCORE";

                StartCoroutine(CalculateScore());
                uIElements.FinishUIElements.gameObject.SetActive(true);
                uIElements.TotalScoreText.gameObject.SetActive(true);

                // Calculate the new total score (previous + current session)
                int maxScore = events.CurrentExamMaxScore;
                uIElements.TotalScoreText.text = $"Score: {events.CurrentFinalScore} / {maxScore}";
                break;
        }
    }

    IEnumerator CalculateScore()
    {
        var scoreValue = 0;
        while (scoreValue < events.CurrentFinalScore)
        {
            scoreValue++;
            uIElements.ResultPanelScoreText.text = scoreValue.ToString();

            yield return null;
        }
    }

    void CreateAnswers(Question question)
    {
        EraseAnswers();

        float offset = 0 - parameters.Margins;
        for (int i = 0; i < question.Answers.Length; i++)
        {
            AnswerData newAnswers = (AnswerData)Instantiate(answerPrefab, uIElements.AnswersArea);
            newAnswers.UpdateData(question.Answers[i].Info, i);

            newAnswers.Rect.anchoredPosition = new Vector2(0, offset);

            offset -= (newAnswers.Rect.sizeDelta.y + parameters.Margins);
            uIElements.AnswersArea.sizeDelta = new Vector2(uIElements.AnswersArea.sizeDelta.x, offset * -1);

            currentAnswer.Add(newAnswers);
        }
    }

    void EraseAnswers()
    {
        foreach (var answer in currentAnswer)
        {
            if (answer != null)
            {
                Destroy(answer.gameObject);
            }
        }
        currentAnswer.Clear(); // Clear the list immediately after destroying GameObjects
    }

    void UpdateScoreUI()
    {
        uIElements.ScoreText.text = events.CurrentFinalScore.ToString();
    }

    void PlayResultSound(ResultScreenType type)
    {
        if (audioSource == null)
        {
            Debug.LogWarning("AudioSource is not assigned in QuizUI!");
            return;
        }

        AudioClip clipToPlay = null;

        switch (type)
        {
            case ResultScreenType.Correct:
                clipToPlay = parameters.CorrectSFX;
                break;
            case ResultScreenType.Incorrect:
                clipToPlay = parameters.IncorrectSFX;
                break;
            case ResultScreenType.Finish:
                clipToPlay = parameters.FinishSFX;
                break;
        }

        if (clipToPlay != null)
        {
            audioSource.PlayOneShot(clipToPlay);
        }
        else
        {
            Debug.LogWarning($"Audio clip for {type} is not assigned!");
        }
    }
}