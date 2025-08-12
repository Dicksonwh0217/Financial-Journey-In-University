using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemContainerInteractController : MonoBehaviour
{
    ItemContainer targetItemContainer;
    InventoryController inventoryController;
    [SerializeField] ItemContainerPanel itemContainerPanel;
    Transform openedChest;
    [SerializeField] float maxDistance = 2.5f;
    private LootContainerInteract currentLootContainer;

    private void Awake()
    {
        inventoryController = GetComponent<InventoryController>();
    }

    private void Update()
    {
        // Handle ESC key to close container
        if (Input.GetKeyDown(KeyCode.Escape) && IsContainerOpen())
        {
            CloseCurrentContainer();
        }

        // Check distance and auto-close if too far
        if (openedChest != null)
        {
            float distance = Vector2.Distance(openedChest.position, transform.position);
            if (distance > maxDistance)
            {
                openedChest.GetComponent<LootContainerInteract>().Close(GetComponent<Character>());
            }
        }
    }

    public void Open(ItemContainer itemContainer, Transform _openedChest)
    {
        targetItemContainer = itemContainer;
        itemContainerPanel.inventory = targetItemContainer;
        inventoryController.Open();
        itemContainerPanel.gameObject.SetActive(true);
        openedChest = _openedChest;
        currentLootContainer = _openedChest.GetComponent<LootContainerInteract>();

        // Set chest position for inventory panel
        GameObject inventoryPanelObject = inventoryController.GetInventoryPanel();
        if (inventoryPanelObject != null)
        {
            InventoryPanel inventoryPanel = inventoryPanelObject.GetComponent<InventoryPanel>();
            if (inventoryPanel != null)
            {
                inventoryPanel.SetTwoPanelPosition();
            }
        }
    }

    public void Close()
    {
        inventoryController.Close();
        itemContainerPanel.gameObject.SetActive(false);
        openedChest = null;
        currentLootContainer = null;
    }

    public bool IsContainerOpen()
    {
        return currentLootContainer != null && itemContainerPanel.gameObject.activeSelf;
    }

    public void CloseCurrentContainer()
    {
        if (currentLootContainer != null)
        {
            currentLootContainer.Close(GetComponent<Character>());
        }
    }
}