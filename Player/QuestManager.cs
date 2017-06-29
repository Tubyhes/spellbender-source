using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

/**
 * Quest Manager is responsible for keeping track of active and completed quests
 * for the player. Assigns an auto-incrementing quest_id.
 * 
 * Quest Items in the world can be assigned this quest_id, and will then notify the quest 
 * manager the quest is completed upon item pickup.
 */
public class QuestManager : MonoBehaviour
{
	[System.Serializable]
	public class Quest
	{
		public int quest_id;
		public bool complete;
		public string description;
	}

	public List<Quest> quests;

	private int quest_id_auto_incr;

	private Outfitter outfitter;

	void Awake ()
	{
		quest_id_auto_incr = 0;
		quests = new List<Quest> ();
	}

	void Start ()
	{
		outfitter = FindObjectOfType<Outfitter> ();
	}

	public int StartQuest (string description)
	{
		outfitter.rightControllerTooltips.UpdateText (VRTK_ControllerTooltips.TooltipButtons.GripTooltip, "New Quest!");
		outfitter.leftControllerTooltips.UpdateText (VRTK_ControllerTooltips.TooltipButtons.GripTooltip, "New Quest!");
		quest_id_auto_incr++;
		Quest q = new Quest { quest_id = quest_id_auto_incr, complete = false, description = description };
		quests.Add (q);
		return q.quest_id;
	}

	public void CompleteQuest (int quest_id)
	{
		if (quest_id <= 0) {
			return;
		}
		quests.Find (x => x.quest_id == quest_id).complete = true;
	}

	public bool IsQuestCompleted (int quest_id)
	{
		return quests.Find (x => x.quest_id == quest_id).complete;
	}
}
