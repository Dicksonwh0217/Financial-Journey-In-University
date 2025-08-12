using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ItemToolbarPanel : ItemPanel
{
    [SerializeField] ToolbarController toolbarController;
    [SerializeField] TextMeshProUGUI showItemText;
    [SerializeField] float fadeInDuration = 0.2f;
    [SerializeField] float displayDuration = 1.5f;
    [SerializeField] float fadeOutDuration = 0.5f;

    private int currentSelectedTool = -1;
    private Coroutine textFadeCoroutine;

    private void Start()
    {
        Init();
        // Check if toolbarController exists before subscribing
        if (toolbarController != null)
        {
            toolbarController.onChange += Highlight;
            Highlight(0);
        }
        else
        {
            Debug.LogError("ToolbarController is not assigned to ItemToolbarPanel!");
        }

        // Initialize text display
        if (showItemText != null)
        {
            showItemText.alpha = 0f;
        }
        else
        {
            Debug.LogWarning("showItemText is not assigned to ItemToolbarPanel!");
        }
    }

    // Override OnClick to only handle selection, not drag-and-drop
    public override void OnClick(int id)
    {
        if (toolbarController != null && IsValidSlotIndex(id))
        {
            toolbarController.Set(id);
        }
    }

    // Override OnLeftClick to only handle selection, not drag-and-drop
    public override void OnLeftClick(int id)
    {
        if (toolbarController != null && IsValidSlotIndex(id))
        {
            toolbarController.Set(id);
        }
    }

    // Override OnRightClick to only handle selection, not drag-and-drop
    public override void OnRightClick(int id)
    {
        if (toolbarController != null && IsValidSlotIndex(id))
        {
            toolbarController.Set(id);
        }
    }

    public void Highlight(int id)
    {
        // Validate buttons list exists
        if (buttons == null || buttons.Count == 0)
        {
            Debug.LogError("Buttons list is null or empty in ItemToolbarPanel!");
            return;
        }

        // Validate new selection index
        if (id < 0 || id >= buttons.Count)
        {
            Debug.LogError($"Highlight index {id} is out of range! Buttons count: {buttons.Count}");
            return;
        }

        // Remove highlight from current selection if valid
        if (currentSelectedTool >= 0 && currentSelectedTool < buttons.Count && buttons[currentSelectedTool] != null)
        {
            buttons[currentSelectedTool].Highlight(false);
        }

        // Set new selection
        currentSelectedTool = id;

        // Apply highlight to new selection if valid
        if (buttons[currentSelectedTool] != null)
        {
            buttons[currentSelectedTool].Highlight(true);
        }
        else
        {
            Debug.LogError($"Button at index {currentSelectedTool} is null!");
        }

        // Show item name with fade effect
        ShowItemNameWithFade(id);
    }

    private void ShowItemNameWithFade(int slotIndex)
    {
        if (showItemText == null || inventory == null)
            return;

        // Stop any existing fade coroutine
        if (textFadeCoroutine != null)
        {
            StopCoroutine(textFadeCoroutine);
        }

        // Get item name
        string itemName = GetItemNameFromSlot(slotIndex);

        // Start fade effect
        textFadeCoroutine = StartCoroutine(FadeItemText(itemName));
    }

    private string GetItemNameFromSlot(int slotIndex)
    {
        // Validate slot index and inventory
        if (inventory == null || inventory.slots == null ||
            slotIndex < 0 || slotIndex >= inventory.slots.Count)
        {
            return "";
        }

        ItemSlot slot = inventory.slots[slotIndex];
        if (slot?.item != null)
        {
            return slot.item.Name;
        }

        return "";
    }

    private IEnumerator FadeItemText(string itemName)
    {
        if (showItemText == null)
            yield break;

        // Set text
        showItemText.text = itemName;

        // Fade in
        float elapsedTime = 0f;
        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeInDuration);
            showItemText.alpha = alpha;
            yield return null;
        }
        showItemText.alpha = 1f;

        // Display duration
        yield return new WaitForSeconds(displayDuration);

        // Fade out
        elapsedTime = 0f;
        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeOutDuration);
            showItemText.alpha = alpha;
            yield return null;
        }
        showItemText.alpha = 0f;

        textFadeCoroutine = null;
    }

    public override void Show()
    {
        base.Show();
        // Check if toolbarController exists before calling its methods
        if (toolbarController != null)
        {
            toolbarController.UpdateHighlightIcon();
        }
    }

    private void OnEnable()
    {
        // Force show when enabled to ensure toolbar is synced
        Show();
    }

    // Helper method to validate slot indices
    private bool IsValidSlotIndex(int index)
    {
        return index >= 0 && buttons != null && index < buttons.Count;
    }

    // Clean up event subscription when destroyed
    private void OnDestroy()
    {
        if (toolbarController != null)
        {
            toolbarController.onChange -= Highlight;
        }

        // Stop any running coroutines
        if (textFadeCoroutine != null)
        {
            StopCoroutine(textFadeCoroutine);
        }
    }
}