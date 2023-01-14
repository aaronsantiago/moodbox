using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeakableBubble : MonoBehaviour
{
    bool bubbleGrabbed = false;

    OVRGrabbable bubbleGrabbableComponent;
    // Start is called before the first frame update
    void Start()
    {
        bubbleGrabbableComponent = GetComponent<OVRGrabbable>();
    }

    // Update is called once per frame
    void Update()
    {

        if (!bubbleGrabbed && bubbleGrabbableComponent.isGrabbed)
        {
            bubbleGrabbed = true;
        }

        if (bubbleGrabbed && !bubbleGrabbableComponent.isGrabbed)
        {
            bubbleGrabbableComponent.enabled = false;
            bubbleGrabbed = false;
            if (BubbleTrigger.currentInstance != null && BubbleTrigger.currentInstance.enabled)
            {
                BubbleTrigger.currentInstance.CompleteBubble(gameObject);
            }
        }
    }
}
