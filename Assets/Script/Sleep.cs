using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sleep : MonoBehaviour
{
    DisableControls disableControls;
    Character character;
    DayTime dayTime;

    private void Start() // Changed from Awake to avoid null reference
    {
        disableControls = GetComponent<DisableControls>();
        character = GetComponent<Character>();
        dayTime = GameManager.instance.timeController;
    }

    internal void DoSleep()
    {
        StartCoroutine(SleepRoutine());
    }

    IEnumerator SleepRoutine()
    {
        ScreenTint screenTint = GameManager.instance.screenTint;

        disableControls.DisableControl();

        screenTint.Tint();
        yield return new WaitForSeconds(2f);

        character.FullHealth();
        character.FullHappiness();
        dayTime.SkipToMorning();


        screenTint.UnTint();
        disableControls.EnableControl();

        yield return null;
    }
}
