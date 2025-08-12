using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCCharacter : TimeAgent
{
    public NPCDefinition character;

    [Range(0, 10)]
    public int relationship;
    public bool talkedToday;
    public int talkedOnTheDayNumber = -1;

    private void Start()
    {
        Init();
        onTimeTick += ResetTalkState;
    }

    internal void IncreaseRelationship(int v)
    {
        if(talkedToday == false)
        {
            relationship += v;
            talkedToday = true;
        }
    }

    void ResetTalkState(DayTime dayTime)
    {
        if(dayTime.days != talkedOnTheDayNumber)
        {
            talkedToday = false;
            talkedOnTheDayNumber = dayTime.days;
        }
    }
}
