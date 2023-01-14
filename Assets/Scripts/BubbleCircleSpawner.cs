using System.Collections;
using System.Collections.Generic;
using UnityEngine; 

public class BubbleCircleSpawner : MonoBehaviour
{
    public GameObject roomBubblePrefab;
    public float howMany = 10;
    public float amplitude = 1f;

    // Start is called before the first frame update
    void Start()
    {

        // 2PI /3
        for (int i = 0; i < howMany; i++) {
            GameObject roomBubbleL1 = Instantiate(roomBubblePrefab);
            roomBubbleL1.transform.position = new Vector3(
                Mathf.Sin(i * (Mathf.PI * 2 / howMany)) * amplitude,
                0,
                Mathf.Cos(i * (Mathf.PI * 2 / howMany)) * amplitude
            );
            bubblyMoves bubblyMovesComponentL1 = roomBubbleL1.GetComponentInChildren<bubblyMoves>();
            bubblyMovesComponentL1.y_speed = Random.value * 0.5f + 0.1f;

            GameObject roomBubbleL2 = Instantiate(roomBubblePrefab);
            roomBubbleL2.transform.position = new Vector3(
                Mathf.Sin(i * (Mathf.PI * 2 / howMany)) * amplitude,
                -3,
                Mathf.Cos(i * (Mathf.PI * 2 / howMany)) * amplitude
            );
            bubblyMoves bubblyMovesComponentL2 = roomBubbleL2.GetComponentInChildren<bubblyMoves>();
            bubblyMovesComponentL2.y_speed = Random.value * 0.5f + 0.1f;
        }
    }

}
