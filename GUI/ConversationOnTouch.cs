using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VRTK;
using VRTK.Highlighters;

[RequireComponent (typeof(VRTK_InteractableObject))]
[RequireComponent (typeof(VRTK_OutlineObjectCopyHighlighter))]
public class ConversationOnTouch : MonoBehaviour
{
	public float hoverHeight = 0f;
	public string text;
	public bool questCritical = false;
	public bool forwardInverse = false;

	private bool untouched = true;

	private VRTK_InteractableObject interactableObject;
	private GameObject conversationCanvasPrefab;
	private GameObject conversationCanvas;
	private GameObject twinklePrefab;
	private GameObject twinkle;
	private GameObject exclamationMarkPrefab;
	private GameObject exclamationMark;

	private Mesh mesh;

	void Start ()
	{
		interactableObject = GetComponent<VRTK_InteractableObject> ();
		interactableObject.InteractableObjectTouched += OnTouch;

		mesh = GetComponent<MeshFilter> ().mesh;

		CreateAttentionObject ();

		conversationCanvasPrefab = Resources.Load<GameObject> ("ConversationCanvasStatic");
	}

	void Update ()
	{
		if (!interactableObject.IsTouched ()) {
			if (conversationCanvas != null) {
				Destroy (conversationCanvas);
				conversationCanvas = null;
			}
		}
	}

	private void CreateCanvas ()
	{
		Debug.Log (mesh.bounds.ToString ());
		float scale = conversationCanvasPrefab.transform.localScale.x;
		float sizeX = mesh.bounds.size.x * transform.localScale.x;
		float sizeZ = mesh.bounds.size.z * transform.localScale.z;
		Vector2 sizeDelta = new Vector2 (sizeX / scale, sizeZ / scale);

		conversationCanvas = Instantiate (conversationCanvasPrefab, Vector3.zero, Quaternion.identity);
		conversationCanvas.GetComponent<RectTransform> ().sizeDelta = sizeDelta;
		conversationCanvas.transform.SetParent (transform);
		conversationCanvas.transform.localPosition = new Vector3 (0f, 0.1f, 0f);
		conversationCanvas.transform.rotation = Quaternion.LookRotation (transform.up, transform.forward * (forwardInverse ? -1 : 1));
		conversationCanvas.GetComponentInChildren<Text> ().text = text;
	}

	public void OnTouch (object interactingObject, VRTK.InteractableObjectEventArgs a)
	{
		if (untouched) {
			DestroyAttentionObject ();
			untouched = false;
		}
		if (conversationCanvas == null) {
			CreateCanvas ();
		}
	}

	private void CreateAttentionObject ()
	{
		if (questCritical) {
			exclamationMarkPrefab = Resources.Load<GameObject> ("ExclamationMark");
			exclamationMark = Instantiate (exclamationMarkPrefab, transform.position, Quaternion.identity);
			exclamationMark.GetComponent<BobUpAndDown> ().hoverHeight = hoverHeight;
		} else {
			twinklePrefab = Resources.Load<GameObject> ("ItemOfInterestTwinkle");
			twinkle = Instantiate (twinklePrefab, Vector3.zero, Quaternion.identity);
			twinkle.transform.SetParent (transform);
			twinkle.transform.position = new Vector3 (transform.position.x, transform.position.y + hoverHeight, transform.position.z);
		}
	}

	private void DestroyAttentionObject ()
	{
		if (questCritical && exclamationMark != null) {
			Destroy (exclamationMark);
		} 
		if (!questCritical && twinkle != null) {
			Destroy (twinkle);
		}
	}
}
