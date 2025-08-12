using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolsCharacterController : MonoBehaviour
{
    CharacterLevel characterLevel;
    CharacterController2D character;
    Rigidbody2D rgbd2d;
    ToolbarController toolbarController;
    Animator animator;
    Character characterStats; // Reference to character stats

    [SerializeField] float offsetDistance = 1f;
    [SerializeField] MarkerManager markerManager;
    [SerializeField] TileMapReadController tileMapReadController;
    [SerializeField] float maxDistance = 1.5f;
    [SerializeField] ItemIconHighlight itemIconHighlight;
    [SerializeField] ToolAction onTilePickUp;

    Vector3Int selectedTilePosition;
    bool selectable;

    private void Awake()
    {
        character = GetComponent<CharacterController2D>();
        rgbd2d = GetComponent<Rigidbody2D>();
        toolbarController = GetComponent<ToolbarController>();
        animator = GetComponent<Animator>();
        characterLevel = GetComponent<CharacterLevel>();
        characterStats = GetComponent<Character>(); // Get character stats component
    }

    private void Update()
    {
        SelectTile();
        CanSelectCheck();
        Marker();

        // Left click for tool usage
        if (Input.GetMouseButtonDown(0))
        {
            if (UseToolWorld() == true)
            {
                return;
            }
            UseToolGrid();
        }

        // Right click for eating
        if (Input.GetMouseButtonDown(1))
        {
            TryEatItem();
        }
    }

    private void SelectTile()
    {
        selectedTilePosition = tileMapReadController.GetGridPosition(Input.mousePosition, true);
    }

    void CanSelectCheck()
    {
        Vector2 characterPosition = transform.position;
        Vector2 cameraPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        selectable = Vector2.Distance(characterPosition, cameraPosition) < maxDistance;
        markerManager.Show(selectable);
        itemIconHighlight.CanSelect = selectable;
    }

    private void Marker()
    {
        markerManager.markedCellPosition = selectedTilePosition;
        itemIconHighlight.cellPosition = selectedTilePosition;
    }

    private bool UseToolWorld()
    {
        Vector2 position = rgbd2d.position + character.lastMotionVector * offsetDistance;
        Item item = toolbarController.GetItem;
        if (item == null || item.onAction == null)
        {
            return false;
        }
        animator.SetTrigger("act");
        bool complete = item.onAction.OnApply(position);
        if (complete == true)
        {
            if (item.onItemUsed != null)
            {
                item.onItemUsed.OnItemUsed(item, GameManager.instance.inventoryContainer);
            }
        }
        return complete;
    }

    private void UseToolGrid()
    {
        if (selectable == true)
        {
            Item item = toolbarController.GetItem;
            if (item == null)
            {
                PickUpTile();
                return;
            }
            if (item.onTileMapAction == null)
            {
                return;
            }
            animator.SetTrigger("act");
            bool complete = item.onTileMapAction.OnApplyToTileMap(selectedTilePosition, tileMapReadController, item);
            if (complete == true)
            {
                if (item.onItemUsed != null)
                {
                    item.onItemUsed.OnItemUsed(item, GameManager.instance.inventoryContainer);
                }
            }
        }
    }

    private void PickUpTile()
    {
        if (onTilePickUp == null)
        {
            return;
        }
        onTilePickUp.OnApplyToTileMap(selectedTilePosition, tileMapReadController, null);
    }

    private void TryEatItem()
    {
        ItemDragAndDropController dragController = FindFirstObjectByType<ItemDragAndDropController>();
        if (dragController != null && dragController.itemIcon.activeInHierarchy)
        {
            return; // Don't consume items while dragging
        }

        InventoryController inventoryController = FindFirstObjectByType<InventoryController>();
        if (inventoryController != null)
        {
            GameObject inventoryPanel = inventoryController.GetInventoryPanel();
            if (inventoryPanel != null && inventoryPanel.activeInHierarchy)
            {
                return; // Don't consume items when inventory is open
            }
        }

        // Check if GameManager instance exists
        if (GameManager.instance == null)
        {
            return;
        }

        // Check if inventory container exists
        ItemContainer inventory = GameManager.instance.inventoryContainer;
        if (inventory == null)
        {
            return;
        }

        // Check if toolbarController exists
        if (toolbarController == null)
        {
            return;
        }

        int selectedSlot = toolbarController.SelectedTool;

        // Check if slots array exists and selectedSlot is valid
        if (inventory.slots == null)
        {
            return;
        }

        if (selectedSlot < 0 || selectedSlot >= inventory.slots.Count)
        {
            return;
        }

        ItemSlot currentSlot = inventory.slots[selectedSlot];

        // Check if current slot exists
        if (currentSlot == null)
        {
            return;
        }

        Item item = currentSlot.item;

        // Check if item exists
        if (item == null)
        {
            return;
        }

        // Check if characterStats exists
        if (characterStats == null)
        {
            return;
        }

        if (!item.consumable)
        {
            return;
        }

        bool canConsume = false;
        string reasonCantConsume = "";

        // Check consumption conditions based on item type
        switch (item.itemType)
        {
            case Item.ItemType.Food:
                // Food can only be consumed if hunger is not full
                if (characterStats.Hunger != null && !characterStats.Hunger.IsFull())
                {
                    canConsume = true;
                }
                else
                {
                    reasonCantConsume = "You're not hungry!";
                }
                break;

            case Item.ItemType.Drink:
                // Drink can only be consumed if thirst is not full
                // Make sure you have a Thirst stat in your Character class
                if (characterStats.Thirst != null && !characterStats.Thirst.IsFull())
                {
                    canConsume = true;
                }
                else
                {
                    reasonCantConsume = "You're not thirsty!";
                }
                break;

            case Item.ItemType.Item:
                // Generic items can be consumed if they restore any stat that isn't full
                bool canConsumeGeneric = false;

                if (item.HungerRestoreAmount > 0 && characterStats.Hunger != null && !characterStats.Hunger.IsFull())
                {
                    canConsumeGeneric = true;
                }
                else if (item.ThirstRestoreAmount > 0 && characterStats.Thirst != null && !characterStats.Thirst.IsFull())
                {
                    canConsumeGeneric = true;
                }

                canConsume = canConsumeGeneric;
                if (!canConsume)
                {
                    reasonCantConsume = "You don't need this right now!";
                }
                break;

            default:
                reasonCantConsume = "This item cannot be consumed!";
                break;
        }

        if (canConsume)
        {
            ConsumeItem(item);

            // Remove one item from inventory
            currentSlot.count--;
            if (currentSlot.count <= 0)
            {
                currentSlot.Clear();
            }
            inventory.isDirty = true;

            // Play eating animation
            if (animator != null)
            {
                animator.SetTrigger("eat");
            }
        }
        else
        {
            // Optional: Show message to player why they can't consume the item
            Debug.Log(reasonCantConsume);
            // You could also display this message in your UI
        }
    }

    private void ConsumeItem(Item item)
    {
        if (characterStats == null)
        {
            return;
        }

        // Restore hunger
        if (item.HungerRestoreAmount > 0)
        {
            characterStats.AddHunger(item.HungerRestoreAmount);
        }

        if (item.ThirstRestoreAmount > 0)
        {
            characterStats.AddThirst(item.ThirstRestoreAmount);
        }

        // Restore health
        if (item.HealthRestoreAmount > 0)
        {
            characterStats.AddHealth(item.HealthRestoreAmount);
        }

        // Restore happiness
        if (item.HappinessRestoreAmount > 0)
        {
            characterStats.AddHappiness(item.HappinessRestoreAmount);
        }
    }
}