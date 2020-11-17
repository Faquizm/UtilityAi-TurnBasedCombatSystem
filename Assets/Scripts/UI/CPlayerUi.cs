using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CPlayerUi : CEntityUi 
{
	// MonoBehaviour-Methods
	void Update()
	{
		if (m_isPreviewed)
		{
			if (Time.frameCount % m_blinkFrames == 0)
			{
				m_blinkFrame.enabled = !m_blinkFrame.enabled;
			}
		}
	}


	// Methods
	public override void Init(string iconPath)
	{
		m_icon.sprite = Resources.Load<Sprite>(iconPath);
	}
}
