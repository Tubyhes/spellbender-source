using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInfoViewController : MonoBehaviour
{
	public List<GameObject> views;

	public void OnButtonClicked (int viewIndex)
	{
		for (int i = 0; i < views.Count; i++) {
			if (i == viewIndex) {
				views [i].SetActive (true);
			} else {
				views [i].SetActive (false);
			}
		}
	}

	public void OnEnable ()
	{
		OnButtonClicked (0);
	}
}
