using System;
using System.Collections.Generic;
using UnityEngine;

public class AppManager : MonoBehaviour
{
	public bool test;
	//public GameObject facego;
	// all start as head fixed
	// when started, other is the worldFixed Game Object

	public GameObject otherGO;
	bool is_trans;
	public TextMesh msgBlocking;
	ContextDetection contextDetection;
	private Camera cam;

	private void Start()
	{
		cam = Camera.main;
		contextDetection = GameObject.Find("Manager").GetComponent<ContextDetection>();
		is_trans = false;

		// test and extra
		test = true;
	}

	private void Update()
	{
		bool blocking = BlockingFace();
		msgBlocking.text = "Is Blocking a Face: " + blocking;
		if (contextDetection.InConversation())
		{
			// if the current app is opaque and blocks the face
			if (!is_trans)
			{
				if (blocking)
				{
					//change game object's to transparent
					gameObject.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.1f);
					GetChildWithName(gameObject, "FixationIcon").GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.1f);
					is_trans = true;
				}
			}
			else if (!blocking) // will be else to if (!is_trans)
			{
				// make it opaque
				gameObject.GetComponent<SpriteRenderer>().color = Color.white;
				GetChildWithName(gameObject, "FixationIcon").GetComponent<SpriteRenderer>().color = Color.white;
				is_trans = false;
			}
		}
	}

	private bool BlockingFace()
	{
		foreach (List<int> faceBox in contextDetection.faces_box)
		{
			if (Overlapping(faceBox))
			{
				return true;
			}
		}
		return false;
	}

	private List<int> Corners(GameObject go)
	{
		Bounds bounds = go.GetComponent<Collider>().bounds;
		Vector3 cen = bounds.center;
		Vector3 ext = bounds.extents;
		float screenheight = Screen.height;
		Vector2 min = cam.WorldToScreenPoint(new Vector3(cen.x - ext.x, cen.y - ext.y, cen.z - ext.z));
		Vector2 max = min;

		//0
		Vector2 point = min;
		min = new Vector2(min.x >= point.x ? point.x : min.x, min.y >= point.y ? point.y : min.y);
		max = new Vector2(max.x <= point.x ? point.x : max.x, max.y <= point.y ? point.y : max.y);

		//1
		point = cam.WorldToScreenPoint(new Vector3(cen.x + ext.x, cen.y - ext.y, cen.z - ext.z));
		min = new Vector2(min.x >= point.x ? point.x : min.x, min.y >= point.y ? point.y : min.y);
		max = new Vector2(max.x <= point.x ? point.x : max.x, max.y <= point.y ? point.y : max.y);


		//2
		point = cam.WorldToScreenPoint(new Vector3(cen.x - ext.x, cen.y - ext.y, cen.z + ext.z));
		min = new Vector2(min.x >= point.x ? point.x : min.x, min.y >= point.y ? point.y : min.y);
		max = new Vector2(max.x <= point.x ? point.x : max.x, max.y <= point.y ? point.y : max.y);

		//3
		point = cam.WorldToScreenPoint(new Vector3(cen.x + ext.x, cen.y - ext.y, cen.z + ext.z));
		min = new Vector2(min.x >= point.x ? point.x : min.x, min.y >= point.y ? point.y : min.y);
		max = new Vector2(max.x <= point.x ? point.x : max.x, max.y <= point.y ? point.y : max.y);

		//4
		point = cam.WorldToScreenPoint(new Vector3(cen.x - ext.x, cen.y + ext.y, cen.z - ext.z));
		min = new Vector2(min.x >= point.x ? point.x : min.x, min.y >= point.y ? point.y : min.y);
		max = new Vector2(max.x <= point.x ? point.x : max.x, max.y <= point.y ? point.y : max.y);

		//5
		point = cam.WorldToScreenPoint(new Vector3(cen.x + ext.x, cen.y + ext.y, cen.z - ext.z));
		min = new Vector2(min.x >= point.x ? point.x : min.x, min.y >= point.y ? point.y : min.y);
		max = new Vector2(max.x <= point.x ? point.x : max.x, max.y <= point.y ? point.y : max.y);

		//6
		point = cam.WorldToScreenPoint(new Vector3(cen.x - ext.x, cen.y + ext.y, cen.z + ext.z));
		min = new Vector2(min.x >= point.x ? point.x : min.x, min.y >= point.y ? point.y : min.y);
		max = new Vector2(max.x <= point.x ? point.x : max.x, max.y <= point.y ? point.y : max.y);

		//7
		point = cam.WorldToScreenPoint(new Vector3(cen.x + ext.x, cen.y + ext.y, cen.z + ext.z));
		min = new Vector2(min.x >= point.x ? point.x : min.x, min.y >= point.y ? point.y : min.y);
		max = new Vector2(max.x <= point.x ? point.x : max.x, max.y <= point.y ? point.y : max.y);

		List<int> ret = new List<int> { Convert.ToInt32(min.x), Convert.ToInt32(min.y), Convert.ToInt32(max.x - min.x), Convert.ToInt32(max.y - min.y) };
		return ret;

	}

	private bool Overlapping(List<int> faceBox)
	{
		float minX1, minY1, maxX1, maxY1, minX2, minY2, maxX2, maxY2;
		List<int> corners = Corners(gameObject);
		minX1 = corners[0];
		minY1 = corners[1];
		maxX1 = corners[2];
		maxY1 = corners[3];


		////////////////////////////////////////////////////for test//////////////////////////
		//corners = Corners(facego);
		//minX2 = corners[0];
		//minY2 = corners[1];
		//maxX2 = corners[2];
		//maxY2 = corners[3];
		minX2 = faceBox[0];
		minY2 = faceBox[1];
		maxX2 = faceBox[2] - minX2;
		maxY2 = faceBox[3] - minY2;
		Rect faceRect = new Rect(minX2, minY2, maxX2 , maxY2 );
		bool r = faceRect.Overlaps(new Rect(minX1, minY1, maxX1 , maxY1 ));
		//print(gameObject.name  + ": " + r);
		if (test)
		{
			print("Face: " + minX2 + ", " + minY2 + ", " + maxX2 + ", " + maxY2);
			print(gameObject.name + ": " + minX1 + ", " + minY1 + ", " + maxX1 + ", " + maxY1);
			test = false;
		}
		return r;
	

		//////////////////////////////////////////////////////////////////////ACTUAL

		//Debug.DrawLine(cam.WorldToViewportPoint(new Vector3(minX1, minY1, 15.0f)), cam.WorldToViewportPoint(new Vector3(maxX1, maxY1, 15.0f)), Color.blue);
		//Debug.DrawLine(cam.ScreenToWorldPoint(new Vector3(minX2, minY2, 15.0f)), cam.ScreenToWorldPoint(new Vector3(maxX2, maxY2, 15.0f)), Color.green);
		//Debug.DrawLine(new Vector3(0.0f, 0.0f, 2.0f), new Vector3(0.0f, 0.0f, 2.0f), Color.green);

	}

	private void DrawLine(Vector3 start, Vector3 end, Color color)
	{
		GameObject myLine = new GameObject();
		myLine.transform.position = start;
		myLine.AddComponent<LineRenderer>();
		LineRenderer lr = myLine.GetComponent<LineRenderer>();
		lr.material = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));
		lr.startColor = color;
		lr.endColor= color;
		lr.startWidth = 0.1f;
		lr.endWidth = 0.1f;
		lr.SetPosition(0, start);
		lr.SetPosition(1, end);
	}

	public void ChangeFixation()
	{
		print(gameObject.name + " changing fixation!");
		
		otherGO.transform.position = transform.position;
		otherGO.transform.rotation = transform.rotation;
		otherGO.GetComponent<AppManager>().msgBlocking.text = msgBlocking.text;
		otherGO.GetComponent<AppManager>().is_trans = is_trans;
		otherGO.GetComponent<AppManager>().otherGO = gameObject;

		otherGO.SetActive(true);
		gameObject.SetActive(false);
	}

	public static GameObject GetChildWithName(GameObject obj, string name)
	{
		foreach (Transform eachChild in obj.transform)
		{
			if (eachChild.name == name)
			{
				return eachChild.gameObject;
			}
			else if (eachChild.transform.Find(name))
				return eachChild.transform.Find(name).gameObject;
		}

		return null;
	}

	private void OnGUI()
	{
		List<int> corners = Corners(gameObject);
		int minX1, minY1, maxX1, maxY1;
		minX1 = corners[0];
		minY1 = corners[1];
		maxX1 = corners[2];
		maxY1 = corners[3];

		Texture2D texture = new Texture2D(1, 1);
		texture.SetPixel(0, 0, Color.cyan);
		texture.Apply();
		GUI.skin.box.normal.background = texture;
		GUI.Box(new Rect(minX1, minY1, maxX1, maxY1), GUIContent.none);

		// FACES
		foreach (List<int> fb in contextDetection.faces_box)
		{
			Texture2D texture2 = new Texture2D(1, 1);
			texture2.SetPixel(0, 0, Color.green);
			texture2.Apply();
			GUI.skin.box.normal.background = texture2;
			GUI.Box(new Rect(fb[0], fb[1], -(fb[2]- fb[0]), -(fb[3] - fb[1])), GUIContent.none);
		}
	}
}
