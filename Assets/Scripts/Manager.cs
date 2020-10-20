using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Manager : MonoBehaviour
{
	// Textbox Management
	public string ipEndPoint;
	TextMesh msgVoice;
	TextMesh msgFace;

	// Context Management
	public bool isTalking;
	private MySpeechContext speechContext;
	public int num_faces;
	public List<List<int>> faces_box;

	// PhotoCapture Variables
	public byte[] imageBufferBytesArray;
//#if UNITY_EDITOR
	//private void Awake()
	//{

		//PlayerSettings.MTRendering = true;
		//PlayerSettings.graphicsJobs = true;
		//PlayerSettings.graphicsJobMode = GraphicsJobMode.Legacy;
		//SystemInfo.renderingthread
	//}
//#endif
	private void Start()
	{
		speechContext = MySpeechContext.None;
		// Textbox Management
		msgFace = GameObject.Find("MessageFace").GetComponent<TextMesh>();
		msgVoice = GameObject.Find("MessageVoice").GetComponent<TextMesh>();
		ipEndPoint = "http://128.173.236.208:9005";
		msgFace.text = "Connect Face detection to the IP Address: " + ipEndPoint;

		// Context Management
		faces_box = new List<List<int>>();
		isTalking = false;
		num_faces = 0;
		imageBufferBytesArray = null;

	}


	private void Update()
	{
		// Update the text boxes
		if (num_faces > 0)
			AppManager.GetChildWithName(Camera.main.gameObject, "incommingConvo").SetActive( true);
		else
			AppManager.GetChildWithName(Camera.main.gameObject, "incommingConvo").SetActive(false);

		msgFace.text = "Number of faces: " + num_faces.ToString();
		if(!isTalking || speechContext == MySpeechContext.None)
			msgVoice.text = "Speech: " + isTalking;
		else 
			msgVoice.text = "Speech about " + speechContext;

	}


	public void Set_SpeechContext(int context)
	{
		speechContext = (MySpeechContext)context;
	}
	public string Get_SpeechContext()
	{
		int n = (int)speechContext;
		//print(speechContext + n.ToString());
		return speechContext + n.ToString();
	}
}

