using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MusicTrack
{
    [Header("Track Settings")]
    public string trackName;
    public AudioClip audioClip;
    [Range(0f, 1f)]
    public float volume = 1f;

    public MusicTrack()
    {
        trackName = "New Track";
        volume = 1f;
    }

    public MusicTrack(string name, AudioClip clip, float vol = 1f)
    {
        trackName = name;
        audioClip = clip;
        volume = vol;
    }
}

public class MusicManager : MonoBehaviour
{
    public static MusicManager instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    [Header("Audio Source")]
    [SerializeField] AudioSource audioSource;

    [Header("Transition Settings")]
    [SerializeField] float timeToSwitch = 2f;

    [Header("Day/Night Music")]
    [SerializeField] MusicTrack dayTheme;
    [SerializeField] MusicTrack nightTheme;

    [Header("Exam Music")]
    [SerializeField] MusicTrack midtermExamTheme;
    [SerializeField] MusicTrack finalExamTheme;

    // Current state
    private bool isExamMode = false;
    private bool isCurrentlyDay = true;
    private MusicTrack previousTheme; // Store what was playing before exam

    private void Start()
    {
        // Start with appropriate day/night music
        StartDayNightSystem();
    }

    private void Update()
    {
        // Only check day/night changes when not in exam mode
        if (!isExamMode)
        {
            CheckDayNightChange();
        }
    }

    #region Day/Night System

    private void StartDayNightSystem()
    {
        if (DayTime.Instance != null)
        {
            CheckDayNightChange();
        }
        else
        {
            // Default to day theme if DayTime not available
            PlayDayTheme();
        }
    }

    private void CheckDayNightChange()
    {
        if (DayTime.Instance == null) return;

        float currentHour = DayTime.Instance.Hours;
        bool shouldBeDay = (currentHour >= 6f && currentHour < 19f); // 6AM to 7PM is day

        if (shouldBeDay != isCurrentlyDay)
        {
            isCurrentlyDay = shouldBeDay;

            if (isCurrentlyDay)
            {
                PlayDayTheme();
            }
            else
            {
                PlayNightTheme();
            }
        }
    }

    private void PlayDayTheme()
    {
        if (dayTheme != null && dayTheme.audioClip != null)
        {
            PlayTrack(dayTheme);
        }
    }

    private void PlayNightTheme()
    {
        if (nightTheme != null && nightTheme.audioClip != null)
        {
            PlayTrack(nightTheme);
        }
    }

    // ADDED: Method to force play current appropriate theme
    private void PlayCurrentDayNightTheme()
    {
        UpdateCurrentTimeState(); // Make sure we have the current time state

        if (isCurrentlyDay)
        {
            PlayDayTheme();
        }
        else
        {
            PlayNightTheme();
        }
    }

    // ADDED: Method to update time state without changing music
    private void UpdateCurrentTimeState()
    {
        if (DayTime.Instance != null)
        {
            float currentHour = DayTime.Instance.Hours;
            isCurrentlyDay = (currentHour >= 6f && currentHour < 19f);
        }
        else
        {
            isCurrentlyDay = true; // Default to day
        }
    }

    #endregion

    #region Exam System

    public void StartMidtermExam()
    {
        if (midtermExamTheme != null && midtermExamTheme.audioClip != null)
        {
            // Store what was playing before exam
            StorePreviousTheme();
            isExamMode = true;
            PlayTrack(midtermExamTheme);
            Debug.Log("Started Midterm Exam Music");
        }
    }

    public void StartFinalExam()
    {
        if (finalExamTheme != null && finalExamTheme.audioClip != null)
        {
            // Store what was playing before exam
            StorePreviousTheme();
            isExamMode = true;
            PlayTrack(finalExamTheme);
            Debug.Log("Started Final Exam Music");
        }
    }

    public void EndExam()
    {
        Debug.Log("Ending exam music, returning to day/night theme");
        isExamMode = false;

        // FIXED: Force play the current appropriate day/night theme
        PlayCurrentDayNightTheme();
    }

    private void StorePreviousTheme()
    {
        // Store current theme based on time
        if (DayTime.Instance != null)
        {
            float currentHour = DayTime.Instance.Hours;
            bool isDayTime = (currentHour >= 6f && currentHour < 19f);
            previousTheme = isDayTime ? dayTheme : nightTheme;
        }
        else
        {
            previousTheme = dayTheme; // Default fallback
        }
    }

    #endregion

    #region Core Music Methods

    private void PlayTrack(MusicTrack track)
    {
        if (track == null || track.audioClip == null)
        {
            Debug.LogWarning($"Cannot play track - track or audioClip is null");
            return;
        }

        Debug.Log($"Playing track: {track.trackName}");
        StartCoroutine(SmoothSwitchMusic(track));
    }

    IEnumerator SmoothSwitchMusic(MusicTrack newTrack)
    {
        // Skip fade out if no music is currently playing
        if (audioSource.isPlaying)
        {
            // Fade out current music
            float startVolume = audioSource.volume;
            float currentTime = 0f;

            while (currentTime < timeToSwitch / 2f)
            {
                currentTime += Time.deltaTime;
                audioSource.volume = Mathf.Lerp(startVolume, 0f, currentTime / (timeToSwitch / 2f));
                yield return new WaitForEndOfFrame();
            }
        }

        // Switch to new track
        audioSource.clip = newTrack.audioClip;
        audioSource.volume = 0f;
        audioSource.Play();

        // Fade in new music
        float currentTime2 = 0f;
        while (currentTime2 < timeToSwitch / 2f)
        {
            currentTime2 += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0f, newTrack.volume, currentTime2 / (timeToSwitch / 2f));
            yield return new WaitForEndOfFrame();
        }

        audioSource.volume = newTrack.volume;
    }

    #endregion

    #region Public Control Methods

    public void Stop()
    {
        StopAllCoroutines();
        audioSource.Stop();
    }

    public void Pause()
    {
        audioSource.Pause();
    }

    public void Resume()
    {
        audioSource.UnPause();
    }

    public bool IsPlaying()
    {
        return audioSource.isPlaying;
    }

    public void SetMasterVolume(float volume)
    {
        // This affects the current playing volume, but doesn't change the track's set volume
        float currentTrackVolume = GetCurrentTrackVolume();
        audioSource.volume = Mathf.Clamp01(volume) * currentTrackVolume;
    }

    private float GetCurrentTrackVolume()
    {
        // Return the volume of currently appropriate track
        if (isExamMode)
        {
            if (audioSource.clip == midtermExamTheme?.audioClip)
                return midtermExamTheme.volume;
            if (audioSource.clip == finalExamTheme?.audioClip)
                return finalExamTheme.volume;
        }

        if (isCurrentlyDay && audioSource.clip == dayTheme?.audioClip)
            return dayTheme.volume;
        if (!isCurrentlyDay && audioSource.clip == nightTheme?.audioClip)
            return nightTheme.volume;

        return 1f; // Default volume
    }

    // ADDED: Method to manually trigger day/night theme (for debugging)
    public void ForcePlayDayNightTheme()
    {
        if (!isExamMode)
        {
            PlayCurrentDayNightTheme();
        }
    }

    #endregion

    #region Debug Methods (Optional)

    [Header("Debug")]
    [SerializeField] bool showDebugInfo = false;

    private void OnGUI()
    {
        if (!showDebugInfo) return;

        GUILayout.BeginArea(new Rect(10, 10, 300, 250));
        GUILayout.Label($"Current Mode: {(isExamMode ? "Exam" : "Day/Night")}");
        GUILayout.Label($"Time: {(isCurrentlyDay ? "Day" : "Night")}");
        GUILayout.Label($"Playing: {(audioSource.clip ? audioSource.clip.name : "None")}");
        GUILayout.Label($"Volume: {audioSource.volume:F2}");
        GUILayout.Label($"Is Playing: {audioSource.isPlaying}");

        if (GUILayout.Button("Test Midterm Exam"))
            StartMidtermExam();
        if (GUILayout.Button("Test Final Exam"))
            StartFinalExam();
        if (GUILayout.Button("End Exam"))
            EndExam();
        if (GUILayout.Button("Force Day/Night Theme"))
            ForcePlayDayNightTheme();

        GUILayout.EndArea();
    }

    #endregion
}