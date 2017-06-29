using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class ApproachCamera : MonoBehaviour
{
	public float startDelta;
	public float endDelta;
	public float approachTime;

	private Transform hmd;

	void Start ()
	{
		hmd = FindObjectOfType<VRTK_SDKManager> ().actualHeadset.transform;
		transform.LookAt (hmd.transform.position);
		transform.position = transform.position + transform.forward * startDelta;
		StartCoroutine (Approach ());
	}

	private IEnumerator Approach ()
	{
		float startTime = Time.time;
		while (Time.time < startTime + approachTime) {
			Vector3 delta = transform.forward * Time.deltaTime * (endDelta - startDelta);
			transform.position = transform.position + delta;
			yield return null;
		}
	}
}

