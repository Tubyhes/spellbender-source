using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using VRTK;

public class SkeletonController : MonsterController
{
	public int numAttackAnims = 3;
	public int numGetHitAnims = 3;
	public int numDeathAnims = 3;
	public float retargetThreshold = 0.5f;
	public float attackCooldownDuration = 0.5f;

	public GameObject spawnRingPrefab;

	private MonsterWeaponCollider[] weapons;

	private bool engaging = false;
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

		weapons = GetComponentsInChildren<MonsterWeaponCollider> ();
		DisableWeapons ();

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
			if (Vector3.Distance (agent.destination, ActualPlayerPosition ()) > retargetThreshold) {
				SetDestinationToPlayer ();
			}

			if (agent.velocity.magnitude > float.Epsilon) {
				anim.SetBool ("move", true);
			} else {
				anim.SetBool ("move", false);
				LookAtTarget ();
			}

			if (ShouldMakeAttack ()) {
				StartCoroutine ("AttackAnimation");
			}

			yield return null;
		}
	}

	public void OnPlayerTeleported (object o, DestinationMarkerEventArgs a)
	{
		if (engaging) {
			SetDestinationToPlayer ();
		}
	}

	public override void GetHit (float dmg, ElementType elementType)
	{
		if (hitpoints <= 0f) {
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

	private IEnumerator HitAnimation ()
	{
		agent.isStopped = true;

		string stateName = "GetHit" + Random.Range (1, numGetHitAnims + 1);
		yield return StartCoroutine (PlayAnimation (stateName));

		agent.isStopped = false;
	}

	private IEnumerator AttackAnimation ()
	{
		attackCooldown = true;
		agent.isStopped = true;
		EnableWeapons ();

		string stateName = "Attack" + Random.Range (1, numAttackAnims + 1);
		yield return StartCoroutine (PlayAnimation (stateName));

		DisableWeapons ();
		agent.isStopped = false;

		yield return new WaitForSeconds (attackCooldownDuration);
		attackCooldown = false;
	}

	private IEnumerator DeathAnimation ()
	{
		string stateName = "Die" + Random.Range (1, numDeathAnims + 1);
		yield return StartCoroutine (PlayAnimation (stateName));

		GetComponent<MeshFader> ().FadeOut (fadeOutTime);
		yield return new WaitForSeconds (fadeOutTime);

		Destroy (gameObject);
	}

	private bool IsInAnimationIdle ()
	{
		return anim.GetCurrentAnimatorStateInfo (0).IsName ("Idle");
	}

	private bool IsInAnimationMove ()
	{
		return anim.GetCurrentAnimatorStateInfo (0).IsName ("Moving");
	}

	private bool ShouldMakeAttack ()
	{
		return Vector3.Distance (transform.position, agent.destination) <= agent.stoppingDistance &&
		!attackCooldown &&
		hitpoints > 0 &&
		IsInAnimationIdle ();
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

	protected void EnableWeapons ()
	{
		foreach (MonsterWeaponCollider weapon in weapons) {
			weapon.enabled = true;
		}
	}

	protected void DisableWeapons ()
	{
		foreach (MonsterWeaponCollider weapon in weapons) {
			weapon.enabled = false;
		}
	}
}
