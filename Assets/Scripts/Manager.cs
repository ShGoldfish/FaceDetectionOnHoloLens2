using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

//internal enum SessionMod { SoloGlanceable = 1, SocialGlanceable= 2, SocialCIA= 3};


public class Manager : MonoBehaviour
{
	public AudioClip audioClip1;
	public AudioClip audioClip2;
	public AudioClip audioClip3;
	public AudioClip audioDing;

	const float PRINT_TIME_INTERVAL = 4.0f;
	// Interface mode: is it glanceable or ACI?
	public static bool is_ACI = false;
	bool solo = true;
	bool firstSocial = true;
	private int questionNum = -1;
	private int trialSetNum;
	private float time_to_ask_next_Q;
	private static float time_asked;
	private float time_last_print;
	private FileLog sessionLog;
	private int[,,] trialSet;

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
		solo = true;
		firstSocial = true;
		// TODO: Should be synced with the webApp
		trialSetNum = UnityEngine.Random.Range(0, 100) % 2;

		Create_Trial_Dataset();
		Change_SessionMod();

		time_asked = Time.time;
		time_to_ask_next_Q = float.NegativeInfinity;
		time_last_print = 0.0f;
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

			// Write line on ACI
			if (Time.time - time_last_print >= PRINT_TIME_INTERVAL)
			{
				// TODO: need to have trial sets from here to make sure consistancy of setting answer with my questions!
				// Should add trialSetNum for non-solo ones
				//Set_Answer(trialSet[trialSetNum, questionNum, 1], trialSet[trialSetNum, questionNum, 2]);

				string socialGlanceable_line =  num_faces + ", " +
												isTalking + ", " +
												GameObject.Find("Weather1").GetComponent<AppManager>().mentioned + ", " +
												GameObject.Find("Email2").GetComponent<AppManager>().mentioned + ", " +
												GameObject.Find("Fitbit3").GetComponent<AppManager>().mentioned + ", " +
												GameObject.Find("Weather1").GetComponent<AppManager>().user_manual_override + ", " +
												GameObject.Find("Email2").GetComponent<AppManager>().user_manual_override + ", " +
												GameObject.Find("Fitbit3").GetComponent<AppManager>().user_manual_override;
				sessionLog.WriteLine(socialGlanceable_line);
				time_last_print = Time.time;
			}
		}
		//if time_to_ask_next_Q is -inf that means they are answering so do not continue  Trial set timer to run the next question
		else if (solo & !float.IsNegativeInfinity(time_to_ask_next_Q))
		{
			if ( Time.time >= time_to_ask_next_Q)
			{
				//TrialSet[trialSetNum][questionNum][1] is 1 or 2 or 3 showing if the question 1 (from app 1) should be asked
				Ask_Question(trialSet[trialSetNum, questionNum, 1]);
				time_asked = Time.time;
				time_to_ask_next_Q = float.NegativeInfinity;
				
			}

		}
		else if (!solo & !is_ACI )
			 // Write line on glanceable social
			if (Time.time - time_last_print >= PRINT_TIME_INTERVAL)
			{
				// TODO: need to have trial sets from here to make sure consistancy of setting answer with my questions!
				//Set_Answer(trialSet[trialSetNum, questionNum, 1], trialSet[trialSetNum, questionNum, 2]);

				string socialGlanceable_line = GameObject.Find("Weather1").GetComponent<AppManager>().user_manual_override + ", " +
								GameObject.Find("Email2").GetComponent<AppManager>().user_manual_override + ", " +
								GameObject.Find("Fitbit3").GetComponent<AppManager>().user_manual_override;
				sessionLog.WriteLine(socialGlanceable_line);
				time_last_print = Time.time;

			}
	}

	private void Ask_Question(int app)
	{

		AudioSource audioSource = GetComponent<AudioSource>();
		Set_Answer(trialSet[trialSetNum, questionNum, 1], trialSet[trialSetNum, questionNum, 2]);

		// Play the audio that asks question 1 [from app 1]
		if (app == 1)
		{
			//Scope.app.speech.synthesizeSpeech({ 'text': 'Say this text out loud'});
			audioSource.clip = audioClip1;
		}
		else if (app == 2)
		{
			audioSource.clip = audioClip2;
		}
		else
		{
			audioSource.clip = audioClip3;
		}
		audioSource.Play();
	}

	private void Set_Answer(int app, int option)
	{
		// TODO: Set the questions answer on app to option
	}

	public void OnClick_NxtSession()
	{
		solo = false;
		Change_SessionMod();
	}

	internal void Change_SessionMod()
	{
		//may get an input from trial manager to set glanceable or non [each has 2 glanceable and 1 intelligent]
		//if (is_ACI)
		//{
		//	gameObject.GetComponent<SpeechHandler>().End_MySH();
		//}
		//print("change session mode from: solo " + solo + " ACI: " + is_ACI);
		sessionLog = new FileLog();
		// come with method for the rotation of Change_SessionMod parameter 
		if (solo)
		{
			is_ACI = false;
			string fileName = DateTime.Now + "_SoloSession";
			fileName = fileName.Replace(@"/", "_").Replace(@":", "_").Replace(@" ", "_");
			sessionLog.SetHeader(fileName, "Trial_number, time_asked, time_trial_ended, " +
											"questioned_app, right_answer, num_user_manual_override_App1, " +
											"num_user_manual_override_App2, num_user_manual_override_App3");

			questionNum = -1;
			Start_nxt_Trial();
		}
		else
		{
			if (firstSocial)
			{
				is_ACI = UnityEngine.Random.Range(0, 100) > 50;
				firstSocial = false;
			}
			else
			{
				is_ACI = !is_ACI;
			}
			string fileName = DateTime.Now + "_SocialSession" + (is_ACI ?
																		"_ACI" :
																		"_Basic");
			fileName = fileName.Replace(@"/", "_").Replace(@":", "_").Replace(@" ", "_");
			sessionLog.SetHeader(fileName, is_ACI ?
													"Number_of_faces, is_tallking, " +
													"Mentioned_App1, Mentioned_App2, Mentioned_App3," +
													"num_user_manual_override_App1, num_user_manual_override_App2, num_user_manual_override_App3" :
													"num_user_manual_override_App1, num_user_manual_override_App2, num_user_manual_override_App3");

		}
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

		//print("change session mode to: solo " + solo + " ACI: " + is_ACI);
		UpdateTooltipText();
	}

	public void Start_nxt_Trial()
	{
		if (solo)
		{
			string trial_line;

			if (questionNum > -1)
			{
				trial_line = questionNum + ", " + 
								time_asked + ", " + Time.time + ", " + 
								trialSet[trialSetNum, questionNum, 1] + ", " + 
								trialSet[trialSetNum, questionNum, 2] + ", " + 
								GameObject.Find("Weather1").GetComponent<AppManager>().user_manual_override + ", " +
								GameObject.Find("Email2").GetComponent<AppManager>().user_manual_override + ", " +
								GameObject.Find("Fitbit3").GetComponent<AppManager>().user_manual_override;
				sessionLog.WriteLine(trial_line);
			}
			questionNum++;
			if (questionNum < 9)
			{
				time_to_ask_next_Q = trialSet[trialSetNum, questionNum, 0] + Time.time;
			}
			else
				solo = false;
			
		}
		// For each app start trial
		GameObject.Find("Weather1").GetComponent<AppManager>().Start_Trial();
		GameObject.Find("Email2").GetComponent<AppManager>().Start_Trial();
		GameObject.Find("Fitbit3").GetComponent<AppManager>().Start_Trial();
	}

	private void Create_Trial_Dataset()
	{

		//trialSets is trialSetNum X question num X 3 [= 0: time, 1: App_num, 2: correct_answer_option]
		trialSet = new int[2, 9, 3]{
			{
				{ 10, 2, 0}, // End Trial 6
				{ 20, 1, 1}, // End Trial 1
				{ 15, 3, 0}, // End Trial 2
				{ 10, 1, 1}, // End Trial 3
				{ 45, 3, 1}, // End Trial 4
				{ 10, 3, 3}, // End Trial 5
				{ 30, 2, 0}, // End Trial 7
				{ 15, 2, 1}, // End Trial 8
				{ 15, 1, 0} // End Trial 9
			}, // End Trial Set 1
			{
				{ 45, 1, 1}, // End Trial 15
				{ 20, 2, 1}, // End Trial 14
				{ 10, 1, 1}, // End Trial 13
				{ 15, 2, 0}, // End Trial 12
				{ 30, 3, 0}, // End Trial 11
				{ 10, 2, 1}, // End Trial 10
				{ 15, 1, 0}, // End Trial 9
				{ 15, 3, 0}, // End Trial 8
				{ 20, 3, 1} // End Trial 7
		}// End Trial Set 2
	};

	//	{ 20, 2, 1}, // End Trial 10
	////	{{ 20, 3, 4}, // End Trial 11
	//		{ 15, 2, 2}, // End Trial 12
	//		{ 10, 1, 2}, // End Trial 13
	//		{ 20, 2, 4}, // End Trial 14
	////		{ 10, 1, 4} // End Trial 15
	//			{ 10, 2, 1}, // End Trial 6
	//			{ 10, 3, 3}, // End Trial 5
	//			{ 45, 3, 2}, // End Trial 4
	//			{ 10, 1, 1}, // End Trial 3
	//			{ 15, 3, 3}, // End Trial 2
	//			{ 20, 1, 4} // End Trial 1
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
		if (solo)
			sessionName = "Solo_Basic";
		else if (is_ACI)
			sessionName = "Social ACI";
		else
			sessionName = "Social_Basic";

		GameObject.Find("CurrentSession").GetComponent<TextMesh>().text =sessionName;
	}
}

