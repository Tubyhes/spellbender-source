using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class MusicManager : MonoBehaviour
{
	public AudioMixerSnapshot neutral;
	public AudioMixerSnapshot danger;
	public AudioMixerSnapshot combat;
	public AudioMixerSnapshot bossfight;

	public AudioMixerSnapshot transitionNeutralDanger;
	public AudioMixerSnapshot transitionNeutralCombat;
	public AudioMixerSnapshot transitionNeutralBossfight;
	public AudioMixerSnapshot transitionDangerCombat;
	public AudioMixerSnapshot transitionDangerBossfight;
	public AudioMixerSnapshot transitionCombatBossfight;

	public float transitionTime = 1f;

	private AudioMixerSnapshot currentState;
	private IEnumerator currentTransition = null;

	void Start ()
	{
		currentState = neutral;
	}

	public void TransitionToNeutral ()
	{
		if (currentState == neutral) {
			return;
		}

		AudioMixerSnapshot transitionSnapshot = null;
		if (currentState == danger) {
			transitionSnapshot = transitionNeutralDanger;
		} else if (currentState == combat) {
			transitionSnapshot = transitionNeutralCombat;
		} else if (currentState == bossfight) {
			transitionSnapshot = transitionNeutralBossfight;
		}

		if (currentTransition != null) {
			StopCoroutine (currentTransition);
		}
		currentTransition = MusicTransition (transitionSnapshot, neutral);
		StartCoroutine (currentTransition);
		currentState = neutral;
	}

	public void TransitionToDanger ()
	{
		if (currentState == danger) {
			return;
		}

		AudioMixerSnapshot transitionSnapshot = null;
		if (currentState == neutral) {
			transitionSnapshot = transitionNeutralDanger;
		} else if (currentState == combat) {
			transitionSnapshot = transitionDangerCombat;
		} else if (currentState == bossfight) {
			transitionSnapshot = transitionDangerBossfight;
		}

		if (currentTransition != null) {
			StopCoroutine (currentTransition);
		}
		currentTransition = MusicTransition (transitionSnapshot, danger);
		StartCoroutine (currentTransition);

		currentState = danger;
	}

	public void TransitionToCombat ()
	{
		if (currentState == combat) {
			return;
		}

		AudioMixerSnapshot transitionSnapshot = null;
		if (currentState == neutral) {
			transitionSnapshot = transitionNeutralCombat;
		} else if (currentState == danger) {
			transitionSnapshot = transitionDangerCombat;
		} else if (currentState == bossfight) {
			transitionSnapshot = transitionCombatBossfight;
		}

		if (currentTransition != null) {
			StopCoroutine (currentTransition);
		}
		currentTransition = MusicTransition (transitionSnapshot, combat);
		StartCoroutine (currentTransition);

		currentState = combat;
	}

	public void TransitionToBossfight ()
	{
		if (currentState == bossfight) {
			return;
		}

		AudioMixerSnapshot transitionSnapshot = null;
		if (currentState == neutral) {
			transitionSnapshot = transitionNeutralBossfight;
		} else if (currentState == danger) {
			transitionSnapshot = transitionDangerBossfight;
		} else if (currentState == combat) {
			transitionSnapshot = transitionCombatBossfight;
		}

		if (currentTransition != null) {
			StopCoroutine (currentTransition);
		}
		currentTransition = MusicTransition (transitionSnapshot, bossfight);
		StartCoroutine (currentTransition);

		currentState = bossfight;
	}

	private IEnumerator MusicTransition (AudioMixerSnapshot transition, AudioMixerSnapshot target)
	{
		transition.TransitionTo (transitionTime);
		yield return new WaitForSeconds (transitionTime);
		target.TransitionTo (transitionTime);
		currentTransition = null;
	}
}


