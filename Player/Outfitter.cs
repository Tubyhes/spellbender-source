using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;
using VRTK;

/**
 * Outfitter is the central hub for player actions and whether or not they are available. 
 * On game start, it waits until both controllers are on and tracking. 
 * 
 * Depending on the current state of the player, outfitter enables and disables the different
 * action player can undertake:
 * - Teleport
 * - Open Player Info
 * - Cast spells
 * - Interact with objects
 */
public class Outfitter : MonoBehaviour
{
	[System.Serializable]
	public enum OutfitterMode
	{
		EXPLORE,
		INACTIVE,
		PLAYERINFO
	}

	public VRTK_SDKManager vrtk_manager;

	// controller tooltips, used by level managers
	[HideInInspector]
	public VRTK_ControllerTooltips rightControllerTooltips;
	[HideInInspector]
	public VRTK_ControllerTooltips leftControllerTooltips;
	// teleporter element, used by monsters to be notified of teleport event
	[HideInInspector]
	public GameObject rightTeleporter;
	[HideInInspector]
	public GameObject leftTeleporter;

	// boolean to check whether outfitter is good to go
	[HideInInspector]
	public bool setupFinished = false;

	[HideInInspector]
	public bool castingAvailable = true;

	private bool casting = false;

	public bool Casting {
		get {
			return casting;
		}
		set {
			if (value != casting) {
				casting = value;
				if (!casting && castingAvailable) {
					EnableTeleporting ();
				} else {
					DisableTeleporting ();
				}
			}
		}
	}

	// player info manager, only used in outfitter
	private PlayerInfoManager playerInfoManager;

	private OutfitterMode currentMode = OutfitterMode.EXPLORE;
	private OutfitterMode previousMode;
	private VRTK_InteractTouch leftInteractTouch;
	private VRTK_InteractTouch rightInteractTouch;
	private VRTK_InteractUse leftInteractUse;
	private VRTK_InteractUse rightInteractUse;

	IEnumerator Start ()
	{
		while (vrtk_manager.modelAliasLeftController.transform.Find ("body") == null || vrtk_manager.modelAliasRightController.transform.Find ("body") == null) {
			yield return new WaitForSeconds (0.1f);
		}

		leftInteractTouch = vrtk_manager.scriptAliasLeftController.GetComponent<VRTK_InteractTouch> ();
		rightInteractTouch = vrtk_manager.scriptAliasRightController.GetComponent<VRTK_InteractTouch> ();
		leftInteractUse = vrtk_manager.scriptAliasLeftController.GetComponent<VRTK_InteractUse> ();
		rightInteractUse = vrtk_manager.scriptAliasRightController.GetComponent<VRTK_InteractUse> ();
		rightControllerTooltips = vrtk_manager.scriptAliasRightController.GetComponentInChildren<VRTK_ControllerTooltips> ();
		leftControllerTooltips = vrtk_manager.scriptAliasLeftController.GetComponentInChildren<VRTK_ControllerTooltips> ();

		playerInfoManager = FindObjectOfType<PlayerInfoManager> ();
		rightTeleporter = vrtk_manager.scriptAliasRightController.transform.Find ("Teleporter").gameObject;
		leftTeleporter = vrtk_manager.scriptAliasLeftController.transform.Find ("Teleporter").gameObject;

		FindObjectOfType<PlayerHealth> ().OnPlayerDied += OnPlayerDied;
		FindObjectOfType<PlayerHealth> ().OnPlayerRevived += OnPlayerRevived;
		playerInfoManager.OnPlayerInfoViewOpened += OnPlayerInfoOpenend;
		playerInfoManager.OnPlayerInfoViewClosed += OnPlayerInfoClosed;

		SetOutfitterMode (OutfitterMode.EXPLORE);

		setupFinished = true;
	}

	public void SetOutfitterMode (OutfitterMode mode)
	{
		previousMode = currentMode;
		currentMode = mode;
		if (currentMode == OutfitterMode.EXPLORE) {
			SetOutfitterModeExplore ();
		} else if (currentMode == OutfitterMode.INACTIVE) {
			
			SetOutfitterModeInactive ();
		} else if (currentMode == OutfitterMode.PLAYERINFO) {
			SetOutfitterModePlayerInfo ();
		}
	}

	private void SetOutfitterModeExplore ()
	{
		castingAvailable = true;
		playerInfoManager.Available = true;
		EnableTeleporting ();
		EnableObjectInteraction ();
	}

	private void SetOutfitterModeInactive ()
	{
		castingAvailable = false;
		playerInfoManager.Available = false;
		DisableTeleporting ();
		DisableObjectInteraction ();
	}

	private void SetOutfitterModePlayerInfo ()
	{
		castingAvailable = false;
		DisableTeleporting ();
		DisableObjectInteraction ();
	}

	private void EnableObjectInteraction ()
	{
		leftInteractTouch.enabled = true;
		leftInteractUse.enabled = true;
		rightInteractTouch.enabled = true;
		rightInteractUse.enabled = true;
	}

	private void DisableObjectInteraction ()
	{
		leftInteractTouch.enabled = false;
		leftInteractUse.enabled = false;
		rightInteractTouch.enabled = false;
		rightInteractUse.enabled = false;
	}

	private void EnableTeleporting ()
	{
		rightTeleporter.SetActive (true);
		leftTeleporter.SetActive (true);
	}

	private void DisableTeleporting ()
	{
		rightTeleporter.SetActive (false);
		leftTeleporter.SetActive (false);
	}

	public void OnPlayerDied ()
	{
		SetOutfitterMode (OutfitterMode.INACTIVE);
	}

	public void OnPlayerRevived ()
	{
		SetOutfitterMode (OutfitterMode.EXPLORE);
	}

	public void OnPlayerInfoOpenend ()
	{
		SetOutfitterMode (OutfitterMode.PLAYERINFO);
	}

	public void OnPlayerInfoClosed ()
	{
		SetOutfitterMode (previousMode);
	}

}
