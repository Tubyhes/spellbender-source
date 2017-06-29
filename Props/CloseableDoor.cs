using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;
using VRTK.Highlighters;

[RequireComponent (typeof(VRTK_InteractableObject))]
[RequireComponent (typeof(VRTK_OutlineObjectCopyHighlighter))]
public class CloseableDoor : MonoBehaviour
{
	public bool isOpen;
	public bool openClockwise;
	public bool playerUsable;

	public AudioClip doorOpenSound;
	public AudioClip doorCloseSound;
	public AudioClip doorLockedSound;

	public delegate void DoorOpenedAction ();

	public delegate void DoorClosedAction ();

	public delegate void DoorLockedAction ();

	public event DoorOpenedAction OnDoorOpened;
	public event DoorClosedAction OnDoorClosed;
	public event DoorLockedAction OnDoorLocked;

	private GameObject twinkle;
	private bool isMoving;
	private VRTK_InteractableObject interactableObject;
	private float openAngle;
	private float closeAngle;
	private AudioSource audioSource;

	void Awake ()
	{
		audioSource = GetComponentInChildren<AudioSource> ();
		interactableObject = GetComponent<VRTK_InteractableObject> ();
		interactableObject.InteractableObjectUsed += OnUse;	
		twinkle = transform.Find ("DoorOfInterestTwinkle").gameObject;

		if (openClockwise) {
			openAngle = 90;
			closeAngle = -90;
		} else {
			openAngle = -90;
			closeAngle = 90;
		}
	}

	public void OnUse (object interactingObject, VRTK.InteractableObjectEventArgs a)
	{
		if (isMoving) {
			return;
		}

		if (!playerUsable) {
			if (!isOpen && !audioSource.isPlaying) {
				audioSource.clip = doorLockedSound;
				audioSource.Play ();
				if (OnDoorLocked != null) {
					OnDoorLocked ();
				}
			}
			return;
		}

		if (isOpen) {
			StartCoroutine (Close ());
		} else {
			StartCoroutine (Open ());
		}
	}

	public void ForceUse ()
	{
		if (isMoving) {
			return;
		}

		if (isOpen) {
			StartCoroutine (Close ());
		} else {
			StartCoroutine (Open ());
		}
	}

	public void SetDoorOfInterest (bool doorOfInterest)
	{
		if (doorOfInterest && !twinkle.activeInHierarchy) {
			twinkle.SetActive (true);
		}
		if (!doorOfInterest && twinkle.activeInHierarchy) {
			twinkle.SetActive (false);
		}
	}

	private IEnumerator Open ()
	{
		isMoving = true;

		audioSource.clip = doorOpenSound;
		audioSource.Play ();
		if (OnDoorOpened != null) {
			OnDoorOpened ();
		}

		Quaternion origin = transform.rotation;
		Vector3 originAngles = transform.eulerAngles;
		Quaternion target = Quaternion.Euler (originAngles.x, originAngles.y + openAngle, originAngles.z);
		for (float f = 0f; f < 1f; f += Time.deltaTime / 0.5f) {
			transform.rotation = Quaternion.Slerp (origin, target, f);
			yield return null;
		}
		transform.rotation = Quaternion.Slerp (origin, target, 1f);

		isMoving = false;
		isOpen = true;
	}

	private IEnumerator Close ()
	{
		isMoving = true;

		audioSource.clip = doorCloseSound;
		audioSource.Play ();
		if (OnDoorClosed != null) {
			OnDoorClosed ();
		}

		Quaternion origin = transform.rotation;
		Vector3 originAngles = transform.eulerAngles;
		Quaternion target = Quaternion.Euler (originAngles.x, originAngles.y + closeAngle, originAngles.z);
		for (float f = 0f; f < 1f; f += Time.deltaTime / 0.5f) {
			transform.rotation = Quaternion.Slerp (origin, target, f);
			yield return null;
		}
		transform.rotation = Quaternion.Slerp (origin, target, 1f);

		isMoving = false;
		isOpen = false;
	}
}
