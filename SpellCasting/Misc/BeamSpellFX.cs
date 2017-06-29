using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * A beam spell effect, not being used yet.
 */
public class BeamSpellFX : MonoBehaviour
{

	public float forcePerSecond = 10f;
	public float damagePerSecond = 5f;
	public ElementType elementType;
	public GameObject beamStartPrefab;
	public GameObject beamPrefab;
	public GameObject beamEndPrefab;

	private GameObject beamStart;
	private GameObject beam;
	private GameObject beamEnd;
	private LineRenderer beamLine;

	private float beamEndOffset = 1f;
	private float beamMaxLength = 100f;
	private float textureScrollSpeed = 8f;
	private float textureLengthScale = 3;

	// Use this for initialization
	void Start ()
	{
		beamStart = Instantiate (beamStartPrefab, transform.position, Quaternion.identity);
		beam = Instantiate (beamPrefab, transform.position, Quaternion.identity);
		beamEnd = Instantiate (beamEndPrefab, transform.position, Quaternion.identity);
		beamLine = beam.GetComponent<LineRenderer> ();

		beamStart.transform.parent = transform;
		beam.transform.parent = transform;
		beamEnd.transform.parent = transform;
	}

	void FixedUpdate ()
	{

		Vector3 endPosition = Vector3.zero; 
		RaycastHit hit;
		if (Physics.Raycast (transform.position, transform.forward, out hit, beamMaxLength)) {
			endPosition = hit.point - (transform.forward.normalized * beamEndOffset);
			if (hit.collider.gameObject.tag == "Monster") {
				hit.collider.gameObject.GetComponent<MonsterController> ().GetHit (damagePerSecond * Time.fixedDeltaTime, elementType);
			} else if (hit.rigidbody != null) {
				Vector3 forceVector = transform.forward * forcePerSecond * Time.fixedDeltaTime;
				hit.rigidbody.AddForce (forceVector);
			}
		} else {
			endPosition = transform.position + (transform.forward.normalized * beamMaxLength);
		}
	
		beamLine.SetPosition (0, transform.position);
		beamLine.SetPosition (1, endPosition);
		beamEnd.transform.position = endPosition;

		beamStart.transform.LookAt (beamEnd.transform.position);
		beamEnd.transform.LookAt (beamStart.transform.position);

		float distance = Vector3.Distance (transform.position, endPosition);
		beamLine.sharedMaterial.mainTextureScale = new Vector2 (distance / textureLengthScale, 1);
		beamLine.sharedMaterial.mainTextureOffset -= new Vector2 (Time.deltaTime * textureScrollSpeed, 0);
	}
}
