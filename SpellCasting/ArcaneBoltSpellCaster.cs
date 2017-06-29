using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

/**
 * The Arcane Bolt is cast by charging the bolt for a short while at the hip with both
 * hands, and then pushing it outwards. (Kamehameha style in Dragonball Z).
 * The GameObject contains a Capsule mesh and collider around the player's hip.
 */
public class ArcaneBoltSpellCaster : SpellCaster
{
	public float maxHandDistance = 0.2f;
	public float chargeTime = 1f;
	public GameObject chargingEffectPrefab;
	public GameObject boltPrefab;

	private VRTK_ControllerEvents rightControllerEvents = null;
	private VRTK_ControllerEvents leftControllerEvents = null;
	private VRTK_ControllerActions rightControllerActions = null;
	private VRTK_ControllerActions leftControllerActions = null;
	private Transform actualRightController;
	private Transform actualLeftController;

	// controller model consists of different parts, keep track of which parts touch the sphere
	private List<GameObject> leftTouchingObjects;
	private List<GameObject> rightTouchingObjects;

	void Awake ()
	{
		available = false;
		leftTouchingObjects = new List<GameObject> ();
		rightTouchingObjects = new List<GameObject> ();
	}

	public override void Start ()
	{
		base.Start ();
		leftControllerEvents = vrtk_manager.scriptAliasLeftController.GetComponent<VRTK_ControllerEvents> ();
		rightControllerEvents = vrtk_manager.scriptAliasRightController.GetComponent<VRTK_ControllerEvents> ();
		leftControllerActions = vrtk_manager.scriptAliasLeftController.GetComponent<VRTK_ControllerActions> ();
		rightControllerActions = vrtk_manager.scriptAliasRightController.GetComponent<VRTK_ControllerActions> ();
		actualRightController = vrtk_manager.actualRightController.transform;
		actualLeftController = vrtk_manager.actualLeftController.transform;
	}

	void Update ()
	{
		if (outfitter.Casting || !outfitter.castingAvailable || !available) {
			return;
		}

		/** 
		* Three conditions are required to start casting: 
		* 	- Touch the hip sphere with both controllers
		* 	- Have controllers close together
		* 	- Press trigger on both controllers
		*/
		if (rightTouchingObjects.Count > 0 && leftTouchingObjects.Count > 0) {
			if (Vector3.Distance (actualLeftController.position, actualRightController.position) < maxHandDistance) {
				if (leftControllerEvents.triggerClicked && rightControllerEvents.triggerClicked) {
					StartCoroutine (Casting ());
				}
			}
		}
	}

	/**
	 * Casting arcane bolt consists of three stages: 
	 * 1: charge the bolt: once the charging has started, player is free to move his controllers, as long
	 * 		as they stay close together. The charging effect will remain between the controllers. Player
	 * 		has to keep pressing trigger buttons.
	 * 2: bolt is charged: the charging effect is replaced by the bolt. The bolt remains between the controllers,
	 * 		and user can move the controllers away from each other. The bolt starts logging it's position.
	 * 3: release bolt: When either of the trigger buttons is released, the bolt is sent on its way and can
	 * 		now also have effect on the world. Bolt moves in average direction it did over past few frames.
	 */
	private IEnumerator Casting ()
	{
		outfitter.Casting = true;
		float startTime = Time.time;

		Vector3 centerPos = (actualLeftController.position + actualRightController.position) / 2f;
		GameObject chargeEffect = Instantiate<GameObject> (chargingEffectPrefab, centerPos, Quaternion.identity);
		chargeEffect.GetComponent<ControllerMiddle> ().firstAnchor = actualLeftController;
		chargeEffect.GetComponent<ControllerMiddle> ().secondAnchor = actualRightController;

		while (Time.time < startTime + chargeTime) {
			if (!outfitter.castingAvailable) {
				outfitter.Casting = false;
				Destroy (chargeEffect);
				yield break;
			}
			if (!leftControllerEvents.triggerClicked || !rightControllerEvents.triggerClicked) {
				outfitter.Casting = false;
				Destroy (chargeEffect);
				yield break;
			}
			if (Vector3.Distance (actualLeftController.position, chargeEffect.transform.position) > maxHandDistance ||
			    Vector3.Distance (actualRightController.position, chargeEffect.transform.position) > maxHandDistance) {
				outfitter.Casting = false;
				Destroy (chargeEffect);
				yield break;
			}
			yield return StartCoroutine (ControllersHaptic ((Time.time - startTime) / chargeTime, 0.1f));
		}

		Destroy (chargeEffect);
		GameObject bolt = Instantiate<GameObject> (boltPrefab, centerPos, Quaternion.identity);
		PlayerMagicBolt hipCastBolt = bolt.GetComponent<PlayerMagicBolt> ();
		hipCastBolt.firstAnchor = actualLeftController;
		hipCastBolt.secondAnchor = actualRightController;

		while (leftControllerEvents.triggerClicked && rightControllerEvents.triggerClicked) {
			if (!outfitter.castingAvailable) {
				outfitter.Casting = false;
				Destroy (bolt);
				yield break;
			}
			yield return null;
		}

		hipCastBolt.Release ();
		outfitter.Casting = false;
	}

	private IEnumerator ControllersHaptic (float strength, float duration)
	{
		leftControllerActions.TriggerHapticPulse (strength, duration, 0.05f);
		rightControllerActions.TriggerHapticPulse (strength, duration, 0.05f);
		yield return new WaitForSeconds (duration);
	}

	// use physics settings to make sure only player colliders can collider with this object
	// so it's either the body collider, or the controllers
	void OnTriggerEnter (Collider other)
	{
		if (other.gameObject == vrtk_manager.actualBoundaries) {
			return;
		}
		if (other.transform.parent.parent.gameObject == vrtk_manager.scriptAliasLeftController) {
			if (!leftTouchingObjects.Contains (other.gameObject)) {
				leftTouchingObjects.Add (other.gameObject);
			}
		}
		if (other.transform.parent.parent.gameObject == vrtk_manager.scriptAliasRightController) {
			if (!rightTouchingObjects.Contains (other.gameObject)) {
				rightTouchingObjects.Add (other.gameObject);
			}
		}
	}

	void OnTriggerExit (Collider other)
	{
		if (leftTouchingObjects.Contains (other.gameObject)) {
			leftTouchingObjects.Remove (other.gameObject);
		}
		if (rightTouchingObjects.Contains (other.gameObject)) {
			rightTouchingObjects.Remove (other.gameObject);
		}
	}


}
