using UnityEngine;

public class ContextDetection : MonoBehaviour
{
	TextMesh msgFace;
	TextMesh msgVoice;
	public int num_faces;
	public bool isTalking;

	private void Start()
	{
		isTalking = false;
		msgFace = GameObject.Find("MessageFace").GetComponent<TextMesh>();
		msgVoice = GameObject.Find("MessageVoice").GetComponent<TextMesh>();
	}

	private void Update()
	{
		// Update text 
		msgFace.text = "Number of faces: " + num_faces.ToString();
		msgVoice.text = "Ongoing conversation: " + isTalking; 

	}

	public bool InConversation()
	{
		if (num_faces > 0 && isTalking)
		{
			return true;
		}
		return false;
	}
}
