using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CEntityUi : MonoBehaviour 
{
	// Member variables
	[Header("Visuals")]
	[SerializeField] protected bool m_isPreviewed = false;
	[SerializeField, Range(1, 30)] protected int m_blinkFrames = 5;

	[Header("UI - Templates")]
	[SerializeField] protected GameObject m_decisionChanged;

	[Header("UI - Elements")]
	[SerializeField] protected Image m_icon;
	[SerializeField] protected Image m_blinkFrame;
	
	
	// Methods
	public virtual void Init(string iconPath)
	{
	}

	public virtual void Init(string iconPath, int index)
	{
	}

	public virtual void StartBlinking()
	{
		m_blinkFrame.gameObject.SetActive(true);
		m_isPreviewed = true;
		transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
	}

	public virtual void StopBlinking()
	{
		m_blinkFrame.gameObject.SetActive(false);
		m_isPreviewed = false;
		transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
	}
}
