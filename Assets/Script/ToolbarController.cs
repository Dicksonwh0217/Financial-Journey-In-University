using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolbarController : MonoBehaviour
{
    [SerializeField] int toolbarSize = 7;
    int selectedTool;
    public Action<int> onChange;
    [SerializeField] ItemIconHighlight itemIconHighlight;

    // Public getter for selected tool index
    public int SelectedTool => selectedTool;

    public Item GetItem
    {
        get
        {
            return GameManager.instance.inventoryContainer.slots[selectedTool].item;
        }
    }

    private void Start()
    {
        onChange += UpdateHighlightIcon;
        UpdateHighlightIcon(selectedTool);
    }

    public void Set(int id)
    {
        selectedTool = id;
        // Trigger the onChange event to update highlight and other systems
        onChange?.Invoke(selectedTool);
    }

    public void UpdateHighlightIcon(int id = 0)
    {
        Item item = GetItem;
        if (item == null)
        {
            itemIconHighlight.Show = false;
            return;
        }
        itemIconHighlight.Show = item.itemIconHighlight;
        if (item.itemIconHighlight)
        {
            itemIconHighlight.Set(item.icon);
        }
    }

    private void Update()
    {
        // Handle mouse scroll
        float delta = Input.mouseScrollDelta.y;
        if (delta != 0)
        {
            if (delta > 0)
            {
                selectedTool -= 1;
                selectedTool = (selectedTool < 0 ? toolbarSize - 1 : selectedTool);
            }
            else
            {
                selectedTool += 1;
                selectedTool = (selectedTool >= toolbarSize ? 0 : selectedTool);
            }
            onChange?.Invoke(selectedTool);
        }

        // Handle keyboard input (1-7 keys)
        for (int i = 1; i <= toolbarSize; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0 + i))
            {
                selectedTool = i - 1; // Convert to 0-based index
                onChange?.Invoke(selectedTool);
                break;
            }
        }
    }
}