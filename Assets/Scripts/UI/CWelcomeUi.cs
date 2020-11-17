using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CWelcomeUi : MonoBehaviour 
{
	// Member variables
	[SerializeField] private RectTransform m_welcomePanel;
	private Transform m_flammaTransform;
	private Vector3 m_startPosition;

	// MonoBehaviour-Methods
	void Awake()
	{
		if (!CWorld.GetInstance().GetIsStarted())
		{
			m_welcomePanel.gameObject.SetActive(true);
		}
		else
		{
			m_welcomePanel.gameObject.SetActive(false);
		}
	}
	
	void Start() 
	{
		if (Time.frameCount < 100)
		{
			m_welcomePanel.gameObject.SetActive(true);
		}

		SearchFlamma();
	}
	
	void Update() 
	{
		if (m_flammaTransform != null)
		{
			if (m_flammaTransform.position != m_startPosition || Input.GetButtonDown("Select"))
			{
				m_welcomePanel.gameObject.SetActive(false);
			}
		}
		else
		{
			SearchFlamma();
		}
		
	}
	
	
	// Methods
	private void SearchFlamma()
	{
		GameObject[] playerCharacters = GameObject.FindGameObjectsWithTag("Player");
		foreach (GameObject playerCharacter in playerCharacters)
		{
			if (playerCharacter.name.Equals("Flamma"))
			{
				m_flammaTransform = playerCharacter.transform;
				m_startPosition = m_flammaTransform.position;
			}
		}
	}
}
