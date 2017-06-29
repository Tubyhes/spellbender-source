using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(AudioSource))]
[RequireComponent (typeof(Renderer))]
public class AudioPulsator : MonoBehaviour
{
	private AudioSource audioSource;
	private Renderer renderer_;

	private string rimPower = "_RimPower";
	private float minRimPower = 1f;
	private float maxRimPower = 4f;

	private float[] clipSamples;
	private float updateFrequency = 0.025f;
	private int num_samples = 256;
	private float lastUpdate = 0f;
	private float currentValue;
	private float nextValue;

	void Start ()
	{
		renderer_ = GetComponent<Renderer> ();
		audioSource = GetComponent<AudioSource> ();
		clipSamples = new float[num_samples];
	}

	void Update ()
	{
		if (!audioSource.isPlaying) {
			renderer_.material.SetFloat (rimPower, maxRimPower);
			return;
		}

		if (Time.time > lastUpdate + updateFrequency) {
			lastUpdate = Time.time;
			audioSource.clip.GetData (clipSamples, audioSource.timeSamples);
			currentValue = average (clipSamples);
			if (audioSource.timeSamples + updateFrequency * audioSource.clip.frequency + num_samples > audioSource.clip.samples) {
				nextValue = 0f;
			} else {
				audioSource.clip.GetData (clipSamples, audioSource.timeSamples + (int)(updateFrequency * audioSource.clip.frequency));
				nextValue = average (clipSamples);
			}
		}

		float x = (Time.time - lastUpdate) / updateFrequency;
		float value = currentValue + x * (nextValue - currentValue);
		float exp = (value - 0.1f) * 30f; // scale value to a range of -6 to 6
		float sigmoid_value = 1f / (1f + Mathf.Exp (-exp)); 
		renderer_.material.SetFloat (rimPower, maxRimPower - sigmoid_value * (maxRimPower - minRimPower));
	}

	private float average (float[] samples)
	{
		float avg = 0;
		for (int i = 0; i < samples.Length; i++) {
			avg += Mathf.Abs (samples [i]);
		}

		return avg / samples.Length;
	}
}
