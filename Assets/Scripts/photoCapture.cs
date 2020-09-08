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

	// Constants
	const float WAIT_TIME4POST = 0.25f;
	const int JPG_QUALITY = 3;

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
			//manager.imageBufferBytesArray = targetTexture.EncodeToJPG(25); // pass a low num 25
			manager.imageBufferBytesArray = targetTexture.EncodeToJPG(JPG_QUALITY); // pass a low num 25

			// Communications to Server:
			StartCoroutine("PostPhoto");
			StartCoroutine("GetFaces");
			//StartCoroutine("HelloWorld");

			// Taken from https://forum.unity.com/threads/implementing-locatable-camera-shader-code.417261/
			photoCaptureFrame.TryGetCameraToWorldMatrix(out manager.cameraToWorldMatrix);
			photoCaptureFrame.TryGetProjectionMatrix(out manager.projectionMatrix);
			// print("/////////////////////TryGetProjectionMatrix" );
		}
	}

	void OnStoppedPhotoMode(PhotoCapture.PhotoCaptureResult result)
	{
		photoCaptureObject.Dispose();
		photoCaptureObject = null;
	}


	// Taken from https://docs.microsoft.com/en-us/windows/mixed-reality/locatable-camera
	// Helper method to convert hololens application space to world space
	public static Vector3 UnProjectVector(Matrix4x4 proj, Vector3 to)
	{
		Vector3 world = new Vector3(0, 0, 0);
		var axsX = proj.GetRow(0);
		var axsY = proj.GetRow(1);
		var axsZ = proj.GetRow(2);
		world.z = to.z / axsZ.z;
		world.y = (to.y - (world.z * axsY.z)) / axsY.y;
		world.x = (to.x - (world.z * axsX.z)) / axsX.x;
		return world;
	}


	/// <summary>
	/// Network Connection Coroutines
	/// Sends a photo through HTTP Request which can be handles by Flask on Python.
	/// </summary>
	private IEnumerator PostPhoto()
	{
		var data = new List<IMultipartFormSection> {
			new MultipartFormFileSection("myImage", manager.imageBufferBytesArray, "test.jpg", "image/jpg")
		};

		using (UnityWebRequest webRequest = UnityWebRequest.Post(manager.ipEndPoint + "/receive-image", data))
		{
			webRequest.SendWebRequest();

			// WaitForSeconds(0.1f) helps preventing multiple GETs without POST:
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
				print("dataReceived from python is: _" + dataReceived + "_");

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
				print("Num-Faces: " + manager.num_faces);
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
