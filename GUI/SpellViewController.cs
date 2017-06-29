using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellViewController : MonoBehaviour
{
	public GameObject arcaneBolt;
	private ArcaneBoltSpellCaster arcaneCastArea;

	void OnEnable ()
	{
		if (arcaneCastArea == null) {
			arcaneCastArea = FindObjectOfType<ArcaneBoltSpellCaster> ();
		}

		arcaneBolt.SetActive (arcaneCastArea.available);
	}

}
