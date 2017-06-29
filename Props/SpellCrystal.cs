using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Spell Crystals are a special type of quest item, because they make a spell available to the player.
 */
[RequireComponent (typeof(QuestItem))]
public class SpellCrystal : MonoBehaviour
{
	public SpellCaster spellCaster;

	void Awake ()
	{
		GetComponent<QuestItem> ().OnItemPickedUp += OnItemPickup;
	}

	public void OnItemPickup (string s)
	{
		spellCaster.SetAvailable ();
	}
}
