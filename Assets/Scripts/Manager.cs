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
		ipEndPoint = "http://128.173.236.208:9005";
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
}

