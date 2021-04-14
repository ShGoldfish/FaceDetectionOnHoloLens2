using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UI;
using UnityEngine;
#if WINDOWS_UWP
using Windows.Storage;
#endif

public class FileLog : MonoBehaviour
{
    private string fileName = "App.csv";
    string path;

	// Use this for initialization
	public void SetHeader(string appName, string header) // header is like "time, accuracy, level, ......"
	{
#if WINDOWS_UWP
        // Get local folder on HoloLense
        path = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
#else
        path = ".\\";
#endif
		fileName = path + "\\" + appName+ ".csv";
        File.AppendAllText(fileName, "\n" + header + "\n");
    }

    public void WriteLine(string line) // you pass in one trial's date "1.1, 1, 4, ..."
    {
		// time_trial_ended may differ from time_answered. we need time_answered from audio rec
		//Lines are [Question_number, time_asked, time_trial_ended, questioned_app, right_answer, num_user_manual_override) 
		File.AppendAllText(fileName, "\n" + line);
    }
}
