using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System;
using UnityEditor;

public class AppManager : MonoBehaviour
{
	// General 
	Manager manager;
	private Camera cam;

	// Each App's vars
	public GameObject otherGO;
	public TextMesh msgBlocking;
	int frameSinceTranslucency;


	private void Start()
	{
		manager = GameObject.Find("Manager").GetComponent<Manager>();
		cam = Camera.main;
		frameSinceTranslucency = 0;
	}


	private void Update()
	{
		Time.timeScale = 0.5f;
		bool blocking = IsBlockingAnyFaces();
		StartCoroutine( UpdateTranslucency(blocking));
		msgBlocking.text = "Is Blocking a Face: " + blocking;
		frameSinceTranslucency++;

	}


	private IEnumerator UpdateTranslucency(bool blocking)
	{
		if (blocking && manager.isTalking)
		{
			// Make it translucent
			gameObject.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.1f);
			GetChildWithName(gameObject, "FixationIcon").GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.1f);
			frameSinceTranslucency = 0;

		}
		else if (frameSinceTranslucency < 3 * 60)
		{
			yield return null ;
		}
		else
		{
			// Make it opaque
			gameObject.GetComponent<SpriteRenderer>().color = Color.white;
			GetChildWithName(gameObject, "FixationIcon").GetComponent<SpriteRenderer>().color = Color.white;
		}
	}


	private bool IsBlockingAnyFaces()
	{
		foreach (List<int> faceBox in manager.faces_box)
		{
			// Commentd for Test purpose only:
			if (IsOverlapping(faceBox))
			{
				return true;
			}
			// uncommentd for Test purpose only:
			//return true;
		}
		return false;
	}


	private bool IsOverlapping(List<int> faceBox)
	{
		float minX1, minY1, maxX1, maxY1, minX2, minY2, maxX2, maxY2;
		// App
		List<int> corners = Corners(gameObject);
		minX1 = corners[0];
		minY1 = corners[1];
		maxX1 = corners[2];
		maxY1 = corners[3];
		Vector3 cam_app_min = manager.cameraToWorldMatrix.inverse * new Vector3(minX1, minY1);
		Vector3 cam_app_max = manager.cameraToWorldMatrix.inverse * new Vector3(maxX1, maxY1);

		// Face
		minX2 = faceBox[0];
		minY2 = faceBox[1];
		maxX2 = faceBox[2] - minX2;
		maxY2 = faceBox[3] - minY2;
		Vector3 faceInRW_min = photoCapture.UnProjectVector(manager.projectionMatrix, new Vector3(minX2, minY2));
		Vector3 faceInRW_max = photoCapture.UnProjectVector(manager.projectionMatrix, new Vector3(maxX2, maxY2));
		Vector3 cam_face_min = manager.cameraToWorldMatrix.inverse * faceInRW_min;
		Vector3 cam_face_max = manager.cameraToWorldMatrix.inverse * faceInRW_max;
		Rect faceBoxOnCam = new Rect(cam_face_min.x, cam_face_min.y, cam_face_max.x, cam_face_max.y);

		//Vector3 pts0 = manager.cameraToWorldMatrix.inverse * photoCapture.UnProjectVector(manager.projectionMatrix, new Vector3(faceBox[0], faceBox[1]));
		//Vector3 pts1 = manager.cameraToWorldMatrix.inverse * photoCapture.UnProjectVector(manager.projectionMatrix, new Vector3(faceBox[0] + faceBox[2], faceBox[1] + faceBox[3]));
		//Rect faceBoxOnCam = new Rect(pts0.x, pts0.y, pts1.x - pts0.x, pts1.y - pts0.y);

		return faceBoxOnCam.Overlaps(new Rect(minX1, minY1, maxX1, maxY1));
	}


	public void ChangeFixation()
	{
		//print(gameObject.name + " changing fixation!");
		
		otherGO.transform.position = transform.position;
		otherGO.transform.rotation = transform.rotation;

		AppManager otherGOAppManager = otherGO.GetComponent<AppManager>();
		otherGOAppManager.manager = GameObject.Find("Manager").GetComponent<Manager>();
		otherGOAppManager.frameSinceTranslucency = frameSinceTranslucency;
		//otherGOAppManager.isTranslucent = isTranslucent;
		otherGOAppManager.cam = Camera.main;
		otherGOAppManager.msgBlocking.text = msgBlocking.text;
		otherGOAppManager.otherGO = gameObject;

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
}
