using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using VRTK;

[RequireComponent (typeof(Animator))]
[RequireComponent (typeof(NavMeshAgent))]
public abstract class MonsterController : MonoBehaviour
{
	public delegate void MonsterDiedAction (MonsterController monsterController);

	public event MonsterDiedAction OnMonsterDied;

	// generic monster config
	public bool engageOnSpawn = false;
	public bool spawnInSight = false;
	public bool engageOnSight = false;
	public float maxHitpoints = 10f;

	// current monster hitpoints
	protected float hitpoints = 0;

	// internal references
	public Transform monsterHead;
	public LayerMask ignoreVisionLayerMask;
	protected Animator anim;
	protected NavMeshAgent agent;

	// player references
	protected CapsuleCollider player;

	private DamageText damageTextPrefab;
	private MonsterHealthIndicator healthIndicator;

	public virtual void Awake ()
	{
		hitpoints = maxHitpoints;
		anim = GetComponent<Animator> ();
		agent = GetComponent<NavMeshAgent> ();
		damageTextPrefab = Resources.Load<DamageText> ("FloatingDamageText");
	}

	public virtual void Start ()
	{
		player = GameObject.FindGameObjectWithTag ("Player").GetComponent<CapsuleCollider> ();
		SetupHealthIndicator ();
	}

	public abstract void GetHit (float dmg, ElementType elementType);

	public virtual void HandleDamage (float dmg)
	{
		hitpoints = Mathf.Max (hitpoints - dmg, 0f);
		if (dmg > 0f) {
			ShowDamage (dmg);
			healthIndicator.SetHitpoints (hitpoints, maxHitpoints);
		}
		if (hitpoints <= 0f) {
			OnDied ();
		}
	}

	protected void ShowDamage (float dmg)
	{
		Vector3 textPosition = transform.position;
		textPosition.y += agent.height * transform.localScale.y;
		DamageText damageText = Instantiate (damageTextPrefab, textPosition, Quaternion.identity, transform);
		damageText.text.text = Mathf.Floor (dmg).ToString ();
	}

	protected virtual void OnDied ()
	{
		Destroy (GetComponent<Collider> ());
		if (OnMonsterDied != null) {
			OnMonsterDied (this);
		}
	}

	protected virtual float DamageModifiers (float dmg, ElementType elementType)
	{
		return dmg;
	}

	protected Vector3 ActualPlayerPosition ()
	{
		Vector3 pos = player.transform.position;
		pos.x += player.center.x;
		pos.z += player.center.z;
		return pos;
	}

	protected void SetDestinationToPlayer ()
	{
		agent.SetDestination (ActualPlayerPosition ());
	}

	protected void LookAtTarget ()
	{
		if (player != null) {
			Vector3 lookAtTarget = ActualPlayerPosition () - transform.position;
			lookAtTarget.y = 0;
			Quaternion targetRotation = Quaternion.LookRotation (lookAtTarget);
			transform.rotation = targetRotation;
		}
	}

	protected bool PlayerInLineOfSight ()
	{
		if (player == null || monsterHead == null) {
			return false;
		}

		Vector3 playerPos = player.transform.position + player.center;

		if (Physics.Linecast (monsterHead.position, playerPos, ignoreVisionLayerMask, QueryTriggerInteraction.Ignore)) {
			return false;
		} else {
			return true;
		}
	}

	protected IEnumerator PlayAnimation (string stateName)
	{
		anim.Play (stateName);
		yield return null;
		while (anim.GetCurrentAnimatorClipInfo (0).Length == 0) {
			yield return null;
		}
		yield return new WaitForSeconds (anim.GetCurrentAnimatorClipInfo (0) [0].clip.length);
	}

	public bool IsAlive ()
	{
		return hitpoints > 0;
	}

	private void SetupHealthIndicator ()
	{
		GameObject prefab = Resources.Load<GameObject> ("MonsterHealthIndicator");
		GameObject indicator = Instantiate (prefab, transform, false);
		indicator.transform.localPosition = new Vector3 (0f, GetComponent<CapsuleCollider> ().height * 1.1f, 0f);
		healthIndicator = indicator.GetComponent<MonsterHealthIndicator> ();
		healthIndicator.SetHitpoints (hitpoints, maxHitpoints);
	}

}