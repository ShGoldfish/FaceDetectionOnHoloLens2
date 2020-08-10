using System.Collections.Generic;
using UnityEngine;

public class ContextDetection : MonoBehaviour
{
	TextMesh msgFace;
	TextMesh msgVoice;
	public int num_faces;
	public List<List<int>> faces_box;

	public bool isTalking;

	private void Start()
	{
		isTalking = false;
		msgFace = GameObject.Find("MessageFace").GetComponent<TextMesh>();
		msgVoice = GameObject.Find("MessageVoice").GetComponent<TextMesh>();
		faces_box = new List<List<int>>();
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
