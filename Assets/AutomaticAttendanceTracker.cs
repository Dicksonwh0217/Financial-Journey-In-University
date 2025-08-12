using System;
using System.Collections.Generic;
using UnityEngine;

public class AutomaticAttendanceTracker : MonoBehaviour
{
    [Header("Attendance Settings")]
    [SerializeField] private float attendanceCheckInterval = 1f; // Check every 1 seconds
    [SerializeField] private float attendanceWindowMinutes = 15f; // 15 minutes window to attend

    private Dictionary<string, bool> classAttendanceChecked = new Dictionary<string, bool>();
    private Dictionary<string, float> classStartTimes = new Dictionary<string, float>();
    private float lastCheckTime = 0f;

    public static AutomaticAttendanceTracker Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (DayTime.Instance == null || TimetableManager.Instance == null)
            return;

        // Check attendance every interval
        if (Time.time - lastCheckTime >= attendanceCheckInterval)
        {
            CheckAndUpdateAttendance();
            lastCheckTime = Time.time;
        }
    }

    private void CheckAndUpdateAttendance()
    {
        DayOfWeek currentDay = DayTime.Instance.GetDayOfWeek();
        float currentTime = DayTime.Instance.Hours;
        int currentDayNumber = DayTime.Instance.days;

        List<Class> weeklyClasses = TimetableManager.Instance.GetWeeklyClasses();

        foreach (Class classItem in weeklyClasses)
        {
            if (classItem.dayOfWeek == currentDay)
            {
                string classKey = GetClassKey(classItem, currentDayNumber);

                // Check if class has started and we haven't processed it yet
                if (currentTime >= classItem.startTime && !classAttendanceChecked.ContainsKey(classKey))
                {
                    // Mark that we've checked this class
                    classAttendanceChecked[classKey] = true;
                    classStartTimes[classKey] = currentTime;

                    // Record as absent initially
                    RecordAttendanceForClass(classItem, false);

                    Debug.Log($"Class {classItem.className} started at {classItem.startTime:F2}. Marked as absent initially.");
                }

                // Check if student can still attend (within attendance window)
                if (classAttendanceChecked.ContainsKey(classKey) &&
                    currentTime >= classItem.startTime &&
                    currentTime <= classItem.startTime + (attendanceWindowMinutes / 60f))
                {
                    // Student can still attend if they haven't been marked present yet
                    if (!HasAttendedClass(classItem, currentDayNumber))
                    {
                        // Check if student is present (you can customize this condition)
                        if (IsStudentPresent(classItem))
                        {
                            // Update attendance to present
                            UpdateAttendanceToPresent(classItem, currentDayNumber);
                            Debug.Log($"Student attended {classItem.className}! Attendance updated to present.");
                        }
                    }
                }
            }
        }

        // Clean up old entries from previous days
        CleanupOldEntries(currentDayNumber);
    }

    private string GetClassKey(Class classItem, int dayNumber)
    {
        return $"{classItem.className}_{classItem.dayOfWeek}_{dayNumber}";
    }

    private void RecordAttendanceForClass(Class classItem, bool attended)
    {
        if (TimetableManager.Instance != null)
        {
            TimetableManager.Instance.RecordAttendance(classItem.className, attended);
        }
    }

    private bool HasAttendedClass(Class classItem, int dayNumber)
    {
        if (TimetableManager.Instance?.attendanceData == null)
            return false;

        var records = TimetableManager.Instance.attendanceData.GetRecordsForClass(classItem.className);

        foreach (var record in records)
        {
            if (record.dayNumber == dayNumber &&
                record.dayOfWeek == classItem.dayOfWeek &&
                record.attended)
            {
                return true;
            }
        }

        return false;
    }

    private void UpdateAttendanceToPresent(Class classItem, int dayNumber)
    {
        if (TimetableManager.Instance?.attendanceData == null)
            return;

        var records = TimetableManager.Instance.attendanceData.records;

        // Find the most recent absent record for this class today
        for (int i = records.Count - 1; i >= 0; i--)
        {
            var record = records[i];
            if (record.className == classItem.className &&
                record.dayNumber == dayNumber &&
                record.dayOfWeek == classItem.dayOfWeek &&
                !record.attended)
            {
                // Update this record to present
                record.attended = true;
                record.attendanceTime = DayTime.Instance.Hours;

                Debug.Log($"Updated attendance for {classItem.className} to present at {record.attendanceTime:F2}");

                // Refresh UI if timetable is open
                if (TimetableManager.Instance.timetablePanel != null &&
                    TimetableManager.Instance.timetablePanel.activeSelf)
                {
                    TimetableManager.Instance.RefreshContent();
                }

                break;
            }
        }
    }

    private bool IsStudentPresent(Class classItem)
    {
        return false;
    }

    private void CleanupOldEntries(int currentDayNumber)
    {
        // Remove entries from previous days to prevent memory buildup
        List<string> keysToRemove = new List<string>();

        foreach (var kvp in classAttendanceChecked)
        {
            string[] parts = kvp.Key.Split('_');
            if (parts.Length >= 3)
            {
                if (int.TryParse(parts[2], out int dayNumber))
                {
                    if (dayNumber < currentDayNumber)
                    {
                        keysToRemove.Add(kvp.Key);
                    }
                }
            }
        }

        foreach (string key in keysToRemove)
        {
            classAttendanceChecked.Remove(key);
            classStartTimes.Remove(key);
        }
    }

    public void MarkAttendanceManually(string className)
    {
        Debug.Log($"=== MarkAttendanceManually called for {className} ===");

        if (DayTime.Instance == null || TimetableManager.Instance == null)
        {
            Debug.LogError("DayTime or TimetableManager instance is null in MarkAttendanceManually");
            return;
        }

        DayOfWeek currentDay = DayTime.Instance.GetDayOfWeek();
        float currentTime = DayTime.Instance.Hours;
        int currentDayNumber = DayTime.Instance.days;

        Debug.Log($"Current: Day={currentDay}, Time={currentTime:F2}, DayNumber={currentDayNumber}");

        List<Class> weeklyClasses = TimetableManager.Instance.GetWeeklyClasses();

        foreach (Class classItem in weeklyClasses)
        {
            if (classItem.className == className && classItem.dayOfWeek == currentDay)
            {
                Debug.Log($"Found matching class: {classItem.className} on {classItem.dayOfWeek}");
                Debug.Log($"Class time: {classItem.startTime:F2} - {classItem.endTime:F2}");

                // Use the same logic as TimetableManager.GetCurrentAttendableClass
                // Allow attendance for first 50% of class duration
                float attendanceEndTime = classItem.startTime + (classItem.endTime - classItem.startTime) * 0.5f;
                Debug.Log($"Attendance window: {classItem.startTime:F2} to {attendanceEndTime:F2}");

                // Check if we're within the attendance window
                bool withinWindow = currentTime >= classItem.startTime && currentTime <= attendanceEndTime;

                Debug.Log($"Within attendance window: {withinWindow}");

                if (withinWindow)
                {
                    string classKey = GetClassKey(classItem, currentDayNumber);
                    Debug.Log($"Class key: {classKey}");

                    // Check if already attended
                    bool alreadyAttended = HasAttendedClass(classItem, currentDayNumber);
                    Debug.Log($"Already attended: {alreadyAttended}");

                    if (alreadyAttended)
                    {
                        Debug.LogWarning($"Student has already attended {className} today!");
                        return;
                    }

                    // If class hasn't started being tracked yet, record as absent first
                    if (!classAttendanceChecked.ContainsKey(classKey))
                    {
                        Debug.Log($"Class not yet tracked, marking as absent first");
                        classAttendanceChecked[classKey] = true;
                        RecordAttendanceForClass(classItem, false);
                    }

                    // Update to present
                    Debug.Log($"Updating attendance to present for {className}");
                    UpdateAttendanceToPresent(classItem, currentDayNumber);

                    Debug.Log($"✅ Successfully marked attendance for {className}");
                    return;
                }
                else
                {
                    Debug.LogWarning($"❌ Cannot mark attendance for {className} - outside attendance window");
                    Debug.LogWarning($"Current time: {currentTime:F2}, Required: {classItem.startTime:F2} to {attendanceEndTime:F2}");
                    return;
                }
            }
        }

        Debug.LogWarning($"❌ No matching class found for {className} on {currentDay}");
    }

    // ENHANCED: Method to check if student can still attend a class
    public bool CanAttendClass(string className)
    {
        Debug.Log($"=== CanAttendClass called for {className} ===");

        if (DayTime.Instance == null || TimetableManager.Instance == null)
        {
            Debug.LogError("DayTime or TimetableManager instance is null in CanAttendClass");
            return false;
        }

        DayOfWeek currentDay = DayTime.Instance.GetDayOfWeek();
        float currentTime = DayTime.Instance.Hours;
        int currentDayNumber = DayTime.Instance.days;

        List<Class> weeklyClasses = TimetableManager.Instance.GetWeeklyClasses();

        foreach (Class classItem in weeklyClasses)
        {
            if (classItem.className == className && classItem.dayOfWeek == currentDay)
            {
                // Use the same logic as TimetableManager.GetCurrentAttendableClass
                // Allow attendance for first 50% of class duration
                float attendanceEndTime = classItem.startTime + (classItem.endTime - classItem.startTime) * 0.5f;
                bool withinWindow = currentTime >= classItem.startTime && currentTime <= attendanceEndTime;

                bool alreadyAttended = HasAttendedClass(classItem, currentDayNumber);

                Debug.Log($"Class: {className}");
                Debug.Log($"  Start time: {classItem.startTime:F2}");
                Debug.Log($"  End time: {classItem.endTime:F2}");
                Debug.Log($"  Attendance end time (50%): {attendanceEndTime:F2}");
                Debug.Log($"  Current time: {currentTime:F2}");
                Debug.Log($"  Within window: {withinWindow}");
                Debug.Log($"  Already attended: {alreadyAttended}");

                bool canAttend = withinWindow && !alreadyAttended;
                Debug.Log($"Can attend {className}: {canAttend}");

                return canAttend;
            }
        }

        Debug.Log($"No matching class found for {className} on {currentDay}");
        return false;
    }

    // Get attendance window information
    public float GetAttendanceWindowMinutes()
    {
        return attendanceWindowMinutes;
    }

    // Set attendance window (useful for different class types)
    public void SetAttendanceWindow(float minutes)
    {
        attendanceWindowMinutes = minutes;
    }

    // ADDED: Debug method to check current state
    public void DebugCurrentState()
    {
        Debug.Log("=== AutomaticAttendanceTracker Debug State ===");
        Debug.Log($"Attendance window: {attendanceWindowMinutes} minutes");
        Debug.Log($"Tracked classes count: {classAttendanceChecked.Count}");

        if (DayTime.Instance != null)
        {
            Debug.Log($"Current day: {DayTime.Instance.GetDayOfWeek()}");
            Debug.Log($"Current time: {DayTime.Instance.Hours:F2}");
            Debug.Log($"Current day number: {DayTime.Instance.days}");
        }

        foreach (var kvp in classAttendanceChecked)
        {
            Debug.Log($"Tracked class: {kvp.Key} = {kvp.Value}");
        }
    }
}