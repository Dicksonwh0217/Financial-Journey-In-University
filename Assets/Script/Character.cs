using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Stat
{
    public int maxVal;
    public int currVal;

    public Stat(int max, int curr)
    {
        maxVal = max;
        currVal = curr;
    }

    internal void Subtract(int amount)
    {
        currVal -= amount;
        if (currVal < 0)
        {
            currVal = 0; // Prevent negative values
        }
    }

    internal void Addition(int amount)
    {
        currVal += amount;
        if (currVal > maxVal)
        {
            currVal = maxVal;
        }
    }

    internal void SetToMax()
    {
        currVal = maxVal;
    }

    internal bool IsEmpty()
    {
        return currVal <= 0;
    }

    internal bool IsFull()
    {
        return currVal >= maxVal;
    }
}

public class Character : MonoBehaviour
{
    [Header("Character Stats")]
    public Stat Health;
    public Stat Happiness;
    public Stat Hunger;
    public Stat Thirst;

    [Header("Stat Deduction Settings")]
    [SerializeField] private int hungerDeductionAmount = 1;
    [SerializeField] private int thirstDeductionAmount = 1;
    [SerializeField] private int hungerDeductionInterval = 3; // Every 3 time ticks (45 minutes)
    [SerializeField] private int thirstDeductionInterval = 2; // Every 2 time ticks (30 minutes)
    [SerializeField] private int healthDeductionFromHunger = 2;
    [SerializeField] private int healthDeductionFromThirst = 3;

    [Header("UI References")]
    [SerializeField] HappinessBar healthBar;
    [SerializeField] HappinessBar happinessBar;
    [SerializeField] HappinessBar hungerBar;
    [SerializeField] HappinessBar thirstBar;

    [Header("Status")]
    public bool isDead;

    private DisableControls disableControls;
    private TimeAgent timeAgent;
    private int timeTickCounter = 0;

    private void Awake()
    {
        disableControls = GetComponent<DisableControls>();

        // Get or add TimeAgent component
        timeAgent = GetComponent<TimeAgent>();
        if (timeAgent == null)
        {
            timeAgent = gameObject.AddComponent<TimeAgent>();
        }
    }

    private void Start()
    {
        UpdateAllBars();

        // Subscribe to time events
        if (timeAgent != null)
        {
            timeAgent.onTimeTick += OnTimeTick;
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from time events
        if (timeAgent != null)
        {
            timeAgent.onTimeTick -= OnTimeTick;
        }
    }

    private void OnTimeTick(DayTime dayTime)
    {
        if (isDead) return;

        timeTickCounter++;

        // Deduct thirst more frequently than hunger
        if (timeTickCounter % thirstDeductionInterval == 0)
        {
            DeductThirst(thirstDeductionAmount);
        }

        if (timeTickCounter % hungerDeductionInterval == 0)
        {
            DeductHunger(hungerDeductionAmount);
        }

        // Check if hunger or thirst are empty and deduct health
        if (Hunger.IsEmpty())
        {
            DeductHealth(healthDeductionFromHunger);
        }

        if (Thirst.IsEmpty())
        {
            DeductHealth(healthDeductionFromThirst);
        }
    }

    private void Dead()
    {
        if (!isDead)
        {
            isDead = true;
            if (disableControls != null)
            {
                disableControls.DisableControl();
            }
            Debug.Log("Character has died!");
        }
    }

    private void CheckDeath()
    {
        if (Health.IsEmpty() || Happiness.IsEmpty())
        {
            Dead();
        }
    }

    private void UpdateAllBars()
    {
        UpdateHealthBar();
        UpdateHappinessBar();
        UpdateHungerBar();
        UpdateThirstBar();
    }

    private void UpdateHealthBar()
    {
        if (healthBar != null)
            healthBar.Set(Health.currVal, Health.maxVal);
    }

    private void UpdateHappinessBar()
    {
        if (happinessBar != null)
            happinessBar.Set(Happiness.currVal, Happiness.maxVal);
    }

    private void UpdateHungerBar()
    {
        if (hungerBar != null)
            hungerBar.Set(Hunger.currVal, Hunger.maxVal);
    }

    private void UpdateThirstBar()
    {
        if (thirstBar != null)
            thirstBar.Set(Thirst.currVal, Thirst.maxVal);
    }

    // Health Methods
    public void DeductHealth(int amount)
    {
        if (isDead) return;

        Health.Subtract(amount);
        UpdateHealthBar();
        CheckDeath();
    }

    public void AddHealth(int amount)
    {
        if (isDead) return;

        Health.Addition(amount);
        UpdateHealthBar();
    }

    public void FullHealth()
    {
        if (isDead) return;

        Health.SetToMax();
        UpdateHealthBar();
    }

    // Happiness Methods
    public void DeductHappiness(int amount)
    {
        if (isDead) return;

        Happiness.Subtract(amount);
        UpdateHappinessBar();
        CheckDeath();
    }

    public void AddHappiness(int amount)
    {
        if (isDead) return;

        Happiness.Addition(amount);
        UpdateHappinessBar();
    }

    public void FullHappiness()
    {
        if (isDead) return;

        Happiness.SetToMax();
        UpdateHappinessBar();
    }

    // Hunger Methods
    public void DeductHunger(int amount)
    {
        if (isDead) return;

        Hunger.Subtract(amount);
        UpdateHungerBar();
    }

    public void AddHunger(int amount)
    {
        if (isDead) return;

        Hunger.Addition(amount);
        UpdateHungerBar();
    }

    public void FullHunger()
    {
        if (isDead) return;

        Hunger.SetToMax();
        UpdateHungerBar();
    }

    // Thirst Methods
    public void DeductThirst(int amount)
    {
        if (isDead) return;

        Thirst.Subtract(amount);
        UpdateThirstBar();
    }

    public void AddThirst(int amount)
    {
        if (isDead) return;

        Thirst.Addition(amount);
        UpdateThirstBar();
    }

    public void FullThirst()
    {
        if (isDead) return;

        Thirst.SetToMax();
        UpdateThirstBar();
    }

    private void Update()
    {
        // You can add other passive updates here if needed
    }
}