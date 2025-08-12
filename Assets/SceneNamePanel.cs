using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class SceneNamePanel : MonoBehaviour
{
    private CanvasGroup canvasGroup;

    [Header("UI Components")]
    public TextMeshProUGUI sceneNameText;

    [Header("Animation Settings")]
    public float fadeInDuration = 1.0f;
    public float displayDuration = 2.0f;
    public float fadeOutDuration = 1.0f;

    private bool isAnimating = false;

    private void Awake()
    {
        // Get or add CanvasGroup component
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // Initialize the panel as hidden
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    public void ShowSceneName(string sceneName)
    {
        if (isAnimating)
            StopAllCoroutines();

        StartCoroutine(DisplaySceneNameSequence(sceneName));
    }

    private IEnumerator DisplaySceneNameSequence(string sceneName)
    {
        isAnimating = true;

        // Set the scene name
        if (sceneNameText != null)
            sceneNameText.text = sceneName;

        // Fade In
        yield return StartCoroutine(FadeIn());

        // Display
        yield return new WaitForSeconds(displayDuration);

        // Fade Out
        yield return StartCoroutine(FadeOut());

        isAnimating = false;
    }

    private IEnumerator FadeIn()
    {
        float elapsedTime = 0f;
        float startAlpha = canvasGroup.alpha;

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = true;

        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / fadeInDuration;

            // Use smooth curve for fade
            float smoothTime = Mathf.SmoothStep(0f, 1f, normalizedTime);
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 1f, smoothTime);

            yield return null;
        }

        canvasGroup.alpha = 1f;
    }

    private IEnumerator FadeOut()
    {
        float elapsedTime = 0f;
        float startAlpha = canvasGroup.alpha;

        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / fadeOutDuration;

            // Use smooth curve for fade
            float smoothTime = Mathf.SmoothStep(0f, 1f, normalizedTime);
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, smoothTime);

            yield return null;
        }

        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
    }

    public void SetAnimationSettings(float fadeIn, float display, float fadeOut)
    {
        fadeInDuration = fadeIn;
        displayDuration = display;
        fadeOutDuration = fadeOut;
    }

    public void ForceHide()
    {
        StopAllCoroutines();
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        isAnimating = false;
    }
}