using System.Collections.Generic;
using UnityEngine.XR.WSA.WebCam;
using UnityEngine.Networking;
using System.Collections;
using System.Linq;
using UnityEngine;
using System;


public class photoCapture : MonoBehaviour
{
	Manager manager;
	
	// Photo Capture Variables
	PhotoCapture photoCaptureObject = null;
	Texture2D targetTexture;
	GameObject m_Canvas = null;
	Renderer m_CanvasRenderer = null;
	CameraParameters m_CameraParameters;

	// Debugging
	float time_before_send;

	// Constants was 0.2 and 20
	const float WAIT_TIME4POST = 0.3f;
	const int JPG_QUALITY = 15;

	void Start()
	{
		manager = GameObject.Find("Manager").GetComponent<Manager>();

		// Photo Capture 
		Resolution cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();
		targetTexture = new Texture2D(cameraResolution.width, cameraResolution.height, TextureFormat.BGRA32, false);
		m_CameraParameters = new CameraParameters(WebCamMode.PhotoMode);
		m_CameraParameters.hologramOpacity = 0.0f;
		m_CameraParameters.cameraResolutionWidth = cameraResolution.width;
		m_CameraParameters.cameraResolutionHeight = cameraResolution.height;
		m_CameraParameters.pixelFormat = CapturePixelFormat.BGRA32;

		PhotoCapture.CreateAsync(false, OnPhotoCaptureCreated);

		// Debugging
		time_before_send = 0.0f;
	}

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
			manager.imageBufferBytesArray = targetTexture.EncodeToJPG(JPG_QUALITY); // pass a low num 25

			// Communications to Server:
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

	/// <summary>
	/// Network Connection Coroutines
	/// Sends a photo through HTTP Request which can be handles by Flask on Python.
	/// </summary>
	private IEnumerator PostPhoto()
	{
		time_before_send = Time.time;

		var data = new List<IMultipartFormSection> {
			new MultipartFormFileSection("myImage", manager.imageBufferBytesArray, "test.jpg", "image/jpg")
		};
		using (UnityWebRequest webRequest = UnityWebRequest.Post(manager.ipEndPoint + "/receive-image", data))
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
		using (UnityWebRequest webRequest = UnityWebRequest.Get(manager.ipEndPoint + "/detect-faces"))
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

				manager.num_faces = 0;
				manager.faces_box = new List<List<int>>();
				if (dataReceived != null && dataReceived != "")
				{
					List<float> numbers = Array.ConvertAll(dataReceived.Split(','), float.Parse).ToList();
					for (int i = 0; i < numbers.Count; i += 4)
					{
						manager.faces_box.Add(new List<int> { Convert.ToInt32(numbers[i]), Convert.ToInt32(numbers[i + 1]), Convert.ToInt32(numbers[i + 2]), Convert.ToInt32(numbers[i + 3]) });
						manager.num_faces++;
					}
				}
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
		using (UnityWebRequest webRequest = UnityWebRequest.Get(manager.ipEndPoint + "/"))
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
