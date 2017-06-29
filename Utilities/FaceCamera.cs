using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class FaceCamera : MonoBehaviour
{
	public bool yAxisFixed = true;
	private Transform hmd;

	void Start ()
	{
		hmd = FindObjectOfType<VRTK_SDKManager> ().actualHeadset.transform;
	}

	void Update ()
	{
		Vector3 lookAtTarget;
		if (yAxisFixed) {
			lookAtTarget = new Vector3 (hmd.transform.position.x, transform.position.y, hmd.transform.position.z);
		} else {
			lookAtTarget = hmd.transform.position;
		}
		transform.LookAt (lookAtTarget);		
	}
}
