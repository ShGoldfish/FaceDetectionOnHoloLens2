using System.Collections.Generic;
using UnityEngine.XR.WSA.WebCam;
using UnityEngine.Networking;
using System.Collections;
using System.Linq;
using UnityEngine;
using System;
//using System.Threading;

public class MyPhotoCapture : MonoBehaviour
{
	public static string ipEndPoint;
	byte[] imageBufferBytesArray;

	Manager manager;

	// Constants (were 0.3 and 15)
	const float WAIT_TIME4POST = 0.1f;
	const int JPG_QUALITY = 25;

	// Photo Capture Variables
	PhotoCapture photoCaptureObject = null;
	Texture2D targetTexture;
	CameraParameters m_CameraParameters;
	Resolution cameraResolution;
	
	// Thread
	//Thread mThread_get; 
	// Debugging
	float time_before_send;

// ############################################# UNITY
	void Start()
	{
		manager = GameObject.Find("Manager").GetComponent<Manager>();
		ipEndPoint = "http://128.173.236.208:9005";
		imageBufferBytesArray = null;

		// Photo Capture 
		cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();
		targetTexture = new Texture2D(cameraResolution.width, cameraResolution.height, TextureFormat.BGRA32, false);
		m_CameraParameters = new CameraParameters(WebCamMode.PhotoMode)
		{
			hologramOpacity = 0.0f,
			cameraResolutionWidth = cameraResolution.width,
			cameraResolutionHeight = cameraResolution.height,
			pixelFormat = CapturePixelFormat.BGRA32
		};
		PhotoCapture.CreateAsync(false, OnPhotoCaptureCreated);

		// Thread
		//ThreadStart ts_get = new ThreadStart(ThreadingStart);
		//mThread_get = new Thread(ts_get);
		//mThread_get.Start();

		// Debugging
		time_before_send = 0.0f;
	}


// ############################################# PHOTO CAPTURE
	private void OnPhotoCaptureCreated(PhotoCapture captureObject)
	{
		photoCaptureObject = captureObject;
		captureObject.StartPhotoModeAsync(m_CameraParameters, OnPhotoModeStarted);
	}
	// Capture a Photo to a Texture2D
	private void OnPhotoModeStarted(PhotoCapture.PhotoCaptureResult result)
	{
		if (result.success)
		{
			photoCaptureObject.TakePhotoAsync(OnCapturedPhotoToMemory);
		}
		else
		{
			Debug.LogError("Unable to start photo mode!");
		}
	}
	private void OnCapturedPhotoToMemory(PhotoCapture.PhotoCaptureResult result, PhotoCaptureFrame photoCaptureFrame)
	{
		if (result.success)
		{
			// Copy the raw image data into our target texture
			photoCaptureFrame.UploadImageDataToTexture(targetTexture);
			targetTexture.wrapMode = TextureWrapMode.Clamp;

			// Take another photo
			photoCaptureObject.TakePhotoAsync(OnCapturedPhotoToMemory);

			// Write to file
			//print("take ohoto");
			imageBufferBytesArray = targetTexture.EncodeToJPG(JPG_QUALITY);

			//StartCoroutine("PostPhoto");
			StartCoroutine("PostPhoto");
			StartCoroutine("GetFaces");
			//StartCoroutine("HelloWorld");
		}
	}
	void OnStoppedPhotoMode(PhotoCapture.PhotoCaptureResult result)
	{
		photoCaptureObject.Dispose();
		photoCaptureObject = null;
	}


// ############################################# THREAD
	//void ThreadingStart()
	//{
	//}


	// ############################################# NETWORK CONNECTION
	/// <summary>
	/// Network Connection Coroutines
	/// Sends a photo through HTTP Request which can be handles by Flask on Python.
	/// </summary>
	private IEnumerator PostPhoto()
	{
		time_before_send = Time.time;
		var data = new List<IMultipartFormSection> {
			new MultipartFormFileSection("myImage", imageBufferBytesArray, "test.jpg", "image/jpg")
		};
		using (UnityWebRequest webRequest = UnityWebRequest.Post(ipEndPoint + "/receive-image", data))
		{
			webRequest.SendWebRequest();
			yield return new WaitForSeconds(WAIT_TIME4POST);
		}
	}
	/// <summary>
	/// Network Connection Coroutines
	/// Recieves the number of faces detected from the image sent by POST above through HTTP Request (Flask on Python).
	/// </summary>
	private IEnumerator GetFaces()
	{
		using (UnityWebRequest webRequest = UnityWebRequest.Get(ipEndPoint + "/detect-faces"))
		{
			yield return webRequest.SendWebRequest();

			if (webRequest.isNetworkError)
			{
				Debug.Log("Error: " + webRequest.error);
			}
			else
			{
				string dataReceived = webRequest.downloadHandler.text;
				print("Network Connection took " + (Time.time - time_before_send) + " seconds.");    // ~0.065seconds
				int n_faces = 0;
				List<List<int>> faces = new List<List<int>>();
				if (dataReceived != null && dataReceived != "")
				{
					List<float> numbers = Array.ConvertAll(dataReceived.Split(','), float.Parse).ToList();
					for (int i = 0; i < numbers.Count; i += 4)
					{
						faces.Add(new List<int> {	Convert.ToInt32(numbers[i]),
													Convert.ToInt32(numbers[i + 1]),
													Convert.ToInt32(numbers[i + 2]),
													Convert.ToInt32(numbers[i + 3]) });
						n_faces++;
					}
					print(n_faces);
				}
				manager.SetFaces(n_faces, faces);
			}
		}
	}
	/// <summary>
	/// Network Connection Coroutines
	/// For the purpose of testing the connection: Recieves a "Hello World string "through HTTP Request which was send by by Flask on Python.
	/// </summary>
	/// <returns></returns>
	private IEnumerator HelloWorld()
	{
		using (UnityWebRequest webRequest = UnityWebRequest.Get(ipEndPoint + "/"))
		{
			yield return webRequest.SendWebRequest();
			if (webRequest.isNetworkError)
			{
				Debug.Log("Error: " + webRequest.error);
			}
			else
			{
				string dataReceived = webRequest.downloadHandler.text;
				print("dataReceived from python is: " + dataReceived);
			}
		}
	}
}
