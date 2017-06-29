using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(QuestItem))]
public class Currency : MonoBehaviour
{
	public int goldWorth;

	void Start ()
	{
		GetComponent<QuestItem> ().OnItemPickedUp += OnCurrencyCollected;			
	}

	public void OnCurrencyCollected (string s)
	{
		FindObjectOfType<InventoryManager> ().GoldPickedUp (goldWorth);
	}
}
