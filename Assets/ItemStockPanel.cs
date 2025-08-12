using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemStockPanel : ItemPanel
{
    [SerializeField] StockTrading stockTrading;

    public override void OnLeftClick(int id)
    {
        if (GameManager.instance.dragAndDropController.itemSlot.item == null)
        {
            BuyStock(id);
        }
        else
        {
            SellStock();
        }
        Show();
    }

    public override void OnRightClick(int id)
    {
        // Right click for partial buying/selling
        if (GameManager.instance.dragAndDropController.itemSlot.item == null)
        {
            BuyStockPartial(id);
        }
        else
        {
            SellStockPartial();
        }
        Show();
    }

    private void BuyStock(int id)
    {
        stockTrading.BuyStock(id);
    }

    private void SellStock()
    {
        stockTrading.SellStock();
    }

    private void BuyStockPartial(int id)
    {
        stockTrading.BuyStockPartial(id);
    }

    private void SellStockPartial()
    {
        stockTrading.SellStockPartial();
    }

}