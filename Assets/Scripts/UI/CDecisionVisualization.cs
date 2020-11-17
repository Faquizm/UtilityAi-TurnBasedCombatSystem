using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CDecisionVisualization : MonoBehaviour 
{
	// Member variables
	[Header("Camera")]
	private Camera m_cameraToLookAt;

	[Header("Animation")]
	private Sprite[] m_animationSprites;
	private int m_currentSprintIndex;
	private int m_startFrame;
	private int m_framesPerSprite;

	private bool m_usesTimer;
	private float m_timerDuration;
	private float m_timerProgress;


	// MonoBehaviour-Methods
	void Awake()
	{
		m_animationSprites = Resources.LoadAll<Sprite>("Animation/Decision Visualization");
		m_currentSprintIndex = 1;
		m_startFrame = Time.frameCount;
		m_framesPerSprite = 30;

		m_usesTimer = false;
		m_timerDuration = 0.0f;
		m_timerProgress = 0.0f;

		gameObject.GetComponent<Canvas>().enabled = true;
	}
	
	void Start() 
	{
		m_cameraToLookAt = CCombatSystemUi.GetInstance().GetCombatCamera();
	}
	
	void Update() 
	{
		if ((Time.frameCount - m_startFrame) % m_framesPerSprite == 0)
		{
			gameObject.GetComponent<Image>().sprite = m_animationSprites[m_currentSprintIndex];
			m_currentSprintIndex++;

			if (m_currentSprintIndex % m_animationSprites.Length == 0)
			{
				m_currentSprintIndex = 0;
			}
		}

		if (m_usesTimer)
		{
			m_timerProgress += Time.deltaTime;

			if (m_timerProgress > m_timerDuration)
			{
				Destroy(this);
			}
		}

		transform.LookAt(m_cameraToLookAt.transform);
		transform.Rotate(Vector3.up, 180.0f);
	}

	void OnDestroy()
	{
		gameObject.GetComponent<Canvas>().enabled = false;
	}


	// Methods
	public void ActivateTimer(float duration)
	{
		m_usesTimer = true;
		m_timerDuration = duration;
	}
}
