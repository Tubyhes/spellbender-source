using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Dialogue
{
	[System.Serializable]
	public class Entry
	{
		public string actor;
		public string text;
		public string audioFile;
		public bool autoContinue;
		public bool audioOnly;
		private AudioClip track;

		public void LoadAudioClip ()
		{
			if (track == null) {
				Debug.Log ("Track == null, load it now");
				track = Resources.Load<AudioClip> (audioFile);
				if (track == null) {
					Debug.Log ("Loading track failed: " + audioFile);
				}
			}
		}

		public AudioClip GetAudioClip ()
		{
			Debug.Log ("Loading Audio Clip: " + audioFile);
			LoadAudioClip ();
			return track;
		}
	}

	public Entry[] entries;
}
