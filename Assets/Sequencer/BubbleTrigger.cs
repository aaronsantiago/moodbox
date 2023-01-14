using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;

public class BubbleTrigger : SequenceItem
{
    public static BubbleTrigger currentInstance;

    public GameObject LerpTargetPrefab;

    bool completedBubble = false;

    void Update()
    {
    }

    protected override void OnActivate()
    {
        currentInstance = this;
        completedBubble = false;
    }

    public override bool IsResponseSatisfied()
    {
        return completedBubble;
    }

    public void CompleteBubble(GameObject bubble)
    {
        completedBubble = true;
        GameObject lerpTarget = Instantiate(LerpTargetPrefab);
        LerpToTarget lerpComponent = bubble.AddComponent<LerpToTarget>();

        lerpComponent.Target = lerpTarget.transform;
    }
}
