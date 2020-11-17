using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CStatChange : CConsideration
{
	// Member variables
	private float m_changeValue;


	// MonoBehaviour-Methods
	sealed protected override void Start()
	{
		base.Start();

		m_changeValue = 0.5f;		// 0.5f, because combat changes are negative. Curve design should consider that
	}


	// Methods
	public override float CalculateConsiderationScore()
	{
		CCombatParticipant target = CUtilityAiSystem.GetInstance().GetContext().GetTargetAsParticipant();
		CEntityStats combatStatChanges = target.GetCombatStatChanges();

		List<float> allCombatChanges = combatStatChanges.GetAllCombatChanges();

		foreach (float change in allCombatChanges)
		{
			m_changeValue += change;
		}

		m_input = MapToBookends(m_changeValue);   // Bookends should be 0.0f and 1.0f

		m_score = m_responseCurve.CalculateResponseCurveValue(m_input);

		return Mathf.Clamp(m_score, 0.0f, 1.0f);
	}


	public override string ToString()
	{
		return string.Format("Consideration Score:; {0}; changeValue: {1}; Input: {2}; ResponseCurve: {3}",
			Mathf.Clamp(m_score, 0.0f, 1.0f), m_changeValue, m_input, m_score);
	}
}
