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
	public Rect rect_faceBoxOnScreen, rect_app;


	private void Start()
	{
		manager = GameObject.Find("Manager").GetComponent<Manager>();
		frameSinceTranslucency = 0;
	}


	private void Update()
	{
		Time.timeScale = 0.5f;
		bool blocking = IsBlockingAnyFaces();
		//StartCoroutine( UpdateTranslucency(blocking));
		UpdateTranslucency(blocking);
		msgBlocking.text = "Is Blocking a Face: " + blocking;
		frameSinceTranslucency++;
	}


	private void UpdateTranslucency(bool blocking)
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
			//yield return null;
		}
		else
		{
			// Make it opaque
			gameObject.GetComponent<SpriteRenderer>().color = Color.white;
			GetChildWithName(gameObject, "FixationIcon").GetComponent<SpriteRenderer>().color = Color.white;
		}
	}
	//private IEnumerator UpdateTranslucency(bool blocking)
	//{
	//	if (blocking && manager.isTalking)
	//	{
	//		// Make it translucent
	//		gameObject.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.1f);
	//		GetChildWithName(gameObject, "FixationIcon").GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.1f);
	//		frameSinceTranslucency = 0;

	//	}
	//	else if (frameSinceTranslucency < 3 * 60)
	//	{
	//		yield return null ;
	//	}
	//	else
	//	{
	//		// Make it opaque
	//		gameObject.GetComponent<SpriteRenderer>().color = Color.white;
	//		GetChildWithName(gameObject, "FixationIcon").GetComponent<SpriteRenderer>().color = Color.white;
	//	}
	//}


	private bool IsBlockingAnyFaces()
	{
		// Renderer purposes
		rect_faceBoxOnScreen = new Rect(0.0f, 0.0f, 0.0f, 0.0f);

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
		// *********************************App 
		// OnScreen x, y, w, h
		List<int> corners = Corners(gameObject);
		rect_app = new Rect(corners[0],
							corners[1],
							corners[2],
							corners[3]);

		// *********************************Face
		Vector3 faceCamSpace_pt0, faceCamSpace_pt2, faceInRW_pt0, faceInRW_pt2, OnScreen_face_pt0, OnScreen_face_pt2;
		// On Camera startX, startY, endX, endY of the face
		float faceStartX_onCam = Math.Min(faceBox[0], faceBox[2]),
				faceEndX_onCam = Math.Max(faceBox[0], faceBox[2]);
		// Min y from py is Max y in unity
		float faceStartY_onCam = Math.Min(Screen.height - faceBox[1], Screen.height - faceBox[3]),
				faceEndY_onCam = Math.Max(Screen.height - faceBox[1], Screen.height - faceBox[3]);

		var worldSpaceCameraPos = Manager.UnProjectVector(Vector3.zero , Manager.projectionMatrix);     // camera location in world space

		rect_faceBoxOnScreen = new Rect(faceStartX_onCam - Screen.width / 2,
										faceStartY_onCam + Screen.height / 2, 
										faceEndX_onCam - faceStartX_onCam,
										faceEndY_onCam - faceStartY_onCam);

		// Unproject the 2D points in the image to get the points in the world using the ph.frame's projectionMatrix
		// Manager.cameraToWorldMatrix Works best so far
		// https://gist.github.com/tarukosu/e2aa45945f38717cf6f54b043939c9ba
		//Vector2 imagePosZeroToOne0 = new Vector2(faceStartX_onCam / Screen.width, faceStartY_onCam / Screen.height),
		//		imagePosZeroToOne2 = new Vector2(faceEndX_onCam / Screen.width, faceEndY_onCam / Screen.height);

		//var imagePosProjected0 = (imagePosZeroToOne0 * 2) - new Vector2(1, 1);
		//var imagePosProjected2 = (imagePosZeroToOne2 * 2) - new Vector2(1, 1);
		//OnScreen_face_pt0 = Camera.main.ViewportToScreenPoint(new Vector3(imagePosZeroToOne0.x, imagePosZeroToOne0.y, 0));
		//OnScreen_face_pt2 = Camera.main.ViewportToScreenPoint(new Vector3(imagePosZeroToOne2.x, imagePosZeroToOne2.y, 0));
		//var cameraSpacePos0 = Manager.UnProjectVector(new Vector3(imagePosProjected0.x, imagePosProjected0.y, 1.0f), Camera.main.projectionMatrix);
		//var cameraSpacePos2 = Manager.UnProjectVector(new Vector3(imagePosProjected2.x, imagePosProjected2.y, 1.0f), Camera.main.projectionMatrix);
		//var cameraSpacePos0 = Manager.UnProjectVector(new Vector3(faceStartX_onCam, faceStartY_onCam, 1), Manager.projectionMatrix);
		//var cameraSpacePos2 = Manager.UnProjectVector(new Vector3(faceEndX_onCam, faceEndY_onCam, 1), Manager.projectionMatrix);

		//var worldSpaceCameraPos = Camera.main.cameraToWorldMatrix.MultiplyPoint(Vector3.zero);     // camera location in world space

		//var worldSpaceBoxPos0 = Camera.main.cameraToWorldMatrix.MultiplyPoint(cameraSpacePos0) ;// - 2 * (new Vector3(worldSpaceCameraPos.x, worldSpaceCameraPos.y, 0.0f));   
		//var worldSpaceBoxPos2 = Camera.main.cameraToWorldMatrix.MultiplyPoint(cameraSpacePos2) ;// - 2 * (new Vector3(worldSpaceCameraPos.x, worldSpaceCameraPos.y, 0.0f));

		//OnScreen_face_pt0 = Camera.main.WorldToScreenPoint(worldSpaceBoxPos0);
		//OnScreen_face_pt2 = Camera.main.WorldToScreenPoint(worldSpaceBoxPos2);



		// Method 1:
		//faceCamSpace_pt0 = Manager.UnProjectVector(new Vector3(faceStartX_onCam, faceStartY_onCam), Camera.main.projectionMatrix);
		//faceCamSpace_pt2 = Manager.UnProjectVector(new Vector3(faceEndX_onCam, faceEndY_onCam), Camera.main.projectionMatrix);

		//// Translate the points from camera space to world
		//faceInRW_pt0 = Camera.main.cameraToWorldMatrix.MultiplyPoint(faceCamSpace_pt0);
		//faceInRW_pt2 = Camera.main.cameraToWorldMatrix.MultiplyPoint(faceCamSpace_pt2);

		//OnScreen_face_pt0 = Camera.main.WorldToScreenPoint(faceInRW_pt0);
		//OnScreen_face_pt2 = Camera.main.WorldToScreenPoint(faceInRW_pt2);

		//rect_faceBoxOnScreen = new Rect(OnScreen_face_pt0.x,
		//								OnScreen_face_pt0.y,
		//								OnScreen_face_pt2.x - OnScreen_face_pt0.x,
		//								OnScreen_face_pt2.y - OnScreen_face_pt0.y);

		return rect_faceBoxOnScreen.Overlaps(rect_app);
	}


	public void ChangeFixation()
	{
		otherGO.transform.position = transform.position;
		otherGO.transform.rotation = transform.rotation;

		AppManager otherGOAppManager = otherGO.GetComponent<AppManager>();
		otherGOAppManager.manager = GameObject.Find("Manager").GetComponent<Manager>();
		otherGOAppManager.frameSinceTranslucency = frameSinceTranslucency;
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
