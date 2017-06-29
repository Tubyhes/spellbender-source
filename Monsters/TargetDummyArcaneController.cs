using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetDummyArcaneController : MonsterController
{
	public enum SizeClass
	{
		Small,
		Medium,
		Large
	}

	public SizeClass dummySize;
	private MeshGlow meshGlowPrefab;

	public bool dead { get; private set; }

	public override void Start ()
	{
		base.Start ();
	}

	protected override void OnDied ()
	{
		if (dead) {
			return;
		}

		dead = true;
		base.OnDied ();

		string meshGlowResourceName = "MagicMeshGlow/ArcaneMeshGlow";
		meshGlowPrefab = Resources.Load<MeshGlow> (meshGlowResourceName);
		MeshGlow meshGlow = Instantiate (meshGlowPrefab, transform, false);
		meshGlow.SetMesh (GetComponent<MeshRenderer> ());
		Destroy (gameObject, 3f);
	}

	public override void GetHit (float dmg, ElementType elementType)
	{
		if (!dead) {
			HandleDamage (DamageModifiers (dmg, elementType));
		}
	}

	protected override float DamageModifiers (float dmg, ElementType elementType)
	{
		if (elementType == ElementType.Arcane) {
			return dmg;
		} 

		return 0f;
	}
}
