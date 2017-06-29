using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

/**
 * Base class for spellcaster objects.
 * Each will probably have quite a distinct functionality, as each spell 
 * will require unique gestures.
 * However, for each a visual effect will be triggered when it first becomes available.
 */ 
public class SpellCaster : MonoBehaviour
{
	public GameObject enabledEffect;

	[HideInInspector]
	public bool available { get; protected set; }

	protected VRTK_SDKManager vrtk_manager;
	protected Outfitter outfitter;

	public virtual void Start ()
	{
		vrtk_manager = FindObjectOfType<VRTK_SDKManager> ();
		outfitter = FindObjectOfType<Outfitter> ();
	}

	public void SetAvailable ()
	{
		if (!available) {
			Vector3 pos = vrtk_manager.actualBoundaries.transform.position;
			pos.x += vrtk_manager.actualBoundaries.GetComponent<CapsuleCollider> ().center.x;
			pos.z += vrtk_manager.actualBoundaries.GetComponent<CapsuleCollider> ().center.z;
			Instantiate (enabledEffect, pos, Quaternion.identity);
			available = true;
			outfitter.rightControllerTooltips.UpdateText (VRTK_ControllerTooltips.TooltipButtons.GripTooltip, "New Spell!");
			outfitter.leftControllerTooltips.UpdateText (VRTK_ControllerTooltips.TooltipButtons.GripTooltip, "New Spell!");
		}
	}
}
