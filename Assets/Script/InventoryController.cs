using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryController : MonoBehaviour
{
    [SerializeField] GameObject panel;
    [SerializeField] GameObject toolbarPanel;
    [SerializeField] GameObject storePanel;
    [SerializeField] GameObject itemDetailPanel;

    [Header("Achievement System")]
    [SerializeField] AchievementManager achievementManager;
    private bool hasCheckedFirstStock = false;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            ItemContainerInteractController containerController = GetComponent<ItemContainerInteractController>();
            if (containerController != null && containerController.IsContainerOpen())
            {
                containerController.CloseCurrentContainer();
            }
            else
            {
                if (panel.activeInHierarchy == false)
                {
                    Open();
                }
                else
                {
                    Close();
                }
            }
        }

        // Check for stock achievement
        CheckStockAchievement();

        // NEW: Check for inventory updates when store panel is open
        if (storePanel != null && storePanel.activeInHierarchy && panel.activeInHierarchy)
        {
            ForceRefreshInventoryDisplay();
        }
    }

    private void CheckStockAchievement()
    {
        // Only check if we haven't completed this achievement yet
        if (hasCheckedFirstStock || achievementManager == null)
            return;

        // Check if achievement is already completed
        if (achievementManager.IsAchievementCompleted(1))
        {
            hasCheckedFirstStock = true;
            return;
        }

        // Get the inventory from the panel
        ItemPanel inventoryPanel = panel.GetComponent<ItemPanel>();
        if (inventoryPanel == null || inventoryPanel.inventory == null)
            return;

        ItemContainer inventory = inventoryPanel.inventory;

        // Check for stock items
        for (int i = 0; i < inventory.slots.Count; i++)
        {
            ItemSlot slot = inventory.slots[i];
            if (slot.item != null && slot.item.itemType == Item.ItemType.Stock && slot.count > 0)
            {
                Debug.Log($"[ACHIEVEMENT] Found first stock item: {slot.item.Name}");
                achievementManager.CompleteAchievementWithPopup(1);
                hasCheckedFirstStock = true;
                break;
            }
        }
    }

    public void Open()
    {
        panel.SetActive(true);
        toolbarPanel.SetActive(false);
        // Set normal inventory position
        InventoryPanel inventoryPanel = panel.GetComponent<InventoryPanel>();
        if (inventoryPanel != null)
        {
            inventoryPanel.SetNormalPosition();
        }
        storePanel.SetActive(false);
    }

    public void Close()
    {
        panel.SetActive(false);
        toolbarPanel.SetActive(true);
        storePanel.SetActive(false);
        itemDetailPanel.SetActive(false);
    }

    public GameObject GetInventoryPanel()
    {
        return panel;
    }

    // NEW: Method to force refresh inventory display
    private void ForceRefreshInventoryDisplay()
    {
        // Force update the main inventory panel
        ItemPanel inventoryPanel = panel.GetComponent<ItemPanel>();
        if (inventoryPanel != null && inventoryPanel.inventory != null)
        {
            // Mark inventory as dirty to force update
            inventoryPanel.inventory.isDirty = true;
            // Also force immediate show to ensure visual update
            inventoryPanel.Show();
        }
        // Also update any other ItemPanel components that might be children
        ItemPanel[] allPanels = panel.GetComponentsInChildren<ItemPanel>();
        foreach (ItemPanel childPanel in allPanels)
        {
            if (childPanel.inventory != null)
            {
                childPanel.inventory.isDirty = true;
                childPanel.Show();
            }
        }
    }

    // Optional: Reset achievement check (useful for testing)
    [ContextMenu("Reset Stock Achievement Check")]
    public void ResetStockAchievementCheck()
    {
        hasCheckedFirstStock = false;
        Debug.Log("[ACHIEVEMENT] Reset stock achievement check");
    }
}