using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class Bill : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image billIcon;
    [SerializeField] private TextMeshProUGUI billNameText;
    [SerializeField] private TextMeshProUGUI expireDayText;
    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private Button payButton;

    [Header("Bill Data - Set in Inspector")]
    [SerializeField] private string billName = "New Bill";
    [SerializeField] private float amount = 100f;
    [SerializeField] private Sprite icon;
    [SerializeField] private int expireDays = 30; // How many days until expiration

    [Header("Runtime Data")]
    [SerializeField] private int creationDay; // Day when bill was created (set automatically)

    private BillPanel billPanel;
    private bool isPaid = false;

    public string BillName => billName;
    public float Amount => amount;
    public bool IsPaid => isPaid;
    public int ExpireDays => expireDays;
    public int CreationDay => creationDay;

    private void Start()
    {
        // Find the bill panel reference
        billPanel = GetComponentInParent<BillPanel>();

        // Setup pay button
        if (payButton != null)
        {
            payButton.onClick.AddListener(OnPayButtonClicked);
        }

        // Set creation day to current day when bill starts
        if (DayTime.Instance != null)
        {
            creationDay = DayTime.Instance.days;
        }

        UpdateUI();
    }

    private void Update()
    {
        UpdateExpireText();
    }

    // Optional: Initialize bill data programmatically (if needed)
    public void Initialize(string name, float cost, Sprite billIcon, int expireInDays)
    {
        billName = name;
        amount = cost;
        icon = billIcon;
        expireDays = expireInDays;

        if (DayTime.Instance != null)
        {
            creationDay = DayTime.Instance.days;
        }

        isPaid = false;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (billNameText != null)
            billNameText.text = billName;

        if (amountText != null)
            amountText.text = "$" + amount.ToString("F2");

        if (billIcon != null && icon != null)
            billIcon.sprite = icon;

        UpdateExpireText();
        UpdatePayButton();
    }

    private void UpdateExpireText()
    {
        if (expireDayText == null || DayTime.Instance == null) return;

        int currentDay = DayTime.Instance.days;
        int daysPassed = currentDay - creationDay;
        int daysRemaining = expireDays - daysPassed;

        if (isPaid)
        {
            expireDayText.text = "PAID";
            expireDayText.color = Color.green;
        }
        else if (daysRemaining <= 0)
        {
            expireDayText.text = "EXPIRED";
            expireDayText.color = Color.red;
        }
        else
        {
            expireDayText.text = $"Expire: {daysRemaining} days";
            expireDayText.color = daysRemaining <= 3 ? Color.yellow : Color.white;
        }
    }

    private void UpdatePayButton()
    {
        if (payButton == null) return;

        if (isPaid)
        {
            payButton.interactable = false;
            payButton.GetComponentInChildren<TextMeshProUGUI>().text = "PAID";
        }
        else if (IsExpired())
        {
            payButton.interactable = false;
            payButton.GetComponentInChildren<TextMeshProUGUI>().text = "EXPIRED";
        }
        else
        {
            payButton.interactable = true;
            payButton.GetComponentInChildren<TextMeshProUGUI>().text = "PAY";
        }
    }

    private void OnPayButtonClicked()
    {
        if (isPaid || IsExpired()) return;

        if (billPanel != null)
        {
            billPanel.ProcessPayment(this);
        }
    }

    public bool IsExpired()
    {
        if (DayTime.Instance == null) return false;

        int currentDay = DayTime.Instance.days;
        int daysPassed = currentDay - creationDay;
        return daysPassed >= expireDays && !isPaid;
    }

    public void MarkAsPaid()
    {
        isPaid = true;
        UpdateUI();
    }

    // For saving/loading
    [System.Serializable]
    public class BillData
    {
        public string billName;
        public float amount;
        public int creationDay;
        public int expireDays;
        public bool isPaid;

        public BillData(Bill bill)
        {
            billName = bill.billName;
            amount = bill.amount;
            creationDay = bill.creationDay;
            expireDays = bill.expireDays;
            isPaid = bill.isPaid;
        }
    }

    public BillData GetBillData()
    {
        return new BillData(this);
    }

    public void LoadFromData(BillData data)
    {
        billName = data.billName;
        amount = data.amount;
        creationDay = data.creationDay;
        expireDays = data.expireDays;
        isPaid = data.isPaid;

        UpdateUI();
    }
}