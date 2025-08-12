using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public enum DayOfWeek
{
    Sunday,
    Monday,
    Tuesday,
    Wednesday,
    Thursday,
    Friday,
    Saturday
}

public class DayTime : MonoBehaviour
{
    const float secondsInDay = 86400f;
    const float phaseLength = 900f; // 15 minutes chunk of time
    const float phasesInDay = 96f; //seconsInDay divided by phaseLength
    
    [SerializeField] Color nightLightColor;
    [SerializeField] AnimationCurve nightTimeCurve;
    [SerializeField] Color dayLightColor = Color.white;
    
    float time;
    [SerializeField] float timeScale = 60f;
    [SerializeField] float startAtTime = 28800f; // in seconds
    [SerializeField] float morningTime = 28800f;
    
    DayOfWeek dayOfWeek;
    
    [SerializeField] TMPro.TextMeshProUGUI text;
    [SerializeField] Text dayOfTheWeekText;
    [SerializeField] TMPro.TextMeshProUGUI dayCountText;
    [SerializeField] Light2D globalLight;

    [Header("Time-based Sprite")]
    [SerializeField] Image timeSprite;
    [SerializeField] Sprite sprite6AM;     // Early Morning
    [SerializeField] Sprite sprite12PM;    // Noon
    [SerializeField] Sprite sprite3PM;     // Afternoon
    [SerializeField] Sprite sprite5PM;     // Late Afternoon
    [SerializeField] Sprite sprite7PM;     // Evening
    [SerializeField] Sprite sprite12AM;    // Midnight

    public int days;
    List<TimeAgent> agents;
    
    // Singleton pattern
    public static DayTime Instance { get; private set; }
    
    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        agents = new List<TimeAgent>();
    }
    
    private void Start()
    {
        time = startAtTime;
        UpdateDayText();
        UpdateDayCountText();
        UpdateTimeSprite();
    }

    private void UpdateTimeSprite()
    {
        if (timeSprite == null) return;

        float currentHour = Hours;

        // Determine which sprite to show based on current time
        if (currentHour >= 0f && currentHour < 6f)
        {
            // Midnight to 6AM - show midnight sprite
            timeSprite.sprite = sprite12AM;
        }
        else if (currentHour >= 6f && currentHour < 12f)
        {
            // 6AM to 12PM - show morning sprite
            timeSprite.sprite = sprite6AM;
        }
        else if (currentHour >= 12f && currentHour < 15f)
        {
            // 12PM to 3PM - show noon sprite
            timeSprite.sprite = sprite12PM;
        }
        else if (currentHour >= 15f && currentHour < 17f)
        {
            // 3PM to 5PM - show afternoon sprite
            timeSprite.sprite = sprite3PM;
        }
        else if (currentHour >= 17f && currentHour < 19f)
        {
            // 5PM to 7PM - show late afternoon sprite
            timeSprite.sprite = sprite5PM;
        }
        else if (currentHour >= 19f && currentHour < 24f)
        {
            // 7PM to Midnight - show evening sprite
            timeSprite.sprite = sprite7PM;
        }
    }

    private void UpdateDayCountText()
    {
        if (dayCountText != null)
        {
            dayCountText.text = (days + 1).ToString("00");
        }
    }

    public void Subscribe(TimeAgent timeAgent)
    {
        agents.Add(timeAgent);
    }
    
    public void Unsubsribe(TimeAgent timeAgent)
    {
        agents.Remove(timeAgent);
    }
    
    public float Hours
    {
        get
        {
            return time / 3600f;
        }
    }
    
    public float Minutes
    {
        get
        {
            return time % 3600f / 60f;
        }
    }
    
    private void Update()
    {
        time += Time.deltaTime * timeScale;
        TimeValueCalculation();
        DayLight();
        UpdateTimeSprite();
        
        if (time > secondsInDay)
        {
            NextDay();
        }
        
        TimeAgents();
        
        if (Input.GetKeyDown(KeyCode.R))
        {
            SkipTime(hours: 4);
        }
    }
    
    int oldPhase = -1;
    
    private void TimeAgents()
    {
        if(oldPhase == -1)
        {
            oldPhase = CalculatePhase();
        }
        
        int currentPhase = CalculatePhase();
        
        while(oldPhase < currentPhase)
        {
            oldPhase += 1;
            for (int i = 0; i < agents.Count; i++)
            {
                agents[i].Invoke(this);
            }
        }
    }
    
    private int CalculatePhase()
    {
        return (int)(time / phaseLength) + (int)(days * phasesInDay);
    }
    
    private void TimeValueCalculation()
    {
        int hh = (int)Hours;
        int mm = (int)Minutes;
        text.text = hh.ToString("00") + ":" + mm.ToString("00");
    }
    
    private void DayLight()
    {
        float v = nightTimeCurve.Evaluate(Hours);
        Color c = Color.Lerp(dayLightColor, nightLightColor, v);
        globalLight.color = c;
    }
    
    private void NextDay()
    {
        time -= secondsInDay;
        days += 1;
        
        int dayNum = (int)dayOfWeek;
        dayNum += 1;
        if (dayNum >= 7)
        {
            dayNum = 0;
        }
        dayOfWeek = (DayOfWeek)dayNum;
        
        UpdateDayText();
        UpdateDayCountText();
    }
    
    private void UpdateDayText()
    {
        string chineseDayText = "";
        switch (dayOfWeek)
        {
            case DayOfWeek.Sunday:
                chineseDayText = "日";
                break;
            case DayOfWeek.Monday:
                chineseDayText = "一";
                break;
            case DayOfWeek.Tuesday:
                chineseDayText = "二";
                break;
            case DayOfWeek.Wednesday:
                chineseDayText = "三";
                break;
            case DayOfWeek.Thursday:
                chineseDayText = "四";
                break;
            case DayOfWeek.Friday:
                chineseDayText = "五";
                break;
            case DayOfWeek.Saturday:
                chineseDayText = "六";
                break;
        }
        dayOfTheWeekText.text = chineseDayText;
    }
    
    public void SkipTime(float seconds = 0, float minute = 0, float hours = 0)
    {
        float timeToSkip = seconds;
        timeToSkip += minute * 60f;
        timeToSkip += hours * 3600f;
        time += timeToSkip;
    }
    
    public void SkipToMorning()
    {
        float secondsToSkip = 0f;
        if(time > morningTime)
        {
            secondsToSkip += secondsInDay - time + morningTime;
        }
        else
        {
            secondsToSkip += morningTime - time;
        }
        SkipTime(secondsToSkip);
    }
    
    // Public methods for accessing time data from other scenes
    public string GetTimeString()
    {
        int hh = (int)Hours;
        int mm = (int)Minutes;
        return hh.ToString("00") + ":" + mm.ToString("00");
    }
    
    public DayOfWeek GetDayOfWeek()
    {
        return dayOfWeek;
    }

    /// Gets the current time scale multiplier
    public float TimeScale
    {
        get { return timeScale; }
    }

    /// <param name="scale">The new time scale (0 = paused, 1 = normal speed, >1 = faster)</param>
    public void SetTimeScale(float scale)
    {
        timeScale = scale;
    }

    /// </summary>
    public void PauseTime()
    {
        timeScale = 0f;
    }

    /// <param name="scale">Optional: specific scale to resume with, defaults to 60f</param>
    public void ResumeTime(float scale = 100f)
    {
        timeScale = scale;
    }
}