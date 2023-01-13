using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;

public class TimerTrigger : SequenceItem
{
    public float time = 1.0f;

    Timer timer;

    bool timerElapsed = false;

    protected override void OnActivate()
    {
        timer = new Timer(time * 1000.0);
        timer.Elapsed += (s, e) =>
        {
            timerElapsed = true;
            timer.Stop();
        };

        timer.AutoReset = true;
        timer.Start();
    }

    public override bool IsResponseSatisfied()
    {
        return timerElapsed;
    }
}
