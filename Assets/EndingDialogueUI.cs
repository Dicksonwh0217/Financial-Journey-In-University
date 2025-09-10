// EndingDialogueUI.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class EndingDialogueUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Button continueButton;

    [Header("Animation Settings")]
    [SerializeField] private float panelAnimationDuration = 0.5f;
    [SerializeField] private Ease panelAnimationEase = Ease.OutBack;
    [SerializeField] private float textTypeSpeed = 0.05f;
    [SerializeField] private float panelPopOutDuration = 0.3f;
    [SerializeField] private Ease panelPopOutEase = Ease.InBack;

    private List<string> currentDialogueLines;
    private int currentLineIndex = 0;
    private bool isTyping = false;
    private Coroutine typingCoroutine;
    private bool shouldStartDialogue = false;
    private EndingData preparedEndingData;

    public System.Action OnDialogueComplete;

    private void Awake()
    {
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinueClicked);
        }

        // Hide panel initially
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
    }

    private void Update()
    {
        // Check if we need to start dialogue after the GameObject becomes active
        if (shouldStartDialogue && gameObject.activeInHierarchy)
        {
            shouldStartDialogue = false;
            DisplayCurrentLine();
        }
    }

    public void PrepareEndingDialogue(EndingData endingData)
    {
        if (endingData == null) return;

        // Store the ending data for later use
        preparedEndingData = endingData;
        currentDialogueLines = endingData.dialogueLines;
        currentLineIndex = 0;

        // Ensure this GameObject is active
        if (!gameObject.activeInHierarchy)
        {
            gameObject.SetActive(true);
        }

        if (backgroundImage != null)
        {
            if (endingData.endingImage != null)
            {
                backgroundImage.sprite = endingData.endingImage;
            }
            backgroundImage.color = endingData.backgroundColor;
        }

        // Keep panel hidden
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
    }

    public void ShowPreparedDialogue()
    {
        if (preparedEndingData == null) return;

        // Show panel with animation
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
            dialoguePanel.transform.localScale = Vector3.zero;

            dialoguePanel.transform.DOScale(Vector3.one, panelAnimationDuration)
                .SetEase(panelAnimationEase)
                .OnComplete(() => {
                    if (gameObject.activeInHierarchy)
                    {
                        DisplayCurrentLine();
                    }
                    else
                    {
                        shouldStartDialogue = true;
                    }
                });
        }
    }

    public void ShowEndingDialogue(EndingData endingData)
    {
        // For backward compatibility, prepare and show immediately
        PrepareEndingDialogue(endingData);
        ShowPreparedDialogue();
    }

    private void DisplayCurrentLine()
    {
        if (currentLineIndex >= currentDialogueLines.Count)
        {
            // All lines displayed, wait and complete with pop-out animation
            StartCoroutine(CompleteDialogueWithPopOut());
            return;
        }

        string lineToDisplay = currentDialogueLines[currentLineIndex];

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        typingCoroutine = StartCoroutine(TypeText(lineToDisplay));
    }

    private IEnumerator TypeText(string text)
    {
        isTyping = true;

        // Ensure dialogueText exists and is active
        if (dialogueText != null)
        {
            dialogueText.text = "";

            foreach (char c in text)
            {
                if (dialogueText != null) // Check again in case it was destroyed
                {
                    dialogueText.text += c;
                }
                yield return new WaitForSeconds(textTypeSpeed);
            }
        }

        isTyping = false;
    }

    private void OnContinueClicked()
    {
        if (isTyping)
        {
            // Skip typing animation
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }
            if (dialogueText != null && currentLineIndex < currentDialogueLines.Count)
            {
                dialogueText.text = currentDialogueLines[currentLineIndex];
            }
            isTyping = false;
        }
        else
        {
            // Move to next line
            currentLineIndex++;
            DisplayCurrentLine();
        }
    }

    private IEnumerator CompleteDialogueWithPopOut()
    {
        // Hide continue button on last line
        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(false);
        }

        // Wait a moment before starting the pop-out animation
        yield return new WaitForSeconds(1f);

        // Pop-out animation for the dialogue panel
        if (dialoguePanel != null)
        {
            dialoguePanel.transform.DOScale(Vector3.zero, panelPopOutDuration)
                .SetEase(panelPopOutEase)
                .OnComplete(() => {
                    dialoguePanel.SetActive(false);
                    OnDialogueComplete?.Invoke();
                });
        }
        else
        {
            // Fallback if no panel to animate
            OnDialogueComplete?.Invoke();
        }
    }

    private IEnumerator CompleteDialogueAfterDelay()
    {
        // Hide continue button on last line
        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(false);
        }

        yield return new WaitForSeconds(1f);

        OnDialogueComplete?.Invoke();
    }

    public void HideDialogue()
    {
        // Stop any running coroutines before hiding
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
    }

    // Method to ensure dialogue is completely hidden (for ending system)
    public void EnsureHidden()
    {
        HideDialogue();
        if (gameObject.activeInHierarchy)
        {
            gameObject.SetActive(false);
        }
    }

    private void OnDisable()
    {
        // Clean up when the component is disabled
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
        shouldStartDialogue = false;
        preparedEndingData = null;
    }
}