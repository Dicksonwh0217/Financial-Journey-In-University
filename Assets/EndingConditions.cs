using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public enum EndingType
{
    StarvingDeath,
    MentalIllnessDeath,
    SleepOnStreet,
    FastFoodWorker,
    NormalOfficer,
    SuccessfulPerson
}

[Serializable]
public class EndingData
{
    [Header("Ending Information")]
    public EndingType endingType;
    public string endingTitle;
    [TextArea(3, 10)]
    public List<string> dialogueLines = new List<string>();

    [Header("Visual Settings")]
    public Sprite endingImage;
    public Color backgroundColor = Color.black;
}

[System.Serializable]
public class EndingConditions
{
    [Header("Death Conditions")]
    public bool checkStarvingDeath = true;
    public bool checkMentalIllnessDeath = true;

    [Header("Sleep on Street Conditions")]
    public bool checkExpiredBills = true;

    [Header("Fast Food Worker Conditions")]
    public bool checkMissedExaminations = true;
    public int midtermDeadlineDay = 45;
    public int midtermDeadlineHour = 10; // 10 AM
    public int finalDeadlineDay = 90;
    public int finalDeadlineHour = 15; // 3 PM

    [Header("Normal Officer Conditions")]
    public float normalOfficerMaxCurrentMoney = 10000f;
    public float normalOfficerMinTotalEarnings = 20000f;
    public string normalOfficerMinGrade = "C"; // Must be C or better (C, B, A)

    [Header("Successful Person Conditions")]
    public float successfulPersonMinCurrentMoney = 10000f;
    public float successfulPersonMinTotalEarnings = 20000f;
    public string successfulPersonRequiredGrade = "A"; // Must get A grade
}