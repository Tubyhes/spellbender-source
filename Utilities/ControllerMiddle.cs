using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerMiddle : MonoBehaviour {

	public Transform firstAnchor = null;
	public Transform secondAnchor = null;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (firstAnchor == null || secondAnchor == null) {
			return;
		}

		transform.position = (firstAnchor.position + secondAnchor.position) / 2;
		transform.rotation = Quaternion.Slerp (firstAnchor.rotation, secondAnchor.rotation, 0.5f);
	}
}
