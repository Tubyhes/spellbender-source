using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class SkeletonRangedController : MonsterController
{
	public int numAttackAnims = 1;
	public int numGetHitAnims = 3;
	public int numDeathAnims = 3;
	public float retargetThreshold = 0.5f;
	public float range = 10f;
	public float attackCooldownDuration = 0.5f;

	public GameObject spawnRingPrefab;
	public GameObject projectilePrefab;

	public Transform projectileSpawn;

	private bool engaging = false;
	private bool attacking = false;
	private bool attackCooldown = false;

	private float fadeOutTime = 1f;
	private float fadeInTime = 3f;
	private GameObject spawnRing;
	private Outfitter outfitter;

	public override void Start ()
	{
		base.Start ();	

		outfitter = FindObjectOfType<Outfitter> ();
		outfitter.leftTeleporter.GetComponent<VRTK_BasicTeleport> ().Teleported += OnPlayerTeleported;
		outfitter.rightTeleporter.GetComponent<VRTK_BasicTeleport> ().Teleported += OnPlayerTeleported;

		if (engageOnSpawn) {
			if (spawnInSight) {
				Invoke ("EngagePlayer", fadeInTime);
			} else {
				EngagePlayer ();
			}
		} else if (engageOnSight) {
			StartCoroutine ("EngageOnSight");
		}

		if (spawnInSight) {
			spawnRing = Instantiate (spawnRingPrefab, transform, false);
			Destroy (spawnRing, fadeInTime);

			GetComponent<MeshFader> ().FadeIn (fadeInTime);
		}
	}

	private IEnumerator EngageOnSight ()
	{
		while (true) {
			if (PlayerInLineOfSight ()) {
				break;
			}

			yield return new WaitForSeconds (0.5f);
		}

		EngagePlayer ();
	}

	public void EngagePlayer ()
	{
		SetDestinationToPlayer ();
		StartCoroutine ("Engage");
	}

	private IEnumerator Engage ()
	{
		engaging = true;

		while (true) {
//			Debug.Log ("getting into position!");
			if (Vector3.Distance (agent.destination, ActualPlayerPosition ()) > retargetThreshold) {
				SetDestinationToPlayer ();
			}

			if (agent.velocity.magnitude > float.Epsilon) {
				anim.SetBool ("move", true);
			} else {
				anim.SetBool ("move", false);
				LookAtTarget ();
			}

			if (Vector3.Distance (transform.position, agent.destination) < range) {
				if (PlayerInLineOfSight ()) {
					anim.SetBool ("move", false);
					agent.isStopped = true;

					yield return StartCoroutine (Attack ());

					agent.isStopped = false;
				}
			}

			yield return null;
		}
	}

	private IEnumerator Attack ()
	{
		attacking = true;

		while (Vector3.Distance (transform.position, agent.destination) < range && PlayerInLineOfSight ()) {
//			Debug.Log ("attacking!");
			LookAtTarget ();

			if (!attackCooldown && IsInAnimationIdle ()) {
				StartCoroutine ("AttackAnimation");
			}

			yield return null;
		}

		attacking = false;
	}

	private IEnumerator HitAnimation ()
	{
		if (!attacking) {
			agent.isStopped = true;
		}

		string stateName = "GetHit" + Random.Range (1, numGetHitAnims + 1);
		yield return StartCoroutine (PlayAnimation (stateName));

		if (!attacking) {
			agent.isStopped = false;
		}
	}

	private IEnumerator AttackAnimation ()
	{
		attackCooldown = true;

		string stateName = "Attack" + Random.Range (1, numAttackAnims + 1);
		yield return StartCoroutine (PlayAnimation (stateName));

		yield return new WaitForSeconds (attackCooldownDuration);
		attackCooldown = false;
	}

	private IEnumerator DeathAnimation ()
	{
		string stateName = "Die" + Random.Range (1, numDeathAnims + 1);
		yield return StartCoroutine (PlayAnimation (stateName));

		Destroy (GetComponent<Collider> ());
		GetComponent<MeshFader> ().FadeOut (fadeOutTime);
		yield return new WaitForSeconds (fadeOutTime);

		Destroy (gameObject);
	}

	public override void GetHit (float dmg, ElementType elementType)
	{
		if (hitpoints <= 0) {
			return;
		}

		float damage = DamageModifiers (dmg, elementType);

		if (damage > 0f) {
			if (IsInAnimationIdle () || IsInAnimationMove ()) {
				StartCoroutine ("HitAnimation");
			}
		}

		HandleDamage (damage);
	}

	protected override void OnDied ()
	{
		StopCoroutine ("Engage");
		StopCoroutine ("HitAnimation");
		StopCoroutine ("AttackAnimation");

		outfitter.leftTeleporter.GetComponent<VRTK_BasicTeleport> ().Teleported -= OnPlayerTeleported;
		outfitter.rightTeleporter.GetComponent<VRTK_BasicTeleport> ().Teleported -= OnPlayerTeleported;
		agent.isStopped = true;
		base.OnDied ();
		StartCoroutine (DeathAnimation ());
	}

	public void OnPlayerTeleported (object o, DestinationMarkerEventArgs a)
	{
		if (engaging) {
			SetDestinationToPlayer ();
		}
	}

	// to be called from animation trigger event
	public void ShootProjectile ()
	{
		Vector3 target = player.transform.position + player.center;
		target.y += player.center.y;

		Debug.Log ("Shoot projectile at: " + target);

		GameObject arrow = Instantiate (projectilePrefab, projectileSpawn.position, Quaternion.identity);
		arrow.GetComponent<MonsterArrow> ().ArrowReleased (target);
	}

	private bool IsInAnimationIdle ()
	{
		return anim.GetCurrentAnimatorStateInfo (0).IsName ("Idle");
	}

	private bool IsInAnimationMove ()
	{
		return anim.GetCurrentAnimatorStateInfo (0).IsName ("Moving");
	}
}
