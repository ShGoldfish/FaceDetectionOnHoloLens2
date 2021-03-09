using System.Collections.Generic;
using UnityEngine.XR.WSA.WebCam;
using UnityEngine.Networking;
using System.Collections;
using System.Linq;
using UnityEngine;
using System;

public class MyPhotoCapture : MonoBehaviour
{
	const int JPG_QUALITY = 15;

	public static string ipEndPoint;
	byte[] imageBufferBytesArray;
	bool posting;
	bool getting;

	// Photo Capture Variables
	PhotoCapture photoCaptureObject = null;
	Texture2D targetTexture;
	CameraParameters m_CameraParameters;
	Resolution cameraResolution;

	//UnityWebRequest postWebRequest;
	//UnityWebRequest getWebRequest;

	// Thread
	//const int NUM_THREADS = 5;
	//Thread[] mThreads_get = new Thread[NUM_THREADS];
	//int thread_num;

	// Debugging
	float time_before_send;

	// ############################################# UNITY
	void Start()
	{
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
		// Debugging
		time_before_send = 0.0f;
	}

	//internal void RunPC()
	//{
	//	imageBufferBytesArray = null;

	//	// Photo Capture 
	//	cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();
	//	targetTexture = new Texture2D(cameraResolution.width, cameraResolution.height, TextureFormat.BGRA32, false);
	//	m_CameraParameters = new CameraParameters(WebCamMode.PhotoMode)
	//	{
	//		hologramOpacity = 0.0f,
	//		cameraResolutionWidth = cameraResolution.width,
	//		cameraResolutionHeight = cameraResolution.height,
	//		pixelFormat = CapturePixelFormat.BGRA32
	//	};
	//	PhotoCapture.CreateAsync(false, OnPhotoCaptureCreated);
	//	// Debugging
	//	time_before_send = 0.0f;
	//}

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
			//if (!Manager.is_ACI)
			//{
			//	photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
			//}
			// Copy the raw image data into our target texture
			photoCaptureFrame.UploadImageDataToTexture(targetTexture);
			targetTexture.wrapMode = TextureWrapMode.Clamp;

			// Take another photo
			// photoCaptureObject.TakePhotoAsync(OnCapturedPhotoToMemory);

			// Write to file
			//Debug.Log("take ohoto");
			imageBufferBytesArray = targetTexture.EncodeToJPG(JPG_QUALITY);

			// Thread
			//CreateWebRequests();
			
			StartCoroutine("PostPhoto");
			StartCoroutine("GetFaces");

		}
	}

	//private void CreateWebRequests()
	//{
	//	if (imageBufferBytesArray != null)
	//	{
	//		var data = new List<IMultipartFormSection> {
	//					new MultipartFormFileSection("myImage", imageBufferBytesArray, "test.jpg", "image/jpg")};
	//		postWebRequest = UnityWebRequest.Post(ipEndPoint + "/receive-image", data);
	//		getWebRequest = UnityWebRequest.Get(ipEndPoint + "/detect-faces");
	//		//Thread
	//		//thread_num = (thread_num + 1) % NUM_THREADS;
	//	}
	//	else
	//	{
	//		postWebRequest = null;
	//		getWebRequest = null;
	//	}
	//}

	void OnStoppedPhotoMode(PhotoCapture.PhotoCaptureResult result)
	{
		photoCaptureObject.Dispose();
		photoCaptureObject = null;
	}


	// ############################################# THREAD
	//void ThreadingStart(object d)
	//{
	//	while (true)
	//	{
	//		if (int.Parse(string.Format("{0}", d)) == thread_num)
	//		{
	//			
	//			PostPhoto();
	//			GetFaces();
	//		}
	//	}
	//}

	// ############################################# NETWORK CONNECTION
	/// <summary>
	/// Network Connection Coroutines
	/// Sends a photo through HTTP Request which can be handles by Flask on Python.
	/// </summary>
	private IEnumerator PostPhoto()
	{
		if (Manager.is_ACI)
		{
			while (getting)
				yield return new WaitForEndOfFrame();
			posting = true;
			time_before_send = Time.time;
			var data = new List<IMultipartFormSection> {
						new MultipartFormFileSection("myImage", imageBufferBytesArray, "test.jpg", "image/jpg")};
			using (UnityWebRequest postWebRequest = UnityWebRequest.Post(ipEndPoint + "/receive-image", data))
			{
				yield return postWebRequest.SendWebRequest();
			}
			Debug.Log("Network Connection took " + (Time.time - time_before_send) + " seconds to Post.");    // ~0.065seconds
			posting = false;
		}

	}
	/// <summary>
	/// Network Connection Coroutines
	/// Recieves the number of faces detected from the image sent by POST above through HTTP Request (Flask on Python).
	/// </summary>
	private IEnumerator GetFaces()
	{
		if (Manager.is_ACI)
		{
			while (posting)
				yield return new WaitForEndOfFrame();
			getting = true;
			time_before_send = Time.time;
			using (UnityWebRequest getWebRequest = UnityWebRequest.Get(ipEndPoint + "/detect-faces"))
			{
				yield return getWebRequest.SendWebRequest();
				if (getWebRequest.isNetworkError)
				{
					Debug.Log("Error: " + getWebRequest.error);
				}
				else
				{
					string dataReceived = getWebRequest.downloadHandler.text;
					int n_faces = 0;
					List<List<int>> faces = new List<List<int>>();
					if (dataReceived != null && dataReceived != "")
					{
						List<float> numbers = Array.ConvertAll(dataReceived.Split(','), float.Parse).ToList();
						for (int i = 0; i < numbers.Count; i += 4)
						{
							faces.Add(new List<int> {   Convert.ToInt32(numbers[i]),
												Convert.ToInt32(numbers[i + 1]),
												Convert.ToInt32(numbers[i + 2]),
												Convert.ToInt32(numbers[i + 3]) });
							n_faces++;
						}
					}
					Manager.Set_Faces(n_faces, faces);
				}
			}
			Debug.Log("Network Connection took " + (Time.time - time_before_send) + " seconds to Get.");    // ~0.065seconds
			getting = false;

		}
		// Take another photo
		photoCaptureObject.TakePhotoAsync(OnCapturedPhotoToMemory);

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
				Debug.Log("dataReceived from python is: " + dataReceived);
			}
		}
	}
}
