using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWaypoint : MonoBehaviour
{
	public delegate void PlayerInWaypointRangeAction (PlayerWaypoint waypoint);

	public event PlayerInWaypointRangeAction OnPlayerInWaypointRange;

	protected int layerMask = 1 << 2;

	protected bool IsEventNull ()
	{
		return OnPlayerInWaypointRange == null;
	}

	protected void TriggerEvent (PlayerWaypoint waypoint)
	{
		if (OnPlayerInWaypointRange != null) {
			OnPlayerInWaypointRange (waypoint);
		}
	}
}
