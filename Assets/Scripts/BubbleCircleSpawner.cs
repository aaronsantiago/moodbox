using System.Collections;
using System.Collections.Generic;
using UnityEngine; 

public class BubbleCircleSpawner : MonoBehaviour
{
    public GameObject roomBubblePrefab;
    public float howMany = 10;

    public float amplitude = 1f;
    public float frequency = 1f;
    public float phase = 0f;

    public float yAmplitude = 1f;
    public float yFrequency = 1f;
    public float yPhase = 0f;
    // Start is called before the first frame update
    void Start()
    {

        // 2PI /3
        for (int i = 0; i < howMany; i++) {
            GameObject roomBubble = Instantiate(roomBubblePrefab);
            roomBubble.transform.position = new Vector3(
                Mathf.Sin(i * (Mathf.PI * 2 / howMany)) * amplitude,
                0,
                Mathf.Cos(i * (Mathf.PI * 2 / howMany)) * amplitude
            );
            
            
            bubblyMoves bubblyMovesComponent = roomBubble.GetComponentInChildren<bubblyMoves>();

            bubblyMovesComponent.y_speed = Random.value * 5f;
        }
    }

}
