using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Store : Interactable
{
    [Header("Store Settings")]
    public ItemContainer storeContent;
    public float buyFromPlayerMultip = 1.8f;
    public float sellToPlayerMultip = 1.0f;

    [Header("Part-Time Job Settings")]
    public bool hasPartTimeJob = false;
    [SerializeField] private float jobStartHour1 = 8f;  // 8:00 AM
    [SerializeField] private float jobStartHour2 = 16f; // 4:00 PM
    [SerializeField] private float timeWindow = 1f;     // 1 hour window to start job

    public override void Interact(Character character)
    {
        Trading trading = character.GetComponent<Trading>();
        if (trading == null)
        {
            return;
        }
        trading.BeginTrading(this);
    }

    /// <summary>
    /// Check if part-time job button should be visible based on current time
    /// </summary>
    public bool ShouldShowPartTimeButton()
    {
        if (!hasPartTimeJob) return false;

        if (DayTime.Instance == null) return false;

        float currentHour = DayTime.Instance.Hours;

        // Check if current time is within the allowed windows
        bool isFirstWindow = (currentHour >= jobStartHour1 && currentHour < jobStartHour1 + timeWindow);
        bool isSecondWindow = (currentHour >= jobStartHour2 && currentHour < jobStartHour2 + timeWindow);

        return isFirstWindow || isSecondWindow;
    }

    /// <summary>
    /// Get the next available part-time job time as a string
    /// </summary>
    public string GetNextJobTime()
    {
        if (!hasPartTimeJob) return "";

        float currentHour = DayTime.Instance.Hours;

        if (currentHour < jobStartHour1)
        {
            return $"{(int)jobStartHour1:00}:00";
        }
        else if (currentHour < jobStartHour2)
        {
            return $"{(int)jobStartHour2:00}:00";
        }
        else
        {
            return $"{(int)jobStartHour1:00}:00 (Tomorrow)";
        }
    }
}