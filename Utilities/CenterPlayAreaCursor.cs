using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class CenterPlayAreaCursor : VRTK_PlayAreaCursor
{
	public override void SetPlayAreaCursorTransform (Vector3 location)
	{
		var offset = Vector3.zero;

		if (playAreaCursor) {
			if (playAreaCursor.activeInHierarchy && handlePlayAreaCursorCollisions && headsetOutOfBoundsIsCollision) {
				var checkPoint = new Vector3 (location.x, playAreaCursor.transform.position.y + (playAreaCursor.transform.localScale.y * 2), location.z);
				if (!playAreaCursorCollider.bounds.Contains (checkPoint)) {
					headsetOutOfBounds = true;
				} else {
					headsetOutOfBounds = false;
				}
			}

			playAreaCursor.transform.position = location + offset;
		}
	}

	public override void SetMaterialColor (Color color)
	{

	}
}
