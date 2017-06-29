using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Mainly a stub for now, since we're not using the inventory yet.
 */
public class InventoryManager : MonoBehaviour
{
	public int gold { get; private set; }

	public List<string> items { get; private set; }

	void Awake ()
	{
		items = new List<string> ();
	}

	public void GoldPickedUp (int gc)
	{
		gold += gc;
	}

	public void GoldSpent (int gc)
	{
		gold = Mathf.Max (0, gold - gc);
	}

	public void ItemPickedUp (string item)
	{
		items.Add (item);
	}

	public void ItemLost (string item)
	{
		items.Remove (item);
	}
}
