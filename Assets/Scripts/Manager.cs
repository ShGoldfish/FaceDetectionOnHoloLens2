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
	public static Matrix4x4 cameraToWorldMatrix;
	public static Matrix4x4 projectionMatrix;

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


	// Taken from https://docs.microsoft.com/en-us/windows/mixed-reality/locatable-camera
	/// <summary>
	/// Helper method to convert hololens application space to world space
	/// </summary>
	/// <param name="to"></param>
	/// 
	/// <returns></returns>
	public static Vector3 UnProjectVector(Vector3 img)
	{
		Vector3 world = new Vector3(0, 0, 0);
		var axsX = projectionMatrix.GetRow(0);
		var axsY = projectionMatrix.GetRow(1);
		var axsZ = projectionMatrix.GetRow(2);
		world.z = img.z / axsZ.z;
		world.y = (img.y - (world.z * axsY.z)) / axsY.y;
		world.x = (img.x - (world.z * axsX.z)) / axsX.x;
		return world;
	}
}

