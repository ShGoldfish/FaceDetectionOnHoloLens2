using System.Collections.Generic;
using UnityEngine;

public class Manager : MonoBehaviour
{
	// Textbox Management
	public string ipEndPoint;
	TextMesh msgVoice;
	TextMesh msgFace;

	// Context Management
	public bool isTalking;
	public int num_faces;
	public List<List<int>> faces_box;

	// PhotoCapture Variables
	public byte[] imageBufferBytesArray;
	public Matrix4x4 cameraToWorldMatrix;
	public Matrix4x4 projectionMatrix;

	private void Start()
	{
		// Textbox Management
		msgFace = GameObject.Find("MessageFace").GetComponent<TextMesh>();
		msgVoice = GameObject.Find("MessageVoice").GetComponent<TextMesh>();
		ipEndPoint = "http://192.168.0.12:9005";
		ipEndPoint = "http://192.168.0.7:9005";
		ipEndPoint = "http://45.3.120.227:9005";
		msgFace.text = "Connect Face detection to the IP Address: " + ipEndPoint;

		// Context Management
		faces_box = new List<List<int>>();
		isTalking = false;
		num_faces = 0;

	}

	private void Update()
	{
		// Update the text boxes
		msgFace.text = "Number of faces: " + num_faces.ToString();
		msgVoice.text = "Ongoing conversation: " + isTalking;
	}


	// Used in App manager
	//public bool InConversation()
	//{
	//	if (num_faces > 0 && isTalking)
	//	{
	//		return true;
	//	}
	//	return false;
	//}

	//private void drawLineRenderer()
	//{
	//	LineRenderer axisRenderer;
	//	if (test)
	//		axisRenderer = gameObject.AddComponent<LineRenderer>();
	//	else
	//		axisRenderer = GetComponent<LineRenderer>();

	//	axisRenderer.material = new Material(Shader.Find("Unlit/Texture"));
	//	axisRenderer.startWidth = 0.02f;
	//	axisRenderer.endWidth = 0.02f;
	//	axisRenderer.positionCount = 4;

	//	axisRenderer.startColor = Color.cyan;
	//	axisRenderer.endColor = Color.cyan;
	//	axisRenderer.material.color = Color.cyan;
	//	axisRenderer.SetPosition(0, new Vector3(Screen.width - faces_box[0][0], Screen.height - faces_box[0][1]));
	//	axisRenderer.SetPosition(1, new Vector3(Screen.width - faces_box[0][0], Screen.height - faces_box[0][3]));
	//	axisRenderer.SetPosition(2, new Vector3(Screen.width - faces_box[0][2], Screen.height - faces_box[0][3]));
	//	axisRenderer.SetPosition(3, new Vector3(Screen.width - faces_box[0][2], Screen.height - faces_box[0][1]));
	//	test = false;
	//}
}
