
// Enhanced QuizUtility with examination types
using UnityEngine;

public class QuizUtility : MonoBehaviour
{
    public const float ResultDelayTime = 1;

    // Save keys for different examination types
    public const string MidtermScorePrefKey = "Exam_Midterm_Score";
    public const string FinalScorePrefKey = "Exam_Final_Score";
    public const string MidtermCompletedPrefKey = "Exam_Midterm_Completed";
    public const string FinalCompletedPrefKey = "Exam_Final_Completed";
    public const string TotalGradePrefKey = "Exam_Total_Grade";

    // Grade calculation weights
    public const float MidtermWeight = 0.3f; // 30%
    public const float FinalWeight = 0.7f;   // 70%

    // Grade boundaries
    public const float GradeA = 90f;
    public const float GradeB = 80f;
    public const float GradeC = 70f;
    public const float GradeD = 60f;

    public static string GetLetterGrade(float percentage)
    {
        if (percentage >= GradeA) return "A";
        else if (percentage >= GradeB) return "B";
        else if (percentage >= GradeC) return "C";
        else if (percentage >= GradeD) return "D";
        else return "F";
    }

    public static float CalculateFinalGrade(float midtermScore, float finalScore, float midtermMax, float finalMax)
    {
        float midtermPercentage = (midtermScore / midtermMax) * 100f;
        float finalPercentage = (finalScore / finalMax) * 100f;

        return (midtermPercentage * MidtermWeight) + (finalPercentage * FinalWeight);
    }
}