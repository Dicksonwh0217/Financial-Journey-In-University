using UnityEngine;

[CreateAssetMenu(fileName = "QuizEvents", menuName = "Quiz/new QuizEvents")]
public class QuizEvents : ScriptableObject
{
    public delegate void UpdateQuestionUICallback(Question question);
    public UpdateQuestionUICallback UpdateQuestionUI;

    public delegate void UpdateQuestionAnswerCallback(AnswerData pickedAnswer);
    public UpdateQuestionAnswerCallback UpdateQuestionAnswer;

    public delegate void DisplayResultScreenCallback(QuizUI.ResultScreenType type, int score);
    public DisplayResultScreenCallback DisplayResultScreen;

    public delegate void ScoreUpdatedCallback();
    public ScoreUpdatedCallback ScoreUpdated;

    [HideInInspector]
    public int CurrentFinalScore;
    [HideInInspector]
    public int StartupTotalScore;
}
