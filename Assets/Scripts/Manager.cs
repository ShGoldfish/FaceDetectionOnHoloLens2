using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

//internal enum SessionMod { SoloGlanceable = 1, SocialGlanceable= 2, SocialCIA= 3};


public class Manager : MonoBehaviour
{
	// Interface mode: is it glanceable or ACI?
	public static bool is_ACI = false;
	private int trialSetNum = 0;
	private int questionNum = -1;
	private float time_to_ask_next_Q;
	private static float time_asked;
	private FileLog sessionLog;
	private int[,,] trialSets;

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
		Create_Trial_Dataset();
		//Test
		OnClick_NxtSession();

		// TODO: must be called in trail manager: mode must be passed in from there
		//Change_SessionMod(is_ACI);
		time_asked = Time.time;
		time_to_ask_next_Q = float.NegativeInfinity;
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
			//test
			// Set_isTalking(test_talking);
		}
		//if time_to_ask_next_Q is -inf that means they are answering so do not continue  Trial set timer to run the next question
		if (!float.IsNegativeInfinity(time_to_ask_next_Q))
		{
			if ( Time.time >= time_to_ask_next_Q)
			{
				//TrialSet[trialSetNum][questionNum][1] is 1 or 2 or 3 showing if the question 1 (from app 1) should be asked
				//ask (play sound) question type TrialSet[trialSetNum][questionNum][1] 
				time_asked = Time.time;
				time_to_ask_next_Q = float.NegativeInfinity;
			}

			// Speech detection "Answer is:" sets
			//float duration_to_ans = Time.time - time_asked;
			//sessionLog.WriteLine(questionNum + ", " + duration_to_ans + ", " + TrialSet[trialSetNum][questionNum][1] + ", " + TrialSet[trialSetNum][questionNum][2]);
		}
	}

	public void OnClick_NxtSession()
	{
		// come with method for the rotation of Change_SessionMod parameter 
		Change_SessionMod(!is_ACI);
		sessionLog = new FileLog();
		trialSetNum = (trialSetNum + 1) % 2;
		questionNum = -1;

		// time_to_ask_next_Q Changes after a question is answered. Trial set rund based on this [will be -inf to indicate do not run fwd]
		sessionLog.SetHeader( "_SessionFile", trialSetNum + "_" + is_ACI + "_" + Time.time);
		Start_nxt_Trial();
	}

	internal void Change_SessionMod(bool input_Mod)
	{
		//may get an input from trial manager to set glanceable or non [each has 2 glanceable and 1 intelligent]
		//if (is_ACI)
		//{
		//	gameObject.GetComponent<SpeechHandler>().End_MySH();
		//}

		is_ACI = input_Mod;
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
			//GameObject.Find("Main Camera").GetComponent<MyPhotoCapture>().RunPC();
			//gameObject.GetComponent<SpeechHandler>().Run_MySH();

		}
		else
		{
			GameObject.Find("MessageFace").GetComponent<MeshRenderer>().enabled = false;
			GameObject.Find("MessageVoice").GetComponent<MeshRenderer>().enabled = false;
		}
		GameObject.Find("Weather1").GetComponent<AppManager>().Start_Session();
		GameObject.Find("Email2").GetComponent<AppManager>().Start_Session();
		GameObject.Find("Fitbit3").GetComponent<AppManager>().Start_Session();
	}

	private void Start_nxt_Trial()
	{
		questionNum++;
		time_to_ask_next_Q = trialSets[trialSetNum, questionNum, 0] + Time.time;
		// For each app start trial
		GameObject.Find("Weather1").GetComponent<AppManager>().Start_Trial();
		GameObject.Find("Email2").GetComponent<AppManager>().Start_Trial();
		GameObject.Find("Fitbit3").GetComponent<AppManager>().Start_Trial();
	}
	private void Create_Trial_Dataset()
	{

		//trialSets is trialSetNum X question num X 3 [= 0: time, 1: App_num, 2: correct_answer_option]
		trialSets = new int[2, 15, 3] {
			{
				{ 20, 1, 4}, // End Trial 1
				{ 15, 3, 3}, // End Trial 2
				{ 10, 1, 1}, // End Trial 3
				{ 45, 3, 2}, // End Trial 4
				{ 10, 3, 3}, // End Trial 5
				{ 10, 2, 1}, // End Trial 6
				{ 30, 2, 3}, // End Trial 7
				{ 15, 3, 1}, // End Trial 8
				{ 15, 1, 3}, // End Trial 9
				{ 20, 2, 1}, // End Trial 10
				{ 20, 3, 4}, // End Trial 11
				{ 15, 2, 2}, // End Trial 12
				{ 10, 1, 2}, // End Trial 13
				{ 20, 2, 4}, // End Trial 14
				{ 10, 1, 4} // End Trial 15
			}, // End Trial Set 1
			{
				{ 10, 1, 4}, // End Trial 15
				{ 20, 2, 4}, // End Trial 14
				{ 10, 1, 2}, // End Trial 13
				{ 15, 2, 2}, // End Trial 12
				{ 20, 3, 4}, // End Trial 11
				{ 20, 2, 1}, // End Trial 10
				{ 15, 1, 3}, // End Trial 9
				{ 15, 3, 1}, // End Trial 8
				{ 30, 2, 3}, // End Trial 7
				{ 10, 2, 1}, // End Trial 6
				{ 10, 3, 3}, // End Trial 5
				{ 45, 3, 2}, // End Trial 4
				{ 10, 1, 1}, // End Trial 3
				{ 15, 3, 3}, // End Trial 2
				{ 20, 1, 4} // End Trial 1
			}// End Trial Set 2
		};

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
		time_asked = Time.time;
		Set_justMentioned(true);
	}

	internal static void Reset_SpeechContext()
	{
		speechContext = MySpeechContext.None;
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

