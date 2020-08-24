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

	public Matrix4x4 cameraToWorldMatrix;
	public Matrix4x4 projectionMatrix;

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

			// Copy the raw image data into our target texture
			photoCaptureFrame.UploadImageDataToTexture(targetTexture);
			targetTexture.wrapMode = TextureWrapMode.Clamp;

			// Take another photo
			photoCaptureObject.TakePhotoAsync(OnCapturedPhotoToMemory);

			// Write to file
			netCon.imageBufferBytesArray = targetTexture.EncodeToJPG();

			// For Coordinate systems, from Lee's code

			// TIME TO DO MAGIC
			// Taken from https://forum.unity.com/threads/implementing-locatable-camera-shader-code.417261/
			print("////////////////////TryGetCameraToWorldMatrix :" + photoCaptureFrame.TryGetCameraToWorldMatrix(out cameraToWorldMatrix));
			print("/////////////////////TryGetProjectionMatrix :" + photoCaptureFrame.TryGetProjectionMatrix(out projectionMatrix));

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
}
