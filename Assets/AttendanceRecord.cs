using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AttendanceRecord
{
    public string className;
    public DayOfWeek dayOfWeek;
    public int dayNumber; // which day (from DayTime.days)
    public float attendanceTime; // what time the player attended
    public bool attended;

    public AttendanceRecord(string name, DayOfWeek day, int dayNum, float time, bool didAttend)
    {
        className = name;
        dayOfWeek = day;
        dayNumber = dayNum;
        attendanceTime = time;
        attended = didAttend;
    }
}

[Serializable]
public class AttendanceData
{
    public List<AttendanceRecord> records = new List<AttendanceRecord>();

    public void AddRecord(AttendanceRecord record)
    {
        records.Add(record);
        Debug.Log($"Added attendance record: {record.className} - {(record.attended ? "Present" : "Absent")} on {record.dayOfWeek}");
    }

    // FIXED: Now returns total HOURS attended, not just number of records
    public int GetAttendanceCount(string className)
    {
        if (TimetableManager.Instance == null)
            return 0;

        int totalHours = 0;
        var weeklyClasses = TimetableManager.Instance.GetWeeklyClasses();

        foreach (var record in records)
        {
            if (record.className == className && record.attended)
            {
                // Find the corresponding class to get its duration
                foreach (var classItem in weeklyClasses)
                {
                    if (classItem.className == className && classItem.dayOfWeek == record.dayOfWeek)
                    {
                        float duration = classItem.endTime - classItem.startTime;
                        totalHours += Mathf.RoundToInt(duration); // Round to nearest hour
                        break; // Found the matching class, break inner loop
                    }
                }
            }
        }

        return totalHours;
    }

    public int GetTotalClassCount(string className)
    {
        if (TimetableManager.Instance == null)
            return 0;

        int totalHours = 0;
        var weeklyClasses = TimetableManager.Instance.GetWeeklyClasses();

        foreach (var record in records)
        {
            if (record.className == className)
            {
                // Find the corresponding class to get its duration
                foreach (var classItem in weeklyClasses)
                {
                    if (classItem.className == className && classItem.dayOfWeek == record.dayOfWeek)
                    {
                        float duration = classItem.endTime - classItem.startTime;
                        totalHours += Mathf.RoundToInt(duration); // Round to nearest hour
                        break; // Found the matching class, break inner loop
                    }
                }
            }
        }

        return totalHours;
    }

    public float GetAttendancePercentage(string className)
    {
        int total = GetTotalClassCount(className);
        if (total == 0)
        {
            return 0f;
        }

        int attended = GetAttendanceCount(className);
        float percentage = (float)attended / total * 100f;
        return percentage;
    }

    // Helper method to calculate how many classes of a specific type have actually occurred
    // This should only be used if you want to show "missed" classes
    public int GetClassesOccurredCount(string className)
    {
        if (DayTime.Instance == null || TimetableManager.Instance == null)
            return 0;

        var weeklyClasses = TimetableManager.Instance.GetWeeklyClasses();
        int classesOccurred = 0;

        DayOfWeek currentDay = DayTime.Instance.GetDayOfWeek();
        float currentTime = DayTime.Instance.Hours;
        int currentDayNumber = DayTime.Instance.days;

        foreach (var classItem in weeklyClasses)
        {
            if (classItem.className == className)
            {
                // Check if this class has already occurred
                if (HasClassOccurred(classItem, currentDay, currentTime, currentDayNumber))
                {
                    classesOccurred++;
                }
            }
        }

        return classesOccurred;
    }

    // Helper method to check if a specific class has already occurred
    private bool HasClassOccurred(Class classItem, DayOfWeek currentDay, float currentTime, int currentDayNumber)
    {
        // Convert days of week to numbers for comparison (Monday = 1, Sunday = 7)
        int classDay = GetDayNumber(classItem.dayOfWeek);
        int todayNumber = GetDayNumber(currentDay);

        // If the class day hasn't come yet this week, it hasn't occurred
        if (classDay > todayNumber)
            return false;

        // If it's the same day, check if the class time has passed
        if (classDay == todayNumber)
        {
            return currentTime >= classItem.endTime; // Class has ended
        }

        // If the class day was earlier this week, it has occurred
        return true;
    }

    // Helper method to convert DayOfWeek to number (Monday = 1, Sunday = 7)
    private int GetDayNumber(DayOfWeek day)
    {
        switch (day)
        {
            case DayOfWeek.Monday: return 1;
            case DayOfWeek.Tuesday: return 2;
            case DayOfWeek.Wednesday: return 3;
            case DayOfWeek.Thursday: return 4;
            case DayOfWeek.Friday: return 5;
            case DayOfWeek.Saturday: return 6;
            case DayOfWeek.Sunday: return 7;
            default: return 1;
        }
    }

    // Method to get attendance records for a specific class
    public List<AttendanceRecord> GetRecordsForClass(string className)
    {
        List<AttendanceRecord> classRecords = new List<AttendanceRecord>();
        foreach (var record in records)
        {
            if (record.className == className)
                classRecords.Add(record);
        }
        return classRecords;
    }

    // Method to clear all records (useful for testing)
    public void ClearAllRecords()
    {
        records.Clear();
        Debug.Log("All attendance records cleared");
    }

    // Method to get attendance summary for debugging
    public void PrintAttendanceSummary()
    {
        Debug.Log("=== ATTENDANCE SUMMARY ===");

        if (records.Count == 0)
        {
            Debug.Log("No attendance records found");
            return;
        }

        HashSet<string> uniqueClasses = new HashSet<string>();
        foreach (var record in records)
        {
            uniqueClasses.Add(record.className);
        }

        foreach (var className in uniqueClasses)
        {
            int attended = GetAttendanceCount(className);
            int total = GetTotalClassCount(className);
            float percentage = GetAttendancePercentage(className);

            Debug.Log($"{className}: {attended}/{total} hours ({percentage:F1}%)");
        }
    }
}