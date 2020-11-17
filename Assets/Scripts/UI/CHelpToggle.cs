using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CHelpToggle : MonoBehaviour 
{
	// Member variables
	[SerializeField] private RectTransform m_controlsImage;
	[SerializeField] private Text m_helpText;

	// MonoBehaviour-Methods
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.H) && m_helpText.text.Contains("show"))
		{
			m_controlsImage.gameObject.SetActive(true);
			m_helpText.text = "Press 'H' to hide help.";
		}
		else if (Input.GetKeyDown(KeyCode.H) && m_helpText.text.Contains("hide"))
		{
			m_controlsImage.gameObject.SetActive(false);
			m_helpText.text = "Press 'H' to show help.";
		}
	}
}
