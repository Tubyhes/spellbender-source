using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MonsterHealthIndicator : MonoBehaviour
{
	public RectTransform healthBar;

	public void SetHitpoints (float hitpoints, float maxHitpoints)
	{
		healthBar.localScale = new Vector3 (hitpoints / maxHitpoints, 1f, 1f);
	}
}
