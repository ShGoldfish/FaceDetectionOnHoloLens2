using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections;

internal class MessageBoxMessages
{
	public static string AppMentioned { get { return "Mentioned!"; } }
	public static string AppNotMentioned { get { return "Not mentioned!"; } }
}

public class AppManager : MonoBehaviour
{
	bool is_trans = true;
	public int user_manual_override = 0;
	// CIA Variables 
	const float MENTION_TIMEOUT = 7.0f;
	const float BLOCKED_TIMEOUT = 1.5f;
	// Each App's vars
	float timeWhenBlocked;
	float timeWhenMentioned;
	public bool mentioned { get; set; }
	bool blocking;
	TextMesh msgBox;
	//GameObject fixationIcon;
	GameObject incommingConvo;
	GameObject mentionedIcon;
	// Renderer purposes
	public Rect rect_faceBoxOnScreen, rect_app;

	private void Start()
	{
		msgBox = null;
		incommingConvo = null;
		mentionedIcon = null;
		Start_Session();
	}

	private void FixedUpdate()
	{
		if (Manager.is_ACI)
		{
			UpdateMentioned();
			IsBlockingAnyFaces();
			UpdateTranslucency();
		}
	}

	public void Start_Session()
	{
		if (Manager.is_ACI)
		{
			GetChildWithName(gameObject, "Msg_Box").GetComponent<MeshRenderer>().enabled = true;
			GetChildWithName(gameObject, "incommingConvo").GetComponent<SpriteRenderer>().enabled = true;
			GetChildWithName(gameObject, "Mentioned").GetComponent<SpriteRenderer>().enabled = true;

			msgBox = GetChildWithName(gameObject, "Msg_Box").GetComponent<TextMesh>();
			incommingConvo = GetChildWithName(gameObject, "incommingConvo");
			//fixationIcon = GetChildWithName(gameObject, "FixationIcon");
			mentionedIcon = GetChildWithName(gameObject, "Mentioned");

		}
		else
		{
			GetChildWithName(gameObject, "Msg_Box").GetComponent<MeshRenderer>().enabled = false;
			GetChildWithName(gameObject, "incommingConvo").GetComponent<SpriteRenderer>().enabled = false;
			GetChildWithName(gameObject, "Mentioned").GetComponent<SpriteRenderer>().enabled = false;

		}
		Start_Trial();
	}

	public void Start_Trial()
	{
		user_manual_override = 0;
		if (Manager.is_ACI)
		{
			is_trans = true;
		}
		else
			is_trans = false;
		UpdateTranslucency();
		ResetTimeMentioned();
		ResetTimeBlocked();
	}

	public void ClickedToUpdateTranslucency()
	{
		user_manual_override++;
		if (is_trans)
			MakeOpaque();
		else
			MakeTranslusent();
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

	private void UpdateTranslucency()
	{
		if (user_manual_override > 0)
			return;
		if (Manager.is_ACI)
		{
			// not talking
			//if (!Manager.Get_isTalking() || Manager.Get_numFaces() < 1)
			//{
			//	MakeOpaque();
			//	return;
			//}
			//if (blocking)
			//{
			//	MakeTranslusent();
			//	return;
			//}
			// Is talking and there is a face that is not blocking:
			// But has recently been blocked => wait till timeOut
			//if (Time.time - timeWhenBlocked >= 0.0f)
			//{
			//	if (Time.time - timeWhenBlocked < BLOCKED_TIMEOUT)
			//	{
			//		return;
			//	}
			//	else
			//	{
			//		ResetTimeBlocked();
			//	}

			GetComponent<BodyFixed>().MoveUp(blocking);
			//if (blocking)
			//{
			//	//MMove up and change img
			//	GetComponent<BodyFixed>().up = true;
			//}
			//else
			//{
			//	// Original Z
			//	// original img
			//	GetComponent<BodyFixed>().up = false;
			//}

			// if talking about this app 
			if (mentioned)
			{
				MakeOpaque();
				return;
			}
			MakeTranslusent();
		}
		else
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
				SetTimeBlocked();
				break;
			}
			//yield return new WaitForEndOfFrame();
		}
	}

	private void MakeTranslusent()
	{
		gameObject.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.1f);
		//fixationIcon.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.1f);
		if (Manager.is_ACI)
		{
			incommingConvo.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.1f);
			mentionedIcon.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.1f);
		}
		is_trans = true;
	}
	private void MakeOpaque()
	{
		gameObject.GetComponent<SpriteRenderer>().color = Color.white;
		//fixationIcon.GetComponent<SpriteRenderer>().color = Color.white;
		if (Manager.is_ACI)
		{
			if (!blocking)
				incommingConvo.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.1f);
			else
				incommingConvo.GetComponent<SpriteRenderer>().color = Color.white;
			if (!mentioned)
				mentionedIcon.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.1f);
			else
				mentionedIcon.GetComponent<SpriteRenderer>().color = Color.white;
		}
		is_trans = false;
	}
	private void ResetTimeMentioned()
	{
		timeWhenMentioned = float.PositiveInfinity;
		if (Manager.is_ACI)
		{
			msgBox.text = MessageBoxMessages.AppNotMentioned;
			mentionedIcon.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.1f);
		}
		mentioned = false;
	}
	private void SetTimeMentioned()
	{
		timeWhenMentioned = Time.time;
		if (Manager.is_ACI)
		{
			msgBox.text = MessageBoxMessages.AppMentioned;
			mentionedIcon.GetComponent<SpriteRenderer>().color = Color.white;
		}
		mentioned = true;
	}
	private void ResetTimeBlocked()
	{
		timeWhenBlocked = float.PositiveInfinity;
	}
	private void SetTimeBlocked()
	{
		timeWhenBlocked = Time.time;
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
		float faceStartX_onCam = Math.Min(faceBox[0], faceBox[2]),
				faceEndX_onCam = Math.Max(faceBox[0], faceBox[2]);
		float faceStartY_onCam = Math.Min(Screen.height - faceBox[1], Screen.height - faceBox[3]),
				faceEndY_onCam = Math.Max(Screen.height - faceBox[1], Screen.height - faceBox[3]);

		// HoloLens 1
		rect_faceBoxOnScreen = new Rect(faceStartX_onCam - Screen.width / 3,
										faceStartY_onCam + Screen.height / 2,
										faceEndX_onCam - faceStartX_onCam,
										faceEndY_onCam - faceStartY_onCam);

		// For HL 2 roughly 
		// Time Scale is  Doubled on HL2
		//rect_faceBoxOnScreen = new Rect(faceStartX_onCam - Screen.width / 2,
		//								faceStartY_onCam + Screen.height / 4,
		//								(faceEndX_onCam - faceStartX_onCam) / 2.0f,
		//								(faceEndY_onCam - faceStartY_onCam) / 2.0f);

		return rect_faceBoxOnScreen.Overlaps(rect_app);
	}
	
	//public void ChangeFixation()
	//{
	//	bool bodyFixed = GetComponent<BodyFixed>().enabled;
		// Change the icon
		//if (bodyFixed)
		//{
			// switching to world fixed:
			//fixationIcon.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("WorldZone");

		//}
		//else
		//{
			// switching to body fixed:
			//fixationIcon.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("BodyZone");
		//}
	//	GetComponent<BodyFixed>().enabled = !bodyFixed;
	//}

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
}
