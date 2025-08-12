using Unity.Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneManager : MonoBehaviour
{
    public static GameSceneManager instance;

    [SerializeField] public ScreenTint screenTint;
    [SerializeField] CameraConfiner cameraConfiner;

    [Header("Scene Name Panel")]
    [SerializeField] public SceneNamePanel sceneNamePanel;

    [Header("Scene Names Configuration")]
    [SerializeField] public SceneNameConfig[] sceneConfigs;

    string currentScene;
    AsyncOperation unload;
    AsyncOperation load;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        currentScene = SceneManager.GetActiveScene().name;
    }

    public void InitSwitchScene(string to, Vector3 targetPosition)
    {
        StartCoroutine(Transition(to, targetPosition));
    }

    IEnumerator Transition(string to, Vector3 targetPosition)
    {
        // Add null check for screenTint
        if (screenTint != null)
        {
            screenTint.Tint();
            yield return new WaitForSeconds(1f / screenTint.speed + 0.1f);
        }
        else
        {
            yield return new WaitForSeconds(0.5f); // Default wait time
        }

        SwitchScene(to, targetPosition);

        // Fixed the condition - should use && not &
        while (load != null && unload != null)
        {
            if (load.isDone)
            {
                load = null;
            }
            if (unload.isDone)
            {
                unload = null;
            }
            yield return new WaitForSeconds(0.1f);
        }

        SceneManager.SetActiveScene(SceneManager.GetSceneByName(to)); // Use 'to' instead of currentScene

        // Add null check for cameraConfiner
        if (cameraConfiner != null)
        {
            cameraConfiner.UpdateBounds();
        }

        // Show scene name panel after scene transition is complete
        ShowSceneNamePanel(to);

        // Add null check for screenTint
        if (screenTint != null)
        {
            screenTint.UnTint();
        }
    }

    public void SwitchScene(string to, Vector3 targetPosition)
    {
        load = SceneManager.LoadSceneAsync(to, LoadSceneMode.Additive);
        unload = SceneManager.UnloadSceneAsync(currentScene);
        currentScene = to;

        // Add null checks for GameManager and player
        if (GameManager.instance != null && GameManager.instance.player != null)
        {
            Transform playerTransform = GameManager.instance.player.transform;

            // Move player to target position
            GameManager.instance.player.transform.position = new Vector3(
                targetPosition.x,
                targetPosition.y,
                playerTransform.position.z
            );

            // Updated camera warping for Cinemachine 3.1.4
            HandleCameraWarp(targetPosition);
        }
    }

    // Proper Cinemachine 3.1.4 warp using CinemachineCore
    private void HandleCameraWarp(Vector3 targetPosition)
    {
        if (GameManager.instance?.player != null)
        {
            Vector3 oldPosition = GameManager.instance.player.transform.position;

            // Move player first
            GameManager.instance.player.transform.position = new Vector3(
                targetPosition.x,
                targetPosition.y,
                GameManager.instance.player.transform.position.z
            );

            // Use CinemachineCore to notify all cameras of the warp
            Vector3 deltaPosition = oldPosition - targetPosition;
            CinemachineCore.OnTargetObjectWarped(
                GameManager.instance.player.transform,
                deltaPosition
            );
        }
    }

    // Alternative method for camera warping
    private IEnumerator HandleCameraWarpAlternative(CinemachineBrain brain, Vector3 targetPosition)
    {
        if (brain == null) yield break;

        // Store original enabled state
        bool originalEnabled = brain.enabled;

        // Temporarily disable the brain to prevent smooth following
        brain.enabled = false;

        // Wait a frame
        yield return null;

        // Move the camera manually to the target position
        if (Camera.main != null)
        {
            Camera.main.transform.position = new Vector3(
                targetPosition.x,
                targetPosition.y,
                Camera.main.transform.position.z
            );
        }

        // Wait another frame to ensure position is set
        yield return null;

        // Re-enable the brain
        brain.enabled = originalEnabled;
    }

    // Scene Name Panel Methods
    public void ShowSceneNamePanel(string sceneName)
    {
        if (sceneNamePanel != null)
        {
            // Get custom scene name if configured
            string displayName = GetSceneDisplayName(sceneName);
            sceneNamePanel.ShowSceneName(displayName);
        }
    }

    private string GetSceneDisplayName(string sceneName)
    {
        if (sceneConfigs != null)
        {
            foreach (var config in sceneConfigs)
            {
                if (config != null && config.sceneName == sceneName)
                    return config.displayName;
            }
        }

        // Return formatted scene name if no config found
        return FormatSceneName(sceneName);
    }

    private string FormatSceneName(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
            return "Unknown Scene";

        // Convert "SomeSceneName" to "Some Scene Name"
        string formatted = "";
        for (int i = 0; i < sceneName.Length; i++)
        {
            if (i > 0 && char.IsUpper(sceneName[i]))
                formatted += " ";
            formatted += sceneName[i];
        }
        return formatted;
    }

    // Method to manually trigger scene name display
    public void DisplaySceneName(string sceneName, float? customFadeIn = null, float? customDisplay = null, float? customFadeOut = null)
    {
        if (sceneNamePanel != null)
        {
            // Apply custom timings if provided
            if (customFadeIn.HasValue || customDisplay.HasValue || customFadeOut.HasValue)
            {
                sceneNamePanel.SetAnimationSettings(
                    customFadeIn ?? sceneNamePanel.fadeInDuration,
                    customDisplay ?? sceneNamePanel.displayDuration,
                    customFadeOut ?? sceneNamePanel.fadeOutDuration
                );
            }

            string displayName = GetSceneDisplayName(sceneName);
            sceneNamePanel.ShowSceneName(displayName);
        }
    }
}

[System.Serializable]
public class SceneNameConfig
{
    public string sceneName;        // The actual scene name in Unity
    public string displayName;      // The name to display to players
}

// Optional: Scene Transition Manager for more advanced transitions
public class SceneTransitionManager : MonoBehaviour
{
    [Header("Transition Settings")]
    public float transitionDuration = 1.0f;
    public CanvasGroup transitionCanvasGroup;

    private static SceneTransitionManager instance;
    public static SceneTransitionManager Instance => instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void TransitionToScene(string sceneName)
    {
        StartCoroutine(TransitionSequence(sceneName));
    }

    private IEnumerator TransitionSequence(string sceneName)
    {
        // Fade to black
        yield return StartCoroutine(FadeTransition(0f, 1f));

        // Load scene
        var asyncLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName, UnityEngine.SceneManagement.LoadSceneMode.Additive);

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // Show scene name panel (GameManager will handle this automatically)

        // Fade from black
        yield return StartCoroutine(FadeTransition(1f, 0f));
    }

    private IEnumerator FadeTransition(float startAlpha, float endAlpha)
    {
        if (transitionCanvasGroup == null) yield break;

        float elapsedTime = 0f;

        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / transitionDuration;
            transitionCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, normalizedTime);
            yield return null;
        }

        transitionCanvasGroup.alpha = endAlpha;
    }
}