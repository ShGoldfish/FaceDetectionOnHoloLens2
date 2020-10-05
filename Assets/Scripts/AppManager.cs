using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System;

public class AppManager : MonoBehaviour
{
	// General 
	Manager manager;

	// Each App's vars
	public GameObject otherGO;
	public TextMesh msgBlocking;
	int frameSinceTranslucency;

	// Renderer purposes
	public List<int> faceBoxToShow = null;


	private void Start()
	{
		manager = GameObject.Find("Manager").GetComponent<Manager>();
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
		// Renderer purposes
		faceBoxToShow = null;

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
		float	appX_onScreen, appY_onScreen, appW_onScreen, appH_onScreen,
				faceStartX_onCam, faceStartY_onCam, faceEndX_onCam, faceEndY_onCam;
		// App
		List<int> corners = Corners(gameObject);
		// onScreen x, y, w, h
		appX_onScreen = corners[0];
		appY_onScreen = corners[1];
		appW_onScreen = corners[2];
		appH_onScreen = corners[3];
		//Vector3 cam_app_min = Manager.cameraToWorldMatrix.inverse * new Vector3(minX1, minY1, Camera.main.nearClipPlane);
		//Vector3 cam_app_max = Manager.cameraToWorldMatrix.inverse * new Vector3(maxX1, maxY1, Camera.main.nearClipPlane);

		// Face
		// On Camera startX, startY, endX, endY of the face
		faceStartX_onCam = faceBox[0];
		faceStartY_onCam = faceBox[1];
		faceEndX_onCam = faceBox[2];
		faceEndY_onCam = faceBox[3];

		// Unproject the 2D points in the image to get the points in the world 
		Vector3 faceInRW_pt0 = Manager.UnProjectVector(new Vector3(faceStartX_onCam,
																	faceStartY_onCam));
		Vector3 faceInRW_pt2 = Manager.UnProjectVector(new Vector3(faceEndX_onCam,
																	faceEndY_onCam));

		//Vector3 faceInRW_pt0 = Manager.cameraToWorldMatrix * new Vector3(faceStartX_onCam,
		//															faceStartY_onCam);
		//Vector3 faceInRW_pt2 = Manager.cameraToWorldMatrix * new Vector3(faceEndX_onCam,
		//															faceEndY_onCam);

		// Translate the points from world space to camera space
		//Vector3 cam_face_pt0 = Manager.cameraToWorldMatrix.inverse * faceInRW_pt0;
		//Vector3 cam_face_pt2 = Manager.cameraToWorldMatrix.inverse * faceInRW_pt2;

		//// Translate the points from world space to Screen space
		Vector3 cam_face_pt0 = Camera.main.WorldToScreenPoint(faceInRW_pt0);
		Vector3 cam_face_pt2 = Camera.main.WorldToScreenPoint(faceInRW_pt2);

		Rect faceBoxOnScreen = new Rect(cam_face_pt0.x,
										cam_face_pt0.y, 
										cam_face_pt2.x - cam_face_pt0.x,
										cam_face_pt2.y - cam_face_pt0.y);

		// Renderer purposes
		
		faceBoxToShow = new List<int>() {	Convert.ToInt32(faceInRW_pt0.x),
											Convert.ToInt32(faceInRW_pt0.y),
											Convert.ToInt32(faceInRW_pt2.x),
											Convert.ToInt32(faceInRW_pt2.y) };

		return faceBoxOnScreen.Overlaps(new Rect(	appX_onScreen, 
													appY_onScreen,
													appW_onScreen,
													appH_onScreen));
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

	/// <summary>
	/// Returns the on screen x, y, w, h of the GameObject go's collider
	/// </summary>
	/// <param name="go"></param>
	/// <returns></returns>
	public static List<int> Corners(GameObject go)
	{
		Bounds bounds = go.GetComponent<Collider>().bounds;
		Vector3 cen = bounds.center;
		Vector3 ext = bounds.extents;
		float screenheight = Screen.height;
		Camera cam = Camera.main;
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
