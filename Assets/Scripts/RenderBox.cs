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
		Draw();
	}


	void Draw()
	{
		float z = gameObject.transform.position.z;
		Vector3 pt0, pt1, pt2, pt3;

		Rect rect = gameObject.GetComponent<AppManager>().rect_faceBoxOnScreen;

		if (rect.width <= 1.0f)
		{
			rect = gameObject.GetComponent<AppManager>().rect_app;
		}

		pt0 = Camera.main.ScreenToWorldPoint(new Vector3(rect.xMin, rect.yMin, z));
		pt1 = Camera.main.ScreenToWorldPoint(new Vector3(rect.xMax, rect.yMin, z));
		pt2 = Camera.main.ScreenToWorldPoint(new Vector3(rect.xMax, rect.yMax, z));
		pt3 = Camera.main.ScreenToWorldPoint(new Vector3(rect.xMin, rect.yMax, z));

		axisRenderer.positionCount = 5;
		axisRenderer.SetPosition(0, pt0);
		axisRenderer.SetPosition(1, pt1);
		axisRenderer.SetPosition(2, pt2);
		axisRenderer.SetPosition(3, pt3);
		axisRenderer.SetPosition(4, pt0);
	}
}