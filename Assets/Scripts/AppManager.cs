﻿using System.Collections.Generic;
using UnityEngine;
using System;

public class AppManager : MonoBehaviour
{
	// Each App's vars
	int frameSinceTranslucency;
	private bool blocking;
	public TextMesh msgBlocking;
	private GameObject fixationIcon;
	private GameObject incommingConvo;


	// Renderer purposes
	public Rect rect_faceBoxOnScreen, rect_app;


	private void Start()
	{
		frameSinceTranslucency = 0;
		fixationIcon = GetChildWithName(gameObject, "FixationIcon");
		incommingConvo = GetChildWithName(gameObject, "incommingConvo");
	}


	private void Update()
	{
		Time.timeScale = 0.5f;
		IsBlockingAnyFaces();
		msgBlocking.text = "Is Blocking a Face: " + blocking;
		UpdateTranslucency();
	}

	private void MakeTranslusent()
	{
		gameObject.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.1f);
		fixationIcon.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.1f);
		incommingConvo.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.1f);
		frameSinceTranslucency = 0;
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
		// Is talking:
		// talking about app which is not blocking
		if (Manager.Get_SpeechContext() == gameObject.name && !blocking)
		{
			MakeOpaque();
			return;
		}
		if (frameSinceTranslucency < 3 * 60)
		{
			frameSinceTranslucency++;
			return;
		}
		if (Manager.Get_numFaces() > 0)
			MakeTranslusent();
	}


	private void IsBlockingAnyFaces()
	{
		// Renderer purposes
		rect_faceBoxOnScreen = new Rect(0.0f, 0.0f, 0.0f, 0.0f);
		foreach (List<int> faceBox in Manager.Get_FaceBoxes())
		{
			if (IsOverlapping(faceBox))
			{
				blocking = true;
				return;
			}
		}
		blocking = false;
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
