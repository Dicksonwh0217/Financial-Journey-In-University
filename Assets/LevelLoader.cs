// Modified LevelLoader.cs
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelLoader : MonoBehaviour
{
    public GameObject loadingScreen;
    public Text progressText;

    private Coroutine loadingTextAnimation;

    public void LoadLevel(int sceneIndex)
    {
        StartCoroutine(LoadAsynchronously(sceneIndex));
    }

    // New method for loading multiple scenes
    public void LoadLevelWithAdditiveScene(string mainSceneName, string additiveSceneName)
    {
        StartCoroutine(LoadScenesAsynchronously(mainSceneName, additiveSceneName));
    }

    IEnumerator LoadAsynchronously(int sceneIndex)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);
        loadingScreen.SetActive(true);

        // Wait one frame to ensure the loading screen is active and animator can start
        yield return null;

        // Don't start text animation if Animator is handling it
        // loadingTextAnimation = StartCoroutine(AnimateLoadingText());

        while (!operation.isDone)
        {
            Debug.Log("Loading Progress: " + operation.progress);
            yield return null;
        }

        // Stop the loading text animation if it was started
        if (loadingTextAnimation != null)
        {
            StopCoroutine(loadingTextAnimation);
        }

        loadingScreen.SetActive(false);
    }

    // New coroutine for loading multiple scenes
    IEnumerator LoadScenesAsynchronously(string mainSceneName, string additiveSceneName)
    {
        loadingScreen.SetActive(true);

        // Wait one frame to ensure the loading screen is active and animator can start
        yield return null;

        // Don't start text animation if Animator is handling it
        // loadingTextAnimation = StartCoroutine(AnimateLoadingText());

        // Load main scene
        AsyncOperation mainSceneOperation = SceneManager.LoadSceneAsync(mainSceneName, LoadSceneMode.Single);

        // Wait for main scene to load
        while (!mainSceneOperation.isDone)
        {
            Debug.Log("Main Scene Progress: " + mainSceneOperation.progress);
            yield return null;
        }

        // Load additive scene
        AsyncOperation additiveSceneOperation = SceneManager.LoadSceneAsync(additiveSceneName, LoadSceneMode.Additive);

        // Wait for additive scene to load
        while (!additiveSceneOperation.isDone)
        {
            Debug.Log("Additive Scene Progress: " + additiveSceneOperation.progress);
            yield return null;
        }

        // Stop the loading text animation if it was started
        if (loadingTextAnimation != null)
        {
            StopCoroutine(loadingTextAnimation);
        }

        // Optional: Keep loading screen visible for a short moment to see final animation
        yield return new WaitForSeconds(0.5f);

        loadingScreen.SetActive(false);
    }

    // Coroutine to animate the loading text
    IEnumerator AnimateLoadingText()
    {
        string baseText = "Loading";
        int dotCount = 0;

        // Wait a bit for animator to start
        yield return new WaitForSeconds(0.1f);

        while (true)
        {
            string displayText = baseText;

            // Add dots based on current count
            for (int i = 0; i <= dotCount; i++)
            {
                displayText += ".";
            }

            // Only update text if progressText is not being controlled by animator
            if (progressText != null)
            {
                progressText.text = displayText;
            }

            // Cycle through 0, 1, 2 dots
            dotCount = (dotCount + 1) % 3;

            // Wait for half a second before changing
            yield return new WaitForSeconds(0.5f);
        }
    }
}