using System;
using UnityEngine;

[Serializable]
public class Class
{
    public string className;
    public DayOfWeek dayOfWeek;
    public float startTime; // in hours (24-hour format)
    public float endTime; // in hours (24-hour format)
    public string teacher;
    public string room;
    public Color classColor = Color.white;

    public Class(string name, DayOfWeek day, float start, float end, string teacherName = "", string roomName = "")
    {
        className = name;
        dayOfWeek = day;
        startTime = start;
        endTime = end;
        teacher = teacherName;
        room = roomName;
    }

    public bool IsActiveNow(DayOfWeek currentDay, float currentTime)
    {
        return dayOfWeek == currentDay && currentTime >= startTime && currentTime <= endTime;
    }

    public bool IsToday(DayOfWeek currentDay)
    {
        return dayOfWeek == currentDay;
    }

    public string GetTimeString()
    {
        int startHour = (int)startTime;
        int startMinute = (int)((startTime - startHour) * 60);
        int endHour = (int)endTime;
        int endMinute = (int)((endTime - endHour) * 60);

        return $"{startHour:00}:{startMinute:00} - {endHour:00}:{endMinute:00}";
    }
}