using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyFixed : MonoBehaviour
{
	private Quaternion rotation;
	private Vector3 position;
	private float y;
	private float up_offset = 0.15f;

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

	public void MoveUp(bool go_up)
	{
		if (go_up)
		{
			position = new Vector3(position.x, y + up_offset, position.z);
		}
		else 
		{
			position = new Vector3(position.x, y, position.z);
		}	
	}

	private void initiate_transform()
	{
		
		position = transform.position;
		y = position.y;
		rotation = transform.rotation;
	}
}
