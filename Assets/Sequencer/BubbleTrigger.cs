using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;

public class BubbleTrigger : SequenceItem
{
    public float time = 1.0f;
    public float completionTimeThreshold = 1.0f;

    public GameObject BubblePrefab;
    public Transform BubbleSpawnLocation;

    bool bubbleGrabbed = false;
    float bubbleGrabbedTime = -1;

    bool completedBubble = false;

    GameObject bubble;
    OVRGrabbable bubbleGrabbableComponent;

    void Update()
    {
        if (IsActive)
        {
            if (!bubbleGrabbed && bubbleGrabbableComponent.isGrabbed)
            {
                bubbleGrabbedTime = Time.time;
                bubbleGrabbed = true;
            }

            if (bubbleGrabbed && !bubbleGrabbableComponent.isGrabbed)
            {
                if (Time.time - bubbleGrabbedTime > completionTimeThreshold)
                {
                    completedBubble = true;
                    bubbleGrabbableComponent.enabled = false;
                }
                bubbleGrabbed = false;
            }
        }
    }

    protected override void OnActivate()
    {
        bubble = Instantiate(BubblePrefab);
        bubble.transform.position = BubbleSpawnLocation.transform.position;

        bubbleGrabbableComponent = bubble.GetComponent<OVRGrabbable>();
    }

    public override bool IsResponseSatisfied()
    {
        return completedBubble;
    }
}
