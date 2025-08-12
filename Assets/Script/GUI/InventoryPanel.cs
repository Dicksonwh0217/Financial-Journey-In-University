using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryPanel : ItemPanel
{
    private RectTransform rectTransform;
    private float normalXPosition = 0f;
    private float normalYPosition = 0f;
    private float twoPanelYPosition = -160f;
    private float StockPanelXPosition = 170f;
    private float StockPanelYPosition = -190f;


    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public override void OnClick(int id)
    {
        GameManager.instance.dragAndDropController.OnClick(inventory.slots[id]);
        Show();
    }

    public void SetNormalPosition()
    {
        if (rectTransform != null)
        {
            Vector2 currentPosition = rectTransform.anchoredPosition;
            rectTransform.anchoredPosition = new Vector2(normalXPosition, normalYPosition);
        }
    }

    public void SetTwoPanelPosition()
    {
        if (rectTransform != null)
        {
            Vector2 currentPosition = rectTransform.anchoredPosition;
            rectTransform.anchoredPosition = new Vector2(normalXPosition, twoPanelYPosition);
        }
    }

    public void SetStockPanelPosition()
    {
        if (rectTransform != null)
        {
            Vector2 currentPosition = rectTransform.anchoredPosition;
            rectTransform.anchoredPosition = new Vector2(StockPanelXPosition, StockPanelYPosition);
        }
    }
}
