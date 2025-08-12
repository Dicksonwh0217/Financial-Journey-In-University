using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeAgent : MonoBehaviour
{
    public Action<DayTime> onTimeTick;

    private void Start()
    {
        Init();
    }

    public void Init()
    {
        GameManager.instance.timeController.Subscribe(this);
    }

    public void Invoke(DayTime dayTime)
    {
        onTimeTick?.Invoke(dayTime);
    }

    private void OnDestroy()
    {
        GameManager.instance.timeController.Unsubsribe(this);
    }
}
