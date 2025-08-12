using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemStorePanel : ItemPanel
{
    [SerializeField] Trading trading;

    // Keep original store behavior - both left and right click do the same thing
    public override void OnLeftClick(int id)
    {
        if (GameManager.instance.dragAndDropController.itemSlot.item == null)
        {
            BuyItem(id);
        }
        else
        {
            SellItem();
        }
        Show();
    }

    public override void OnRightClick(int id)
    {
        // Same behavior as left click for stores to prevent accidental purchases
        if (GameManager.instance.dragAndDropController.itemSlot.item == null)
        {
            BuyItem(id);
        }
        else
        {
            SellItem();
        }
        Show();
    }

    private void BuyItem(int id)
    {
        trading.BuyItem(id);
    }

    private void SellItem()
    {
        trading.SellItem();
    }
}