using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SequenceItem : MonoBehaviour
{

    public bool IsActive = false;
    public bool ActivateOnStart = false;

    private void Start()
    {
        if (ActivateOnStart) Activate();
    }

    public void Activate()
    {
        gameObject.SetActive(true);
        IsActive = true;
        OnActivate();
    }

    protected virtual void OnActivate() { }

    public void Deactivate()
    {
        IsActive = false;
        gameObject.SetActive(false);
        OnDeactivate();
    }

    protected virtual void OnDeactivate() { }


    public virtual bool IsResponseSatisfied()
    {
        return true;
    }
}
