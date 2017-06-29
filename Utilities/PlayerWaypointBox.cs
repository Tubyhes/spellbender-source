using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWaypointBox : PlayerWaypoint
{
	public float x_size;
	public float y_size;
	public float z_size;

	void FixedUpdate ()
	{
		if (!base.IsEventNull ()) {
			Vector3 center = new Vector3 (transform.position.x, transform.position.y + y_size / 2f, transform.position.z);
			Vector3 halfExtents = new Vector3 (x_size / 2f, y_size / 2f, z_size / 2f);
			Collider[] colliders = Physics.OverlapBox (center, halfExtents, Quaternion.identity, layerMask, QueryTriggerInteraction.Collide);
			foreach (Collider c in colliders) {
				if (c.gameObject.tag == "Player") {
					base.TriggerEvent (this);
					break;
				}
			}
		}	
	}
}
