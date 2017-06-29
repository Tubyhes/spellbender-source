using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NecromancerController : MonsterController
{
	public delegate void DestinationReachedAction ();

	public event DestinationReachedAction OnDestinationReached;

	public int numCastBoltAnims = 2;
	public int numGetHitAnims = 3;
	public int numDeathAnims = 3;
	public int numSkeletonsPerSummon = 2;
	public int meleeSkeletonSpawnChance = 60;
	public float fadeInTime = 2f;
	public float attackCooldownDuration = 0.5f;

	public List <AudioClip> necromancerLines;
	public AudioSource voice;

	public Transform necromancerLeftHand;
	public Transform necromancerRightHand;

	public List<Transform> skeletonSummonLocations;
	public GameObject meleeSkeletonPrefab;
	public GameObject rangedSkeletonPrefab;

	public GameObject darkChargePrefab;
	public GameObject darkBoltPrefab;
	public GameObject darkExplosionPrefab;
	public GameObject spawnRingPrefab;

	private Vector2 smoothDeltaPosition = Vector2.zero;
	private Vector2 velocity = Vector2.zero;

	private bool invincible = false;
	private bool attackCooldown = false;

	private bool stageOneSummon = false;
	private bool stageTwoSummon = false;
	private bool stageThreeSummon = false;
	private bool stageFourExplosion = false;

	private GameObject leftChargeEffect = null;
	private GameObject rightChargeEffect = null;
	private GameObject spawnRing = null;

	public override void Awake ()
	{
		base.Awake ();
		agent.updatePosition = false;
	}

	public override void Start ()
	{
		base.Start ();
		spawnRing = Instantiate (spawnRingPrefab, transform, false);
		Destroy (spawnRing, fadeInTime);
		GetComponent<MeshFader> ().FadeIn (fadeInTime);
		LookAtTarget ();
	}

	public void MoveToDestination (Vector3 position)
	{
		agent.SetDestination (position);
		StartCoroutine (Move ());
	}

	public void EngagePlayer ()
	{
		StartCoroutine (Engage ());
	}

	public float Speak (int line_nr)
	{
		if (line_nr >= necromancerLines.Count) {
			return 0f;
		}

		AudioClip clip = necromancerLines [line_nr];
		voice.clip = clip;
		voice.Play ();

		return clip.length;
	}

	private IEnumerator Engage ()
	{
		while (true) {
			if (Vector3.Distance (agent.destination, transform.position) > agent.stoppingDistance) {
				Debug.Log ("Starting Move Coroutine!");
				yield return StartCoroutine (Move ());
			} else {
				Debug.Log ("Starting Combat Coroutine!");
				yield return StartCoroutine (Combat ());
			}
		}
	}

	private IEnumerator Combat ()
	{
		while (Vector3.Distance (agent.destination, transform.position) <= agent.stoppingDistance) {
			LookAtTarget ();

			if (hitpoints < 0.95f * maxHitpoints && !stageOneSummon) {
				stageOneSummon = true;
				yield return CastSummonAnimation ();
			} else if (hitpoints < 0.75f * maxHitpoints && !stageTwoSummon) {
				stageTwoSummon = true;
				yield return CastSummonAnimation ();
			} else if (hitpoints < 0.5f * maxHitpoints && !stageThreeSummon) {
				yield return CastSummonAnimation ();
				stageThreeSummon = true;
			} else if (hitpoints < 0.25f * maxHitpoints && !stageFourExplosion) {
				yield return CastExplosionAnimation ();
				stageFourExplosion = true;
			} else {
				yield return CastBoltAnimation ();
			}
		}
	}

	private IEnumerator Move ()
	{

		while (Vector3.Distance (agent.destination, transform.position) > agent.stoppingDistance) {
			Vector3 worldDeltaPosition = agent.nextPosition - transform.position;

			float dx = Vector3.Dot (transform.right, worldDeltaPosition);
			float dy = Vector3.Dot (transform.forward, worldDeltaPosition);
			Vector2 deltaPosition = new Vector2 (dx, dy);

			float smooth = Mathf.Min (1.0f, Time.deltaTime / 0.15f);
			smoothDeltaPosition = Vector2.Lerp (smoothDeltaPosition, deltaPosition, smooth);

			if (Time.deltaTime > 1e-5f) {
				velocity = smoothDeltaPosition / Time.deltaTime;
			}

			bool shouldMove = velocity.magnitude > 0.5f && agent.remainingDistance > agent.radius;

			anim.SetBool ("move", shouldMove);
			anim.SetFloat ("velx", velocity.x);
			anim.SetFloat ("vely", velocity.y);

			yield return null;
		}

		LookAtTarget ();
		if (OnDestinationReached != null) {
			OnDestinationReached ();
		}
	}

	/**
	 * Animation Coroutines
	 */
	private IEnumerator HitAnimation ()
	{
		string stateName = "GetHit" + Random.Range (1, numGetHitAnims + 1);
		yield return StartCoroutine (PlayAnimation (stateName));
	}

	private IEnumerator CastBoltAnimation ()
	{
		attackCooldown = true;

		string stateName = "BoltSpell" + Random.Range (1, numCastBoltAnims + 1);
		yield return StartCoroutine (PlayAnimation (stateName));

		yield return new WaitForSeconds (attackCooldownDuration);
		attackCooldown = false;
	}

	private IEnumerator CastSummonAnimation ()
	{
		attackCooldown = true;

		yield return StartCoroutine (PlayAnimation ("SummonSpell"));
		yield return new WaitForSeconds (attackCooldownDuration);

		attackCooldown = false;
	}

	private IEnumerator CastExplosionAnimation ()
	{
		attackCooldown = true;
		invincible = true;

		yield return new WaitForSeconds (Speak (2) + 1f);
		yield return StartCoroutine (PlayAnimation ("ExplosionSpell"));
		yield return new WaitForSeconds (attackCooldownDuration);

		invincible = false;
		attackCooldown = false;
	}

	public override void GetHit (float dmg, ElementType elementType)
	{
		if (hitpoints <= 0) {
			return;
		}

		float damage = 0f;
		if (!invincible) {
			damage = DamageModifiers (dmg, elementType);
		}

		if (damage > 0f) {
			if (IsInAnimationIdle ()) {
				StartCoroutine (HitAnimation ());
			}
		}

		HandleDamage (damage);
	}

	/**
	 * Animation Event Callbacks
	 */
	public void OnStartCasting ()
	{
		// instantiate dark channeling effects at hands
		leftChargeEffect = Instantiate<GameObject> (darkChargePrefab, necromancerLeftHand, false);
		rightChargeEffect = Instantiate<GameObject> (darkChargePrefab, necromancerRightHand, false);
	}

	public void OnEndCasting ()
	{
		DestroyChargeEffects ();
	}

	public void OnFinishCastingBolt ()
	{
		// instantiate dark bolt and launch it at player
		Vector3 position = (necromancerLeftHand.position + necromancerRightHand.position) / 2;
		GameObject bolt = Instantiate (darkBoltPrefab, position, transform.rotation);
		Vector3 boltTarget = player.transform.position + player.center;
		boltTarget.y += player.center.y * 0.5f;
		bolt.transform.LookAt (boltTarget);

		bolt.GetComponent<Rigidbody> ().AddForce (bolt.transform.forward * 1000);
	}

	public void OnFinishCastingSummon ()
	{
		// instantiate a number of skeletons at summon locations
		List<Transform> spawnPositions = new List<Transform> (skeletonSummonLocations);
		for (int i = 0; i < numSkeletonsPerSummon; i++) {
			if (spawnPositions.Count > 0) {
				Transform spawnPosition = spawnPositions [Random.Range (0, spawnPositions.Count)];
				if (Random.Range (0, 100) < meleeSkeletonSpawnChance) {
					SpawnMeleeSkeleton (spawnPosition.position);
				} else {
					SpawnRangedSkeleton (spawnPosition.position);
				}
				spawnPositions.Remove (spawnPosition);
			}			
		}
	}

	private void SpawnMeleeSkeleton (Vector3 position)
	{
		GameObject skeleton = Instantiate (meleeSkeletonPrefab, position, Quaternion.identity);
		SkeletonController skeletonController = skeleton.GetComponent<SkeletonController> ();
		skeletonController.engageOnSpawn = true;
		skeletonController.spawnInSight = true;
	}

	private void SpawnRangedSkeleton (Vector3 position)
	{
		GameObject skeleton = Instantiate (rangedSkeletonPrefab, position, Quaternion.identity);
		SkeletonRangedController skeletonController = skeleton.GetComponent<SkeletonRangedController> ();
		skeletonController.engageOnSpawn = true;
		skeletonController.spawnInSight = true;
	}

	public void OnFinishCastingExplosion ()
	{
		// instantiate explosion underneath player
		Vector3 targetPos = ActualPlayerPosition ();
//		Vector3 targetPos = GameObject.Find ("Capsule").transform.position;
		Instantiate (darkExplosionPrefab, targetPos, darkExplosionPrefab.transform.rotation);
	}

	private void DestroyChargeEffects ()
	{
		if (leftChargeEffect != null) {
			Destroy (leftChargeEffect);
			leftChargeEffect = null;
		}
		if (rightChargeEffect != null) {
			Destroy (rightChargeEffect);
			rightChargeEffect = null;
		}
	}

	/**
	 * Animation State Helpers
	 */
	private bool IsInAnimationIdle ()
	{
		return anim.GetCurrentAnimatorStateInfo (0).IsName ("Idle");
	}

	private bool IsInAnimationMove ()
	{
		return anim.GetCurrentAnimatorStateInfo (0).IsName ("Moving");
	}

	void OnAnimatorMove ()
	{
		transform.position = agent.nextPosition;
	}
}
