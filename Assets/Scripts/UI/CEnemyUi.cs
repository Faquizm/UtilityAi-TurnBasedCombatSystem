using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CEnemyUi : CEntityUi 
{
	// Member variables
	[SerializeField] private Text m_indexText;
	[SerializeField] private Image m_blinkIndexFrame;


	// MonoBehaviour-Methods
	void Update()
	{
		if (m_isPreviewed)
		{
			if (Time.frameCount % m_blinkFrames == 0)
			{
				m_blinkFrame.enabled = !m_blinkFrame.enabled;
				m_blinkIndexFrame.enabled = !m_blinkIndexFrame.enabled;

				if (m_indexText.color == Color.black)
				{
					m_indexText.color = new Color(0.977f, 0.301f, 0.309f, 1.0f);
				}
				else
				{
					m_indexText.color = Color.black;
				}
			}
		}
	}


	// Methods
	public override void Init(string iconPath, int index)
	{
		m_icon.sprite = Resources.Load<Sprite>(iconPath);
		m_indexText.text = index.ToString();
	}

	public override void StartBlinking()
	{
		m_blinkIndexFrame.gameObject.SetActive(true);
		base.StartBlinking();
	}

	public override void StopBlinking()
	{
		m_indexText.color = Color.black;
		m_blinkIndexFrame.gameObject.SetActive(false);
		base.StopBlinking();
	}
}
