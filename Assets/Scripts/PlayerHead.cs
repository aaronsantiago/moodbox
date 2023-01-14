using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHead : MonoBehaviour
{
    public static GameObject Instance;

    void Start()
    {
        Instance = gameObject;
    }
}
