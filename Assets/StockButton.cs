using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StockButton : InventoryButton
{
    [Header("Stock-Specific UI References")]
    [SerializeField] Text itemNameText;           // Assign to ItemName in inspector
    [SerializeField] Text itemPriceText;          // Assign to ItemPrice in inspector  
    [SerializeField] Text priceChangeText;        // Assign to PriceChangePercentage in inspector
    [SerializeField] Image priceChangeIcon;       // Assign to PriceChangeIcon in inspector

    [Header("Stock Colors")]
    [SerializeField] Color positiveColor = Color.green;
    [SerializeField] Color negativeColor = Color.red;
    [SerializeField] Color neutralColor = Color.white;

    [Header("Price Change Icons")]
    [SerializeField] Sprite upArrowSprite;        // Assign an up arrow sprite
    [SerializeField] Sprite downArrowSprite;      // Assign a down arrow sprite
    [SerializeField] Sprite neutralSprite;        // Assign a neutral/flat sprite

    private Stock currentStock; // Keep reference to current stock

    private void OnEnable()
    {
        // Subscribe to price change events when the button is enabled
        if (currentStock != null)
        {
            currentStock.OnPriceChanged += OnStockPriceChanged;
        }
    }

    private void OnDisable()
    {
        // Unsubscribe from price change events when the button is disabled
        if (currentStock != null)
        {
            currentStock.OnPriceChanged -= OnStockPriceChanged;
        }
    }

    // Event handler for stock price changes
    private void OnStockPriceChanged(Stock stock)
    {
        if (stock == currentStock)
        {
            UpdateStockDisplay();
        }
    }

    // Override the Set method to add stock-specific functionality
    public override void Set(ItemSlot slot)
    {
        base.Set(slot); // Call the parent Set method first (handles icon and count)

        // Unsubscribe from previous stock if any
        if (currentStock != null)
        {
            currentStock.OnPriceChanged -= OnStockPriceChanged;
        }

        if (slot.item is Stock stock)
        {
            currentStock = stock; // Store reference for event subscription

            // Subscribe to the new stock's price change events
            currentStock.OnPriceChanged += OnStockPriceChanged;

            UpdateStockDisplay();
        }
        else
        {
            currentStock = null; // Clear reference for non-stock items
            // Hide stock-specific UI elements for non-stock items
            if (itemNameText != null) itemNameText.gameObject.SetActive(false);
            if (itemPriceText != null) itemPriceText.gameObject.SetActive(false);
            if (priceChangeText != null) priceChangeText.gameObject.SetActive(false);
            if (priceChangeIcon != null) priceChangeIcon.gameObject.SetActive(false);
        }
    }

    // Separate method to update stock display
    private void UpdateStockDisplay()
    {
        if (currentStock == null) return;

        // Display stock name (item name)
        if (itemNameText != null)
        {
            itemNameText.gameObject.SetActive(true);
            itemNameText.text = currentStock.name; // Use the item's name from ScriptableObject
        }

        // Display current price
        if (itemPriceText != null)
        {
            itemPriceText.gameObject.SetActive(true);
            itemPriceText.text = $"{currentStock.currentPrice:F2}";
        }

        // Display daily change percentage
        if (priceChangeText != null)
        {
            priceChangeText.gameObject.SetActive(true);
            float changePercent = currentStock.GetDailyChangePercent();
            priceChangeText.text = $"{(changePercent >= 0 ? "+" : "")}{changePercent:F2}%";

            // Color based on change
            if (changePercent > 0)
            {
                priceChangeText.color = positiveColor;
            }
            else if (changePercent < 0)
            {
                priceChangeText.color = negativeColor;
            }
            else
            {
                priceChangeText.color = neutralColor;
            }
        }

        // Price change indicator icon
        if (priceChangeIcon != null)
        {
            priceChangeIcon.gameObject.SetActive(true);
            float changePercent = currentStock.GetDailyChangePercent();

            if (changePercent > 0)
            {
                priceChangeIcon.sprite = upArrowSprite;
            }
            else if (changePercent < 0)
            {
                priceChangeIcon.sprite = downArrowSprite;
            }
            else
            {
                priceChangeIcon.sprite = neutralSprite;
            }
        }
    }

    // Override Clean method to clean stock-specific elements
    public override void Clean()
    {
        base.Clean(); // Call parent Clean method

        // Unsubscribe from events before cleaning
        if (currentStock != null)
        {
            currentStock.OnPriceChanged -= OnStockPriceChanged;
        }

        currentStock = null; // Clear the stock reference

        if (itemNameText != null) itemNameText.gameObject.SetActive(false);
        if (itemPriceText != null) itemPriceText.gameObject.SetActive(false);
        if (priceChangeText != null) priceChangeText.gameObject.SetActive(false);
        if (priceChangeIcon != null) priceChangeIcon.gameObject.SetActive(false);
    }
}