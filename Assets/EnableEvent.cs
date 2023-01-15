using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableEvent : SequenceItem
{
    public GameObject ToEnable;
    protected override void OnActivate()
    {
        ToEnable.SetActive(true);
    }

    public override bool IsResponseSatisfied()
    {
        return true;
    }
}
