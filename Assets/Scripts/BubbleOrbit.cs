using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleOrbit : MonoBehaviour
{
    public Transform orbitTransform;
    // Start is called before the first frame update
    void Start()
    {
        orbitTransform = PlayerHead.Instance.transform;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
