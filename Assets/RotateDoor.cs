using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateDoor : MonoBehaviour
{

    public GameObject objectToRotate;
    public float rotationSpeed = 33f;

    void Update()
    {
        if (Input.GetKey(KeyCode.R))
        {
            objectToRotate.transform.Rotate(0f, 0f,-(rotationSpeed * Time.deltaTime));
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // set the z angle to to 0
        Debug.Log("Ouch, I clashed");
    }


}
