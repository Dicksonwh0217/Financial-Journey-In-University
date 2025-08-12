using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDetailPanel : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] TMPro.TextMeshProUGUI itemName;
    [SerializeField] TMPro.TextMeshProUGUI itemDescription;
    [SerializeField] TMPro.TextMeshProUGUI itemPrice;
    [SerializeField] TMPro.TextMeshProUGUI itemType;

    [Header("Positioning Settings")]
    [SerializeField] float offsetDistance = 15f; // Distance from cursor
    [SerializeField] float screenPadding = 10f; // Padding from screen edges

    private RectTransform rectTransform;
    private Canvas canvas;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
    }

    public void ShowItemDetails(Item item, Vector3 mousePosition)
    {
        if (item == null)
        {
            Hide();
            return;
        }

        // Show the panel first so components are active
        gameObject.SetActive(true);

        // Re-get component references if they're null (fixes SetActive issue)
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();
        if (canvas == null)
            canvas = GetComponentInParent<Canvas>();

        // Update UI elements with item data
        if (itemName != null)
            itemName.text = item.Name;
        if (itemDescription != null)
            itemDescription.text = item.description;
        if (itemPrice != null)
            itemPrice.text = item.price.ToString();
        if (itemType != null)
            itemType.text = item.itemType.ToString();

        // Force layout rebuild to get accurate size
        Canvas.ForceUpdateCanvases();

        // Position the panel near the mouse
        PositionPanel(mousePosition);
    }

    private void PositionPanel(Vector3 mousePosition)
    {
        // Check for null references after re-getting them
        if (rectTransform == null)
        {
            Debug.LogError("ItemDetailPanel: Could not find RectTransform component");
            return;
        }
        if (canvas == null)
        {
            Debug.LogError("ItemDetailPanel: Could not find Canvas in parent hierarchy");
            return;
        }

        // Get canvas RectTransform
        RectTransform canvasRectTransform = canvas.transform as RectTransform;
        if (canvasRectTransform == null)
        {
            Debug.LogWarning("ItemDetailPanel: Canvas doesn't have RectTransform component");
            return;
        }

        // Convert mouse position to canvas space
        Vector2 localPoint;
        bool success = RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRectTransform,
            mousePosition,
            canvas.worldCamera,
            out localPoint);

        if (!success)
        {
            Debug.LogWarning("ItemDetailPanel: Failed to convert screen point to local point");
            return;
        }

        // Get panel size
        Vector2 panelSize = rectTransform.sizeDelta;

        // Get canvas bounds
        Vector2 canvasSize = canvasRectTransform.sizeDelta;
        Vector2 canvasMin = -canvasSize * 0.5f;
        Vector2 canvasMax = canvasSize * 0.5f;

        // Calculate initial position (right of cursor)
        Vector2 targetPosition = localPoint + new Vector2(offsetDistance, 0);

        // Smart positioning logic
        Vector2 finalPosition = GetSmartPosition(targetPosition, panelSize, canvasMin, canvasMax, localPoint);

        rectTransform.localPosition = finalPosition;
    }

    private Vector2 GetSmartPosition(Vector2 targetPosition, Vector2 panelSize, Vector2 canvasMin, Vector2 canvasMax, Vector2 cursorPosition)
    {
        Vector2 finalPosition = targetPosition;

        // Check horizontal bounds
        float panelLeft = finalPosition.x - panelSize.x * rectTransform.pivot.x;
        float panelRight = finalPosition.x + panelSize.x * (1 - rectTransform.pivot.x);

        if (panelRight > canvasMax.x - screenPadding)
        {
            // Panel goes off right edge, position it to the left of cursor
            finalPosition.x = cursorPosition.x - offsetDistance - panelSize.x * (1 - rectTransform.pivot.x);
        }
        else if (panelLeft < canvasMin.x + screenPadding)
        {
            // Panel goes off left edge, clamp to left boundary
            finalPosition.x = canvasMin.x + screenPadding + panelSize.x * rectTransform.pivot.x;
        }

        // Check vertical bounds
        float panelTop = finalPosition.y + panelSize.y * (1 - rectTransform.pivot.y);
        float panelBottom = finalPosition.y - panelSize.y * rectTransform.pivot.y;

        if (panelTop > canvasMax.y - screenPadding)
        {
            // Panel goes off top edge, move it down
            finalPosition.y = canvasMax.y - screenPadding - panelSize.y * (1 - rectTransform.pivot.y);
        }
        else if (panelBottom < canvasMin.y + screenPadding)
        {
            // Panel goes off bottom edge, move it up
            finalPosition.y = canvasMin.y + screenPadding + panelSize.y * rectTransform.pivot.y;
        }

        // Final check: if panel is still overlapping cursor after repositioning,
        // try to position it above or below the cursor
        if (IsPanelOverlappingCursor(finalPosition, panelSize, cursorPosition))
        {
            // Try positioning above cursor first
            Vector2 abovePosition = new Vector2(finalPosition.x, cursorPosition.y + offsetDistance);
            if (abovePosition.y + panelSize.y * (1 - rectTransform.pivot.y) <= canvasMax.y - screenPadding)
            {
                finalPosition.y = abovePosition.y;
            }
            else
            {
                // Position below cursor
                finalPosition.y = cursorPosition.y - offsetDistance - panelSize.y * (1 - rectTransform.pivot.y);
            }
        }

        return finalPosition;
    }

    private bool IsPanelOverlappingCursor(Vector2 panelPosition, Vector2 panelSize, Vector2 cursorPosition)
    {
        float panelLeft = panelPosition.x - panelSize.x * rectTransform.pivot.x;
        float panelRight = panelPosition.x + panelSize.x * (1 - rectTransform.pivot.x);
        float panelTop = panelPosition.y + panelSize.y * (1 - rectTransform.pivot.y);
        float panelBottom = panelPosition.y - panelSize.y * rectTransform.pivot.y;

        return cursorPosition.x >= panelLeft && cursorPosition.x <= panelRight &&
               cursorPosition.y >= panelBottom && cursorPosition.y <= panelTop;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}