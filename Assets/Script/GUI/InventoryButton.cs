using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class InventoryButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] Image icon;
    [SerializeField] Text text;
    [SerializeField] Image highlight;
    int myIndex;
    ItemPanel itemPanel;
    ItemSlot currentSlot;
    private ItemDetailPanel itemDetailPanel;

    public void SetIndex(int index)
    {
        myIndex = index;
    }

    public void SetItemPanel(ItemPanel source)
    {
        itemPanel = source;
    }

    public void SetDetailPanel(ItemDetailPanel panel)
    {
        itemDetailPanel = panel;
    }

    public virtual void Set(ItemSlot slot)
    {
        currentSlot = slot;
        icon.gameObject.SetActive(true);
        icon.sprite = slot.item.icon;
        if (slot.item.stackable == true)
        {
            text.gameObject.SetActive(true);
            text.text = slot.count.ToString();
        }
        else
        {
            text.gameObject.SetActive(false);
        }
    }

    public virtual void Clean()
    {
        currentSlot = null;
        icon.sprite = null;
        icon.gameObject.SetActive(false);
        text.gameObject.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Check if it's left click (0) or right click (1)
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            // Left click behavior
            itemPanel.OnLeftClick(myIndex);
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            // Right click behavior
            itemPanel.OnRightClick(myIndex);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (itemPanel != null && itemPanel.ShowItemDetails && currentSlot != null && currentSlot.item != null)
        {
            itemDetailPanel.ShowItemDetails(currentSlot.item, Input.mousePosition);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (itemDetailPanel != null)
        {
            itemDetailPanel.Hide();
        }
    }

    public void Highlight(bool b)
    {
        highlight.gameObject.SetActive(b);
    }
}