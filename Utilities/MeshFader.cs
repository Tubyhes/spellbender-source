using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshFader : MonoBehaviour
{
	private List<Renderer> renderers;

	void Awake ()
	{
		renderers = new List<Renderer> ();
		renderers.AddRange (GetComponentsInChildren<MeshRenderer> ());
		renderers.AddRange (GetComponentsInChildren<SkinnedMeshRenderer> ());
	}

	public void FadeIn (float fadeTime)
	{
		StartCoroutine (FadeInRoutine (fadeTime));
	}

	private IEnumerator FadeInRoutine (float fadeTime)
	{
		float fadeStartTime = Time.time;
		SetAllBlendModeFade ();

		while (Time.time < fadeStartTime + fadeTime) {
			SetFadeLevel ((Time.time - fadeStartTime) / fadeTime);
			yield return null;
		}

		SetAllBlendModeOpaque ();
	}

	public void FadeOut (float fadeTime)
	{
		StartCoroutine (FadeOutRoutine (fadeTime));
	}

	private IEnumerator FadeOutRoutine (float fadeTime)
	{
		float fadeStartTime = Time.time;
		SetAllBlendModeFade ();

		while (Time.time < fadeStartTime + fadeTime) {
			SetFadeLevel ((fadeTime - (Time.time - fadeStartTime)) / fadeTime);
			yield return null;
		}

		SetFadeLevel (0);
	}

	private void SetFadeLevel (float alpha)
	{
		foreach (Renderer renderer in renderers) {
			foreach (Material mat in renderer.materials) {
				Color c = mat.color;
				c.a = alpha;
				mat.color = c;
			}
		}
	}

	private void SetBlendModeFade (Material mat)
	{
		mat.SetFloat ("_Mode", 2);
		mat.SetInt ("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
		mat.SetInt ("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
		mat.SetInt ("_ZWrite", 0);
		mat.DisableKeyword ("_ALPHATEST_ON");
		mat.EnableKeyword ("_ALPHABLEND_ON");
		mat.DisableKeyword ("_ALPHAPREMULTIPLY_ON");
		mat.renderQueue = 3000;
	}

	private void SetAllBlendModeFade ()
	{
		foreach (Renderer renderer in renderers) {
			foreach (Material mat in renderer.materials) {
				SetBlendModeFade (mat);
			}
		}
	}

	private void SetBlendModeOpaque (Material mat)
	{
		mat.SetFloat ("_Mode", 0);
		mat.SetInt ("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
		mat.SetInt ("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
		mat.SetInt ("_ZWrite", 1);
		mat.DisableKeyword ("_ALPHATEST_ON");
		mat.EnableKeyword ("_ALPHABLEND_ON");
		mat.DisableKeyword ("_ALPHAPREMULTIPLY_ON");
		mat.renderQueue = -1;
	}

	private void SetAllBlendModeOpaque ()
	{
		foreach (Renderer renderer in renderers) {
			foreach (Material mat in renderer.materials) {
				SetBlendModeOpaque (mat);
			}
		}
	}
}
