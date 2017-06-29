using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class FollowPlayerCollider : MonoBehaviour
{
	private GameObject cameraRig;
	private CapsuleCollider playerCollider;

	IEnumerator Start ()
	{
		cameraRig = FindObjectOfType<VRTK_SDKManager> ().actualBoundaries;

		while (cameraRig.GetComponent<CapsuleCollider> () == null) {
			yield return new WaitForSeconds (0.5f);
		}

		playerCollider = cameraRig.GetComponent<CapsuleCollider> ();
	}

	void Update ()
	{
		if (cameraRig == null || playerCollider == null) {
			return;
		}	
			
		Vector3 position = cameraRig.transform.position;
		position.x += playerCollider.center.x;
		position.z += playerCollider.center.z;
		transform.position = position;
	}
}
