using System;
using System.Collections.Generic;
using UnityEngine;

public class AppManager : MonoBehaviour
{
	bool is_trans;
	ContextDetection contextDetection;
	private Camera cam;

	private void Start()
	{
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
		minX1 = Convert.ToInt32(gameObject.GetComponent<SpriteRenderer>().sprite.bounds.min.x);
		minY1 = Convert.ToInt32(gameObject.GetComponent<SpriteRenderer>().sprite.bounds.min.y);
		maxX1 = Convert.ToInt32(gameObject.GetComponent<SpriteRenderer>().sprite.bounds.max.x);
		maxY1 = Convert.ToInt32(gameObject.GetComponent<SpriteRenderer>().sprite.bounds.max.y);
		print("App: " + minX1 + ", " + minY1 + ", " + maxX1 + ", " + maxY1);

		//Vector3 point1 = new Vector3();
		//Vector3 point2 = new Vector3();
		//point1 = cam.ScreenToWorldPoint(new Vector3(minX1, minY1, cam.nearClipPlane));
		//point2 = cam.ScreenToWorldPoint(new Vector3(maxX1, maxY1, cam.nearClipPlane));
		//print("App: " + point1.x + ", " + point1.y + ", " + point2.x + ", " + point2.y);


		minX2 = faceBox[0];
		minY2 = faceBox[1];
		maxX2 = faceBox[2];
		maxY2 = faceBox[3];
		print("Face: " + minX2 + ", " + minY2 + ", " + maxX2 + ", " + maxY2);

		//point1 = cam.ScreenToWorldPoint(new Vector3(minX2, minY2, cam.nearClipPlane));
		//point2 = cam.ScreenToWorldPoint(new Vector3(maxX2, maxY2, cam.nearClipPlane));
		//print("Face 2: " + point1.x + ", " + point1.y + ", " + point2.x + ", " + point2.y);

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

	private void Update()
	{
		if (contextDetection.InConversation())
		{
			print("is BlockingFace: " + BlockingFace());
			if (!is_trans)
			{
				// change game object's color to transparent
				gameObject.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0f);

			}
			is_trans = true;
		}
		else
		{
			if (is_trans)
			{
				// change game object's color to non-transparent
				gameObject.GetComponent<SpriteRenderer>().color = Color.white;

			}
			is_trans = false;
		}
	}
}
