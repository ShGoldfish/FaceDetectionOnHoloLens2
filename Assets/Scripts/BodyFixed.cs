using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyFixed : MonoBehaviour
{
	private Quaternion rotation;
	private Vector3 position;
	

	void Awake()
	{
		initiate_transform();
	}


	private void OnEnable()
	{
		initiate_transform();
	}


	void LateUpdate()
	{
		transform.position = Camera.main.transform.position + position; 
		transform.rotation= rotation;
	}


	private void initiate_transform()
	{
		int app_num;
		float app_x, app_y, app_z;
		app_num = Convert.ToInt16(gameObject.name.Substring(gameObject.name.Length - 1));
		app_x = 0.14f * (app_num - 2);
		app_y = 0.0f;
		app_z = 1.0f;
		if (app_num == 2)
			app_z = 1.04f;
		position = new Vector3(app_x, app_y, app_z);
		rotation = transform.rotation;
	}
}
