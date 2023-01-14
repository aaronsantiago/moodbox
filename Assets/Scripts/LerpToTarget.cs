using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LerpToTarget : MonoBehaviour
{

    public Transform Target;
    public float LerpAlpha = 0.5f;

    // Smoothing rate dictates the proportion of source remaining after one second
    //
    public static float Damp(float source, float target, float smoothing, float dt)
    {
        return Mathf.Lerp(source, target, 1 - Mathf.Pow(smoothing, dt));
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(
            Damp(transform.position.x, Target.position.x, LerpAlpha, Time.deltaTime),
            Damp(transform.position.y, Target.position.y, LerpAlpha, Time.deltaTime),
            Damp(transform.position.z, Target.position.z, LerpAlpha, Time.deltaTime));
    }
}
