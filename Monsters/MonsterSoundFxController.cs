using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSoundFxController : MonoBehaviour
{
	public AudioSource audioSource;
	public AudioClip[] footstep;
	public AudioClip[] bodyHitFloor;

	public void PlayFootStepSound ()
	{
		if (footstep.Length == 0) {
			return;
		}
		audioSource.clip = footstep [Random.Range (0, footstep.Length)];
		audioSource.Play ();
	}

	public void PlayBodyHitFloor ()
	{
		if (bodyHitFloor.Length == 0) {
			return;
		}
		audioSource.clip = bodyHitFloor [Random.Range (0, bodyHitFloor.Length)];
		audioSource.Play ();
	}

}
