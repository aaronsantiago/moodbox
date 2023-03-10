using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bubblyMoves : MonoBehaviour
{
	public float amplitude = 0.5f;
	public float y_speed = 0.5f;
	public float frequency = 0.5f;
	public float maxHeight = 3.0f;
	public float minHeight = 0.0f;
	public float phase = 0.0f;

	// Update is called once per frame
	void Update()
    {
		transform.localPosition = new Vector3(
			Mathf.Sin(phase + frequency* Time.time) * amplitude,
			transform.localPosition.y + Time.deltaTime * y_speed,
			0
			);

		if (transform.localPosition.y > maxHeight)
		{
			transform.localPosition = new Vector3(
									transform.localPosition.x,	
									minHeight,
									transform.localPosition.z);
		}

    }



}
