using UnityEngine;

public class AppManager : MonoBehaviour
{
	bool is_trans;
	ContextDetection contextDetection;

	private void Start()
	{
		contextDetection = GameObject.Find("Manager").GetComponent<ContextDetection>();
		is_trans = false;
	}

	private void Update()
	{
		if (contextDetection.InConversation())
		{
			if (!is_trans)
			{
				// change game object's color to transparent
				gameObject.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0f);

			}
			is_trans = true;
		}
		else
		{
			if (is_trans)
			{
				// change game object's color to non-transparent
				gameObject.GetComponent<SpriteRenderer>().color = Color.white;

			}
			is_trans = false;
		}
	}
}
