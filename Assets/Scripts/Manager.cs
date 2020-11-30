using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Manager : MonoBehaviour
{
	// Textbox Management
	static TextMesh msgVoice;
	static TextMesh msgFace;

	// Context Management
	static bool isTalking;
	static MySpeechContext speechContext;
	static bool justMentioned;
	static int num_faces;
	static List<List<int>> faces_box;

	// test
	// public bool test_talking;
//#if UNITY_EDITOR
	//private void Awake()
	//{

		//PlayerSettings.MTRendering = true;
		//PlayerSettings.graphicsJobs = true;
		//PlayerSettings.graphicsJobMode = GraphicsJobMode.Legacy;
		//SystemInfo.renderingthread
	//}
//#endif
	void Start()
	{
		speechContext = MySpeechContext.None;
		// Textbox Management
		msgFace = GameObject.Find("MessageFace").GetComponent<TextMesh>();
		msgVoice = GameObject.Find("MessageVoice").GetComponent<TextMesh>();
		msgFace.text = "Connect Face detection to the IP Address: " + MyPhotoCapture.ipEndPoint;

		// Context Management
		faces_box = new List<List<int>>();
		isTalking = false;
		num_faces = 0;

	}
	void Update()
	{
		// Update the text boxes
		msgFace.text = "Number of faces: " + num_faces.ToString();

		if(!isTalking || speechContext == MySpeechContext.None)
			msgVoice.text = "Speech: " + isTalking;
		else 
			msgVoice.text = "Speech about " + speechContext;
		//test
		// Set_isTalking(test_talking);
	}

	internal static void Set_justMentioned(bool v)
	{
		if (speechContext == MySpeechContext.None)
			justMentioned = false;
		else
			justMentioned = v;
	}

	internal static bool Get_justMentioned()
	{
		return justMentioned;
	}


	internal static bool Get_isTalking()
	{
		return isTalking;
	}
	internal static void Set_isTalking(bool b)
	{
		isTalking = b;
		if(!isTalking)
			Reset_SpeechContext();
	}

	internal static void Set_SpeechContext(int context)
	//public void Set_SpeechContext(int context)
	{
		if (context == (int)MySpeechContext.None)
			return;
		speechContext = (MySpeechContext)context;
		Set_justMentioned(true);

		// Debug.Log purpose
		//int n = (int)speechContext;
		//Debug.Log(speechContext + n.ToString());
	}

	internal static void Reset_SpeechContext()
	{
		speechContext = MySpeechContext.None;
		//int n = (int)speechContext;
		//Debug.Log(speechContext + n.ToString());
	}
	internal static string Get_SpeechContext()
	{
		int n = (int)speechContext;
		return speechContext + n.ToString();
	}

	internal static int Get_numFaces()
	{
		return num_faces;
	}
	internal static List<List<int>> Get_FaceBoxes()
	{
		return faces_box;
	}

	internal static void Set_Faces(int n_faces, List<List<int>> faces)
	{
		num_faces = n_faces;
		faces_box = faces;
	}
}

