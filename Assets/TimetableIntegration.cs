using UnityEngine;
using UnityEngine.UI;

public class TimetableIntegration : MonoBehaviour
{
    [Header("Timetable Integration")]
    [SerializeField] private TimetableManager timetableManager;

    private void Start()
    {
        // Find TimetableManager if not assigned
        if (timetableManager == null)
        {
            timetableManager = FindFirstObjectByType<TimetableManager>();
        }
    }

    private void Update()
    {
        // Update logic can be added here if needed
    }

    public void MarkAttendanceForCurrentClass()
    {
        if (TimetableManager.Instance == null || AutomaticAttendanceTracker.Instance == null)
            return;

        Class currentClass = TimetableManager.Instance.GetCurrentClass();

        if (currentClass != null)
        {
            bool canAttend = AutomaticAttendanceTracker.Instance.CanAttendClass(currentClass.className);

            if (canAttend)
            {
                AutomaticAttendanceTracker.Instance.MarkAttendanceManually(currentClass.className);
                Debug.Log($"Marked attendance for {currentClass.className}!");
            }
            else
            {
                Debug.Log($"Cannot mark attendance for {currentClass.className}. Either already attended or attendance window has passed.");
            }
        }
        else
        {
            Debug.Log("No current class to mark attendance for.");
        }
    }

    // Call this method when player wants to check current class
    public void CheckCurrentClass()
    {
        if (TimetableManager.Instance != null)
        {
            Class currentClass = TimetableManager.Instance.GetCurrentClass();
            if (currentClass != null)
            {
                bool canAttend = AutomaticAttendanceTracker.Instance.CanAttendClass(currentClass.className);
                string attendanceStatus = canAttend ? "Can attend" : "Cannot attend";
                Debug.Log($"Current class: {currentClass.className} at {currentClass.GetTimeString()} - {attendanceStatus}");
            }
            else
            {
                Debug.Log("No class is currently scheduled.");
            }
        }
    }

    // Call this method to get today's schedule
    public void CheckTodaysSchedule()
    {
        if (TimetableManager.Instance != null)
        {
            var todaysClasses = TimetableManager.Instance.GetTodaysClasses();
            if (todaysClasses.Count > 0)
            {
                Debug.Log("Today's classes:");
                foreach (var classItem in todaysClasses)
                {
                    Debug.Log($"- {classItem.className} at {classItem.GetTimeString()}");
                }
            }
            else
            {
                Debug.Log("No classes scheduled for today.");
            }
        }
    }

    // Method to manually record attendance (if needed)
    public void RecordAttendance(string className, bool attended)
    {
        if (TimetableManager.Instance != null)
        {
            TimetableManager.Instance.RecordAttendance(className, attended);
        }
    }

    // Public method to toggle timetable (can be called from other scripts)
    public void ToggleTimetableFromOtherScript()
    {
        if (TimetableManager.Instance != null)
        {
            TimetableManager.Instance.ToggleTimetable();
        }
    }

    // Method to set attendance window for specific scenarios
    public void SetAttendanceWindow(float minutes)
    {
        if (AutomaticAttendanceTracker.Instance != null)
        {
            AutomaticAttendanceTracker.Instance.SetAttendanceWindow(minutes);
        }
    }
}