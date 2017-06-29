using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextFade : MonoBehaviour
{
	public float visibleTime = 2f;
	public float fadeTime = 2f;

	private Text text;

	void Start ()
	{
		text = GetComponentInChildren<Text> ();
		StartCoroutine (FadeOutAfter ());
	}

	private IEnumerator FadeOutAfter ()
	{
		yield return new WaitForSeconds (visibleTime);
		float startTime = Time.time;

		while (Time.time < startTime + fadeTime) {
			SetAlpha (1f - (Time.time - startTime) / fadeTime);
			yield return null;
		}

		SetAlpha (0f);
	}

	private void SetAlpha (float alpha)
	{
		Color color = text.color;
		color.a = alpha;
		text.color = color;
	}
}
