using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGlow : MonoBehaviour
{
	private ParticleSystem[] particles;

	public void SetMesh (MeshRenderer meshRenderer)
	{
		particles = GetComponentsInChildren<ParticleSystem> ();
		foreach (ParticleSystem p in particles) {
			var shape = p.shape;
			shape.shapeType = ParticleSystemShapeType.MeshRenderer;
			shape.meshRenderer = meshRenderer;
		}
	}
}
