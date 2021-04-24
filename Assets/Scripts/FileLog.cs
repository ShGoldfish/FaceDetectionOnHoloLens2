using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UI;
using UnityEngine;
using System;
using System.Globalization;
#if WINDOWS_UWP
using Windows.Storage;
#endif

public class FileLog : MonoBehaviour
{
    private string fileName = "App.csv";
    string path;

	// Use this for initialization
	public void SetFileName(string appName) 
	{
#if WINDOWS_UWP
        // Get local folder on HoloLense
        path = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
#else
        path = ".\\";
#endif
		fileName = path + "\\" + appName+ ".csv";
    }

    public void WriteLine(string line) // you pass in one trial's date "1.1, 1, 4, ..."
    {
		File.AppendAllText(fileName, "\n" + GetTimeInEasternStandardTime() + ", " + line);
    }

	public string GetTimeInEasternStandardTime()
	{
		TimeZoneInfo easternStandardTime = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
		DateTimeOffset timeInEST = TimeZoneInfo.ConvertTime(DateTime.Now, easternStandardTime);
		var culture = new CultureInfo("en-US");
		return timeInEST.ToString(culture);
	}
}
