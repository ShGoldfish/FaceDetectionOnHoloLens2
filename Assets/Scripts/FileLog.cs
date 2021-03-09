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
    public string fileName = "log.csv";
    string path;

    // Use this for initialization
    public void SetHeader(string header) // header is like "time, accuracy, level, ......"
    {
#if WINDOWS_UWP
        // Get local folder on HoloLense
        path = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
#else
        path = ".\\";
#endif
        File.AppendAllText(path + "\\" + fileName, "\n" + header + "\n");
    }

    public void WriteLine(string line) // you pass in one trial's date "1.1, 1, 4, ..."
    {
        File.AppendAllText(path + "\\" + fileName, "\n" + line);
    }
}
