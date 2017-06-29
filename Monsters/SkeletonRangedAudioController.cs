using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkeletonRangedAudioController : MonsterSoundFxController
{

	public AudioClip[] arrowPullBack;
	public AudioClip[] arrowRelease;

	public void PlayArrowPullBack ()
	{
		if (arrowPullBack.Length == 0) {
			return;
		}

		audioSource.clip = arrowPullBack [Random.Range (0, arrowPullBack.Length)];
		audioSource.Play ();
	}

	public void PlayArrowRelease ()
	{
		if (arrowRelease.Length == 0) {
			return;
		}
			
		audioSource.clip = arrowRelease [Random.Range (0, arrowRelease.Length)];
		audioSource.Play ();
	}
}
