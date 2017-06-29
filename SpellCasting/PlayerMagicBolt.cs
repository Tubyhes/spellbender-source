using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;
using VRTK;

/**
 * Magic Bolt cast by player. Since player has to actually aim it and give it force, this
 * is a bit different from magic bolts being cast by NPCs. 
 */
public class PlayerMagicBolt : MonoBehaviour
{
	public float velocityModifier;
	public float explosionForce = 10f;
	public float explosionRadius = 1f;
	public float damage = 1f;
	public ElementType elementType;

	public GameObject impactParticlePrefab;
	public GameObject projectileParticlePrefab;
	public GameObject[] trailParticles;

	[HideInInspector]
	public Transform firstAnchor = null;
	[HideInInspector]
	public Transform secondAnchor = null;
	[HideInInspector]
	public Vector3 impactNormal;

	private VelocityEstimator velocityEstimator;
	private bool released = false;
	private bool hasCollided = false;
	private GameObject impactParticle;
	private GameObject projectileParticle;

	void Awake ()
	{
		velocityEstimator = GetComponent<VelocityEstimator> ();
	}

	void Start ()
	{
		projectileParticle = Instantiate (projectileParticlePrefab, transform.position, transform.rotation) as GameObject;
		projectileParticle.transform.parent = transform;
	}

	void Update ()
	{
		if (released) {
			return;
		}
		if (firstAnchor == null || secondAnchor == null) {
			return;
		}

		transform.position = (firstAnchor.position + secondAnchor.position) / 2;
		transform.rotation = Quaternion.Slerp (firstAnchor.rotation, secondAnchor.rotation, 0.5f);
	}

	/**
	 * Send bolt on its way after release. Calculations borrowed from Throwable.cs in 
	 * SteamVR InteractionSystem library.
	 */ 
	public void Release ()
	{
		released = true;

		Rigidbody rb = GetComponent<Rigidbody> ();
		rb.isKinematic = false;
		rb.interpolation = RigidbodyInterpolation.Interpolate;

		Vector3 position = Vector3.zero;
		Vector3 velocity = Vector3.zero;
		Vector3 angularVelocity = Vector3.zero;
		velocityEstimator.FinishEstimatingVelocity ();
		velocity = velocityEstimator.GetVelocityEstimate ();
		angularVelocity = velocityEstimator.GetAngularVelocityEstimate ();
		position = velocityEstimator.transform.position;

		Vector3 r = transform.TransformPoint (rb.centerOfMass) - position;
		rb.velocity = (velocity + Vector3.Cross (angularVelocity, r)) * velocityModifier;
		rb.angularVelocity = angularVelocity;

		// Make the object travel at the release velocity for the amount
		// of time it will take until the next fixed update, at which
		// point Unity physics will take over
		float timeUntilFixedUpdate = (Time.fixedDeltaTime + Time.fixedTime) - Time.time;
		transform.position += timeUntilFixedUpdate * velocity;
		float angle = Mathf.Rad2Deg * angularVelocity.magnitude;
		Vector3 axis = angularVelocity.normalized;
		transform.rotation *= Quaternion.AngleAxis (angle * timeUntilFixedUpdate, axis);

		Invoke ("TimeOut", 5f);
	}

	private void TimeOut ()
	{
		if (!hasCollided) {
			CleanUp ();
		}
	}

	private void CleanUp ()
	{
		foreach (GameObject trail in trailParticles) {
			GameObject curTrail = transform.Find (projectileParticle.name + "/" + trail.name).gameObject;
			curTrail.transform.parent = null;
			Destroy (curTrail, 3f);
		}

		Destroy (projectileParticle, 3f);
		Destroy (gameObject);
		if (impactParticle != null) {
			Destroy (impactParticle, 5f);
		}
	}

	void OnCollisionEnter (Collision hit)
	{
		if (!hasCollided && released) {
			hasCollided = true;
			impactParticle = Instantiate (impactParticlePrefab, transform.position, Quaternion.FromToRotation (Vector3.up, impactNormal)) as GameObject;

			if (hit.gameObject.tag == "Monster") {
				hit.gameObject.GetComponent<MonsterController> ().GetHit (damage, elementType);
			} else if (hit.gameObject.tag == "Destructible") {
				Destroy (hit.gameObject);
			} else if (hit.rigidbody != null) {
				hit.rigidbody.AddExplosionForce (explosionForce, hit.contacts [0].point, explosionRadius);
			}

			CleanUp ();
		}
	}

}
