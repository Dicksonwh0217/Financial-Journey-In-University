using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StockTrading : MonoBehaviour
{
    [SerializeField] GameObject stockMarketPanel;
    [SerializeField] GameObject inventoryPanel;
    [SerializeField] ItemToolbarPanel toolbarPanel;
    [SerializeField] StockAgentDialogue agentDialogue; // Add this reference

    StockMarket stockMarket;
    Currency money;
    ItemStockPanel itemStockPanel;
    [SerializeField] ItemContainer playerInventory;
    [SerializeField] ItemContainer playerStockPortfolio; // Separate container for owned stocks
    [SerializeField] ItemPanel inventoryItemPanel;

    private void Awake()
    {
        money = GetComponent<Currency>();
        itemStockPanel = stockMarketPanel.GetComponent<ItemStockPanel>();

        // If agentDialogue is not assigned, try to find it in the stockMarketPanel
        if (agentDialogue == null)
        {
            agentDialogue = stockMarketPanel.GetComponentInChildren<StockAgentDialogue>();
        }
    }

    public void BeginStockTrading(StockMarket market)
    {
        this.stockMarket = market;
        itemStockPanel.SetInventory(market.stockInventory);
        stockMarketPanel.SetActive(true);
        inventoryPanel.SetActive(true);

        // Set inventory panel position for trading
        InventoryPanel invPanel = inventoryPanel.GetComponent<InventoryPanel>();
        if (invPanel != null)
        {
            invPanel.SetStockPanelPosition();
        }

        // Show greeting message and start idle messages
        if (agentDialogue != null)
        {
            agentDialogue.ShowGreeting();

            // Start idle messages after a delay
            StartCoroutine(StartIdleMessagesAfterDelay(5f));
        }
    }

    public void StopStockTrading()
    {
        // Show farewell message before closing
        if (agentDialogue != null)
        {
            agentDialogue.ShowFarewell();
            agentDialogue.StopIdleMessages();
        }

        // Small delay before closing to show farewell message
        StartCoroutine(CloseAfterDelay(1.5f));
    }

    private IEnumerator CloseAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        stockMarket = null;
        stockMarketPanel.SetActive(false);
        inventoryPanel.SetActive(false);

        // Reset inventory panel position when trading ends
        InventoryPanel invPanel = inventoryPanel.GetComponent<InventoryPanel>();
        if (invPanel != null)
        {
            invPanel.SetNormalPosition();
        }
    }

    private IEnumerator StartIdleMessagesAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (agentDialogue != null)
        {
            agentDialogue.StartIdleMessages();
        }
    }

    public void BuyStock(int id)
    {
        if (stockMarket == null || !stockMarket.IsMarketOpen())
        {
            if (agentDialogue != null)
            {
                agentDialogue.ShowMarketClosed();
            }
            return;
        }

        ItemSlot stockSlot = stockMarket.stockInventory.slots[id];
        if (stockSlot.item == null || !(stockSlot.item is Stock stock)) return;

        int stockPrice = Mathf.RoundToInt(stock.currentPrice);

        if (money.Check(stockPrice))
        {
            money.Decrease(stockPrice);

            // Add stock to player's portfolio
            if (playerStockPortfolio != null)
            {
                playerStockPortfolio.Add(stock, 1);
            }
            else
            {
                playerInventory.Add(stock, 1);
            }

            inventoryItemPanel.Show();

            // Update toolbar when stocks are bought
            if (toolbarPanel != null)
            {
                toolbarPanel.Show();
            }

            // Show success message
            if (agentDialogue != null)
            {
                agentDialogue.ShowBuySuccess(stock.name);
            }

            Debug.Log($"Bought 1 share of {stock.stockSymbol} for RM{stockPrice}");
        }
        else
        {
            // Show failure message
            if (agentDialogue != null)
            {
                agentDialogue.ShowBuyFail();
            }

            Debug.Log("Not enough money to buy this stock!");
        }
    }

    public void SellStock()
    {
        if (stockMarket == null || !stockMarket.IsMarketOpen())
        {
            if (agentDialogue != null)
            {
                agentDialogue.ShowMarketClosed();
            }
            return;
        }

        ItemDragAndDropController dragController = GameManager.instance.dragAndDropController;
        if (dragController.itemSlot.item == null || !(dragController.itemSlot.item is Stock stock))
        {
            if (agentDialogue != null)
            {
                agentDialogue.ShowSellFail();
            }
            Debug.Log("No stock selected to sell!");
            return;
        }

        int sellPrice = Mathf.RoundToInt(stock.currentPrice);

        // Calculate money gain
        int moneyGain = stock.stackable ?
            sellPrice * dragController.itemSlot.count :
            sellPrice;

        money.Add(moneyGain);

        // Show success message before clearing the slot
        if (agentDialogue != null)
        {
            agentDialogue.ShowSellSuccess(stock.name, moneyGain);
        }

        dragController.itemSlot.Clear();
        dragController.UpdateIcon();

        // Update both inventory and toolbar when stocks are sold
        inventoryItemPanel.Show();
        if (toolbarPanel != null)
        {
            toolbarPanel.Show();
        }

        Debug.Log($"Sold stock for RM{moneyGain}");
    }

    public void BuyStockPartial(int id)
    {
        // For stocks, we typically buy 1 share at a time
        BuyStock(id);
    }

    public void SellStockPartial()
    {
        if (stockMarket == null || !stockMarket.IsMarketOpen())
        {
            if (agentDialogue != null)
            {
                agentDialogue.ShowMarketClosed();
            }
            return;
        }

        ItemDragAndDropController dragController = GameManager.instance.dragAndDropController;
        if (dragController.itemSlot.item == null || !(dragController.itemSlot.item is Stock stock))
        {
            if (agentDialogue != null)
            {
                agentDialogue.ShowSellFail();
            }
            return;
        }

        int sellPrice = Mathf.RoundToInt(stock.currentPrice);
        money.Add(sellPrice);

        // Show success message for partial sell
        if (agentDialogue != null)
        {
            agentDialogue.ShowSellSuccess(stock.name, sellPrice);
        }

        // Remove one stock from held stack
        dragController.itemSlot.count -= 1;
        if (dragController.itemSlot.count <= 0)
        {
            dragController.itemSlot.Clear();
        }

        dragController.UpdateIcon();

        // Update both inventory and toolbar when stocks are sold
        inventoryItemPanel.Show();
        if (toolbarPanel != null)
        {
            toolbarPanel.Show();
        }

        Debug.Log($"Sold 1 share for RM{sellPrice}");
    }

    public float GetPortfolioValue()
    {
        float totalValue = 0f;
        ItemContainer portfolio = playerStockPortfolio != null ? playerStockPortfolio : playerInventory;

        foreach (var slot in portfolio.slots)
        {
            if (slot.item != null && slot.item is Stock stock)
            {
                totalValue += stock.currentPrice * slot.count;
            }
        }

        return totalValue;
    }
}