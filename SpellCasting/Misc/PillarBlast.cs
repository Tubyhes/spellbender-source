using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

/**
 * A single explosion of high potency.
 */
public class PillarBlast : MonoBehaviour
{
	public float damage;
	public ElementType elementType;
	public float radius;
	public LayerMask layersToHit;

	void Start ()
	{
		Destroy (gameObject, 3f);
		HitTargets ();
	}

	private void HitTargets ()
	{
		Collider[] colliders = Physics.OverlapSphere (transform.position, radius, layersToHit, QueryTriggerInteraction.Collide);
		Debug.Log ("Number of colliders hit: " + colliders.Length);

		foreach (Collider collider in colliders) {
			if (VRTK_PlayerObject.IsPlayerObject (collider.gameObject, VRTK_PlayerObject.ObjectTypes.CameraRig)) {
				FindObjectOfType<PlayerHealth> ().GetHit (damage, ElementType.Physical);
			}
			if (collider.gameObject.tag == "Monster") {
				collider.gameObject.GetComponent<MonsterController> ().GetHit (damage, elementType);
			}
		}
	}
}
