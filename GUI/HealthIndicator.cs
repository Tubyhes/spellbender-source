using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthIndicator : MonoBehaviour
{
	public Text text;
	public RectTransform healthBar;
	public PlayerHealth playerHealth;

	// Use this for initialization
	void Start ()
	{
		playerHealth.OnHitpointsUpdated += HitpointsUpdated;
		HitpointsUpdated (playerHealth.hitpoints, playerHealth.maxHitpoints);
	}

	public void HitpointsUpdated (float hitpoints, float maxHitPoints)
	{
		healthBar.localScale = new Vector3 (hitpoints / maxHitPoints, 1f, 1f);
		text.text = "" + (int)Mathf.Floor (hitpoints) + " / " + (int)maxHitPoints;		
	}
}
