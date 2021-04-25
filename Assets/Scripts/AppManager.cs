using System.Collections.Generic;
using UnityEngine;
using System;

internal class MessageBoxMessages
{
	public static string AppMentioned { get { return "Mentioned!"; } }
	public static string AppNotMentioned { get { return "Not mentioned!"; } }
}

public class AppManager : MonoBehaviour
{ 
	const float MENTION_TIMEOUT = 7.0f;
	const float BLOCKED_TIMEOUT = 7.0f;
	const float CLICKED_TIMEOUT = 6.0f;

	// Each App's vars
	private int user_manual_override;
	private bool mentioned { get; set; }
	private bool blocking;
	private bool is_trans;
	private bool is_up;
		// time holders
	private float timeWhenBlocked;
	private float timeWhenMentioned;
	private float time_clicked;

	//TextMesh msgBox;
	//GameObject fixationIcon;
	//GameObject incommingConvo;
	//GameObject mentionedIcon;

	// Renderer purposes
	public Rect rect_faceBoxOnScreen, rect_app;

	//Log fiels
	private FileLog sessionLog;


	private void Awake()
	{
		sessionLog = new FileLog();
		sessionLog.SetFileName(gameObject.name);
	}

	public void Start_Session()
	{
		sessionLog.WriteLine("  ");
		sessionLog.WriteLine("Resetting Everything to Start new session");
		GetChildWithName(gameObject, "Msg_Box").GetComponent<MeshRenderer>().enabled = false;
		GetChildWithName(gameObject, "incommingConvo").GetComponent<SpriteRenderer>().enabled = false;
		GetChildWithName(gameObject, "Mentioned").GetComponent<SpriteRenderer>().enabled = false;

		is_trans = !Manager.is_ACI;

		ResetClicked();
		ResetMentioned();
		ResetBlocked();
		ResetY();
		UpdateTranslucency();

		sessionLog.WriteLine(Manager.is_ACI ? "ACI Session Started" : "Glanceable Session Started");
		sessionLog.WriteLine("  ");

	}

	private void FixedUpdate()
	{
		if (Manager.is_ACI)
		{
			UpdateMentioned();
			IsBlockingAnyFaces();
			UpdateTranslucency();
			UpdateY();
		}
	}

	public void ClickedToUpdateTranslucency()
	{
		SetClicked();
		if (is_trans)
		{
			MakeOpaque();
		}
		else
		{
			MakeTranslusent();
		}
	}

	private void UpdateMentioned()
	{
		// Is only called in ACI
		if (!Manager.Get_isTalking())
		{
			if(mentioned)
				ResetMentioned();
			return;
		}
		if (Manager.Get_justMentioned() && Manager.Get_SpeechContext() == gameObject.name)
		{
			SetMentioned();
			Manager.Set_justMentioned(false);
			return;
		}
		if (Time.time - timeWhenMentioned >= MENTION_TIMEOUT) // Timeout
		{
			if (mentioned)
			{
				ResetMentioned();
				if (Manager.Get_SpeechContext() == gameObject.name)
				{
					Manager.Reset_SpeechContext();
				}
			}
			return;
		}
	}
	private void UpdateTranslucency()
	{
		// If manually over-ride 
		if (user_manual_override > 0)
		{
			// if it is ACI and timed out for manual, run defaults of that mode
			if (Manager.is_ACI && Time.time - time_clicked >= CLICKED_TIMEOUT)  // Timeout
			{
				ResetClicked();
			}
			// if it's not ACI or hasn't timedout => don't do anything [and follow user's manual over-ride]
			else
			{
				return;
			}
		}

		if (!Manager.is_ACI)
		{
			// if it's not ACI always keep app's opaque
			if ( is_trans)
				MakeOpaque();	
		}
		else
		{
			// if talking about this app 
			if (mentioned)
			{
				if (is_trans)
					MakeOpaque();
			}
			else if (!is_trans)
				MakeTranslusent();
		}
	}
	private void UpdateY()
	{
		if (blocking && !is_trans)
		{
			if (!is_up)
			{
				sessionLog.WriteLine("Moved up");
				GetComponent<BodyFixed>().MoveUp(true);
				is_up = true;
				return;
			}
		}
		else if (is_up)
			ResetY();
		
	}

	private void MakeTranslusent()
	{
		if (user_manual_override == 0)
			sessionLog.WriteLine("Made Translucent");
		else
			sessionLog.WriteLine("Made Translucent Manually");
		gameObject.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.2f);
		is_trans = true;
	}
	private void MakeOpaque()
	{
		if(user_manual_override == 0)
			sessionLog.WriteLine("Made Opaque");
		else
			sessionLog.WriteLine("Made Opaque Manually");
		gameObject.GetComponent<SpriteRenderer>().color = Color.white;
		is_trans = false;
	}

	public void ResetMentioned()
	{
		sessionLog.WriteLine("Not mentioned");
		timeWhenMentioned = float.NegativeInfinity;
		mentioned = false;
	}
	private void ResetBlocked()
	{
		sessionLog.WriteLine("Is not blocking a face");
		timeWhenBlocked = float.NegativeInfinity;
		blocking = false;
	}
	private void ResetClicked()
	{
		sessionLog.WriteLine("Manual override time out");
		time_clicked = float.NegativeInfinity;
		user_manual_override = 0;
	}
	private void ResetY()
	{
		sessionLog.WriteLine("Moved down");
		GetComponent<BodyFixed>().MoveUp(false);
		is_up = false;
	}

	private void SetMentioned()
	{
		sessionLog.WriteLine("Mentioned in Conversation");
		timeWhenMentioned = Time.time;
		mentioned = true;
	}
	private void SetBlocked()
	{
		sessionLog.WriteLine("Is blocking a face");
		timeWhenBlocked = Time.time;
		blocking = true;
	}
	private void SetClicked()
	{
		time_clicked = Time.time;
		user_manual_override ++;
	}

	void IsBlockingAnyFaces()
	{
		if (blocking && Time.time - timeWhenBlocked < BLOCKED_TIMEOUT)  // not Timeout
		{
			return;
		}

		// Renderer purposes
		rect_faceBoxOnScreen = new Rect(0.0f, 0.0f, 0.0f, 0.0f);
		List<List<int>> faceboxes = Manager.Get_FaceBoxes();

		bool was_blocking = blocking;
		blocking = false;
		foreach (List<int> faceBox in faceboxes)
		{
			bool overlapping = IsOverlapping(faceBox);
			if (overlapping)
			{
				SetBlocked();
				return;
			}
		}
		// if code gets here means it is not blocking rn => if it wasn't blocking already we don't need to change anything
		if (was_blocking)
			ResetBlocked();
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
