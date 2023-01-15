using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeakableBubble : MonoBehaviour
{
    bool bubbleGrabbed = false;

    public bool ForceSatisfy = false;
    public AudioSource GrabSound;
    public AudioSource ReleaseSound;

    public Material GrabbedMaterial;

    OVRGrabbable bubbleGrabbableComponent;

    public List<MonoBehaviour> ToDisable;
    // Start is called before the first frame update
    void Start()
    {
        bubbleGrabbableComponent = GetComponent<OVRGrabbable>();
    }

    // Update is called once per frame
    void Update()
    {

        if (!bubbleGrabbed && bubbleGrabbableComponent.isGrabbed || ForceSatisfy)
        {
            bubbleGrabbed = true;
            foreach(MonoBehaviour toDisable in ToDisable)
            {
                toDisable.enabled = false;
            }
            GrabSound.Play();
        }

        if (bubbleGrabbed && !bubbleGrabbableComponent.isGrabbed || ForceSatisfy)
        {
            ForceSatisfy = false;
            bubbleGrabbableComponent.enabled = false;
            bubbleGrabbed = false;
            if (BubbleTrigger.currentInstance != null && BubbleTrigger.currentInstance.enabled)
            {
                GetComponent<MeshRenderer>().material = GrabbedMaterial;
                BubbleTrigger.currentInstance.CompleteBubble(gameObject);
            }
            ReleaseSound.Play();
        }
    }
}
