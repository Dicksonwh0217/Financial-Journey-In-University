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
    }

    internal void Addition(int amount)
    {
        currVal += amount;

        if(currVal > maxVal)
        {
            currVal = maxVal;
        }
    }

    internal void SetToMax()
    {
        currVal = maxVal;
    }
}

public class Character : MonoBehaviour
{
    public Stat Health;
    [SerializeField] HappinessBar healthBar;
    public Stat Happiness;
    [SerializeField] HappinessBar happinessBar;



    public bool isDead;

    private void Start()
    {
        UpdateHealthBar();
        UpdateHappinessBar();
    }

    private void UpdateHealthBar()
    {
        healthBar.Set(Health.currVal, Health.maxVal);
    }
    private void UpdateHappinessBar()
    {
        happinessBar.Set(Happiness.currVal, Happiness.maxVal);
    }
    public void DeductHealth(int amount)
    {
        Health.Subtract(amount);
        if(Health.currVal <= 0)
        {
            isDead = true;
        }
        UpdateHealthBar();
    }

    public void AddHealth(int amount)
    {
        Health.Addition(amount);
        UpdateHealthBar();
    }

    public void FullHealth()
    {
        Health.SetToMax();
        UpdateHealthBar();
    }

    public void DeductHappiness(int amount)
    {
        Happiness.Subtract(amount);
        if (Health.currVal <= 0)
        {
            isDead = true;
        }
        UpdateHappinessBar();
    }


    public void AddHappiness(int amount)
    {
        Happiness.Addition(amount);
        UpdateHappinessBar();
    }

    public void FullHappineess(int amount)
    {
        Happiness.SetToMax();
        UpdateHappinessBar();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            DeductHealth(10);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            AddHealth(10);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            DeductHappiness(10);
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            AddHappiness(10);
        }
    }
}
