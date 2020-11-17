using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections;

public class AppManager : MonoBehaviour
{
	const int MENTION_TIMEOUT = 5 * 60;
	const int TRANSLUCENCY_TIMEOUT = 3 * 60;
	// Each App's vars
	int frameSinceTranslucency;
	int frameSinceMentioned;
	bool blocking;
	TextMesh msgBlocking;
	GameObject fixationIcon;
	GameObject incommingConvo;

	// Renderer purposes
	public Rect rect_faceBoxOnScreen, rect_app;


	private void Start()
	{
		msgBlocking = GetChildWithName(gameObject, "Msg_Bocking").GetComponent<TextMesh>();
		fixationIcon = GetChildWithName(gameObject, "FixationIcon");
		incommingConvo = GetChildWithName(gameObject, "incommingConvo");
		frameSinceMentioned = 0;
		frameSinceTranslucency = 0;
	}


	private void Update()
	{
		Time.timeScale = 0.5f;
		UpdateMentioned();
		//StartCoroutine("IsBlockingAnyFaces");
		IsBlockingAnyFaces();
		msgBlocking.text = "Is Blocking a Face: " + blocking;
		UpdateTranslucency();
	}

	private void UpdateMentioned()
	{
		if (!Manager.Get_isTalking())
		{
			frameSinceMentioned = 0;
			return;
		}
		if (Manager.Get_SpeechContext() == gameObject.name)
		{
			// just mentioned is to make sure if count down already started but is mentioned again
			if (frameSinceMentioned == 0 || Manager.Get_justMentioned())
			{
				frameSinceMentioned = 1;
				Manager.Set_justMentioned(false);
				return;
			}
			if (frameSinceMentioned >= MENTION_TIMEOUT)
			{
				frameSinceMentioned = 0;
				Manager.SpeechContext_TimeOut();
				return;
			}
		}
		if (frameSinceMentioned >= MENTION_TIMEOUT)
		{
			frameSinceMentioned = 0;
			return;
		}
		if (frameSinceMentioned > 0)
		{
			frameSinceMentioned++;
			return;
		}
	}

	private void MakeTranslusent()
	{
		gameObject.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.1f);
		fixationIcon.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.1f);
		incommingConvo.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.1f);
		frameSinceTranslucency = 1;
	}
	private void MakeOpaque()
	{

		gameObject.GetComponent<SpriteRenderer>().color = Color.white;
		fixationIcon.GetComponent<SpriteRenderer>().color = Color.white;
		if (!blocking)
			incommingConvo.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.1f);
		else
			incommingConvo.GetComponent<SpriteRenderer>().color = Color.white;
	}

	private void UpdateTranslucency()
	{
		// not talking
		if (!Manager.Get_isTalking())
		{
			MakeOpaque();
			return;
		}
		if (blocking)
		{
			MakeTranslusent();
			return;
		}
		// Is talking and not blocking:
		if (frameSinceTranslucency > 0 && frameSinceTranslucency < TRANSLUCENCY_TIMEOUT)
		{
			frameSinceTranslucency++;
			return;
		}
		// talking about app which 
		if (frameSinceMentioned > 0)
		{
			MakeOpaque();
			return;
		}
		if (Manager.Get_numFaces() > 0)
		{
			MakeTranslusent();
			return;
		}
		// is talking but there is no face
		MakeOpaque();
	}


	//IEnumerator IsBlockingAnyFaces()
	void IsBlockingAnyFaces()
	{
		// Renderer purposes
		rect_faceBoxOnScreen = new Rect(0.0f, 0.0f, 0.0f, 0.0f);
		List<List<int>> faceboxes = Manager.Get_FaceBoxes();
		blocking = false;
		foreach (List<int> faceBox in faceboxes)
		{
			bool overlapping = IsOverlapping(faceBox);
			if (overlapping)
			{
				blocking = true;
				break;
			}
			//yield return new WaitForEndOfFrame();
		}
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
		// HoloLens 1
		float faceStartX_onCam = Math.Min(faceBox[0], faceBox[2]),
				faceEndX_onCam = Math.Max(faceBox[0], faceBox[2]);
		float faceStartY_onCam = Math.Min(Screen.height - faceBox[1], Screen.height - faceBox[3]),
				faceEndY_onCam = Math.Max(Screen.height - faceBox[1], Screen.height - faceBox[3]);
		rect_faceBoxOnScreen = new Rect(faceStartX_onCam - Screen.width / 3,
										faceStartY_onCam + Screen.height / 2,
										faceEndX_onCam - faceStartX_onCam,
										faceEndY_onCam - faceStartY_onCam);

		// For HL 2 roughly 
		//float faceStartX_onCam = Math.Min(faceBox[0], faceBox[2]),
		//		faceEndX_onCam = Math.Max(faceBox[0], faceBox[2]);
		//float faceStartY_onCam = Math.Min(Screen.height - faceBox[1], Screen.height - faceBox[3]),
		//		faceEndY_onCam = Math.Max(Screen.height - faceBox[1], Screen.height - faceBox[3]);

		//rect_faceBoxOnScreen = new Rect(faceStartX_onCam - Screen.width / 2,
		//								faceStartY_onCam - Screen.height / 2,
		//								(faceEndX_onCam - faceStartX_onCam) / 2.0f,
		//								(faceEndY_onCam - faceStartY_onCam) / 2.0f);

		return rect_faceBoxOnScreen.Overlaps(rect_app);
	}


	public void ChangeFixation()
	{
		bool bodyFixed = GetComponent<BodyFixed>().enabled;
		// Change the icon
		if (bodyFixed)
		{
			// switching to world fixed:
			fixationIcon.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("WorldZone");

		}
		else
		{
			// switching to body fixed:
			fixationIcon.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("BodyZone");
		}
		GetComponent<BodyFixed>().enabled = !bodyFixed;
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
