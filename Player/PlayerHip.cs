using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

/**
 * Keeps an object at the approximate location of the player's hip:
 * Directly beneath the headset, at about 55% of its height.
 */ 
public class PlayerHip : MonoBehaviour
{
	public float hipModifier = 0.55f;

	private VRTK_SDKManager vrtk_manager;
	private Transform playArea;
	private Transform head;

	void Start ()
	{
		vrtk_manager = FindObjectOfType<VRTK_SDKManager> ();
		playArea = vrtk_manager.actualBoundaries.transform;
		head = vrtk_manager.actualHeadset.transform;
	}

	void Update ()
	{
		Vector3 pos = playArea.position;
		pos.x += head.localPosition.x;
		pos.z += head.localPosition.z;
		pos.y += head.localPosition.y * hipModifier;
		transform.position = pos;
	}
}
