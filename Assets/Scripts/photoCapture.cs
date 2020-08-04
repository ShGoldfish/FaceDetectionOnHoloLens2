using System.Linq;
using UnityEngine;
using UnityEngine.XR.WSA.WebCam;


public class photoCapture : MonoBehaviour
{
	PhotoCapture photoCaptureObject = null;
	Texture2D targetTexture;
	GameObject m_Canvas = null;
	Renderer m_CanvasRenderer = null;
	CameraParameters m_CameraParameters;
	NetworkCon netCon;

	void Start()
	{
		netCon = GameObject.Find("Manager").GetComponent<NetworkCon>();

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
			Matrix4x4 projectionMatrix;
			photoCaptureFrame.TryGetProjectionMatrix(out projectionMatrix);

			// Copy the raw image data into our target texture
			photoCaptureFrame.UploadImageDataToTexture(targetTexture);
			targetTexture.wrapMode = TextureWrapMode.Clamp;

			// Take another photo
			photoCaptureObject.TakePhotoAsync(OnCapturedPhotoToMemory);
			
			// Write to file
			netCon.imageBufferBytesArray = targetTexture.EncodeToJPG();
		}
	}

	void OnStoppedPhotoMode(PhotoCapture.PhotoCaptureResult result)
	{
		photoCaptureObject.Dispose();
		photoCaptureObject = null;
	}

}
