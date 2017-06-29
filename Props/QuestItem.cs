using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VRTK;
using VRTK.Highlighters;

/**
 * This class turns props into quest items. 
 * Quest items can be assigned a quest id, and will automatically complete the associated quest
 * when they are pickud up. Quest items have an exclamation mark bobbing up and down above them
 * to draw attention (loosely borrowed from WoW). 
 * Additionally, they can display some text and play a sound when picked up.
 */
[RequireComponent (typeof(VRTK_OutlineObjectCopyHighlighter))]
[RequireComponent (typeof(Collider))]
public class QuestItem : MonoBehaviour
{
	public string itemName;
	public AudioClip itemPickupSoundFX;
	public bool startPlayerUsable = false;
	public bool showExclamationMark = false;
	public float hoverHeight = 0f;

	public delegate void ItemPickedUpAction (string itemName);

	public event ItemPickedUpAction OnItemPickedUp;

	private bool playerUsable = false;
	private int quest_id = 0;

	private VRTK_InteractableObject interactableObject;
	private GameObject exclamationMarkPrefab;
	private GameObject exclamationMark;
	private GameObject pickupTextPrefab;

	void Awake ()
	{
		if (showExclamationMark) {
			exclamationMarkPrefab = Resources.Load<GameObject> ("ExclamationMark");
		}

		if (startPlayerUsable) {
			SetPlayerUsable (0);
		}
	}

	void Start ()
	{

	}

	public void OnUse (object interactingObject, VRTK.InteractableObjectEventArgs a)
	{
		Destroy (interactableObject);

		if (OnItemPickedUp != null) {
			OnItemPickedUp (itemName);
		}

		FindObjectOfType<QuestManager> ().CompleteQuest (quest_id);

		if (itemPickupSoundFX != null) {
			AudioSource.PlayClipAtPoint (itemPickupSoundFX, transform.position);
		}
		MakePickupText ();

		Destroy (gameObject);
		if (exclamationMark != null) {
			Destroy (exclamationMark);
		}
	}

	public void SetPlayerUsable (int questId)
	{
		quest_id = questId;

		if (playerUsable) {
			return;
		}

		interactableObject = gameObject.AddComponent<VRTK_InteractableObject> ();
		interactableObject.isUsable = true;
		interactableObject.touchHighlightColor = new Color (54f / 255f, 64f / 255f, 1f, 1f);
		interactableObject.useOverrideButton = VRTK_ControllerEvents.ButtonAlias.Trigger_Click;
		interactableObject.InteractableObjectUsed += OnUse;

		if (showExclamationMark) {
			MakeExclamationMark ();
		}
		playerUsable = true;
	}

	private void MakeExclamationMark ()
	{
		exclamationMark = Instantiate (exclamationMarkPrefab, transform.position, Quaternion.identity);
		exclamationMark.GetComponent<BobUpAndDown> ().hoverHeight = hoverHeight;
		if (!gameObject.activeSelf) {
			exclamationMark.SetActive (false);
		}
	}

	private void MakePickupText ()
	{
		GameObject pickupText = Instantiate (Resources.Load<GameObject> ("ItemPickupText"), transform.position, Quaternion.identity);
		pickupText.GetComponentInChildren<Text> ().text = itemName;
	}

	public void OnDisable ()
	{
		if (exclamationMark != null) {
			exclamationMark.SetActive (false);
		}
	}

	public void OnEnable ()
	{
		if (exclamationMark != null) {
			exclamationMark.SetActive (true);
		}
	}
}
