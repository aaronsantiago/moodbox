using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;

public class SwitchScenesOnGrab : MonoBehaviour
{

    OVRGrabbable bubbleGrabbableComponent;
    public string NextScene = "";

    void Start()
    {
        bubbleGrabbableComponent = GetComponent<OVRGrabbable>();
    }

    void Update()
    {

        if (bubbleGrabbableComponent.isGrabbed)
        {
            Debug.Log("hi");
            SceneManager.LoadScene(NextScene);
        }
    }
}
