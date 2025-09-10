using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorController : MonoBehaviour
{
    [Header("Tutor Settings")]
    [SerializeField] private string tutorObjectName = "Tutor"; // Name to find the tutor GameObject
    [SerializeField] private int targetDay = 45; // Day when tutor should appear
    [SerializeField] private float startTime = 8f; // 8:00 AM
    [SerializeField] private float endTime = 10f; // 10:00 AM

    [Header("Performance Settings")]
    [SerializeField] private float tutorSearchInterval = 2f; // How often to search for tutor if missing (in seconds)

    private DayTime timeController;
    private GameObject tutorObject;
    private bool tutorCurrentlyActive = false;
    private bool hasSearchedThisScene = false; // Track if we've already searched in current scene
    private float lastSearchTime = 0f; // When we last searched for tutor
    private int lastSceneBuildIndex = -1; // Track scene changes

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

        // Subscribe to scene change events
        SceneManager.sceneLoaded += OnSceneLoaded;

        // Find the tutor object in the current scene
        FindTutorObject();
        lastSceneBuildIndex = SceneManager.GetActiveScene().buildIndex;
    }

    private void OnDestroy()
    {
        // Unsubscribe from scene change events to prevent memory leaks
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Reset search flag when new scene loads
        hasSearchedThisScene = false;
        tutorObject = null; // Clear reference since we're in a new scene
        lastSceneBuildIndex = scene.buildIndex;

        // Start coroutine to search after a small delay (ensures scene is fully loaded)
        StartCoroutine(DelayedTutorSearch());
    }

    private IEnumerator DelayedTutorSearch()
    {
        // Wait a frame to ensure scene is fully loaded
        yield return null;
        FindTutorObject();
    }

    private void OnEnable()
    {
        // Check if we're in a different scene than last time
        int currentScene = SceneManager.GetActiveScene().buildIndex;
        if (currentScene != lastSceneBuildIndex)
        {
            hasSearchedThisScene = false;
            tutorObject = null;
            lastSceneBuildIndex = currentScene;
        }

        // When this script becomes active, find the tutor object if we haven't already
        if (!hasSearchedThisScene || tutorObject == null)
        {
            FindTutorObject();
        }
    }

    private void FindTutorObject()
    {
        // Try to find the tutor object by name in the current scene
        tutorObject = GameObject.Find(tutorObjectName);
        hasSearchedThisScene = true; // Mark that we've searched in this scene
        lastSearchTime = Time.time; // Record when we searched

        if (tutorObject != null)
        {
            // Ensure tutor starts with correct state based on current time
            CheckAndSetInitialTutorState();
            Debug.Log($"TutorController: Found tutor object '{tutorObjectName}' in scene '{SceneManager.GetActiveScene().name}'");
        }
        else
        {
            Debug.Log($"TutorController: Tutor object '{tutorObjectName}' not found in current scene '{SceneManager.GetActiveScene().name}'");
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

        // Only search for tutor if we don't have a reference AND enough time has passed since last search
        if (tutorObject == null && (Time.time - lastSearchTime) >= tutorSearchInterval)
        {
            FindTutorObject();
            return; // Exit early if we just searched, let next frame handle schedule check
        }

        // Only check schedule if we have a tutor object
        if (tutorObject != null)
        {
            CheckTutorSchedule();
        }
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
        if (tutorObject == null)
        {
            FindTutorObject();
        }

        if (tutorObject != null)
        {
            tutorObject.SetActive(true);
            tutorCurrentlyActive = true;
        }
    }

    public void ManualDeactivateTutor()
    {
        if (tutorObject == null)
        {
            FindTutorObject();
        }

        if (tutorObject != null)
        {
            tutorObject.SetActive(false);
            tutorCurrentlyActive = false;
        }
    }

    // Force refresh - useful when transitioning to a scene with the tutor
    public void RefreshTutorReference()
    {
        hasSearchedThisScene = false; // Allow new search
        FindTutorObject();
    }

    // Force immediate search (bypasses interval)
    public void ForceSearchTutor()
    {
        lastSearchTime = 0f; // Reset search time to allow immediate search
        hasSearchedThisScene = false;
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

    public float TutorSearchInterval
    {
        get { return tutorSearchInterval; }
        set { tutorSearchInterval = value; }
    }

    // Utility method to check current status
    public bool HasTutorReference => tutorObject != null;
    public bool IsTutorCurrentlyActive => tutorCurrentlyActive;

    // Debug info
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    private void OnDrawGizmosSelected()
    {
        // Show debug info in scene view when selected
        if (Application.isPlaying)
        {
            UnityEditor.Handles.Label(transform.position,
                $"Tutor Found: {HasTutorReference}\n" +
                $"Should Be Active: {IsTutorScheduledActive}\n" +
                $"Currently Active: {IsTutorCurrentlyActive}");
        }
    }
}