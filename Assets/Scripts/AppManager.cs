using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections;

internal class MessageBoxMessages
{
	//private MessageBoxMessages(string message) { Message = message; }
	//public string Message { get; set; }
	public static string AppMentioned { get { return "Is mentioned in the conversation"; } }
	public static string AppNotMentioned { get { return "Is not mentioned in the conversation"; } }
}


public class AppManager : MonoBehaviour
{
	const int MENTION_TIMEOUT = 7;
	const int TRANSLUCENCY_TIMEOUT = 3;
	// Each App's vars
	float timeWhenTranslucent;
	float timeWhenMentioned;
	bool mentioned;
	bool trans;
	bool blocking;
	TextMesh msgBox;
	GameObject fixationIcon;
	GameObject incommingConvo;

	// Renderer purposes
	public Rect rect_faceBoxOnScreen, rect_app;


	private void Start()
	{
		msgBox = GetChildWithName(gameObject, "Msg_Box").GetComponent<TextMesh>();
		fixationIcon = GetChildWithName(gameObject, "FixationIcon");
		incommingConvo = GetChildWithName(gameObject, "incommingConvo");
		ResetTimeMentioned();
		ResetTimeTranslucent();
	}


	private void FixedUpdate()
	{
		Time.timeScale = 0.5f;
		UpdateMentioned();
		//StartCoroutine("IsBlockingAnyFaces");
		IsBlockingAnyFaces();
		UpdateTranslucency();
	}

	private void ResetTimeMentioned()
	{
		timeWhenMentioned = float.PositiveInfinity;
		msgBox.text = MessageBoxMessages.AppNotMentioned;
		mentioned = false;

	}
	private void SetTimeMentioned()
	{
		timeWhenMentioned = Time.time;
		msgBox.text = MessageBoxMessages.AppMentioned;
		mentioned = true;
	}
	private void ResetTimeTranslucent()
	{
		timeWhenTranslucent = float.PositiveInfinity;
		trans = false;
	}
	private void SetTimeTranslucent()
	{
		timeWhenTranslucent = Time.time;
		trans = true;
	}

	private void UpdateMentioned()
	{
		if (!Manager.Get_isTalking())
		{
			ResetTimeMentioned();
			return;
		}
		if (Manager.Get_justMentioned() && Manager.Get_SpeechContext() == gameObject.name)
		{
			SetTimeMentioned();
			Manager.Set_justMentioned(false);
			return;
		}
		if (Time.time - timeWhenMentioned >= MENTION_TIMEOUT)
		{
			ResetTimeMentioned();
			if (Manager.Get_SpeechContext() == gameObject.name)
			{
				Manager.Reset_SpeechContext();
			}
			return;
		}
	}

	private void MakeTranslusent()
	{
		SetTimeTranslucent();
		gameObject.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.1f);
		fixationIcon.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.1f);
		incommingConvo.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.1f);
	}
	private void MakeOpaque()
	{
		ResetTimeTranslucent();
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
		if (!Manager.Get_isTalking() || Manager.Get_numFaces() < 1)
		{
			MakeOpaque();
			return;
		}
		if (blocking)
		{
			MakeTranslusent();
			return;
		}
		// Is talking and there is a face that is not blocking:
		// if already translucent, wait til the end of timeOut
		if (trans && Time.time - timeWhenTranslucent < TRANSLUCENCY_TIMEOUT){
			return;
		} //else
		// if talking about this app 
		if (mentioned)
		{
			MakeOpaque();
			return;
		}
		MakeTranslusent();
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
