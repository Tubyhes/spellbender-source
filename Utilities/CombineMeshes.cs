using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(MeshFilter))]
[RequireComponent (typeof(MeshRenderer))]
public class CombineMeshes : MonoBehaviour
{
	void Start ()
	{
		MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter> ();
		CombineInstance[] combine = new CombineInstance[meshFilters.Length];

		for (int i = 0; i < meshFilters.Length; i++) {
			combine [i].mesh = meshFilters [i].sharedMesh;
			combine [i].transform = Matrix4x4.TRS (meshFilters [i].transform.localPosition, Quaternion.identity, Vector3.one);
			meshFilters [i].gameObject.SetActive (false);
		}

		GetComponent<MeshFilter> ().mesh = new Mesh ();
		GetComponent<MeshFilter> ().mesh.CombineMeshes (combine, true, true);
		GetComponent<MeshCollider> ().sharedMesh = GetComponent<MeshFilter> ().mesh;

		gameObject.SetActive (true);
	}
}
