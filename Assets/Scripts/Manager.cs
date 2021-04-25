using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEditor;
using UnityEngine;

//internal enum SessionMod { SoloGlanceable = 1, SocialGlanceable= 2, SocialCIA= 3};


public class Manager : MonoBehaviour
{
	//public AudioClip audioClip1;
	//public AudioClip audioClip2;
	//public AudioClip audioClip3;
	public AudioClip audioDing;

	const float PRINT_TIME_INTERVAL = 4.0f;
	// Interface mode: is it glanceable or ACI?
	public static bool is_ACI = false;
	//bool solo = true;
	//bool firstSocial = true;
	//private int questionNum = -1;
	//private int trialSetNum;
	//private float time_to_ask_next_Q;
	//private static float time_asked;
	private float time_last_print;
	private static FileLog managerLog;
	private int option = 1;
	//private int[,,] trialSet;

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
	void Start()
	{
		////////////TEST
		//////////////////END TEST
		//solo = true;
		//firstSocial = true;
		// TODO: Should be synced with the webApp
		//trialSetNum = UnityEngine.Random.Range(0, 100) % 2;

		//Create_Trial_Dataset();
		is_ACI = true;
		managerLog = new FileLog();
		managerLog.SetFileName("manager");
		Change_SessionMod();

		//time_asked = Time.time;
		//time_to_ask_next_Q = float.NegativeInfinity;
		time_last_print = 0.0f;

		//Reset Answers Every 5 seconds
		option = 1;
		InvokeRepeating("Reset_Answers", 10.0f, 10.0f);

	}

	void Update()
	{
		if (is_ACI)
		{
			// Update the text boxes
			msgFace.text = "Number of faces: " + num_faces.ToString();

			if (!isTalking || speechContext == MySpeechContext.None)
				msgVoice.text = "Speech: " + isTalking;
			else
				msgVoice.text = "Speech about " + speechContext;
		
		}
		
	}

	
	private void Reset_Answers()
	{
			GameObject.Find("Email").GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("email" + option);
			GameObject.Find("Fitbit").GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("fitbit" + option);
			GameObject.Find("Weather").GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("weather" + option);
			managerLog.WriteLine(" App answers changed to " + option);
			option= (UnityEngine.Random.Range(0, 100)) %6;
	}

	public void OnClick_NxtSession()
	{
		//solo = false;
		Change_SessionMod();
	}

	internal void Change_SessionMod()
	{
		string mod;
		is_ACI = !is_ACI;
		if (is_ACI)
		{
			// Textbox Management
			GameObject.Find("MessageFace").GetComponent<MeshRenderer>().enabled = true;
			GameObject.Find("MessageVoice").GetComponent<MeshRenderer>().enabled = true;
			msgFace = GameObject.Find("MessageFace").GetComponent<TextMesh>();
			msgVoice = GameObject.Find("MessageVoice").GetComponent<TextMesh>();
			msgFace.text = "Connect Face detection to the IP Address: " + MyPhotoCapture.ipEndPoint;
			
			// Context Management
			speechContext = MySpeechContext.None;
			faces_box = new List<List<int>>();
			isTalking = false;
			num_faces = 0;

			mod = "Social ACI";
			//GameObject.Find("Main Camera").GetComponent<MyPhotoCapture>().RunPC();
			//gameObject.GetComponent<SpeechHandler>().Run_MySH();

		}
		else
		{
			GameObject.Find("MessageFace").GetComponent<MeshRenderer>().enabled = false;
			GameObject.Find("MessageVoice").GetComponent<MeshRenderer>().enabled = false;

			mod = "Basic AR";
		}

		GameObject.Find("Weather").GetComponent<AppManager>().Start_Session();
		GameObject.Find("Email").GetComponent<AppManager>().Start_Session();
		GameObject.Find("Fitbit").GetComponent<AppManager>().Start_Session();

		managerLog.WriteLine("  ");
		managerLog.WriteLine(" Session Changed to " + mod);
		managerLog.WriteLine("  ");
		UpdateTooltipText();
	}

	// All ACI Related Functions
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
		if (isTalking != b)
		{
			isTalking = b;
			if (!isTalking)
				Reset_SpeechContext();
			managerLog.WriteLine(isTalking ? " Started Talking" : " Stopped Talking");
		}
	}

	public static void Set_SpeechContext(int context)
	{
		// change the context only if it is different (to improve performance and control IO repeats)
		if ((int)speechContext != context)
		{
			if (context == (int)MySpeechContext.None)
			{
				Reset_SpeechContext();
				managerLog.WriteLine("Talking about None");
				return;
			}
			speechContext = (MySpeechContext)context;
			managerLog.WriteLine(" Talking about " + (MySpeechContext)context);
			Set_justMentioned(true);
		}
		else if (context != (int)MySpeechContext.None)
		{
			// If the context isn't changing but an app was mentioned again make sure the mentioned time-out keeps it in mind
			Set_justMentioned(true);
		}
	}

	internal static void Reset_SpeechContext()
	{
		speechContext = MySpeechContext.None;
	}
	internal static string Get_SpeechContext()
	{
		int n = (int)speechContext;
		return speechContext.ToString();// + n.ToString();
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
		if (num_faces != n_faces)
		{
			managerLog.WriteLine( " " + n_faces + " Faces");
			num_faces = n_faces;
		}
		faces_box = faces;
	}

	public void PlayDing()
	{
		AudioSource audioSource = GetComponent<AudioSource>();
		audioSource.clip = audioDing;
		audioSource.Play();
	}

	public void UpdateTooltipText()
	{
		GameObject.Find("CurrentSession").GetComponent<MeshRenderer>().enabled = true;
		string sessionName;
		if (is_ACI)
			sessionName = "Social ACI";
		else
			sessionName = "Basic AR";

		GameObject.Find("CurrentSession").GetComponent<TextMesh>().text = sessionName;
	}
}
