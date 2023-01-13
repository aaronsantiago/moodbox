using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogEvent : SequenceItem
{
    public string logString = "log";
    protected override void OnActivate()
    {
        Debug.Log(logString);
    }
}
