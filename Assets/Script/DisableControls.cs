using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableControls : MonoBehaviour
{
    CharacterController2D characterController;
    ToolsCharacterController toolsCharacter;
    InventoryController inventoryController;
    ToolbarController toolbarController;
    ItemContainerInteractController itemContainerInteractController;


    private void Awake()
    {
        characterController = GetComponent<CharacterController2D>();
        toolsCharacter = GetComponent<ToolsCharacterController>();
        inventoryController = GetComponent<InventoryController>();
        toolbarController = GetComponent<ToolbarController>();
        itemContainerInteractController = GetComponent<ItemContainerInteractController>();
    }

    public void DisableControl()
    {
        characterController.enabled = false;
        toolsCharacter.enabled = false;
        inventoryController.enabled = false;
        toolbarController.enabled = false;
        itemContainerInteractController.enabled = false;
    }

    public void EnableControl()
    {
        characterController.enabled = true;
        toolsCharacter.enabled = true;
        inventoryController.enabled = true;
        toolbarController.enabled = true;
        itemContainerInteractController.enabled = true;
    }
}
