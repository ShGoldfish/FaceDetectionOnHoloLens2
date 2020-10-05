using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System;
using Microsoft.MixedReality.Toolkit.Utilities;

public class RenderBox : MonoBehaviour
{
	private Color c2 = Color.blue;
	public const float xOffset = 0f;
	public const float yOffset = 0f;
	LineRenderer axisRenderer;
	Vector3 pt0, pt1, pt2, pt3;
	// Use this for initialization

	private void Start()
	{
		gameObject.AddComponent<MixedRealityLineRenderer>();//.LineMaterial = new Material(Shader.Find("Unlit/LRmat"));
		
		axisRenderer = gameObject.GetComponent<LineRenderer>();
		axisRenderer.material = new Material(Shader.Find("Custom/lrShader"));
		axisRenderer.enabled = true;
		axisRenderer.startWidth = 0.1f; //0.002f;
		axisRenderer.endWidth = 0.1f; //0.002f;
		axisRenderer.material.color = c2;
		axisRenderer.startColor = c2;
		axisRenderer.endColor = c2;
	}
	void Update()
	{
		DrawCurrentApp();
		DrawFaceBox();
	}

	void DrawCurrentApp()
	{
		List<int> corners = AppManager.Corners(gameObject);
		float z = gameObject.transform.position.z;
		pt0 = Camera.main.ScreenToWorldPoint(	new Vector3(	corners[0], 
																corners[1],
																z));
		pt1 = Camera.main.ScreenToWorldPoint(	new Vector3(	corners[0] + corners[2],
																corners[1],
																z)); //Camera.main.nearClipPlane));
		pt2 = Camera.main.ScreenToWorldPoint(	new Vector3(	corners[0] + corners[2],
																corners[1] + corners[3],
																z));
		pt3 = Camera.main.ScreenToWorldPoint(	new Vector3(	corners[0],
																corners[1] + corners[3],
																z));

		axisRenderer.positionCount = 5;
		axisRenderer.SetPosition(0, pt0);
		axisRenderer.SetPosition(1, pt1);
		axisRenderer.SetPosition(2, pt2);
		axisRenderer.SetPosition(3, pt3);
		axisRenderer.SetPosition(4, pt0);
	}

	void DrawFaceBox()
	{
		float z = gameObject.transform.position.z;
		// Get minX, minY, maxX and maxY of the face in RW
		List<int> corners = gameObject.GetComponent<AppManager>().faceBoxToShow;
		if (corners == null)
		{
			return;
		}


		pt0 = Camera.main.ScreenToWorldPoint(new Vector3(corners[0], corners[1], z));
		pt1 = Camera.main.ScreenToWorldPoint(new Vector3(corners[2], corners[1], z));
		pt2 = Camera.main.ScreenToWorldPoint(new Vector3(corners[2], corners[3], z));
		pt3 = Camera.main.ScreenToWorldPoint(new Vector3(corners[0], corners[3], z));

		axisRenderer.positionCount = 5;
		axisRenderer.SetPosition(0, pt0);
		axisRenderer.SetPosition(1, pt1);
		axisRenderer.SetPosition(2, pt2);
		axisRenderer.SetPosition(3, pt3);
		axisRenderer.SetPosition(4, pt0);
	}
}