using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bubblyMoves : MonoBehaviour
{
	public float amplitude = 1.0f;
	public float y_speed = 1.0f;
	public float frequency = 1.0f;
	public float maxHeight = 3.0f;
	public float minHeight = 0.0f;

	// Update is called once per frame
	void Update()
    {
		transform.position = new Vector3(Mathf.Sin(frequency* Time.time) * amplitude, transform.position.y + (Time.deltaTime * y_speed), 0);
		if (transform.position.y > maxHeight)
        {
			transform.position = new Vector3(transform.position.x, minHeight, transform.position.y);
		}

    }



}
