using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CGaugeUnit 
{
	// Member variables
	private CCombatParticipant m_participantReference;
	private RectTransform m_participantUi;
	private bool m_hasVisualizedDecisionChange;


	// Constructor
	public CGaugeUnit(CCombatParticipant combatParticipant, RectTransform participantUi)
	{
		m_participantReference = combatParticipant;
		m_participantUi = participantUi;
	}


	// Methods
	public void UpdateIconPosition()
	{
		m_participantUi.anchoredPosition = new Vector3(m_participantReference.GetGaugePosition(), m_participantUi.anchoredPosition.y, 0.0f);

		if (m_participantReference.GetEntity().IsDefeated())
		{
			m_participantUi.gameObject.SetActive(false);
		}
	}


	// Getter/Setter
	public CCombatParticipant GetCombatParticipant()
	{
		return m_participantReference;
	}

	public RectTransform GetParticipantUi()
	{
		return m_participantUi;
	}
}
