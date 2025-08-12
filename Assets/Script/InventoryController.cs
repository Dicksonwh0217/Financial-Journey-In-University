using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryController : MonoBehaviour
{
    [SerializeField] GameObject panel;
    [SerializeField] GameObject toolbarPanel;
    [SerializeField] GameObject storePanel;
    [SerializeField] GameObject itemDetailPanel;

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
}