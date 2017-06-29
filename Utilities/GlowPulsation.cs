using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlowPulsation : MonoBehaviour
{

	public float periodLength = 2.0f;
	public float periodOffset = 0f;

	private Renderer renderer_;
	private string glowPower = "_MKGlowPower";
	private float maxGlowPower = 0.03f;
	private string glowTextureStrength = "_MKGlowTexStrength";
	private float maxGlowTextureStrength = 10f;

	// Use this for initialization
	void Start ()
	{
		renderer_ = GetComponent<Renderer> ();
	}
	
	// Update is called once per frame
	void Update ()
	{
		float x = (Time.time + periodOffset) * Mathf.PI * (2 / periodLength);
		float strength = (Mathf.Sin (x) + 1) / 2;

		renderer_.material.SetFloat (glowPower, strength * maxGlowPower);
		renderer_.material.SetFloat (glowTextureStrength, strength * maxGlowTextureStrength);
	}
}
