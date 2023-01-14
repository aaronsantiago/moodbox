using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleOrbit : MonoBehaviour
{
    public Transform orbitTransform;
    public float amplitude = 1f;
    public float frequency = 1f;
    public float phase = 0f;

    public float yAmplitude = 1f;
    public float yFrequency = 1f;
    public float yPhase = 0f;

    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(
            Mathf.Sin(Time.time * frequency + phase * Mathf.PI * 2) * amplitude + orbitTransform.position.x,
            Mathf.Sin(Time.time * yFrequency + yPhase * Mathf.PI * 2) * yAmplitude + orbitTransform.position.y,
            Mathf.Cos(Time.time * frequency + phase * Mathf.PI * 2) * amplitude + orbitTransform.position.z
          );

    }
}
