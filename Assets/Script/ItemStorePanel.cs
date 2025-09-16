using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ItemStorePanel : ItemPanel
{
    [SerializeField] Trading trading;

    [Header("Part-Time Job UI")]
    [SerializeField] private Button partTimeJobButton;
    [SerializeField] private TMPro.TextMeshProUGUI jobButtonText;
    [SerializeField] private ScreenTint screenTint;
    [SerializeField] private GameObject workingImage;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMPro.TextMeshProUGUI dialogueText;

    [Header("Animation Settings")]
    [SerializeField] private float workingImageAnimationDuration = 0.5f;
    [SerializeField] private Ease workingImageAnimationEase = Ease.OutBack;
    [SerializeField] private float dialoguePanelAnimationDuration = 0.3f;
    [SerializeField] private Ease dialoguePanelAnimationEase = Ease.OutQuart;
    [SerializeField] private float textTypeSpeed = 0.05f;
    [SerializeField] private float dialogueDisplayTime = 2f;

    private Store currentStore;
    private Coroutine typingCoroutine;
    private bool isTyping = false;
    private bool isInitialized = false;

    // Part-time job dialogue variations
    private string[] workDialogues = {
        "You work hard in serving customers, 4 hours gone",
        "You helped many customers find what they need, 4 hours have passed",
        "You organized shelves and assisted shoppers, time flew by in 4 hours",
        "You handled the cash register efficiently, another 4 hours of work completed"
    };

    private void Start()
    {
        // Call parent initialization first
        base.Start();

        // Initialize part-time job components
        InitializePartTimeJob();

        isInitialized = true;
    }

    private void InitializePartTimeJob()
    {
        // Ensure button starts as inactive
        if (partTimeJobButton != null)
        {
            partTimeJobButton.gameObject.SetActive(false);
            partTimeJobButton.onClick.AddListener(StartPartTimeJob);
        }

        // Initialize animated elements to be hidden
        if (workingImage != null)
        {
            workingImage.SetActive(false);
        }

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
            dialoguePanel.transform.localScale = Vector3.zero;
        }
    }

    private void Update()
    {
        // Only update if properly initialized and store is set
        if (isInitialized && currentStore != null)
        {
            UpdatePartTimeJobButton();
        }
    }

    private void UpdatePartTimeJobButton()
    {
        if (partTimeJobButton == null) return;

        // Check if store has part-time job and if it's the right time
        bool shouldShow = currentStore != null && currentStore.hasPartTimeJob && currentStore.ShouldShowPartTimeButton();

        // Only show button at 8:00 AM or 4:00 PM
        partTimeJobButton.gameObject.SetActive(shouldShow);

        if (shouldShow && jobButtonText != null)
        {
            jobButtonText.text = "Start Part-Time Job";
        }
    }

    public void SetStore(Store store)
    {
        currentStore = store;

        // Immediately update button visibility when store is set
        if (partTimeJobButton != null)
        {
            if (store != null && store.hasPartTimeJob && store.ShouldShowPartTimeButton())
            {
                partTimeJobButton.gameObject.SetActive(true);
                if (jobButtonText != null)
                {
                    jobButtonText.text = "Start Part-Time Job";
                }
            }
            else
            {
                partTimeJobButton.gameObject.SetActive(false);
            }
        }
        else
        {
            Debug.LogWarning("[ItemStorePanel] partTimeJobButton is null!");
        }
    }

    // Fixed OnLeftClick method for ItemStorePanel
    public override void OnLeftClick(int id)
    {
        // Check if the clicked slot has an item first
        if (inventory == null || id >= inventory.slots.Count)
            return;

        ItemSlot clickedSlot = inventory.slots[id];

        if (GameManager.instance.dragAndDropController.itemSlot.item == null)
        {
            // Only buy if the slot contains an item
            if (clickedSlot.item != null)
            {
                BuyItem(id);
            }
            // If slot is empty, do nothing
        }
        else
        {
            // Player is holding an item, try to sell it
            SellItem();
        }
        Show();
    }

    // Same fix for OnRightClick
    public override void OnRightClick(int id)
    {
        // Check if the clicked slot has an item first
        if (inventory == null || id >= inventory.slots.Count)
            return;

        ItemSlot clickedSlot = inventory.slots[id];

        if (GameManager.instance.dragAndDropController.itemSlot.item == null)
        {
            // Only buy if the slot contains an item
            if (clickedSlot.item != null)
            {
                BuyItem(id);
            }
            // If slot is empty, do nothing
        }
        else
        {
            // Player is holding an item, try to sell it
            SellItem();
        }
        Show();
    }

    private void BuyItem(int id)
    {
        trading.BuyItem(id);
    }

    private void SellItem()
    {
        trading.SellItem();
    }

    public void StartPartTimeJob()
    {

        // Try to get current store if not set
        if (currentStore == null && trading != null)
        {
            // Try to get the store from the trading component
            var storeField = trading.GetType().GetField("currentStore",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (storeField != null)
            {
                currentStore = storeField.GetValue(trading) as Store;
            }
        }

        StartCoroutine(PartTimeJobSequence());
    }

    private IEnumerator PartTimeJobSequence()
    {
        // Hide the part-time job button during work
        if (partTimeJobButton != null)
        {
            partTimeJobButton.gameObject.SetActive(false);
        }

        // Store original time scale and pause time
        float originalTimeScale = DayTime.Instance != null ? DayTime.Instance.TimeScale : 60f;
        if (DayTime.Instance != null)
        {
            DayTime.Instance.PauseTime();
        }

        // Screen tint
        if (screenTint != null)
        {
            screenTint.Tint();
            yield return new WaitForSeconds(0.5f);
        }

        // Show working image with animation
        yield return StartCoroutine(ShowWorkingImageAnimated());

        // Screen untint
        if (screenTint != null)
        {
            screenTint.UnTint();
            yield return new WaitForSeconds(0.5f);
        }

        // Skip time forward 4 hours
        if (DayTime.Instance != null)
        {
            DayTime.Instance.SkipTime(hours: 4);
        }

        // Show dialogue with animation
        yield return StartCoroutine(ShowDialogueAnimated());

        // Wait for dialogue display time
        yield return new WaitForSeconds(dialogueDisplayTime);

        // Hide dialogue
        yield return StartCoroutine(HideDialogueAnimated());

        // Tint screen again
        if (screenTint != null)
        {
            screenTint.Tint();
            yield return new WaitForSeconds(0.5f);
        }

        // Hide working image
        yield return StartCoroutine(HideWorkingImageAnimated());

        // Final untint
        if (screenTint != null)
        {
            screenTint.UnTint();
            yield return new WaitForSeconds(0.5f);
        }

        // Resume time
        if (DayTime.Instance != null)
        {
            DayTime.Instance.ResumeTime(originalTimeScale);
        }

        // Give payment
        Currency playerMoney = FindFirstObjectByType<Currency>();
        if (playerMoney != null)
        {
            int payment = 60;
            playerMoney.Add(payment);
            Debug.Log($"Part-time job completed! Earned {payment} coins.");
        }

        // Update button visibility after job completion (it should be hidden now since time has passed)
        UpdatePartTimeJobButton();
    }

    private IEnumerator ShowWorkingImageAnimated()
    {
        if (workingImage == null) yield break;

        workingImage.SetActive(true);
        workingImage.transform.localScale = Vector3.zero;

        yield return workingImage.transform.DOScale(Vector3.one, workingImageAnimationDuration)
            .SetEase(workingImageAnimationEase)
            .WaitForCompletion();
    }

    private IEnumerator HideWorkingImageAnimated()
    {
        if (workingImage == null) yield break;

        yield return workingImage.transform.DOScale(Vector3.zero, workingImageAnimationDuration)
            .SetEase(Ease.InBack)
            .WaitForCompletion();

        workingImage.SetActive(false);
    }

    private IEnumerator ShowDialogueAnimated()
    {
        if (dialoguePanel == null || dialogueText == null) yield break;

        string randomDialogue = workDialogues[UnityEngine.Random.Range(0, workDialogues.Length)];

        dialoguePanel.SetActive(true);
        dialoguePanel.transform.localScale = Vector3.zero;

        yield return dialoguePanel.transform.DOScale(Vector3.one, dialoguePanelAnimationDuration)
            .SetEase(dialoguePanelAnimationEase)
            .WaitForCompletion();

        yield return StartCoroutine(TypeText(randomDialogue));
    }

    private IEnumerator HideDialogueAnimated()
    {
        if (dialoguePanel == null) yield break;

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
            isTyping = false;
        }

        yield return dialoguePanel.transform.DOScale(Vector3.zero, dialoguePanelAnimationDuration)
            .SetEase(Ease.InBack)
            .WaitForCompletion();

        dialoguePanel.SetActive(false);
    }

    private IEnumerator TypeText(string text)
    {
        if (dialogueText == null) yield break;

        isTyping = true;
        dialogueText.text = "";

        foreach (char c in text)
        {
            if (dialogueText != null && isTyping)
            {
                dialogueText.text += c;
                yield return new WaitForSeconds(textTypeSpeed);
            }
            else
            {
                break;
            }
        }

        isTyping = false;
    }

    private void OnDisable()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
        isTyping = false;
    }

    private void OnDestroy()
    {
        // Clean up DOTween animations
        if (workingImage != null)
        {
            workingImage.transform.DOKill();
        }
        if (dialoguePanel != null)
        {
            dialoguePanel.transform.DOKill();
        }
    }
}