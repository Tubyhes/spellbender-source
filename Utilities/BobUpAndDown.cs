using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BobUpAndDown : MonoBehaviour
{
	public float periodLength = 2.0f;
	public float periodOffset = 0f;
	public float amplitude = 0.2f;
	public float hoverHeight = 0.1f;
	public bool relativeToParent = false;

	private Vector3 startPosition;

	void Start ()
	{
		startPosition = transform.position;
	}

	void Update ()
	{
		float x = (Time.time + periodOffset) * Mathf.PI * (2 / periodLength);
		float strength = Mathf.Sin (x);

		if (!relativeToParent) {
			float y = startPosition.y + hoverHeight + strength * amplitude;
			transform.position = new Vector3 (startPosition.x, y, startPosition.z);
		} else {
			float y = hoverHeight + strength * amplitude;
			transform.localPosition = new Vector3 (0f, y, 0f);
		}
	}
}
