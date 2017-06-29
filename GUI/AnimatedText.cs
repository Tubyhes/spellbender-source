using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent (typeof(Text))]
public class AnimatedText : MonoBehaviour
{
	public float animationSpeed = 0.08f;

	private Text text;
	private IEnumerator currentCoroutine;

	void Awake ()
	{
		text = GetComponent<Text> ();		
	}

	public void ShowText (string t, float durationPerLetter)
	{
		Debug.Log ("Showing text with animationSpeed: " + durationPerLetter);
		animationSpeed = durationPerLetter;

		if (currentCoroutine != null) {
			StopCoroutine (currentCoroutine);
		}		
		currentCoroutine = AnimateText (t); 
		StartCoroutine (currentCoroutine);
	}

	private IEnumerator AnimateText (string t)
	{
		string s = "";
		for (int i = 0; i < t.Length; i++) {
			s += t [i];
			text.text = s;
			yield return new WaitForSeconds (animationSpeed);
		}

		currentCoroutine = null;
	}
}
