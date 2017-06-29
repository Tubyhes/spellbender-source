using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfter : MonoBehaviour
{
	public float after;

	void Start ()
	{
		Destroy (gameObject, after);
	}
	
}
