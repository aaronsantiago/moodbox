using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerlinMove : MonoBehaviour
{
    public float StartSeed = 1f;
    public Vector3 PerlinScale = new Vector3(1.0f, 1.0f, 1.0f);
    public float Speed = 0.5f;

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(
            Mathf.PerlinNoise(StartSeed * 100f + 1235f, Time.time * Speed + StartSeed * 12.1523f) * PerlinScale.x,
            Mathf.PerlinNoise(StartSeed * 400f + 1235f, Time.time * Speed + StartSeed * 12.5223f) * PerlinScale.y,
            Mathf.PerlinNoise(StartSeed * 700f + 1235f, Time.time * Speed + StartSeed * 12.7523f) * PerlinScale.z);
    }
}
