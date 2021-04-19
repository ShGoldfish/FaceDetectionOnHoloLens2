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
		//position = transform.position;
		//rotation = transform.rotation;
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
		//float app_x, app_y, app_z;
		//switch (gameObject.name.ToLower()) {
		//	case "email":
		//		app_x = 0.075f;
		//		app_y = 0.075f;
		//		app_z = 1.25f;
		//		break;
		//	case "fitbit":
		//		app_x = 0.77f;
		//		app_y = 0.073f;
		//		app_z = 1.091f;
		//		break;
		//	case "weather":
		//		app_x = -0.621f;
		//		app_y = 0.072f;
		//		app_z = 1.075f;
		//		break;
		//	default:
		//		app_x = 0.14f;
		//		app_y = -0.1f;
		//		app_z = 0.5f;
		//		break;
		//}
		//position = new Vector3(app_x, app_y, app_z+.075f);
		position = transform.position;
		rotation = transform.rotation;
	}
}
