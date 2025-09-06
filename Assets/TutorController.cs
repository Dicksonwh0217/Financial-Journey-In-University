using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorController : MonoBehaviour
{
    [Header("Tutor Settings")]
    [SerializeField] private string tutorObjectName = "Tutor"; // Name to find the tutor GameObject
    [SerializeField] private int targetDay = 45; // Day when tutor should appear
    [SerializeField] private float startTime = 8f; // 8:00 AM
    [SerializeField] private float endTime = 10f; // 10:00 AM

    private DayTime timeController;
    private GameObject tutorObject;
    private bool tutorCurrentlyActive = false;

    private void Start()
    {
        // Get reference to the DayTime controller through GameManager
        if (GameManager.instance != null && GameManager.instance.timeController != null)
        {
            timeController = GameManager.instance.timeController;
        }
        else
        {
            Debug.LogError("TutorController: Could not find DayTime controller through GameManager!");
        }

        // Find the tutor object in the current scene
        FindTutorObject();
    }

    private void OnEnable()
    {
        // When this script becomes active (scene loads), find the tutor object
        FindTutorObject();
    }

    private void FindTutorObject()
    {
        // Try to find the tutor object by name in the current scene
        tutorObject = GameObject.Find(tutorObjectName);

        if (tutorObject != null)
        {
            // Ensure tutor starts with correct state based on current time
            CheckAndSetInitialTutorState();
            Debug.Log($"TutorController: Found tutor object '{tutorObjectName}' in scene");
        }
        else
        {
            Debug.Log($"TutorController: Tutor object '{tutorObjectName}' not found in current scene");
        }
    }

    private void CheckAndSetInitialTutorState()
    {
        if (timeController == null || tutorObject == null) return;

        // Check if tutor should be active based on current time
        bool shouldBeActive = IsTutorScheduledActive;

        tutorObject.SetActive(shouldBeActive);
        tutorCurrentlyActive = shouldBeActive;

        if (shouldBeActive)
        {
            Debug.Log($"TutorController: Tutor set active on scene load - Day {timeController.days + 1} at {timeController.GetTimeString()}");
        }
    }

    private void Update()
    {
        if (timeController == null) return;

        // If tutor object is null, try to find it (in case scene just loaded)
        if (tutorObject == null)
        {
            FindTutorObject();
            return;
        }

        CheckTutorSchedule();
    }

    private void CheckTutorSchedule()
    {
        // Determine if tutor should be active
        bool shouldBeActive = IsTutorScheduledActive;

        // Only change state if needed to avoid unnecessary calls
        if (shouldBeActive && !tutorCurrentlyActive)
        {
            ActivateTutor();
        }
        else if (!shouldBeActive && tutorCurrentlyActive)
        {
            DeactivateTutor();
        }
    }

    private void ActivateTutor()
    {
        if (tutorObject != null)
        {
            tutorObject.SetActive(true);
            tutorCurrentlyActive = true;
            Debug.Log($"Tutor activated on day {timeController.days + 1} at {timeController.GetTimeString()}");
        }
    }

    private void DeactivateTutor()
    {
        if (tutorObject != null)
        {
            tutorObject.SetActive(false);
            tutorCurrentlyActive = false;
            Debug.Log($"Tutor deactivated on day {timeController.days + 1} at {timeController.GetTimeString()}");
        }
    }

    // Public methods for manual control (useful for testing)
    public void ManualActivateTutor()
    {
        if (tutorObject == null) FindTutorObject();

        if (tutorObject != null)
        {
            tutorObject.SetActive(true);
            tutorCurrentlyActive = true;
        }
    }

    public void ManualDeactivateTutor()
    {
        if (tutorObject == null) FindTutorObject();

        if (tutorObject != null)
        {
            tutorObject.SetActive(false);
            tutorCurrentlyActive = false;
        }
    }

    // Force refresh - useful when transitioning to a scene with the tutor
    public void RefreshTutorReference()
    {
        FindTutorObject();
    }

    // Property to check if tutor is currently supposed to be active
    public bool IsTutorScheduledActive
    {
        get
        {
            if (timeController == null) return false;

            bool isTargetDay = (timeController.days + 1) == targetDay;
            bool isWithinTimeRange = timeController.Hours >= startTime && timeController.Hours < endTime;

            return isTargetDay && isWithinTimeRange;
        }
    }

    // Properties for easy access to settings
    public int TargetDay
    {
        get { return targetDay; }
        set { targetDay = value; }
    }

    public float StartTime
    {
        get { return startTime; }
        set { startTime = value; }
    }

    public float EndTime
    {
        get { return endTime; }
        set { endTime = value; }
    }

    public string TutorObjectName
    {
        get { return tutorObjectName; }
        set { tutorObjectName = value; }
    }
}