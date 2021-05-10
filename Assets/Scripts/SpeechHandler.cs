using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Windows.Speech;
using Microsoft.MixedReality.Toolkit.Input;
using System;

internal enum MySpeechContext { Weather = 1, Email = 2, Fitbit = 3, None = 4 };

public class SpeechHandler : MonoBehaviour, IMixedRealitySpeechHandler
{
	const float TIMEOUT = 7.0f;

    [SerializeField]
    private DictationRecognizer dictationRecognizer;
    private static string deviceName = string.Empty;
    private int samplingRate;
    private const int messageLength = 15;
	
    private void Awake()
    {

		dictationRecognizer = new DictationRecognizer();
		dictationRecognizer.AutoSilenceTimeoutSeconds = TIMEOUT;
		// 3.a: Register for dictationRecognizer.DictationHypothesis and implement DictationHypothesis below
		// This event is fired while the user is talking. As the recognizer listens, it provides text of what it's heard so far.
		dictationRecognizer.DictationHypothesis += DictationRecognizer_DictationHypothesis;

		// 3.a: Register for dictationRecognizer.DictationComplete and implement DictationComplete below
		// This event is fired when the recognizer stops, whether from Stop() being called, a timeout occurring, or some other error.
		dictationRecognizer.DictationComplete += DictationRecognizer_DictationComplete;

		dictationRecognizer = new DictationRecognizer();
		dictationRecognizer.AutoSilenceTimeoutSeconds = TIMEOUT;
		// 3.a: Register for dictationRecognizer.DictationHypothesis and implement DictationHypothesis below
		// This event is fired while the user is talking. As the recognizer listens, it provides text of what it's heard so far.
		dictationRecognizer.DictationHypothesis += DictationRecognizer_DictationHypothesis;

		// 3.a: Register for dictationRecognizer.DictationComplete and implement DictationComplete below
		// This event is fired when the recognizer stops, whether from Stop() being called, a timeout occurring, or some other error.
		dictationRecognizer.DictationComplete += DictationRecognizer_DictationComplete;

		PhraseRecognitionSystem.Shutdown();
		dictationRecognizer.Start();
		// Query the maximum frequency of the default microphone. Use 'unused' to ignore the minimum frequency.
		int unused;
		Microphone.GetDeviceCaps(deviceName, out unused, out samplingRate);
		Microphone.Start(deviceName, false, messageLength, samplingRate);

	}

	//internal void Run_MySH()
	//{
	//	dictationRecognizer = new DictationRecognizer();
	//	dictationRecognizer.AutoSilenceTimeoutSeconds = TIMEOUT;
	//	// 3.a: Register for dictationRecognizer.DictationHypothesis and implement DictationHypothesis below
	//	// This event is fired while the user is talking. As the recognizer listens, it provides text of what it's heard so far.
	//	dictationRecognizer.DictationHypothesis += DictationRecognizer_DictationHypothesis;

	//	// 3.a: Register for dictationRecognizer.DictationComplete and implement DictationComplete below
	//	// This event is fired when the recognizer stops, whether from Stop() being called, a timeout occurring, or some other error.
	//	dictationRecognizer.DictationComplete += DictationRecognizer_DictationComplete;

	//	PhraseRecognitionSystem.Shutdown();
	//	dictationRecognizer.Start();
	//	// Query the maximum frequency of the default microphone. Use 'unused' to ignore the minimum frequency.
	//	int unused;
	//	Microphone.GetDeviceCaps(deviceName, out unused, out samplingRate);
	//	Microphone.Start(deviceName, false, messageLength, samplingRate);

	//}

	//internal void End_MySH()
	//{
		//is_ACI = false;
		//PhraseRecognitionSystem.Restart();
		//dictationRecognizer.Dispose();
		//Microphone.End(deviceName);
	//}

	private void Update()
	{
		if (Manager.is_ACI)
		{
			if (!Manager.Get_isTalking())
			{
				dictationRecognizer.Start();
			}
		}
	}

	/// <summary>
	/// This event is fired while the user is talking. As the recognizer listens, it provides text of what it's heard so far.
	/// </summary>
	/// <param name="text">The currently hypothesized recognition.</param>
	private void DictationRecognizer_DictationHypothesis(string text)
    {
		if (text != null && text.Length != 0)
		{
			Manager.Set_isTalking(true);
			RecognizeMyKeywords(text.ToLower());
		}
    }

	private void RecognizeMyKeywords(string text)
	{
		int context = 4;
		if (text.Contains("By what percentage is it going to Rain on Saturday?") || text.Contains("percentage") || text.Contains("rain") ||
			text.Contains("Saturday") )
		{
			context = 1;
		}
		if (text.Contains("How many unread messages do you have on your gmail?") || text.Contains("messages") || text.Contains("message") || text.Contains("gmail") 
			|| text.Contains("unread"))
		{
			context = 2;
		}
		if (text.Contains("step") || text.Contains("steps")
			|| text.Contains("walked") || text.Contains("walk")
			|| text.Contains("How many steps have you walked so far today?") )
		{
			context = 3;
		}
		Manager.Set_SpeechContext(context);
	}

    /// <summary>
    /// This event is fired when the recognizer stops, whether from Stop() being called, a timeout occurring, or some other error.
    /// Typically, this will simply return "Complete". In this case, we check to see if the recognizer timed out.
    /// </summary>
    /// <param name="cause">An enumerated reason for the session completing.</param>
    private void DictationRecognizer_DictationComplete(DictationCompletionCause cause)
    {
        if (cause == DictationCompletionCause.TimeoutExceeded)
        {
			Manager.Set_isTalking(false);
        }
    }

    /// <summary>
    /// The dictation recognizer may not turn off immediately, so this call blocks on
    /// the recognizer reporting that it has actually stopped.
    /// </summary>
    public IEnumerator WaitForDictationToStop()
    {
        while (dictationRecognizer != null && dictationRecognizer.Status == SpeechSystemStatus.Running)
        {
            yield return null;
        }
    }

    void IMixedRealitySpeechHandler.OnSpeechKeywordRecognized(SpeechEventData eventData)
    {
        Debug.Log("OnSpeechKeywordRecognized: " + Manager.Get_isTalking());
    }

}
