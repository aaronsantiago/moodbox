using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeInEvent : SequenceItem
{
    public OVRPassthroughLayer passthroughComponent;
    public float FadeTime = 1f;
    float totalTime = 0f;

    void Update()
    {
        if (IsActive)
        {
            totalTime += Time.deltaTime;

            passthroughComponent.textureOpacity = 1f - Mathf.Min(totalTime / FadeTime, 1f);
            Debug.Log(passthroughComponent.textureOpacity);
        }
    }

    protected override void OnActivate()
    {
        totalTime = 0f;
    }

    public override bool IsResponseSatisfied()
    {
        return totalTime / FadeTime > 1f;
    }
}
