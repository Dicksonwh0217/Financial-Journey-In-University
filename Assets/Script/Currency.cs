using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Currency : MonoBehaviour
{
    [SerializeField] float amount;
    [SerializeField] TMPro.TextMeshProUGUI InventoryText;

    private void Start()
    {
        amount = 1000f;
        UpdateText();
    }

    private void UpdateText()
    {
        InventoryText.text = amount.ToString("F2"); // Format to 2 decimal places
    }

    internal void Add(float moneyGain)
    {
        amount += moneyGain;
        UpdateText();
    }

    internal bool Check(float totalPrice)
    {
        return amount >= totalPrice;
    }

    internal void Decrease(float totalPrice)
    {
        amount -= totalPrice;
        if (amount < 0)
        {
            amount = 0;
        }
        UpdateText();
    }

    // Additional getter for current amount
    public float GetAmount()
    {
        return amount;
    }

    // Method to set amount directly (useful for testing or loading save data)
    public void SetAmount(float newAmount)
    {
        amount = newAmount;
        if (amount < 0)
        {
            amount = 0;
        }
        UpdateText();
    }

}
