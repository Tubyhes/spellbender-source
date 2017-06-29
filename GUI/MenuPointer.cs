using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR.InteractionSystem;

public class MenuPointer : MonoBehaviour
{
	public Transform pointerOriginTransform;
	public Color pointerColor;
	public float thickness = 0.002f;
	public LayerMask layerMask;

	private GameObject laser = null;

	// Update is called once per frame
	void Update ()
	{
		if (laser != null) {
			Ray ray = new Ray (transform.position, transform.forward);
			RaycastHit hit;
			if (Physics.Raycast (ray, out hit, 100f, layerMask)) {
				laser.transform.localScale = new Vector3 (thickness, thickness, hit.distance);
				laser.transform.localPosition = new Vector3 (0f, 0f, hit.distance / 2f);
			}
		}
	}

	void OnEnable ()
	{
		DrawLaser ();
	}

	void OnDisable ()
	{
		if (laser != null) {
			Destroy (laser);			
		}
	}

	private void DrawLaser ()
	{
		laser = GameObject.CreatePrimitive (PrimitiveType.Cube);
		laser.transform.parent = pointerOriginTransform;
		laser.transform.localScale = new Vector3 (thickness, thickness, 100f);
		laser.transform.localPosition = new Vector3 (0f, 0f, 50f);
		laser.transform.localRotation = Quaternion.identity;
		Destroy (laser.GetComponent<Collider> ());

		Material newMaterial = new Material (Shader.Find ("Unlit/Color"));
		newMaterial.SetColor ("_Color", pointerColor);
		laser.GetComponent<MeshRenderer> ().material = newMaterial;
	}
}
