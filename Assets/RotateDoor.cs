using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateDoor : SequenceItem
{

    public GameObject objectToRotate;
    public float rotationAmount = 22f;
    public float rotationTime = 1f;
    float totalTime = 0f;

    void Update()
    {
        if (IsActive)
        {
            totalTime += Time.deltaTime;
            objectToRotate.transform.Rotate(0f, 0f, -(rotationAmount/rotationTime * Time.deltaTime));
        }
    }

    protected override void OnActivate()
    {
        totalTime = 0f;
    }

    public override bool IsResponseSatisfied()
    {
        return totalTime / rotationTime > 1f;
    }
}
