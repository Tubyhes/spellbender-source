using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent (typeof(NavMeshAgent))]
public class Alfred : MonoBehaviour
{
	public delegate void DestinationReachedAction ();

	public event DestinationReachedAction OnDestinationReached;

	public GameObject conversationCanvas;

	private AnimatedText animatedText;
	private AudioSource voice;
	private NavMeshAgent agent;
	private MeshRenderer meshRenderer;

	private string rimColorProperty = "_RimColor";
	private Color rimColorBlue = new Color (0, 192f / 255f, 1f, 1f);
	private Color rimColorRed = new Color (236f / 255f, 0f, 0f, 1f);

	void Start ()
	{
		voice = GetComponentInChildren<AudioSource> ();
		animatedText = conversationCanvas.GetComponentInChildren<AnimatedText> ();
		agent = GetComponent <NavMeshAgent> ();
		meshRenderer = GetComponentInChildren<MeshRenderer> ();
	}

	public void SetColorBlue ()
	{
		meshRenderer.material.SetColor (rimColorProperty, rimColorBlue);
	}

	public void SetColorRed ()
	{
		meshRenderer.material.SetColor (rimColorProperty, rimColorRed);
	}

	public float Speak (Dialogue.Entry entry)
	{
		AudioClip clip = entry.GetAudioClip ();
		float durationPerLetter = (clip != null && !entry.audioOnly) ? clip.length / entry.text.Length : 0.1f;

		if (entry.audioOnly && conversationCanvas.activeInHierarchy) {
			conversationCanvas.SetActive (false);
		} else if (!entry.audioOnly) {
//			if (!conversationCanvas.activeInHierarchy) {
//				conversationCanvas.SetActive (true);
//			}
//			animatedText.ShowText (entry.text, durationPerLetter);
		}

		if (clip != null) {
			voice.clip = clip;
			voice.Play ();
			return clip.length;
		}

		return entry.text.Length * 0.1f;
	}

	public void Move (Vector3 position, Quaternion rotation, float stoppingDistance = 0f)
	{
		if (conversationCanvas.activeInHierarchy) {
			conversationCanvas.SetActive (false);
		}		
		if (stoppingDistance != agent.stoppingDistance) {
			agent.stoppingDistance = stoppingDistance;
		}

		StartCoroutine (MoveRoutine (position, rotation));
	}

	public void CloseConversationCanvas ()
	{
		if (conversationCanvas.activeInHierarchy) {
			conversationCanvas.SetActive (false);
		}
	}

	private IEnumerator MoveRoutine (Vector3 position, Quaternion rotation)
	{
		agent.SetDestination (position);
		agent.isStopped = false;
		while (Vector3.Distance (transform.position, agent.destination) > agent.stoppingDistance) {
			yield return null;
		}
		agent.isStopped = true;

		transform.eulerAngles = rotation.eulerAngles;
		if (OnDestinationReached != null) {
			OnDestinationReached ();
		}
	}
	 
}