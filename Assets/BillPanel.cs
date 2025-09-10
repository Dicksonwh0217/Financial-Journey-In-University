using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BillPanel : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform billContainer; // Parent transform for bill items
    [SerializeField] private GameObject billPrefab;
    [SerializeField] private TextMeshProUGUI payStatusText;
    [SerializeField] private Button closePanelButton;

    [Header("Pre-configured Bills - Set in Inspector")]
    [SerializeField] private List<GameObject> predefinedBills = new List<GameObject>();

    [Header("Settings")]
    [SerializeField] private float statusDisplayTime = 2f;

    private List<Bill> activeBills = new List<Bill>();
    private Currency currencySystem;
    private Coroutine statusCoroutine;

    // Singleton pattern
    public static BillPanel Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // Find currency system
        currencySystem = FindFirstObjectByType<Currency>();

        // Setup close button
        if (closePanelButton != null)
        {
            closePanelButton.onClick.AddListener(CloseBillPanel);
        }

        // Hide status text initially
        if (payStatusText != null)
        {
            payStatusText.gameObject.SetActive(false);
        }

        // Initialize predefined bills from inspector
        InitializePredefinedBills();
    }

    private void Update()
    {
        // Clean up expired bills periodically
        CleanupExpiredBills();
    }

    public void ToggleBillPanel()
    {
        bool isActive = gameObject.activeSelf;
        gameObject.SetActive(!isActive);

        if (!isActive)
        {
            RefreshBillDisplay();
        }
    }

    public void OpenBillPanel()
    {
        gameObject.SetActive(true);
        RefreshBillDisplay();
    }

    public void CloseBillPanel()
    {
        gameObject.SetActive(false);
    }

    public void ProcessPayment(Bill bill)
    {
        if (currencySystem == null)
        {
            ShowPaymentStatus("Currency system not found!", false);
            return;
        }

        if (bill.IsPaid)
        {
            ShowPaymentStatus("Bill already paid!", false);
            return;
        }

        if (bill.IsExpired())
        {
            ShowPaymentStatus("Bill has expired!", false);
            return;
        }

        // Check if player has enough money
        if (currencySystem.Check(bill.Amount))
        {
            // Deduct money and mark bill as paid
            currencySystem.Decrease(bill.Amount);
            bill.MarkAsPaid();

            ShowPaymentStatus($"Payment successful! Paid ${bill.Amount:F2}", true);
            Debug.Log($"Paid bill: {bill.BillName} - ${bill.Amount:F2}");
        }
        else
        {
            ShowPaymentStatus("Insufficient funds!", false);
            Debug.Log($"Failed to pay bill: {bill.BillName} - Not enough money");
        }
    }

    private void ShowPaymentStatus(string message, bool isSuccess)
    {
        if (payStatusText == null) return;

        // Stop previous coroutine if running
        if (statusCoroutine != null)
        {
            StopCoroutine(statusCoroutine);
        }

        // Set text and color
        payStatusText.text = message;
        payStatusText.color = isSuccess ? Color.green : Color.red;
        payStatusText.gameObject.SetActive(true);

        // Hide after delay
        statusCoroutine = StartCoroutine(HideStatusAfterDelay());
    }

    private IEnumerator HideStatusAfterDelay()
    {
        yield return new WaitForSeconds(statusDisplayTime);

        if (payStatusText != null)
        {
            payStatusText.gameObject.SetActive(false);
        }

        statusCoroutine = null;
    }

    // Method to add new bills at runtime (optional - for dynamic bill creation)
    public void AddBill(string billName, float amount, Sprite icon, int expireInDays)
    {
        if (billPrefab == null || billContainer == null)
        {
            Debug.LogError("Bill prefab or container not assigned!");
            return;
        }

        // Create new bill
        GameObject newBillObj = Instantiate(billPrefab, billContainer);
        Bill newBill = newBillObj.GetComponent<Bill>();

        if (newBill != null)
        {
            newBill.Initialize(billName, amount, icon, expireInDays);
            activeBills.Add(newBill);
            Debug.Log($"Added new bill: {billName} - ${amount:F2} (expires in {expireInDays} days)");
        }
    }

    private void RefreshBillDisplay()
    {
        // Remove any null references
        activeBills.RemoveAll(bill => bill == null);

        // Update all bill displays
        foreach (Bill bill in activeBills)
        {
            // Bills will update themselves in their Update method
        }
    }

    private void CleanupExpiredBills()
    {
        // Remove bills that have been expired for too long (e.g., 7 days after expiration)
        for (int i = activeBills.Count - 1; i >= 0; i--)
        {
            Bill bill = activeBills[i];
            if (bill != null && bill.IsExpired())
            {
                int currentDay = DayTime.Instance != null ? DayTime.Instance.days : 0;
                int daysPastExpiration = currentDay - (bill.CreationDay + bill.ExpireDays);

                // Remove bills that expired more than 7 days ago
                if (daysPastExpiration > 7)
                {
                    activeBills.RemoveAt(i);
                    if (bill.gameObject != null)
                    {
                        Destroy(bill.gameObject);
                    }
                }
            }
        }
    }

    // Initialize bills that are set up in the inspector
    private void InitializePredefinedBills()
    {
        foreach (GameObject billObj in predefinedBills)
        {
            if (billObj != null)
            {
                Bill bill = billObj.GetComponent<Bill>();
                if (bill != null)
                {
                    activeBills.Add(bill);
                }
            }
        }
    }

    // Optional: Method to add random bills (can be called from other systems)
    public void GenerateRandomBill()
    {
        string[] billNames = { "Phone Bill", "Gas Bill", "Cable TV", "Gym Membership", "Car Payment" };
        float[] amounts = { 45f, 75f, 85f, 30f, 350f };

        int randomIndex = Random.Range(0, billNames.Length);
        int randomExpireDays = Random.Range(15, 45);

        AddBill(billNames[randomIndex], amounts[randomIndex], null, randomExpireDays);
    }

    // Public methods for external access
    public int GetActiveBillCount()
    {
        return activeBills.Count;
    }

    public int GetUnpaidBillCount()
    {
        int unpaidCount = 0;
        foreach (Bill bill in activeBills)
        {
            if (bill != null && !bill.IsPaid && !bill.IsExpired())
            {
                unpaidCount++;
            }
        }
        return unpaidCount;
    }

    public float GetTotalUnpaidAmount()
    {
        float totalAmount = 0f;
        foreach (Bill bill in activeBills)
        {
            if (bill != null && !bill.IsPaid && !bill.IsExpired())
            {
                totalAmount += bill.Amount;
            }
        }
        return totalAmount;
    }

    // Context menu for testing
    [ContextMenu("Add Random Bill")]
    public void AddRandomBillFromMenu()
    {
        GenerateRandomBill();
    }

    [ContextMenu("Refresh Bills")]
    public void RefreshBillsFromMenu()
    {
        RefreshBillDisplay();
    }

    public bool HasExpiredBills()
    {
        foreach (Bill bill in activeBills)
        {
            if (bill != null && bill.IsExpired() && !bill.IsPaid)
            {
                return true;
            }
        }
        return false;
    }

    // Get count of expired bills
    public int GetExpiredBillCount()
    {
        int expiredCount = 0;
        foreach (Bill bill in activeBills)
        {
            if (bill != null && bill.IsExpired() && !bill.IsPaid)
            {
                expiredCount++;
            }
        }
        return expiredCount;
    }

    // Get total amount of expired bills
    public float GetTotalExpiredAmount()
    {
        float totalAmount = 0f;
        foreach (Bill bill in activeBills)
        {
            if (bill != null && bill.IsExpired() && !bill.IsPaid)
            {
                totalAmount += bill.Amount;
            }
        }
        return totalAmount;
    }

    // Method to check if player has any critical expired bills (for ending conditions)
    public bool HasCriticalExpiredBills(int gracePeriodDays = 7)
    {
        int currentDay = DayTime.Instance != null ? DayTime.Instance.days : 0;

        foreach (Bill bill in activeBills)
        {
            if (bill != null && !bill.IsPaid)
            {
                int expirationDay = bill.CreationDay + bill.ExpireDays;
                int daysPastExpiration = currentDay - expirationDay;

                // If bill expired more than grace period days ago, it's critical
                if (daysPastExpiration > gracePeriodDays)
                {
                    return true;
                }
            }
        }
        return false;
    }

    // Debug method to log bill status
    [ContextMenu("Debug Bill Status")]
    public void DebugBillStatus()
    {
        Debug.Log($"=== BILL STATUS DEBUG ===");
        Debug.Log($"Active Bills: {activeBills.Count}");
        Debug.Log($"Unpaid Bills: {GetUnpaidBillCount()}");
        Debug.Log($"Expired Bills: {GetExpiredBillCount()}");
        Debug.Log($"Has Critical Expired Bills: {HasCriticalExpiredBills()}");
        Debug.Log($"Total Unpaid Amount: ${GetTotalUnpaidAmount():F2}");
        Debug.Log($"Total Expired Amount: ${GetTotalExpiredAmount():F2}");

        foreach (Bill bill in activeBills)
        {
            if (bill != null)
            {
                string status = bill.IsPaid ? "PAID" : (bill.IsExpired() ? "EXPIRED" : "ACTIVE");
                Debug.Log($"- {bill.BillName}: ${bill.Amount:F2} [{status}]");
            }
        }
    }

    // Method to reset all bills (useful for testing)
    [ContextMenu("Clear All Bills")]
    public void ClearAllBills()
    {
        for (int i = activeBills.Count - 1; i >= 0; i--)
        {
            if (activeBills[i] != null && activeBills[i].gameObject != null)
            {
                Destroy(activeBills[i].gameObject);
            }
        }
        activeBills.Clear();
        Debug.Log("All bills cleared");
    }
}