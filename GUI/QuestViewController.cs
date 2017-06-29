using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestViewController : MonoBehaviour
{
	public Transform openQuestContent;
	public Transform finishedQuestContent;

	public GameObject openQuestItemPrefab;
	public GameObject finishedQuestItemPrefab;

	private QuestManager questManager;
	private List<GameObject> openQuestItemPool;
	private List<GameObject> finishedQuestItemPool;

	void Awake ()
	{	
		InitializePools ();
	}

	private void InitializePools ()
	{
		openQuestItemPool = new List<GameObject> ();
		finishedQuestItemPool = new List<GameObject> ();
		for (int i = 0; i < 20; i++) {
			GameObject oq = Instantiate (openQuestItemPrefab, transform, false);
			oq.SetActive (false);
			openQuestItemPool.Add (oq);

			GameObject fq = Instantiate (finishedQuestItemPrefab, transform, false);
			fq.SetActive (false);
			finishedQuestItemPool.Add (fq);
		}
	}

	private GameObject GetOpenQuestItem ()
	{
		for (int i = 0; i < openQuestItemPool.Count; i++) {
			if (!openQuestItemPool [i].activeInHierarchy) {
				openQuestItemPool [i].SetActive (true);
				return openQuestItemPool [i];
			}
		}
		return null;
	}

	private GameObject GetFinishedQuestItem ()
	{
		for (int i = 0; i < finishedQuestItemPool.Count; i++) {
			if (!finishedQuestItemPool [i].activeInHierarchy) {
				finishedQuestItemPool [i].SetActive (true);
				return finishedQuestItemPool [i];
			}
		}
		return null;
	}

	private void ReturnOpenQuestItem (GameObject go)
	{
		go.transform.SetParent (transform);
		go.SetActive (false);
	}

	private void ReturnFinishedQuestItem (GameObject go)
	{
		go.transform.SetParent (transform);
		go.SetActive (false);
	}

	public void OnEnable ()
	{
		if (questManager == null) {
			questManager = FindObjectOfType<QuestManager> ();
		}

		// delete all elements in open quest view
		for (int i = openQuestContent.childCount - 1; i >= 0; --i) {
			ReturnOpenQuestItem (openQuestContent.GetChild (i).gameObject);
		}

		// delete all elements in finished quest view
		for (int i = finishedQuestContent.childCount - 1; i >= 0; --i) {
			ReturnFinishedQuestItem (finishedQuestContent.GetChild (i).gameObject);
		}

		// populate quest views
		foreach (QuestManager.Quest quest in questManager.quests) {
			if (quest.complete) {
				GameObject questView = GetFinishedQuestItem ();
				questView.transform.GetComponentInChildren<Text> ().text = quest.description;
				questView.transform.SetParent (finishedQuestContent);
			} else {
				GameObject questView = GetOpenQuestItem ();
				questView.transform.GetComponentInChildren<Text> ().text = quest.description;
				questView.transform.SetParent (openQuestContent);
			}
		}
	}

}
