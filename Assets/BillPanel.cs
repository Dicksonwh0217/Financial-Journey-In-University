using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

[System.Serializable]
public class ScheduledBill
{
    public string billName;
    public float amount;
    public Sprite icon;
    public int expireDays = 15;
}

public class BillPanel : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform billContainer; // Parent transform for bill items
    [SerializeField] private GameObject billPrefab;
    [SerializeField] private TextMeshProUGUI payStatusText;
    [SerializeField] private Button closePanelButton;

    [Header("Pre-configured Bills - Set in Inspector")]
    [SerializeField] private List<GameObject> predefinedBills = new List<GameObject>();

    [Header("Fixed Expense Bills (Days 1, 31, 61)")]
    [SerializeField]
    private ScheduledBill[] fixedExpenseBills = new ScheduledBill[]
    {
        new ScheduledBill { billName = "Rental", amount = 300f, expireDays = 15 },
        new ScheduledBill { billName = "Utility", amount = 50f, expireDays = 15 },
        new ScheduledBill { billName = "Phone Bill", amount = 35f, expireDays = 15 }
    };

    [Header("University Bills")]
    [SerializeField]
    private ScheduledBill[] universityBills = new ScheduledBill[]
    {
        new ScheduledBill { billName = "University Tuition Fee - Semester 1", amount = 2000f, expireDays = 30 },
        new ScheduledBill { billName = "University Tuition Fee - Semester 2", amount = 2000f, expireDays = 30 }
    };

    [Header("Bill Generation Settings")]
    [SerializeField] private int[] fixedExpenseDays = { 1, 31, 61 };
    [SerializeField] private int[] universityBillDays = { 25, 55 };

    [Header("Settings")]
    [SerializeField] private float statusDisplayTime = 2f;

    private List<Bill> activeBills = new List<Bill>();
    private Currency currencySystem;
    private Coroutine statusCoroutine;
    private HashSet<string> generatedBills; // Track which bills have been generated
    private int lastCheckedDay = -1;

    // Singleton pattern
    public static BillPanel Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            generatedBills = new HashSet<string>();
            LoadGeneratedBills();
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

        // Check for bills to generate
        CheckForScheduledBills();
    }

    private void Update()
    {
        // Check for new bills to generate
        CheckForScheduledBills();

        // Clean up expired bills periodically
        CleanupExpiredBills();
    }

    private void CheckForScheduledBills()
    {
        if (DayTime.Instance == null) return;

        int currentDay = DayTime.Instance.days + 1; // Add 1 because days is 0-indexed

        // Only check if we haven't checked this day yet
        if (currentDay != lastCheckedDay)
        {
            lastCheckedDay = currentDay;

            // Check for fixed expense bills (days 1, 31, 61)
            foreach (int day in fixedExpenseDays)
            {
                if (currentDay == day)
                {
                    GenerateFixedExpenseBills(day);
                    break;
                }
            }

            // Check for university bills (days 25, 55)
            for (int i = 0; i < universityBillDays.Length; i++)
            {
                if (currentDay == universityBillDays[i])
                {
                    GenerateUniversityBill(currentDay, i);
                    break;
                }
            }
        }
    }

    private void GenerateFixedExpenseBills(int day)
    {
        foreach (ScheduledBill scheduledBill in fixedExpenseBills)
        {
            string billKey = $"{scheduledBill.billName}_Day{day}";

            if (!generatedBills.Contains(billKey))
            {
                CreateBill(scheduledBill.billName, scheduledBill.amount, scheduledBill.icon, scheduledBill.expireDays);
                generatedBills.Add(billKey);
                SaveGeneratedBills();

                Debug.Log($"Generated fixed expense bill: {scheduledBill.billName} - ${scheduledBill.amount} on Day {day}");
            }
        }
    }

    private void GenerateUniversityBill(int day, int semesterIndex)
    {
        if (semesterIndex < universityBills.Length)
        {
            ScheduledBill universityBill = universityBills[semesterIndex];
            string billKey = $"{universityBill.billName}_Day{day}";

            if (!generatedBills.Contains(billKey))
            {
                CreateBill(universityBill.billName, universityBill.amount, universityBill.icon, universityBill.expireDays);
                generatedBills.Add(billKey);
                SaveGeneratedBills();

                Debug.Log($"Generated university bill: {universityBill.billName} - ${universityBill.amount} on Day {day}");
            }
        }
    }

    private void CreateBill(string billName, float amount, Sprite icon, int expireDays)
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
            newBill.Initialize(billName, amount, icon, expireDays);
            activeBills.Add(newBill);
        }
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
        CreateBill(billName, amount, icon, expireInDays);
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

    // Save and load generated bills tracking
    private void SaveGeneratedBills()
    {
        string generatedBillsString = string.Join(",", generatedBills);
        PlayerPrefs.SetString("GeneratedBills", generatedBillsString);
        PlayerPrefs.Save();
    }

    private void LoadGeneratedBills()
    {
        string generatedBillsString = PlayerPrefs.GetString("GeneratedBills", "");
        generatedBills.Clear();

        if (!string.IsNullOrEmpty(generatedBillsString))
        {
            string[] billKeys = generatedBillsString.Split(',');
            foreach (string billKey in billKeys)
            {
                if (!string.IsNullOrEmpty(billKey))
                {
                    generatedBills.Add(billKey);
                }
            }
        }
    }

    // Reset bill generation for new games
    public void ResetBillGeneration()
    {
        generatedBills.Clear();
        SaveGeneratedBills();
        lastCheckedDay = -1;
        Debug.Log("Bill generation reset for new game!");
    }

    // Optional: Method to add random bills (can be called from other systems)
    public void GenerateRandomBill()
    {
        string[] billNames = { "Insurance", "Car Maintenance", "Internet Bill", "Water Bill" };
        float[] amounts = { 120f, 200f, 60f, 25f };

        int randomIndex = UnityEngine.Random.Range(0, billNames.Length);
        int randomExpireDays = UnityEngine.Random.Range(10, 20);

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

    public bool HasCriticalExpiredBills(int gracePeriodDays = 7)
    {
        int currentDay = DayTime.Instance != null ? DayTime.Instance.days : 0;

        foreach (Bill bill in activeBills)
        {
            if (bill != null && !bill.IsPaid)
            {
                int expirationDay = bill.CreationDay + bill.ExpireDays;
                int daysPastExpiration = currentDay - expirationDay;

                if (daysPastExpiration > gracePeriodDays)
                {
                    return true;
                }
            }
        }
        return false;
    }

    // Context menu methods for testing
    [ContextMenu("Force Generate Day 1 Bills")]
    public void ForceGenerateDay1Bills()
    {
        GenerateFixedExpenseBills(1);
    }

    [ContextMenu("Force Generate Day 25 University Bill")]
    public void ForceGenerateDay25UniversityBill()
    {
        GenerateUniversityBill(25, 0);
    }

    [ContextMenu("Force Generate Day 31 Bills")]
    public void ForceGenerateDay31Bills()
    {
        GenerateFixedExpenseBills(31);
    }

    [ContextMenu("Force Generate Day 55 University Bill")]
    public void ForceGenerateDay55UniversityBill()
    {
        GenerateUniversityBill(55, 1);
    }

    [ContextMenu("Force Generate Day 61 Bills")]
    public void ForceGenerateDay61Bills()
    {
        GenerateFixedExpenseBills(61);
    }

    [ContextMenu("Show Bill Generation Status")]
    public void ShowBillGenerationStatus()
    {
        int currentDay = DayTime.Instance != null ? DayTime.Instance.days + 1 : 0;
        Debug.Log($"=== BILL GENERATION STATUS ===");
        Debug.Log($"Current Day: {currentDay}");
        Debug.Log($"Last Checked Day: {lastCheckedDay}");
        Debug.Log($"Generated Bills: [{string.Join(", ", generatedBills)}]");
        Debug.Log($"Active Bills Count: {activeBills.Count}");
        Debug.Log($"Unpaid Bills Count: {GetUnpaidBillCount()}");
        Debug.Log($"Total Unpaid Amount: ${GetTotalUnpaidAmount():F2}");
    }

    [ContextMenu("Reset Bill Generation (Testing)")]
    public void ResetBillGenerationFromMenu()
    {
        ResetBillGeneration();
    }

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
}