using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using VRTK;

public class LevelManager : MonoBehaviour
{
	// waypoints
	public PlayerWaypoint trainingRoomWaypoint;
	public PlayerWaypoint fourthFloorWaypoint;
	public PlayerWaypoint masterRoomWaypoint;
	public PlayerWaypoint kitchenHallWaypoint;
	public PlayerWaypoint mainHallWaypoint;
	public List<Transform> alfredWaypoints;
	public Transform necromancerWaypoint;

	// monster spawn points
	public List<Transform> introSpawnPoints;
	public List<Transform> rangedLingerSpawnPoints;
	public List<Transform> meleeLingerSpawnPoints;
	public List<Transform> backpackEventSpawnPoints;
	public List<Transform> servantRoomEventSpawnPoints;
	public List<Transform> guardRoomEventSpawnPoints;
	public List<Transform> necromancerEventSpawnPoints;
	public Transform necromancerSpawnPoint;
	public Transform targetDummySpawnPoint;

	// doors we have to operate in the script
	public CloseableDoor trainingRoomDoor;
	public CloseableDoor masterRoomDoor;
	public CloseableDoor kitchenDoor;
	public CloseableDoor guardRoomDoor;
	public CloseableDoor guestRoomDoor;
	public CloseableDoor exitDoor;
	public CloseableDoor StaircaseDoor;
	public List<CloseableDoor> servantRoomDoors;
	public List<CloseableDoor> mainHallDoors;

	// quest items we have to operate
	public QuestItem arcaneCrystal;
	public QuestItem darkCrystal;
	public QuestItem backpack;
	public QuestItem food;
	public QuestItem map;
	public QuestItem keys;

	// actors
	public Alfred orb;

	// monster prefabs
	private MonsterManager monsterManager;
	public GameObject targetDummyPrefab;
	public GameObject necromancerPrefab;

	// sound effects
	public AudioClip femaleScream;
	public AudioClip maleScream;

	// healing spell effect prefab
	public GameObject healingCirclePrefab;

	// monster instantiated
	private GameObject smallArcaneDummy;
	private NecromancerController necromancer;

	// script helpers
	private bool scriptContinue = false;
	private int currentEntry = 0;
	public Dialogue dialogue;
	private float textTime = 0.10f;
	private int questItemsPickedUp = 0;

	// reference to external singletons
	private Outfitter outfitter;
	private PlayerHealth playerHealth;
	private QuestManager questManager;
	private MusicManager musicManager;

	IEnumerator Start ()
	{
		outfitter = FindObjectOfType<Outfitter> ();
		playerHealth = FindObjectOfType<PlayerHealth> ();
		questManager = FindObjectOfType<QuestManager> ();
		musicManager = FindObjectOfType<MusicManager> ();
		monsterManager = GetComponent<MonsterManager> ();

		TextAsset guiTextsJson = Resources.Load<TextAsset> ("Dialogue1.2");
		dialogue = JsonUtility.FromJson<Dialogue> (guiTextsJson.text);
		foreach (Dialogue.Entry entry in dialogue.entries) {
			entry.LoadAudioClip ();
		}

		while (!outfitter.setupFinished) {
			yield return new WaitForSeconds (0.1f);
		}
		StartCoroutine (PlayScript ());
	}


	private IEnumerator PlayScript ()
	{
		outfitter.rightTeleporter.GetComponent<VRTK_BasicTeleport> ().Teleported += OnFirstPlayerTeleport;
		outfitter.leftTeleporter.GetComponent<VRTK_BasicTeleport> ().Teleported += OnFirstPlayerTeleport;
		outfitter.rightControllerTooltips.UpdateText (VRTK_ControllerTooltips.TooltipButtons.TouchpadTooltip, "Teleport");
		outfitter.leftControllerTooltips.UpdateText (VRTK_ControllerTooltips.TooltipButtons.TouchpadTooltip, "Teleport");

		trainingRoomWaypoint.OnPlayerInWaypointRange += OnPlayerWaypointReached;
		yield return StartCoroutine (WaitForCondition ());

		yield return StartCoroutine (PlayConversation ());

		outfitter.rightControllerTooltips.UpdateText (VRTK_ControllerTooltips.TooltipButtons.TriggerTooltip, "Use Objects");
		outfitter.leftControllerTooltips.UpdateText (VRTK_ControllerTooltips.TooltipButtons.TriggerTooltip, "Use Objects");

		while (trainingRoomDoor.isOpen) {
			yield return null;
		}
		outfitter.rightControllerTooltips.UpdateText (VRTK_ControllerTooltips.TooltipButtons.TriggerTooltip, "");
		outfitter.leftControllerTooltips.UpdateText (VRTK_ControllerTooltips.TooltipButtons.TriggerTooltip, "");
		trainingRoomDoor.playerUsable = false;

		yield return new WaitForSeconds (1f);
		yield return StartCoroutine (PlayConversation ());

		int quest_id = 0;
		quest_id = questManager.StartQuest ("Pick up the arcane crystal.");
		arcaneCrystal.SetPlayerUsable (quest_id);
		arcaneCrystal.OnItemPickedUp += OnArcaneCrystalPickedUp;
		yield return StartCoroutine (WaitForCondition ());

		yield return new WaitForSeconds (1f);
		yield return StartCoroutine (PlayConversation ());

		smallArcaneDummy = Instantiate<GameObject> (targetDummyPrefab, targetDummySpawnPoint.position, targetDummySpawnPoint.rotation);
		smallArcaneDummy.GetComponent<MonsterController> ().OnMonsterDied += OnSmallArcaneDummyDied;
		yield return StartCoroutine (WaitForCondition ());

		yield return StartCoroutine (PlayConversation ());
		musicManager.TransitionToDanger ();
		yield return new WaitForSeconds (3.5f); // hardcoded :(
		monsterManager.SpawnMonsterMelee (MonsterManager.MonsterEngageType.Immediate, false, introSpawnPoints [0]);
		trainingRoomDoor.ForceUse ();
		musicManager.TransitionToCombat ();

		yield return new WaitForSeconds (1.5f);
		yield return StartCoroutine (PlayConversation ());

		orb.Move (alfredWaypoints [0].position, alfredWaypoints [0].rotation);

		while (monsterManager.ExistLivingMonsters ()) {
			yield return null;
		}

		monsterManager.SpawnMonsterMelee (MonsterManager.MonsterEngageType.Immediate, true, introSpawnPoints [1]);
		yield return new WaitForSeconds (2f);
		monsterManager.SpawnMonsterMelee (MonsterManager.MonsterEngageType.Immediate, true, introSpawnPoints [2]);
		yield return new WaitForSeconds (2f);
		monsterManager.SpawnMonsterMelee (MonsterManager.MonsterEngageType.Immediate, true, introSpawnPoints [0]);
		yield return new WaitForSeconds (4f);
		monsterManager.SpawnMonsterMelee (MonsterManager.MonsterEngageType.Immediate, true, introSpawnPoints [1]);

		while (monsterManager.ExistLivingMonsters ()) {
			yield return null;
		}
		musicManager.TransitionToDanger ();

		yield return new WaitForSeconds (1f);
		yield return StartCoroutine (PlayConversation ());
		yield return new WaitForSeconds (6f); // last line from Alfred is 4.3 seconds, wait for it to finish.

		// Alfred moves to master's room
		orb.Move (alfredWaypoints [1].position, alfredWaypoints [1].rotation);

		// wait for player to move to 4th floor
		fourthFloorWaypoint.OnPlayerInWaypointRange += OnPlayerWaypointFourthFloor;

		// open door to master's room
		masterRoomDoor.ForceUse ();

		// wait till user enters master's room
		masterRoomWaypoint.OnPlayerInWaypointRange += OnPlayerWaypointReached;
		yield return StartCoroutine (WaitForCondition ());
		musicManager.TransitionToCombat ();

		// summon skeletons and let them engage
		monsterManager.SpawnMonsterMelee (MonsterManager.MonsterEngageType.Immediate, true, introSpawnPoints [5]);
		yield return new WaitForSeconds (2f);
		// let Alfred call out for help
		yield return StartCoroutine (PlayConversation ());
		monsterManager.SpawnMonsterRanged (MonsterManager.MonsterEngageType.Immediate, true, introSpawnPoints [6]);
		yield return new WaitForSeconds (2f);
		monsterManager.SpawnMonsterMelee (MonsterManager.MonsterEngageType.Immediate, true, introSpawnPoints [7]);

		// wait for all skellies to die
		while (monsterManager.ExistLivingMonsters ()) {
			yield return null;
		}
		musicManager.TransitionToDanger ();

		// spawn more skeletons in the mansion
		foreach (Transform t in rangedLingerSpawnPoints) {
			monsterManager.SpawnMonsterRanged (MonsterManager.MonsterEngageType.OnSight, false, t);
		}
		foreach (Transform t in meleeLingerSpawnPoints) {
			monsterManager.SpawnMonsterMelee (MonsterManager.MonsterEngageType.OnSight, false, t);
		}

		// enable all events that can happen now
		kitchenHallWaypoint.OnPlayerInWaypointRange += OnPlayerEnteredKitchenHallway;
		kitchenDoor.playerUsable = true;
		kitchenDoor.OnDoorOpened += OnPlayerOpenedKitchenDoor;
		guardRoomDoor.OnDoorLocked += OnPlayerTriedOpenGuardRoomDoor;
		backpack.OnItemPickedUp += OnPlayerPickedUpBackpack;

		// let alfred send the player on his mission through the mansion
		yield return new WaitForSeconds (1f);
		yield return StartCoroutine (PlayConversation ());

		// enable quest items, subscribe to their pickup, wait for them to be picked up
		quest_id = questManager.StartQuest ("Find the dark crystal in one of the chests.");
		darkCrystal.SetPlayerUsable (quest_id);
		darkCrystal.OnItemPickedUp += OnQuestItemPickedUp;

		quest_id = questManager.StartQuest ("Collect some food for the road from the kitchen downstairs.");
		food.SetPlayerUsable (quest_id);
		food.OnItemPickedUp += OnQuestItemPickedUp;

		quest_id = questManager.StartQuest ("Find a map of the surrounding lands. Perhaps in the library?");
		map.SetPlayerUsable (quest_id);
		map.OnItemPickedUp += OnQuestItemPickedUp;

		quest_id = questManager.StartQuest ("Pick up your backpack. You probably left it in your room.");
		backpack.SetPlayerUsable (quest_id);
		backpack.OnItemPickedUp += OnQuestItemPickedUp;

		quest_id = questManager.StartQuest ("Take the keys to the mansion. The house guards usually keep some spares downstairs.");
		keys.SetPlayerUsable (quest_id);
		keys.OnItemPickedUp += OnQuestItemPickedUp;

		yield return WaitForCondition ();
		int meet_alfred_quest_id = questManager.StartQuest ("Meet Alfred at the exit.");

		// wait for player to enter the main hall
		mainHallWaypoint.OnPlayerInWaypointRange += OnPlayerEnteredMainHall;
		yield return WaitForCondition ();

		// disable player teleportation 
		outfitter.SetOutfitterMode (Outfitter.OutfitterMode.INACTIVE);

		// close all doors, let necromancer emerge from small meeting room
		foreach (CloseableDoor door in mainHallDoors) {
			if (door.isOpen) {
				door.ForceUse ();
			}
			door.playerUsable = false;
		}

		// also let Alfred move in position for when player dies
		if (!masterRoomDoor.isOpen) {
			masterRoomDoor.ForceUse ();
		}
		orb.Move (alfredWaypoints [2].position, Quaternion.identity);

		// spawn necromancer and let him insult player
		GameObject o = Instantiate<GameObject> (necromancerPrefab, necromancerSpawnPoint.position, Quaternion.identity);
		necromancer = o.GetComponent<NecromancerController> ();
		necromancer.skeletonSummonLocations = necromancerEventSpawnPoints;
		yield return new WaitForSeconds (necromancer.fadeInTime);
		yield return new WaitForSeconds (necromancer.Speak (0));

		necromancer.OnDestinationReached += OnNecromancerInPosition;
		necromancer.MoveToDestination (necromancerWaypoint.position);
		yield return WaitForCondition ();
		musicManager.TransitionToBossfight ();

		yield return new WaitForSeconds (necromancer.Speak (1) + 1f);

		// enable player teleportation and let necromancer engage
		outfitter.SetOutfitterMode (Outfitter.OutfitterMode.EXPLORE);
		necromancer.EngagePlayer ();

		// wait till player dies
		playerHealth.OnPlayerDied += OnPlayerDeath;
		yield return WaitForCondition ();
		// wait another 2 seconds to make sure player can't see or hear anything
		yield return new WaitForSeconds (2f);

		// destroy necromancer and remaining skeletons
		MonsterController[] monsters = FindObjectsOfType<MonsterController> ();
		foreach (MonsterController m in monsters) {
			Destroy (m.gameObject);
		}
		musicManager.TransitionToNeutral ();

		// move Alfred next to player
		questManager.CompleteQuest (meet_alfred_quest_id);
		StaircaseDoor.ForceUse ();
		orb.OnDestinationReached += OnAlfredReachedDestination;
		CapsuleCollider player = GameObject.FindGameObjectWithTag ("Player").GetComponent<CapsuleCollider> ();
		Vector3 pos = player.transform.position;
		pos.x += player.center.x;
		pos.z += player.center.z;
		orb.Move (pos, Quaternion.identity, 2f);
		yield return WaitForCondition ();

		// instantiate healing effect and heal player back up
		GameObject healingCircle = Instantiate (healingCirclePrefab, Vector3.zero, healingCirclePrefab.transform.rotation);
		while (playerHealth.hitpoints < playerHealth.maxHitpoints) {
			playerHealth.Heal (10);
			yield return new WaitForSeconds (1f);
			if (playerHealth.hitpoints > 35 && playerHealth.hitpoints <= 45) {
				yield return PlayConversation ();
			}
		}
		Destroy (healingCircle);
		yield return new WaitForSeconds (1f);

		// let Alfred explain situation and send player on next quest
		orb.Move (alfredWaypoints [3].position, alfredWaypoints [3].rotation, 0f);
		yield return WaitForCondition ();
		yield return PlayConversation ();
		int last_quest = questManager.StartQuest ("Leave the mansion through the front door.");

		// wait for player to use outside door and exit level
		foreach (CloseableDoor door in mainHallDoors) {
			door.playerUsable = true;
		}
		exitDoor.OnDoorLocked += OnPlayerUsedExitDoor;
		exitDoor.SetDoorOfInterest (true);
		yield return WaitForCondition ();

		// END OF LEVEL
		questManager.CompleteQuest (last_quest);
		Debug.Log ("LEVEL FINISHED!");
		yield return new WaitForSeconds (1f);
		SceneManager.LoadSceneAsync ("GameEnd");
	}

	/**
	 * Event callbacks
	 */
	public void OnArcaneCrystalPickedUp (string s)
	{
		scriptContinue = true;
	}

	public void OnSmallArcaneDummyDied (MonsterController monsterController)
	{
		smallArcaneDummy.GetComponent<MonsterController> ().OnMonsterDied -= OnSmallArcaneDummyDied;
		outfitter.rightControllerTooltips.UpdateText (VRTK_ControllerTooltips.TooltipButtons.TriggerTooltip, "");
		outfitter.leftControllerTooltips.UpdateText (VRTK_ControllerTooltips.TooltipButtons.TriggerTooltip, "");
		scriptContinue = true;
	}

	public void OnPlayerWaypointReached (PlayerWaypoint waypoint)
	{
		scriptContinue = true;
		waypoint.OnPlayerInWaypointRange -= OnPlayerWaypointReached;
	}

	public void OnPlayerWaypointFourthFloor (PlayerWaypoint waypoint)
	{
		waypoint.OnPlayerInWaypointRange -= OnPlayerWaypointFourthFloor;
		StartCoroutine (PlayerEnteredFourthFloorEvent ());
	}

	private IEnumerator PlayerEnteredFourthFloorEvent ()
	{ 
		monsterManager.SpawnMonsterMelee (MonsterManager.MonsterEngageType.Immediate, true, introSpawnPoints [3]);
		yield return new WaitForSeconds (1.5f);
		monsterManager.SpawnMonsterMelee (MonsterManager.MonsterEngageType.Immediate, true, introSpawnPoints [4]);
	}

	public void OnQuestItemPickedUp (string s)
	{
		questItemsPickedUp++;
		if (questItemsPickedUp == 5) {
			scriptContinue = true;
		}
	}

	public void OnFirstPlayerTeleport (object o, DestinationMarkerEventArgs a)
	{
		outfitter.rightControllerTooltips.UpdateText (VRTK_ControllerTooltips.TooltipButtons.TouchpadTooltip, "");
		outfitter.rightTeleporter.GetComponent<VRTK_BasicTeleport> ().Teleported -= OnFirstPlayerTeleport;
		outfitter.leftControllerTooltips.UpdateText (VRTK_ControllerTooltips.TooltipButtons.TouchpadTooltip, "");
		outfitter.leftTeleporter.GetComponent<VRTK_BasicTeleport> ().Teleported -= OnFirstPlayerTeleport;
	}

	public void OnPlayerEnteredKitchenHallway (PlayerWaypoint waypoint)
	{
		waypoint.OnPlayerInWaypointRange -= OnPlayerEnteredKitchenHallway;
		StartCoroutine (PlayerEnteredKitchenHallwayEvent ());
	}

	private IEnumerator PlayerEnteredKitchenHallwayEvent ()
	{
		AudioSource.PlayClipAtPoint (femaleScream, servantRoomEventSpawnPoints [0].position);
		yield return new WaitForSeconds (1f);
		AudioSource.PlayClipAtPoint (maleScream, servantRoomEventSpawnPoints [1].position);
	}

	public void OnPlayerOpenedKitchenDoor ()
	{
		kitchenDoor.OnDoorOpened -= OnPlayerOpenedKitchenDoor;
		foreach (CloseableDoor door in servantRoomDoors) {
			door.ForceUse ();
		}
		foreach (Transform t in servantRoomEventSpawnPoints) {
			monsterManager.SpawnMonsterMelee (MonsterManager.MonsterEngageType.Immediate, false, t);
		}
	}

	public void OnPlayerTriedOpenGuardRoomDoor ()
	{
		guardRoomDoor.OnDoorLocked -= OnPlayerTriedOpenGuardRoomDoor;
		StartCoroutine (PlayerTriedOpenGuardRoomDoorEvent ());
	}

	private IEnumerator PlayerTriedOpenGuardRoomDoorEvent ()
	{
		AudioSource.PlayClipAtPoint (maleScream, guardRoomEventSpawnPoints [0].position);
		yield return new WaitForSeconds (1f);
		AudioSource.PlayClipAtPoint (femaleScream, guardRoomEventSpawnPoints [0].position);
		yield return new WaitForSeconds (2f);
		monsterManager.SpawnMonsterRanged (MonsterManager.MonsterEngageType.Immediate, false, guardRoomEventSpawnPoints [0]);
		monsterManager.SpawnMonsterMelee (MonsterManager.MonsterEngageType.Immediate, false, guardRoomEventSpawnPoints [1]);
		guardRoomDoor.ForceUse ();
	}

	public void OnPlayerPickedUpBackpack (string s)
	{
		backpack.OnItemPickedUp -= OnPlayerPickedUpBackpack;
		monsterManager.SpawnMonsterMelee (MonsterManager.MonsterEngageType.Immediate, true, backpackEventSpawnPoints [0]);
		monsterManager.SpawnMonsterMelee (MonsterManager.MonsterEngageType.Immediate, true, backpackEventSpawnPoints [1]);
		if (!guestRoomDoor.isOpen) {
			guestRoomDoor.ForceUse ();
		}
	}

	public void OnPlayerEnteredMainHall (PlayerWaypoint waypoint)
	{
		waypoint.OnPlayerInWaypointRange -= OnPlayerEnteredMainHall;
		scriptContinue = true;
		Debug.Log ("This is where the dying happens");
	}

	public void OnNecromancerInPosition ()
	{
		Debug.Log ("Necromancer in Position?");
		necromancer.OnDestinationReached -= OnNecromancerInPosition;
		scriptContinue = true;
	}

	public void OnPlayerDeath ()
	{
		playerHealth.OnPlayerDied -= OnPlayerDeath;
		scriptContinue = true;
	}

	public void OnAlfredReachedDestination ()
	{
		scriptContinue = true;
	}

	public void OnPlayerUsedExitDoor ()
	{
		exitDoor.OnDoorLocked -= OnPlayerUsedExitDoor;
		scriptContinue = true;
	}

	/**
	 *  Script helper methods
	 */
	private IEnumerator PlayConversation ()
	{
		while (true) {
			if (currentEntry >= dialogue.entries.Length) {
				yield break;
			}

			CancelInvoke ("CloseConversation");
			Dialogue.Entry entry = dialogue.entries [currentEntry];
			float entryDuration = orb.Speak (entry);
			currentEntry++;

			if (!entry.autoContinue) {
				Invoke ("CloseConversation", entryDuration + 2f);
				yield break;
			} else {
				yield return new WaitForSeconds (entryDuration + 2f);
			}
		}
	}

	private void CloseConversation ()
	{
		orb.CloseConversationCanvas ();	
	}

	private IEnumerator WaitForCondition ()
	{
		while (!scriptContinue) {
			yield return null;
		}
		scriptContinue = false;
	}


}
