using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

[RequireComponent (typeof(Collider))]
[RequireComponent (typeof(AudioSource))]
public class MonsterWeaponCollider : MonoBehaviour
{
	public float damage = 1f;
	public float hitCooldown = 0.5f;
	public List<AudioClip> weaponHitSounds;
	private PlayerHealth playerHealth = null;
	private Collider weaponCollider = null;
	private AudioSource audioSource = null;
	private float lastHitTime;

	void Start ()
	{
		PopulateObjects ();
	}

	void OnEnable ()
	{
		PopulateObjects ();
		weaponCollider.enabled = true;
		lastHitTime = 0;
	}

	void OnDisable ()
	{
		weaponCollider.enabled = false;
	}

	void OnTriggerEnter (Collider other)
	{
		if (Time.time < lastHitTime + hitCooldown) {
			return;
		}

		if (VRTK_PlayerObject.IsPlayerObject (other.gameObject, VRTK_PlayerObject.ObjectTypes.CameraRig)) {
			lastHitTime = Time.time;
			playerHealth.GetHit (damage, ElementType.Physical);
			PlayWeaponHitSound ();
		}
	}

	private void PlayWeaponHitSound ()
	{
		if (weaponHitSounds.Count > 0) {
			audioSource.clip = weaponHitSounds [Random.Range (0, weaponHitSounds.Count)];
			audioSource.Play ();
		}
	}

	private void PopulateObjects ()
	{
		if (playerHealth == null) {
			playerHealth = FindObjectOfType<PlayerHealth> ();
		}
		if (weaponCollider == null) {
			weaponCollider = GetComponent<Collider> ();
		}
		if (audioSource == null) {
			audioSource = GetComponent<AudioSource> ();
		}
	}
}
