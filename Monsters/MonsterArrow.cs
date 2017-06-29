using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;
using VRTK;

public class MonsterArrow : MonoBehaviour
{
	public Rigidbody arrowHeadRB;
	public Rigidbody shaftRB;
	public float arrowVelocity = 30f;
	public float damage = 1f;
	public List<AudioClip> hitPlayerSounds;
	public List<AudioClip> hitObjectSounds;

	private Vector3 prevPosition;
	private Quaternion prevRotation;
	private Vector3 prevVelocity;
	private Vector3 prevHeadPosition;

	private int travelledFrames = 0;

	private GameObject scaleParentObject = null;

	void FixedUpdate ()
	{
		prevPosition = transform.position;
		prevRotation = transform.rotation;
		prevVelocity = GetComponent<Rigidbody> ().velocity;
		prevHeadPosition = arrowHeadRB.transform.position;
		travelledFrames++;
	}

	public void ArrowReleased (Vector3 target)
	{
		transform.LookAt (target);

		shaftRB.isKinematic = false;
		shaftRB.useGravity = true;
		shaftRB.transform.GetComponent<BoxCollider> ().enabled = true;

		arrowHeadRB.isKinematic = false;
		arrowHeadRB.useGravity = true;
		arrowHeadRB.transform.GetComponent<BoxCollider> ().enabled = true;

		arrowHeadRB.AddForce (transform.forward * arrowVelocity, ForceMode.VelocityChange);
		arrowHeadRB.AddTorque (transform.forward * 10);

		travelledFrames = 0;
		prevPosition = transform.position;
		prevRotation = transform.rotation;
		prevHeadPosition = arrowHeadRB.transform.position;
		prevVelocity = GetComponent<Rigidbody> ().velocity;

		Destroy (gameObject, 30);
	}

	void OnCollisionEnter (Collision collision)
	{
		Rigidbody rb = GetComponent<Rigidbody> ();
		float rbSpeed = rb.velocity.sqrMagnitude;

		if (travelledFrames < 2) {
			// Reset transform but halve your velocity
			transform.position = prevPosition - prevVelocity * Time.deltaTime;
			transform.rotation = prevRotation;

			Vector3 reflfectDir = Vector3.Reflect (arrowHeadRB.velocity, collision.contacts [0].normal);
			arrowHeadRB.velocity = reflfectDir * 0.25f;
			shaftRB.velocity = reflfectDir * 0.25f;

			travelledFrames = 0;
			return;
		}

		if (rbSpeed > 0.1f) {
			StickInTarget (collision, travelledFrames < 2);
			PlayHitObjectSound ();
		}
	}

	void OnTriggerEnter (Collider other)
	{
		Rigidbody rb = GetComponent<Rigidbody> ();
		float rbSpeed = rb.velocity.sqrMagnitude;

		if (rbSpeed > 0.1f && VRTK_PlayerObject.IsPlayerObject (other.gameObject, VRTK_PlayerObject.ObjectTypes.CameraRig)) {
			Debug.Log ("collided with player!");
			FindObjectOfType<PlayerHealth> ().GetHit (damage, ElementType.Physical);
			PlayHitPlayerSound ();
			Destroy (gameObject);
		}
	}

	private void StickInTarget (Collision collision, bool bSkipRayCast)
	{
		Vector3 prevForward = prevRotation * Vector3.forward;

		// Only stick in target if the collider is front of the arrow head
		if (!bSkipRayCast) {
			RaycastHit[] hitInfo;
			hitInfo = Physics.RaycastAll (prevHeadPosition - prevVelocity * Time.deltaTime, prevForward, prevVelocity.magnitude * Time.deltaTime * 2.0f);
			bool properHit = false;
			for (int i = 0; i < hitInfo.Length; ++i) {
				RaycastHit hit = hitInfo [i];

				if (hit.collider == collision.collider) {
					properHit = true;
					break;
				}
			}

			if (!properHit) {
				return;
			}
		}
			
		shaftRB.velocity = Vector3.zero;
		shaftRB.angularVelocity = Vector3.zero;
		shaftRB.isKinematic = true;
		shaftRB.useGravity = false;
		shaftRB.transform.GetComponent<BoxCollider> ().enabled = false;

		arrowHeadRB.velocity = Vector3.zero;
		arrowHeadRB.angularVelocity = Vector3.zero;
		arrowHeadRB.isKinematic = true;
		arrowHeadRB.useGravity = false;
		arrowHeadRB.transform.GetComponent<BoxCollider> ().enabled = false;

		// If the hit item has a parent, dock an empty object to that
		// this fixes an issue with scaling hierarchy. I suspect this is not sustainable for a large object / scaling hierarchy.
		scaleParentObject = new GameObject ("Arrow Scale Parent");
		Transform parentTransform = collision.collider.transform;
		scaleParentObject.transform.parent = parentTransform;

		// Move the arrow to the place on the target collider we were expecting to hit prior to the impact itself knocking it around
		transform.parent = scaleParentObject.transform;
		transform.rotation = prevRotation;
		transform.position = prevPosition;
		transform.position = collision.contacts [0].point - transform.forward * (0.75f - (Util.RemapNumberClamped (prevVelocity.magnitude, 0f, 10f, 0.0f, 0.1f) + Random.Range (0.0f, 0.05f)));
	}

	void OnDestroy ()
	{
		if (scaleParentObject != null) {
			Destroy (scaleParentObject);
		}
	}

	private void PlayHitPlayerSound ()
	{
		AudioSource.PlayClipAtPoint (hitPlayerSounds [Random.Range (0, hitPlayerSounds.Count)], transform.position);
	}

	private void PlayHitObjectSound ()
	{
		AudioSource.PlayClipAtPoint (hitObjectSounds [Random.Range (0, hitObjectSounds.Count)], transform.position);
	}
}