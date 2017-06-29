using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWaypointSphere : PlayerWaypoint
{

	public float height = 1f;
	public float radius = 0.75f;

	void FixedUpdate ()
	{
		if (!base.IsEventNull ()) {
			Vector3 sphereCenter = new Vector3 (transform.position.x, transform.position.y + height, transform.position.z);
			Collider[] colliders = Physics.OverlapSphere (sphereCenter, radius, layerMask, QueryTriggerInteraction.Collide);
			foreach (Collider c in colliders) {
				if (c.gameObject.tag == "Player") {
					base.TriggerEvent (this);
					break;
				}
			}
		}	
	}
}
