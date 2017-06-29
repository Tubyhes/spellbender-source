using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

/**
 * Player Info is the main place of information for the player. It provides an overview of
 * quests, available spells and inventory. PlayerInfoManager controls opening and closing the
 * Player Info.
 * 
 * There are two instances of Player Info, both a canvas attached to a controller. Either 
 * Player Info can be accessed by holding the grip button on the corresponding controller for
 * some time. Upon opening the Player Info, the other controller will turn into a pointer, allowing
 * the player to interact with the Player Info. 
 * 
 * When Player Info is open, teleporting, casting spells and using items is disabled on both 
 * controllers. 
 * 
 * The tooltips for grip buttons will alert the player when new information is available in the
 * Player Info, and will guide the player in opening and closing it.
 */
public class PlayerInfoManager : MonoBehaviour
{
	public float gripHoldTime = 1f;
	public string defaultTooltip = "";
	public string openingTooltip = "Hold to Open";
	public string closeTooltip = "Close";

	public RectTransform leftTooltipFillbar;
	public RectTransform rightTooltipFillbar;

	public GameObject playerInfoViewPrefab;

	private bool available = true;

	public bool Available { 
		get { 
			return available; 
		}
		set {
			if (value != available) {
				available = value;
				if (available) {
					SetAvailable ();
				} else {
					SetUnavailable ();
				}
			}
		}
	}

	public delegate void PlayerInfoViewOpenedAction ();

	public delegate void PlayerInfoViewClosedAction ();

	public event PlayerInfoViewOpenedAction OnPlayerInfoViewOpened;
	public event PlayerInfoViewClosedAction OnPlayerInfoViewClosed;

	private VRTK_SDKManager vrtk_manager;
	private Outfitter outfitter;
	// UI pointers not used outside PlayerInfoManager
	private GameObject rightUIPointer;
	private GameObject leftUIPointer;
	// playerInfo UI elements, not used outside PlayerInfoManager
	private GameObject leftPlayerInfoView;
	private GameObject rightPlayerInfoView;

	private struct PlayerInfoSide
	{
		public VRTK_ControllerEvents events;
		public VRTK_ControllerTooltips tooltips;
		public VRTK_ControllerTooltips tooltipsOther;
		public RectTransform fillBar;
		public GameObject playerInfoView;
		public GameObject uiPointer;
	}

	private PlayerInfoSide left;
	private PlayerInfoSide right;
	private Coroutine openPlayerInfoRoutine = null;

	IEnumerator Start ()
	{
		vrtk_manager = FindObjectOfType<VRTK_SDKManager> ();
		outfitter = FindObjectOfType<Outfitter> ();
		while (!outfitter.setupFinished) {
			yield return null;
		}

		rightUIPointer = vrtk_manager.scriptAliasRightController.transform.Find ("UIPointer").gameObject;
		leftUIPointer = vrtk_manager.scriptAliasLeftController.transform.Find ("UIPointer").gameObject;

		leftPlayerInfoView = Instantiate<GameObject> (playerInfoViewPrefab, Vector3.zero, Quaternion.identity);
		leftPlayerInfoView.transform.SetParent (vrtk_manager.scriptAliasLeftController.transform);
		leftPlayerInfoView.transform.localPosition = new Vector3 (0.27f, 0, -0.03f);
		leftPlayerInfoView.transform.localRotation = playerInfoViewPrefab.transform.rotation;

		rightPlayerInfoView = Instantiate<GameObject> (playerInfoViewPrefab, Vector3.zero, Quaternion.identity);
		rightPlayerInfoView.transform.SetParent (vrtk_manager.scriptAliasRightController.transform);
		rightPlayerInfoView.transform.localPosition = new Vector3 (-0.27f, 0, -0.03f);
		rightPlayerInfoView.transform.localRotation = playerInfoViewPrefab.transform.rotation;

		left = new PlayerInfoSide {
			events = vrtk_manager.scriptAliasLeftController.GetComponent <VRTK_ControllerEvents> (),
			tooltips = outfitter.leftControllerTooltips,
			tooltipsOther = outfitter.rightControllerTooltips,
			fillBar = leftTooltipFillbar,
			playerInfoView = leftPlayerInfoView,
			uiPointer = rightUIPointer
		};
		right = new PlayerInfoSide {
			events = vrtk_manager.scriptAliasRightController.GetComponent <VRTK_ControllerEvents> (),
			tooltips = outfitter.rightControllerTooltips,
			tooltipsOther = outfitter.leftControllerTooltips,
			fillBar = rightTooltipFillbar,
			playerInfoView = rightPlayerInfoView,
			uiPointer = leftUIPointer
		};

		SetAllInactive ();
		if (available) {
			SetAvailable ();
		}
	}

	private void SetAvailable ()
	{
		left.events.GripPressed += OnLeftGripPressed;
		right.events.GripPressed += OnRightGripPressed;
		left.tooltips.UpdateText (VRTK_ControllerTooltips.TooltipButtons.GripTooltip, defaultTooltip);
		right.tooltips.UpdateText (VRTK_ControllerTooltips.TooltipButtons.GripTooltip, defaultTooltip);
	}

	private void SetUnavailable ()
	{
		if (openPlayerInfoRoutine != null) {
			StopCoroutine (openPlayerInfoRoutine);
		}

		SetAllInactive ();
		left.events.GripPressed -= OnLeftGripPressed;
		right.events.GripPressed -= OnRightGripPressed;
		left.tooltips.UpdateText (VRTK_ControllerTooltips.TooltipButtons.GripTooltip, "");
		right.tooltips.UpdateText (VRTK_ControllerTooltips.TooltipButtons.GripTooltip, "");
	}

	public void OnLeftGripPressed (object sender, ControllerInteractionEventArgs args)
	{
		// three options: left info is open, right info is open, no info is open
		if (rightPlayerInfoView.activeInHierarchy || openPlayerInfoRoutine != null) {
			return;
		}
		if (leftPlayerInfoView.activeInHierarchy) {
			left.tooltips.UpdateText (VRTK_ControllerTooltips.TooltipButtons.GripTooltip, defaultTooltip);
			right.tooltips.UpdateText (VRTK_ControllerTooltips.TooltipButtons.GripTooltip, defaultTooltip);
			SetAllInactive ();
			if (OnPlayerInfoViewClosed != null) {
				OnPlayerInfoViewClosed ();
			}
		} else {
			openPlayerInfoRoutine = StartCoroutine (OpenPlayerInfo (left));
		}
	}

	public void OnRightGripPressed (object sender, ControllerInteractionEventArgs args)
	{
		if (leftPlayerInfoView.activeInHierarchy || openPlayerInfoRoutine != null) {
			return;
		}
		if (rightPlayerInfoView.activeInHierarchy) {
			right.tooltips.UpdateText (VRTK_ControllerTooltips.TooltipButtons.GripTooltip, defaultTooltip);
			left.tooltips.UpdateText (VRTK_ControllerTooltips.TooltipButtons.GripTooltip, defaultTooltip);
			SetAllInactive ();
			if (OnPlayerInfoViewClosed != null) {
				OnPlayerInfoViewClosed ();
			}
		} else {
			openPlayerInfoRoutine = StartCoroutine (OpenPlayerInfo (right));
		}
	}

	private IEnumerator OpenPlayerInfo (PlayerInfoSide side)
	{
		string sideTooltipTmp = side.tooltips.gripText;
		string othersideTooltipTmp = side.tooltipsOther.gripText;
		side.tooltips.UpdateText (VRTK_ControllerTooltips.TooltipButtons.GripTooltip, openingTooltip);
		side.tooltipsOther.UpdateText (VRTK_ControllerTooltips.TooltipButtons.GripTooltip, "");
		side.fillBar.localScale = new Vector3 (0f, 1f, 1f);
		float startTime = Time.time;
		while (Time.time < startTime + gripHoldTime) {
			if (!side.events.gripPressed) {
				openPlayerInfoRoutine = null;
				side.tooltips.UpdateText (VRTK_ControllerTooltips.TooltipButtons.GripTooltip, sideTooltipTmp);
				side.tooltipsOther.UpdateText (VRTK_ControllerTooltips.TooltipButtons.GripTooltip, othersideTooltipTmp);
				side.fillBar.localScale = new Vector3 (0f, 1f, 1f);
				yield break;
			}
			side.fillBar.localScale = new Vector3 ((Time.time - startTime) / gripHoldTime, 1f, 1f);
			yield return null;
		}

		side.playerInfoView.SetActive (true);
		side.uiPointer.SetActive (true);
		if (OnPlayerInfoViewOpened != null) {
			OnPlayerInfoViewOpened ();
		}
		side.fillBar.localScale = new Vector3 (0f, 1f, 1f);
		side.tooltips.UpdateText (VRTK_ControllerTooltips.TooltipButtons.GripTooltip, closeTooltip);
		openPlayerInfoRoutine = null;
	}

	private void SetAllInactive ()
	{
		leftPlayerInfoView.SetActive (false);
		rightPlayerInfoView.SetActive (false);
		leftUIPointer.SetActive (false);
		rightUIPointer.SetActive (false);
	}
}
