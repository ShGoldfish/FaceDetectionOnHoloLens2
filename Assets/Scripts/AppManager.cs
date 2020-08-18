using System;
using System.Collections.Generic;
using UnityEngine;

public class AppManager : MonoBehaviour
{
	public bool test;
	// all start as head fixed
	// when started, other is the worldFixed Game Object
	public GameObject otherGO;

	bool is_trans;
	public TextMesh msgBlocking;
	ContextDetection contextDetection;
	private Camera cam;

	private void Start()
	{
		test = false;
		cam = Camera.main;
		contextDetection = GameObject.Find("Manager").GetComponent<ContextDetection>();
		is_trans = false;
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

	private bool Overlapping(List<int> faceBox)
	{
		float minX1, minY1, maxX1, maxY1, minX2, minY2, maxX2, maxY2;
		minX1 = transform.position.x; //cam.WorldToScreenPoint(transform.position).x;		// or -29
		maxX1 = minX1 + 58; // or 29
		minY1 = transform.position.y; //cam.WorldToScreenPoint(transform.position).y;		// or -58
		maxY1 = minY1 + 116; // or 58

		minX2 = faceBox[0];
		minY2 = faceBox[1];
		maxX2 = faceBox[2];
		maxY2 = faceBox[3];

		//print("Face: " + minX2 + ", " + minY2 + ", " + maxX2 + ", " + maxY2);	
		//print(gameObject.name + ": " + minX1 + " " + minY1+ " " + gameObject.GetComponent<SpriteRenderer>().sprite.bounds.size.ToString());
		//Debug.DrawLine(cam.WorldToViewportPoint( new Vector3(minX1, minY1, 15.0f)), cam.WorldToViewportPoint(new Vector3(maxX1, maxY1, 15.0f)), Color.blue);
		//Debug.DrawLine(cam.ScreenToWorldPoint(new Vector3(minX2, minY2, 15.0f)), cam.ScreenToWorldPoint(new Vector3(maxX2, maxY2, 15.0f)), Color.green);
		//Debug.DrawLine(new Vector3(0.0f, 0.0f, 2.0f), new Vector3(0.0f, 0.0f, 2.0f), Color.green);


		if ((maxX1 <= maxX2 && maxX1 >= minX2) || (minX1 <= maxX2 && minX1 >= minX2))
		{
			if (minY1 >= maxY2)
				return false;
			if (maxY1 <= minY2)
				return false;
			return true;

		}
		else if ((maxX1 <= minX2 && maxX1 >= maxX2))
		{
			if (minY1 >= maxY2)
				return false;
			if (maxY1 <= minY2)
				return false;
			return true;

		}
		return false;	
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

	private void Update()
	{
		// Test:
		if (test)
		{
			ChangeFixation();
		}
		bool blocking = BlockingFace();
		msgBlocking.text = "Is Blocking a Face: " + blocking;
		if (contextDetection.InConversation())
		{
			// if the current app is opaque and blocks the face
			if (!is_trans)
			{
				//if (blocking)
				//{
				// change game object's to transparent
				gameObject.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.1f);
				is_trans = true;
				//}
			}
		}
		else //if (!blocking) // will be else to if (!is_trans)
		{
			// make it opaque
			gameObject.GetComponent<SpriteRenderer>().color = Color.white;
			is_trans = false;
		}
		//}
	}

	public void ChangeFixation()
	{
		test = false;
		otherGO.GetComponent<AppManager>().test = false;
		
		otherGO.transform.localPosition = transform.localPosition;
		otherGO.GetComponent<AppManager>().msgBlocking.text = msgBlocking.text;
		otherGO.GetComponent<AppManager>().is_trans = is_trans;
		otherGO.GetComponent<AppManager>().otherGO = gameObject;

		otherGO.SetActive(true);
		gameObject.SetActive(false);
	}

}
