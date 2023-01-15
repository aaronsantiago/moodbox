using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;

public class SwitchScenesOnGrab : MonoBehaviour
{

    OVRGrabbable bubbleGrabbableComponent;
    public string NextScene = "";
    public bool grabbed = false;

    void Start()
    {
        bubbleGrabbableComponent = GetComponent<OVRGrabbable>();
    }

    void Update()
    {

        if (bubbleGrabbableComponent.isGrabbed)
        {
            grabbed = true;
            SceneManager.LoadScene(NextScene);
        }
        else if(grabbed)
        {
            Destroy(gameObject);
        }
    }
}
