using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

/**
 * Class that keeps track of the player's current health.
 * 
 * In addition to this, it applies a red blink when a player is hit, and a fade to black
 * and muffle sounds effect when the player dies.
 */ 
public class PlayerHealth : MonoBehaviour
{
	public float maxHitpoints;
	public VRTK_HeadsetFade headsetFade;
	public float deathAnimationDuration = 2f;

	public delegate void HitpointsUpdatedAction (float hitpoints, float maxHitpoints);

	public event HitpointsUpdatedAction OnHitpointsUpdated;

	public delegate void PlayerDiedAction ();

	public event PlayerDiedAction OnPlayerDied;

	public delegate void PlayerRevivedAction ();

	public event PlayerRevivedAction OnPlayerRevived;

	private float _hitpoints;

	public float hitpoints { 
		get {
			return _hitpoints; 
		} 
		private set {
			_hitpoints = value;
			if (OnHitpointsUpdated != null) {
				OnHitpointsUpdated (_hitpoints, maxHitpoints);
			}
		}
	}

	void Start ()
	{
		hitpoints = maxHitpoints;
	}

	public void GetHit (float damage, ElementType elementType)
	{
		if (hitpoints > 0) {
			// player dies
			if (damage >= hitpoints) {
				Invoke ("FadeToBlack", 0.5f);
				if (OnPlayerDied != null) {
					OnPlayerDied ();
				}
			}

			hitpoints = Mathf.Max (hitpoints - damage, 0f);
			ScreenFlash ();
		}	
	}

	public void Heal (float health)
	{
		// player is revived
		if (hitpoints <= 0f) {
			UnfadeFromBlack ();
			if (OnPlayerRevived != null) {
				OnPlayerRevived ();
			}
		}
		if (hitpoints < maxHitpoints) {
			hitpoints = Mathf.Min (maxHitpoints, hitpoints + health);
		}
	}

	private void ScreenFlash ()
	{
		headsetFade.Fade (Color.red, 0.2f);
		Invoke ("UnScreenFlash", 0.2f);
	}

	private void UnScreenFlash ()
	{
		headsetFade.Unfade (0.2f);
	}

	private void FadeToBlack ()
	{
		headsetFade.Fade (Color.black, deathAnimationDuration);
		StartCoroutine (MuffleSounds ());
	}

	private void UnfadeFromBlack ()
	{
		headsetFade.Unfade (deathAnimationDuration);
		StartCoroutine (UnmuffleSounds ());
	}

	private IEnumerator MuffleSounds ()
	{
		float startTime = Time.time;
		while (Time.time < startTime + deathAnimationDuration) {
			AudioListener.volume = 1f - (Time.time - startTime) / deathAnimationDuration;
			yield return null;
		}

		AudioListener.volume = 0f;
	}

	private IEnumerator UnmuffleSounds ()
	{
		float startTime = Time.time;
		while (Time.time < startTime + deathAnimationDuration) {
			AudioListener.volume = (Time.time - startTime) / deathAnimationDuration;
			yield return null;
		}

		AudioListener.volume = 1f;
	}
}
