using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class Trading : MonoBehaviour
{
    [SerializeField] GameObject storePanel;
    [SerializeField] GameObject inventoryPanel;
    [SerializeField] ItemToolbarPanel toolbarPanel; // Add reference to toolbar panel
    Store store;
    Currency money;
    ItemStorePanel itemStorePanel;
    [SerializeField] ItemContainer playerInventory;
    [SerializeField] ItemPanel inventoryItemPanel;
    [SerializeField] AudioClip PurchaseSFX;

    private void Awake()
    {
        money = GetComponent<Currency>();
        itemStorePanel = storePanel.GetComponent<ItemStorePanel>();
    }

    public void BeginTrading(Store store)
    {
        this.store = store;
        itemStorePanel.SetInventory(store.storeContent);
        itemStorePanel.SetStore(store);
        storePanel.SetActive(true);
        inventoryPanel.SetActive(true);

        // Set inventory panel position for trading
        InventoryPanel invPanel = inventoryPanel.GetComponent<InventoryPanel>();
        if (invPanel != null)
        {
            invPanel.SetTwoPanelPosition();
        }
    }

    public void StopTrading()
    {
        store = null;
        storePanel.SetActive(false);
        inventoryPanel.SetActive(false);

        // Reset inventory panel position when trading ends
        InventoryPanel invPanel = inventoryPanel.GetComponent<InventoryPanel>();
        if (invPanel != null)
        {
            invPanel.SetNormalPosition();
        }
    }

    internal void BuyItem(int id)
    {
        if (store?.storeContent?.slots == null || id >= store.storeContent.slots.Count)
        {
            return;
        }

        if (store.storeContent.slots[id].item == null)
        {
            return;
        }

        Item itemToBuy = store.storeContent.slots[id].item;

        int totalPrice = (int)(itemToBuy.price * store.sellToPlayerMultip);
        if (money.Check(totalPrice) == true)
        {
            AudioManager.instance.Play(PurchaseSFX);
            money.Decrease(totalPrice);
            playerInventory.Add(itemToBuy);
            inventoryItemPanel.Show();

            // Update toolbar when items are bought
            if (toolbarPanel != null)
            {
                toolbarPanel.Show();
            }
        }
        else
        {
        }
    }

    public void SellItem()
    {
        if (GameManager.instance.dragAndDropController.CheckForSale() == true)
        {
            ItemSlot itemToSell = GameManager.instance.dragAndDropController.itemSlot;
            int moneyGain = itemToSell.item.stackable == true ?
                (int)(itemToSell.item.price * itemToSell.count * store.buyFromPlayerMultip) :  // total money gain if item is stackable
                (int)(itemToSell.item.price * store.buyFromPlayerMultip); // total money gain if item is not stackable

            money.Add(moneyGain);
            itemToSell.Clear();
            GameManager.instance.dragAndDropController.UpdateIcon();

            // Update both inventory and toolbar when items are sold
            inventoryItemPanel.Show();
            if (toolbarPanel != null)
            {
                toolbarPanel.Show();
            }
        }
    }
}