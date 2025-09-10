using UnityEngine;

[DefaultExecutionOrder(-100)]
public class PauseMenuController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] GameObject PauseMenuPanel;
    [SerializeField] GameObject[] otherUIPanels; // Drag other UI panels here

    [Header("Sound Effects")]
    [SerializeField] AudioSource sfxAudioSource; // Separate audio source for SFX
    [SerializeField] AudioClip pauseOpenSFX;     // Sound when pause menu opens
    [SerializeField] AudioClip pauseCloseSFX;    // Sound when pause menu closes
    [Range(0f, 1f)]
    [SerializeField] float sfxVolume = 1f;       // Volume for sound effects

    private bool isPaused = false;
    private bool wasMusicPlayingBeforePause = false;

    private void Start()
    {
        // Create SFX audio source if not assigned
        if (sfxAudioSource == null)
        {
            // Create a child GameObject for SFX
            GameObject sfxObject = new GameObject("PauseMenu_SFX");
            sfxObject.transform.SetParent(this.transform);
            sfxAudioSource = sfxObject.AddComponent<AudioSource>();

            // Configure SFX audio source
            sfxAudioSource.playOnAwake = false;
            sfxAudioSource.loop = false;
            sfxAudioSource.volume = sfxVolume;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HandleEscapeKey();
        }
    }

    private void HandleEscapeKey()
    {
        // First check if any other UI panels are open and close the most recent one
        if (HasOpenUIPanel())
        {
            return;
        }
        // If no other UI is open, toggle pause menu
        else
        {
            TogglePauseMenu();
        }
    }

    private bool HasOpenUIPanel()
    {
        foreach (GameObject panel in otherUIPanels)
        {
            if (panel != null && panel.activeInHierarchy)
            {
                return true;
            }
        }
        return false;
    }

    public void TogglePauseMenu()
    {
        if (isPaused)
        {
            ClosePauseMenu();
        }
        else
        {
            OpenPauseMenu();
        }
    }

    public void OpenPauseMenu()
    {
        isPaused = true;
        PauseMenuPanel.SetActive(true);

        // Play open sound effect
        PlaySFX(pauseOpenSFX);

        // Pause music and remember if it was playing
        PauseMusicManager();

        // Pause the game
        Time.timeScale = 0f;

        Debug.Log("Pause Menu Opened");
    }

    public void ClosePauseMenu()
    {
        isPaused = false;
        PauseMenuPanel.SetActive(false);

        // Play close sound effect
        PlaySFX(pauseCloseSFX);

        // Resume music if it was playing before pause
        ResumeMusicManager();

        // Resume the game
        Time.timeScale = 1f;

        Debug.Log("Pause Menu Closed");
    }

    private void PlaySFX(AudioClip clip)
    {
        if (sfxAudioSource != null && clip != null)
        {
            sfxAudioSource.volume = sfxVolume;
            sfxAudioSource.PlayOneShot(clip);
        }
    }

    private void PauseMusicManager()
    {
        if (MusicManager.instance != null)
        {
            wasMusicPlayingBeforePause = MusicManager.instance.IsPlaying();
            if (wasMusicPlayingBeforePause)
            {
                MusicManager.instance.Pause();
                Debug.Log("Music paused");
            }
        }
    }

    private void ResumeMusicManager()
    {
        if (MusicManager.instance != null && wasMusicPlayingBeforePause)
        {
            MusicManager.instance.Resume();
            Debug.Log("Music resumed");
        }
    }

    // Call this method from other UI controllers when they open
    public void RegisterOpenUI(GameObject uiPanel)
    {
        // You could implement a stack-based system here for better UI management
    }

    // Public method to set SFX volume (useful for settings menu)
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        if (sfxAudioSource != null)
        {
            sfxAudioSource.volume = sfxVolume;
        }
    }

    // Property to check if pause menu is currently open
    public bool IsPaused => isPaused;

    #region Editor Debug (Optional)

    [Header("Debug")]
    [SerializeField] bool showDebugButtons = false;

    private void OnGUI()
    {
        if (!showDebugButtons) return;

        GUILayout.BeginArea(new Rect(Screen.width - 200, 10, 180, 100));
        GUILayout.Label("Pause Menu Debug");
        GUILayout.Label($"Is Paused: {isPaused}");

        if (GUILayout.Button("Toggle Pause"))
            TogglePauseMenu();
        if (GUILayout.Button("Test Open SFX"))
            PlaySFX(pauseOpenSFX);
        if (GUILayout.Button("Test Close SFX"))
            PlaySFX(pauseCloseSFX);

        GUILayout.EndArea();
    }

    #endregion
}