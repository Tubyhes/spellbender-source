using UnityEngine;
using System.Collections;
using VRTK;

/**
 * The magic bolt is a simple sphere object with visual effects that travels through the
 * scene until it collides with something or times out after a while. Upon colliding with something
 * it can either deal damage (if it's a mob or a player), destroy something (if it's a destructible object) 
 * or cause explosive force, which might send some objects flying. 
 */
public class MagicBolt : MonoBehaviour
{
	public float explosionForce = 10f;
	public float explosionRadius = 1f;
	public float damage = 1f;
	public ElementType elementType;

	public GameObject impactParticlePrefab;
	public GameObject projectileParticlePrefab;
	public GameObject[] trailParticles;

	[HideInInspector]
	public Vector3 impactNormal;

	private bool hasCollided = false;
	private GameObject impactParticle;
	private GameObject projectileParticle;

	void Start ()
	{
		projectileParticle = Instantiate (projectileParticlePrefab, transform.position, transform.rotation) as GameObject;
		projectileParticle.transform.parent = transform;
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
		if (!hasCollided) {
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

	void OnTriggerEnter (Collider other)
	{
		if (!hasCollided) {
			if (VRTK_PlayerObject.IsPlayerObject (other.gameObject, VRTK_PlayerObject.ObjectTypes.CameraRig)) {
				hasCollided = true;
				impactParticle = Instantiate (impactParticlePrefab, transform.position, Quaternion.FromToRotation (Vector3.up, impactNormal)) as GameObject;
				FindObjectOfType<PlayerHealth> ().GetHit (damage, ElementType.Physical);
			}

			CleanUp ();
		}
	}
}