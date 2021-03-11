using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyFixed : MonoBehaviour
{
	private Quaternion rotation;
	private Vector3 position;
	float time_moved;
	const float MOVE_TIMEOUT = 4.0f;

	void Awake()
	{
		initiate_transform();
		time_moved = Time.time;
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

	public void MoveUp(bool go_up)
	{
		float app_y;
		
			if (go_up)
			{
				app_y = 0.2f;
				time_moved = Time.time;
				position = new Vector3(position.x, app_y, position.z);
			}
			else if (Time.time - time_moved > MOVE_TIMEOUT)
			{
				app_y = -0.1f;
				position = new Vector3(position.x, app_y, position.z);
			}	
	}

	private void initiate_transform()
	{
		int app_num;
		float app_x, app_y, app_z;
		app_num = Convert.ToInt16(gameObject.name.Substring(gameObject.name.Length - 1));
		if (app_num == 0)
		{
			app_x = 0.5f ;
			app_y = -0.5f;
			app_z = .0f;
		}
		else
		{
			app_x = 0.14f * (app_num - 2);
			app_y = -0.1f;
			app_z = 1.0f;
			if (app_num == 2)
				app_z = 1.04f;
				
		}
		position = new Vector3(app_x, app_y, app_z);
		rotation = transform.rotation;
	}
}
