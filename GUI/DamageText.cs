using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VRTK;

public class DamageText : MonoBehaviour
{
	public Animator animator;
	public Text text;

	private Transform hmd;

	void Start ()
	{
		Destroy (gameObject, animator.GetCurrentAnimatorClipInfo (0) [0].clip.length);				
		hmd = FindObjectOfType<VRTK_SDKManager> ().actualHeadset.transform;
	}

	void Update ()
	{
		transform.LookAt (hmd);		
	}
}
