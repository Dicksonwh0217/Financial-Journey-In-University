using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TimetableManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] public GameObject timetablePanel;
    [SerializeField] private Transform timetableContainer;
    [SerializeField] private GameObject classSlotPrefab;
    [SerializeField] private Transform attendanceContainer;
    [SerializeField] private GameObject attendanceSlotPrefab;
    [SerializeField] private ScrollRect timetableScrollRect;
    [SerializeField] private ScrollRect attendanceScrollRect;

    [Header("Prefab Resources (backup if Inspector refs are lost)")]
    [SerializeField] private string classSlotPrefabResourcePath = "Prefabs/Timetable/ClassSlot";
    [SerializeField] private string attendanceSlotPrefabResourcePath = "Prefabs/Timetable/AttendanceSlot";

    [Header("Classes Configuration")]
    [SerializeField] private List<Class> weeklyClasses = new List<Class>();

    [Header("Attendance")]
    [SerializeField] public AttendanceData attendanceData;

    [Header("Smooth Transition")]
    [SerializeField] private float transitionDuration = 0.3f;
    [SerializeField] private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Real-time Updates")]
    [SerializeField] private float updateInterval = 1.0f; // Update every 5 seconds
    [SerializeField] private bool enableRealTimeUpdates = true;

    private GameObject originalClassSlotPrefab;
    private GameObject originalAttendanceSlotPrefab;
    private CanvasGroup timetableCanvasGroup;
    private bool isContentCached = false;
    private bool isToggling = false;

    // Real-time update coroutine
    private Coroutine realTimeUpdateCoroutine;

    public static TimetableManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            originalClassSlotPrefab = classSlotPrefab;
            originalAttendanceSlotPrefab = attendanceSlotPrefab;
            SetupCanvasGroup();
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (attendanceData == null)
            attendanceData = new AttendanceData();
    }

    private void SetupCanvasGroup()
    {
        if (timetablePanel != null)
        {
            timetableCanvasGroup = timetablePanel.GetComponent<CanvasGroup>();
            if (timetableCanvasGroup == null)
            {
                timetableCanvasGroup = timetablePanel.AddComponent<CanvasGroup>();
            }
        }
    }

    private void Start()
    {
        ValidateUIReferences();
        PreBuildContent();
        StartRealTimeUpdates();
    }

    private void OnDestroy()
    {
        StopRealTimeUpdates();
    }

    private void PreBuildContent()
    {
        if (timetablePanel != null)
        {
            bool originalState = timetablePanel.activeSelf;
            timetablePanel.SetActive(true);
            timetableCanvasGroup.alpha = 0f;

            UpdateTimetableDisplay();
            UpdateAttendanceDisplay();
            // REMOVED: ResetAllScrollbarsToTop(); - No longer called here
            isContentCached = true;

            timetablePanel.SetActive(originalState);
            if (originalState)
            {
                timetableCanvasGroup.alpha = 1f;
            }
        }
    }

    private void StartRealTimeUpdates()
    {
        if (enableRealTimeUpdates && realTimeUpdateCoroutine == null)
        {
            realTimeUpdateCoroutine = StartCoroutine(RealTimeUpdateLoop());
        }
    }

    private void StopRealTimeUpdates()
    {
        if (realTimeUpdateCoroutine != null)
        {
            StopCoroutine(realTimeUpdateCoroutine);
            realTimeUpdateCoroutine = null;
        }
    }

    private IEnumerator RealTimeUpdateLoop()
    {
        while (enableRealTimeUpdates)
        {
            yield return new WaitForSeconds(updateInterval);

            // Only update if timetable panel is visible
            if (timetablePanel != null && timetablePanel.activeSelf)
            {
                RefreshContent();
            }
        }
    }

    // Enhanced RefreshContent with real-time updates
    public void RefreshContent()
    {
        isContentCached = false;
        if (timetablePanel != null && timetablePanel.activeSelf)
        {
            UpdateTimetableDisplay();
            UpdateAttendanceDisplay();
            isContentCached = true;
        }
    }

    // Public method to enable/disable real-time updates
    public void SetRealTimeUpdates(bool enabled)
    {
        enableRealTimeUpdates = enabled;
        if (enabled)
        {
            StartRealTimeUpdates();
        }
        else
        {
            StopRealTimeUpdates();
        }
    }

    // Public method to change update interval
    public void SetUpdateInterval(float interval)
    {
        updateInterval = Mathf.Max(1.0f, interval); // Minimum 1 second

        // Restart coroutine with new interval
        if (realTimeUpdateCoroutine != null)
        {
            StopRealTimeUpdates();
            StartRealTimeUpdates();
        }
    }

    private void ValidateAndRestorePrefabReferences()
    {
        bool prefabsRestored = false;

        if (classSlotPrefab == null)
        {

            if (originalClassSlotPrefab != null)
            {
                classSlotPrefab = originalClassSlotPrefab;
                prefabsRestored = true;
            }
            else
            {
                classSlotPrefab = Resources.Load<GameObject>(classSlotPrefabResourcePath);
                if (classSlotPrefab != null)
                {
                    originalClassSlotPrefab = classSlotPrefab;
                    prefabsRestored = true;
                }
                else
                {
                }
            }
        }

        if (attendanceSlotPrefab == null)
        {
            if (originalAttendanceSlotPrefab != null)
            {
                attendanceSlotPrefab = originalAttendanceSlotPrefab;
                prefabsRestored = true;
            }
            else
            {
                attendanceSlotPrefab = Resources.Load<GameObject>(attendanceSlotPrefabResourcePath);
                if (attendanceSlotPrefab != null)
                {
                    originalAttendanceSlotPrefab = attendanceSlotPrefab;
                    prefabsRestored = true;
                }
                else
                {
                }
            }
        }

        if (prefabsRestored)
        {
            isContentCached = false;
        }
    }

    private void ValidateUIReferences()
    {
        ValidateAndRestorePrefabReferences();

        if (classSlotPrefab == null)
            Debug.LogError("❌ ClassSlotPrefab is not assigned and could not be restored!");
        else
        {
            Debug.Log("✅ ClassSlotPrefab assigned");
            ValidatePrefabStructure();
        }
    }

    private void ValidatePrefabStructure()
    {

        string[] requiredChildren = { "ClassName", "TimeText", "DayText", "TeacherText", "RoomText" };

        foreach (string childName in requiredChildren)
        {
            Transform child = classSlotPrefab.transform.Find(childName);
            if (child == null)
            {
                Debug.LogError($"❌ Missing child '{childName}' in ClassSlotPrefab!");
            }
            else
            {
                TextMeshProUGUI textComponent = child.GetComponent<TextMeshProUGUI>();
                if (textComponent == null)
                {
                    Debug.LogError($"❌ Child '{childName}' missing TextMeshProUGUI component!");
                }
                else
                {
                    Debug.Log($"✅ {childName} found with TextMeshProUGUI");
                }
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T) && !isToggling)
        {
            StartCoroutine(SmoothToggleTimetableCoroutine());
        }
    }

    private IEnumerator SmoothToggleTimetableCoroutine()
    {
        isToggling = true;

        if (timetablePanel == null)
        {
            isToggling = false;
            yield break;
        }

        if (timetableCanvasGroup == null)
        {
            SetupCanvasGroup();
        }

        ValidateAndRestorePrefabReferences();

        if (classSlotPrefab == null || attendanceSlotPrefab == null)
        {
            isToggling = false;
            yield break;
        }

        bool currentState = timetablePanel.activeSelf;
        Debug.Log($"Before toggle - Panel active: {currentState}");

        if (!currentState)
        {
            timetablePanel.SetActive(true);
            timetableCanvasGroup.alpha = 0f;

            if (!isContentCached)
            {
                UpdateTimetableDisplay();
                UpdateAttendanceDisplay();
                isContentCached = true;
            }

            // ONLY reset scrollbars when showing via 'T' key press, before fade-in starts
            ResetAllScrollbarsToTop();

            yield return null;
            yield return StartCoroutine(FadeCanvasGroup(timetableCanvasGroup, 0f, 1f));
        }
        else
        {
            yield return StartCoroutine(FadeCanvasGroup(timetableCanvasGroup, 1f, 0f));
            timetablePanel.SetActive(false);
        }

        bool newState = timetablePanel.activeSelf;
        Debug.Log($"After toggle - Panel active: {newState}");

        isToggling = false;
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float fromAlpha, float toAlpha)
    {
        float elapsedTime = 0f;

        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / transitionDuration;
            float curveValue = transitionCurve.Evaluate(normalizedTime);

            canvasGroup.alpha = Mathf.Lerp(fromAlpha, toAlpha, curveValue);

            yield return null;
        }

        canvasGroup.alpha = toAlpha;
    }

    public void ToggleTimetable()
    {
        if (isToggling) return;
        StartCoroutine(SmoothToggleTimetableCoroutine());
    }

    public void ForceShowTimetable()
    {
        if (timetablePanel != null)
        {
            StartCoroutine(ForceShowTimetableCoroutine());
        }
    }

    private IEnumerator ForceShowTimetableCoroutine()
    {
        ValidateAndRestorePrefabReferences();

        if (timetableCanvasGroup == null)
        {
            SetupCanvasGroup();
        }

        timetablePanel.SetActive(true);
        timetableCanvasGroup.alpha = 0f;

        if (!isContentCached)
        {
            UpdateTimetableDisplay();
            UpdateAttendanceDisplay();
            isContentCached = true;
        }

        // REMOVED: ResetAllScrollbarsToTop(); - No longer called here for force show

        yield return null;
        yield return StartCoroutine(FadeCanvasGroup(timetableCanvasGroup, 0f, 1f));
    }

    public void ForceHideTimetable()
    {
        if (timetablePanel != null)
        {
            StartCoroutine(ForceHideTimetableCoroutine());
        }
    }

    private IEnumerator ForceHideTimetableCoroutine()
    {
        if (timetableCanvasGroup == null)
        {
            SetupCanvasGroup();
        }

        yield return StartCoroutine(FadeCanvasGroup(timetableCanvasGroup, 1f, 0f));

        timetablePanel.SetActive(false);
    }

    private void UpdateTimetableDisplay()
    {
        if (timetableContainer == null)
        {
            return;
        }

        ClearContainer(timetableContainer);

        Dictionary<DayOfWeek, List<Class>> classByDay = new Dictionary<DayOfWeek, List<Class>>();

        foreach (var classItem in weeklyClasses)
        {
            if (!classByDay.ContainsKey(classItem.dayOfWeek))
                classByDay[classItem.dayOfWeek] = new List<Class>();

            classByDay[classItem.dayOfWeek].Add(classItem);
        }

        DayOfWeek[] weekdays = { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday };

        int totalSlotsCreated = 0;
        foreach (var day in weekdays)
        {
            if (classByDay.ContainsKey(day))
            {
                classByDay[day].Sort((a, b) => a.startTime.CompareTo(b.startTime));

                foreach (var classItem in classByDay[day])
                {
                    bool success = CreateClassSlot(classItem);
                    if (success) totalSlotsCreated++;
                }
            }
        }
    }

    private void UpdateAttendanceDisplay()
    {
        if (attendanceContainer == null)
        {
            return;
        }

        ClearContainer(attendanceContainer);

        HashSet<string> uniqueClasses = new HashSet<string>();
        foreach (var classItem in weeklyClasses)
        {
            uniqueClasses.Add(classItem.className);
        }

        int slotsCreated = 0;
        foreach (var className in uniqueClasses)
        {
            bool success = CreateAttendanceSlot(className);
            if (success) slotsCreated++;
        }

    }

    private void ClearContainer(Transform container)
    {
        List<Transform> children = new List<Transform>();
        foreach (Transform child in container)
        {
            children.Add(child);
        }

        for (int i = children.Count - 1; i >= 0; i--)
        {
            if (children[i] != null)
            {
                DestroyImmediate(children[i].gameObject);
            }
        }
    }

    private void ResetAllScrollbarsToTop()
    {
        if (timetableScrollRect != null)
        {
            timetableScrollRect.verticalNormalizedPosition = 1.0f;
            timetableScrollRect.horizontalNormalizedPosition = 0.0f;
        }

        if (attendanceScrollRect != null)
        {
            attendanceScrollRect.verticalNormalizedPosition = 1.0f;
            attendanceScrollRect.horizontalNormalizedPosition = 0.0f;
        }
    }

    private bool CreateClassSlot(Class classItem)
    {
        if (classSlotPrefab == null)
        {
            return false;
        }

        try
        {
            GameObject slot = Instantiate(classSlotPrefab, timetableContainer);
            slot.name = $"ClassSlot_{classItem.className}_{classItem.dayOfWeek}";
            slot.SetActive(true);

            TextMeshProUGUI className = slot.transform.Find("ClassName")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI timeText = slot.transform.Find("TimeText")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI dayText = slot.transform.Find("DayText")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI teacherText = slot.transform.Find("TeacherText")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI roomText = slot.transform.Find("RoomText")?.GetComponent<TextMeshProUGUI>();
            Image background = slot.GetComponent<Image>();

            if (className != null) className.text = classItem.className;
            if (timeText != null) timeText.text = classItem.GetTimeString();
            if (dayText != null) dayText.text = classItem.dayOfWeek.ToString();
            if (teacherText != null) teacherText.text = classItem.teacher;
            if (roomText != null) roomText.text = classItem.room;

            if (DayTime.Instance != null && classItem.IsActiveNow(DayTime.Instance.GetDayOfWeek(), DayTime.Instance.Hours))
            {
                if (background != null) background.color = Color.yellow;
            }
            else
            {
                if (background != null) background.color = classItem.classColor;
            }

            return true;
        }
        catch (System.Exception)
        {
            return false;
        }
    }

    private bool CreateAttendanceSlot(string className)
    {
        if (attendanceSlotPrefab == null)
        {
            return false;
        }

        if (attendanceData == null)
        {
            return false;
        }

        try
        {
            GameObject slot = Instantiate(attendanceSlotPrefab, attendanceContainer);
            slot.name = $"AttendanceSlot_{className}";
            slot.SetActive(true);

            int attended = attendanceData.GetAttendanceCount(className);
            int total = attendanceData.GetTotalClassCount(className);
            float percentage = attendanceData.GetAttendancePercentage(className);

            TextMeshProUGUI classNameText = slot.transform.Find("ClassName")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI attendanceText = slot.transform.Find("AttendanceText")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI percentageText = slot.transform.Find("PercentageText")?.GetComponent<TextMeshProUGUI>();

            if (classNameText != null) classNameText.text = className;
            if (attendanceText != null) attendanceText.text = $"{attended}/{total}hr";
            if (percentageText != null) percentageText.text = $"{percentage:F1}%";

            return true;
        }
        catch (System.Exception)
        {
            return false;
        }
    }

    public void RecordAttendance(string className, bool attended)
    {
        if (DayTime.Instance == null) return;

        AttendanceRecord record = new AttendanceRecord(
            className,
            DayTime.Instance.GetDayOfWeek(),
            DayTime.Instance.days,
            DayTime.Instance.Hours,
            attended
        );

        attendanceData.AddRecord(record);

        RefreshContent();
    }

    public Class GetCurrentClass()
    {
        if (DayTime.Instance == null) return null;

        DayOfWeek currentDay = DayTime.Instance.GetDayOfWeek();
        float currentTime = DayTime.Instance.Hours;

        foreach (var classItem in weeklyClasses)
        {
            if (classItem.IsActiveNow(currentDay, currentTime))
                return classItem;
        }

        return null;
    }

    public List<Class> GetTodaysClasses()
    {
        if (DayTime.Instance == null) return new List<Class>();

        DayOfWeek currentDay = DayTime.Instance.GetDayOfWeek();
        List<Class> todaysClasses = new List<Class>();

        foreach (var classItem in weeklyClasses)
        {
            if (classItem.IsToday(currentDay))
                todaysClasses.Add(classItem);
        }

        todaysClasses.Sort((a, b) => a.startTime.CompareTo(b.startTime));
        return todaysClasses;
    }

    public List<Class> GetWeeklyClasses()
    {
        return weeklyClasses;
    }

    public void AddClass(Class newClass)
    {
        weeklyClasses.Add(newClass);
        RefreshContent();
    }

    public void RemoveClass(Class classToRemove)
    {
        weeklyClasses.Remove(classToRemove);
        RefreshContent();
    }

    [ContextMenu("Test UI Creation")]
    public void TestUICreation()
    {
        ValidateUIReferences();

        if (timetablePanel != null)
        {
            ForceShowTimetable();
        }
    }


    public void RecordAttendanceButtonPressed()
    {
        Debug.Log("=== RecordAttendanceButtonPressed() Called ===");

        // Check if all required systems are available
        if (DayTime.Instance == null)
        {
            Debug.LogError("Cannot record attendance - DayTime.Instance is null");
            return;
        }

        if (GameSceneManager.instance?.screenTint == null)
        {
            Debug.LogError("Cannot record attendance - ScreenTint is not available");
            return;
        }

        Debug.Log("✅ DayTime.Instance and ScreenTint are available");

        // Get current time and day
        DayOfWeek currentDay = DayTime.Instance.GetDayOfWeek();
        float currentTime = DayTime.Instance.Hours;
        int currentDayNumber = DayTime.Instance.days;

        Debug.Log($"Current Day: {currentDay}, Current Time: {currentTime:F2}, Day Number: {currentDayNumber}");

        // Find the current active class that the student should attend
        Class currentClass = GetCurrentAttendableClass(currentDay, currentTime);

        if (currentClass == null)
        {
            List<Class> todaysClasses = GetTodaysClasses();
            Debug.Log($"No attendable class found. Today's classes ({todaysClasses.Count} total):");
            foreach (var cls in todaysClasses)
            {
                Debug.Log($"  - {cls.className}: {cls.GetTimeString()} (Start: {cls.startTime:F2}, End: {cls.endTime:F2})");
            }
            return;
        }

        Debug.Log($"✅ Found current attendable class: {currentClass.className} ({currentClass.GetTimeString()})");

        // Start the attendance recording coroutine with visual effects
        StartCoroutine(RecordAttendanceWithEffects(currentClass));
    }

    private IEnumerator RecordAttendanceWithEffects(Class currentClass)
    {
        Debug.Log($"Starting attendance recording with effects for {currentClass.className}");

        // Store the original time scale
        float originalTimeScale = DayTime.Instance != null ? GetTimeScale() : 60f;

        // Step 1: Tint the screen
        GameSceneManager.instance.screenTint.Tint();
        Debug.Log("Screen tinting started");

        // Wait for tint to complete
        float tintDuration = 1f / GameSceneManager.instance.screenTint.speed + 0.1f;
        yield return new WaitForSeconds(tintDuration);
        Debug.Log("Screen tint completed");

        // Step 2: Stop time counting (pause the day/night cycle)
        if (DayTime.Instance != null)
        {
            SetTimeScale(0f);
            Debug.Log("Time counting stopped");
        }

        // Step 3: Record the attendance
        Debug.Log($"Recording attendance for {currentClass.className}");

        // Try using AutomaticAttendanceTracker if available
        if (AutomaticAttendanceTracker.Instance != null)
        {
            bool canAttend = AutomaticAttendanceTracker.Instance.CanAttendClass(currentClass.className);
            Debug.Log($"Can attend {currentClass.className}: {canAttend}");

            if (canAttend)
            {
                AutomaticAttendanceTracker.Instance.MarkAttendanceManually(currentClass.className);
                Debug.Log("✅ Attendance recorded via AutomaticAttendanceTracker");
            }
            else
            {
                Debug.LogWarning("Cannot attend class - outside attendance window");
            }
        }
        else
        {
            RecordAttendance(currentClass.className, true);
            Debug.Log("✅ Attendance recorded via TimetableManager");
        }

        // Step 4: Jump time to end of class
        float timeToJump = currentClass.endTime - DayTime.Instance.Hours;
        if (timeToJump > 0)
        {
            Debug.Log($"Jumping time forward by {timeToJump:F2} hours (to {currentClass.endTime:F2})");
            DayTime.Instance.SkipTime(hours: timeToJump);
        }
        else
        {
            Debug.Log("Class has already ended or is ending, no time jump needed");
        }

        // Small delay to let the time update process
        yield return new WaitForSecondsRealtime(0.1f);

        // Step 5: Restore time counting
        if (DayTime.Instance != null)
        {
            SetTimeScale(originalTimeScale);
            Debug.Log($"Time counting restored with scale: {originalTimeScale}");
        }

        // Step 6: Untint the screen
        GameSceneManager.instance.screenTint.UnTint();
        Debug.Log("Screen untinting started");

        // Wait for untint to complete
        yield return new WaitForSeconds(tintDuration);
        Debug.Log("✅ Attendance recording with effects completed");

        // Refresh the UI to show updated information
        RefreshContent();
    }

    private float GetTimeScale()
    {
        return DayTime.Instance?.TimeScale ?? 60f;
    }

    private void SetTimeScale(float scale)
    {
        if (DayTime.Instance != null)
        {
            DayTime.Instance.SetTimeScale(scale);
        }
    }


    private Class GetCurrentAttendableClass(DayOfWeek currentDay, float currentTime)
    {
        Debug.Log($"Looking for attendable class on {currentDay} at {currentTime:F2}");

        foreach (var classItem in weeklyClasses)
        {
            if (classItem.dayOfWeek == currentDay)
            {
                Debug.Log($"Checking class: {classItem.className} (Start: {classItem.startTime:F2}, End: {classItem.endTime:F2})");

                // Allow attendance for first 50% of class duration
                float attendanceEndTime = classItem.startTime + (classItem.endTime - classItem.startTime) * 0.5f;
                bool isInTimeRange = currentTime >= classItem.startTime &&
                                    currentTime <= attendanceEndTime;

                Debug.Log($"  - Time range check: {currentTime:F2} >= {classItem.startTime:F2} && {currentTime:F2} <= {attendanceEndTime:F2} = {isInTimeRange}");

                if (isInTimeRange)
                {
                    Debug.Log($"✅ Found attendable class: {classItem.className}");
                    return classItem;
                }
            }
        }

        Debug.Log("❌ No attendable class found");
        return null;
    }
}