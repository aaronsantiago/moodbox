using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateOverTime : MonoBehaviour
{
    public Vector3 RotateBy;

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(RotateBy * Time.deltaTime);
    }
}
