using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPanel : MonoBehaviour
{
    [Header("Panel Settings")]
    [SerializeField] protected bool showItemDetails = true;

    public bool ShowItemDetails => showItemDetails; //getter

    public ItemContainer inventory;
    public List<InventoryButton> buttons;
    [SerializeField] protected ItemDetailPanel itemDetailPanel;

    private void Start()
    {
        Init();
    }

    public void Init()
    {
        SetSourcePanel();
        SetIndex();
        Show();
    }

    private void SetSourcePanel()
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            buttons[i].SetItemPanel(this);
            if (itemDetailPanel != null)
            {
                buttons[i].SetDetailPanel(itemDetailPanel);
            }
        }
    }

    private void OnEnable()
    {
        Clear();
        Show();
    }

    // Hide detail panel when this panel is disabled
    private void OnDisable()
    {
        if (itemDetailPanel != null)
        {
            itemDetailPanel.Hide();
        }
    }

    private void LateUpdate()
    {
        if (inventory == null)
        {
            return;
        }
        if (inventory.isDirty)
        {
            Show();
            inventory.isDirty = false;
        }
    }

    private void SetIndex()
    {
        for (int i = 0; i < inventory.slots.Count && i < buttons.Count; i++)
        {
            buttons[i].SetIndex(i);
        }
    }

    public virtual void Show()
    {
        if (inventory == null)
        {
            return;
        }
        for (int i = 0; i < inventory.slots.Count && i < buttons.Count; i++)
        {
            if (inventory.slots[i].item == null)
            {
                buttons[i].Clean();
            }
            else
            {
                buttons[i].Set(inventory.slots[i]);
            }
        }
    }

    public void Clear()
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            buttons[i].Clean();
        }
    }

    public void SetInventory(ItemContainer newInventory)
    {
        inventory = newInventory;
    }

    // Legacy method for backward compatibility
    public virtual void OnClick(int id)
    {
        OnLeftClick(id);
    }

    // New left click method - takes/puts ALL items
    public virtual void OnLeftClick(int id)
    {
        if (inventory == null || id >= inventory.slots.Count)
            return;

        ItemSlot clickedSlot = inventory.slots[id];
        ItemDragAndDropController dragController = GameManager.instance.dragAndDropController;

        if (dragController.itemSlot.item == null)
        {
            // Not holding anything - take ALL items from clicked slot
            if (clickedSlot.item != null)
            {
                dragController.itemSlot.Copy(clickedSlot);
                clickedSlot.Clear();
                inventory.isDirty = true;
            }
        }
        else
        {
            // Holding something - put ALL held items
            if (clickedSlot.item == null)
            {
                // Empty slot - place all held items
                clickedSlot.Copy(dragController.itemSlot);
                dragController.itemSlot.Clear();
                inventory.isDirty = true;
            }
            else if (clickedSlot.item == dragController.itemSlot.item && clickedSlot.item.stackable)
            {
                // Same stackable item - combine stacks
                clickedSlot.count += dragController.itemSlot.count;
                dragController.itemSlot.Clear();
                inventory.isDirty = true;
            }
            else
            {
                // Different item - swap
                ItemSlot temp = new ItemSlot();
                temp.Copy(clickedSlot);
                clickedSlot.Copy(dragController.itemSlot);
                dragController.itemSlot.Copy(temp);
                inventory.isDirty = true;
            }
        }

        dragController.UpdateIcon();
    }

    // New right click method - takes/puts HALF or ONE item
    public virtual void OnRightClick(int id)
    {
        if (inventory == null || id >= inventory.slots.Count)
            return;

        ItemSlot clickedSlot = inventory.slots[id];
        ItemDragAndDropController dragController = GameManager.instance.dragAndDropController;

        if (dragController.itemSlot.item == null)
        {
            // Not holding anything - take HALF of items from clicked slot
            if (clickedSlot.item != null && clickedSlot.count > 0)
            {
                if (clickedSlot.item.stackable)
                {
                    int halfAmount = Mathf.CeilToInt(clickedSlot.count / 2f); // Round up for odd numbers
                    dragController.itemSlot.Set(clickedSlot.item, halfAmount);
                    clickedSlot.count -= halfAmount;

                    if (clickedSlot.count <= 0)
                    {
                        clickedSlot.Clear();
                    }
                }
                else
                {
                    // Non-stackable items - take the whole item
                    dragController.itemSlot.Copy(clickedSlot);
                    clickedSlot.Clear();
                }
                inventory.isDirty = true;
            }
        }
        else
        {
            // Holding something - put ONE item
            if (clickedSlot.item == null)
            {
                // Empty slot - place one item
                clickedSlot.Set(dragController.itemSlot.item, 1);
                dragController.itemSlot.count -= 1;

                if (dragController.itemSlot.count <= 0)
                {
                    dragController.itemSlot.Clear();
                }
                inventory.isDirty = true;
            }
            else if (clickedSlot.item == dragController.itemSlot.item && clickedSlot.item.stackable)
            {
                // Same stackable item - add one to the stack
                clickedSlot.count += 1;
                dragController.itemSlot.count -= 1;

                if (dragController.itemSlot.count <= 0)
                {
                    dragController.itemSlot.Clear();
                }
                inventory.isDirty = true;
            }
            // If different items, do nothing (or you could implement swap behavior)
        }

        dragController.UpdateIcon();
    }
}