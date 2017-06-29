using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;
using VRTK.Highlighters;

public class CloseableChest : MonoBehaviour
{
	public bool isOpen;
	public bool playerUsable;
	public bool disableAfterOpen;
	public List<GameObject> contents;

	public AudioClip chestOpenSound;
	public AudioClip chestCloseSound;
	public AudioClip chestLockedSound;

	public delegate void ChestOpenedAction ();

	public delegate void ChestClosedAction ();

	public delegate void ChestLockedAction ();

	public event ChestOpenedAction OnChestOpened;
	public event ChestClosedAction OnChestClosed;
	public event ChestLockedAction OnChestLocked;

	private bool isMoving;
	private VRTK_InteractableObject interactableObject;
	private Animation anim;

	void Start ()
	{
		anim = GetComponent<Animation> ();
		interactableObject = GetComponent<VRTK_InteractableObject> ();
		interactableObject.InteractableObjectUsed += OnUse;

		if (!isOpen) {
			DisableContents ();
		}
	}

	public void OnUse (object interactingObject, VRTK.InteractableObjectEventArgs a)
	{
		if (anim.isPlaying) {
			return;
		}

		if (!playerUsable) {
			if (!isOpen) {
				if (chestLockedSound != null) {
					AudioSource.PlayClipAtPoint (chestLockedSound, transform.position);
				}
				if (OnChestLocked != null) {
					OnChestLocked ();
				}
			}
			return;
		}

		if (isOpen) {
			if (chestCloseSound != null) {
				AudioSource.PlayClipAtPoint (chestCloseSound, transform.position);
			}
			if (OnChestClosed != null) {
				OnChestClosed ();
			}
			anim.Play ("woodenchest_large_close");
			isOpen = false;
			Invoke ("DisableContents", 1f);
		} else {
			if (chestOpenSound != null) {
				AudioSource.PlayClipAtPoint (chestOpenSound, transform.position);
			}
			if (OnChestOpened != null) {
				OnChestOpened ();
			}
			Debug.Log ("Open chest!");
			anim.Play ("woodenchest_large_open");
			isOpen = true;
			EnableContents ();
			if (disableAfterOpen) {
				interactableObject.enabled = false;
				GetComponent<VRTK_OutlineObjectCopyHighlighter> ().enabled = false;
			}
		}
	}

	private void DisableContents ()
	{
		if (contents != null) {
			foreach (GameObject o in contents) {
				o.SetActive (false);
			}
		}
	}

	private void EnableContents ()
	{
		if (contents != null) {
			foreach (GameObject o in contents) {
				o.SetActive (true);
			}
		}
	}
}


