using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CTargetDamageReduction : CConsideration 
{
	// Member variables
	private float m_damageReductionInPercent;
	
	
	// MonoBehaviour-Methods
	sealed protected override void Start() 
	{
		base.Start();

		m_damageReductionInPercent = 0.0f;
	}


	// Methods
	public override float CalculateConsiderationScore()
	{
		CCombatParticipant target = CUtilityAiSystem.GetInstance().GetContext().GetTargetAsParticipant();
		m_damageReductionInPercent = target.GetActiveDamageReduction();

		m_input = MapToBookends(m_damageReductionInPercent);          // Bookends should be 0.0f and 1.0f

		m_score = m_responseCurve.CalculateResponseCurveValue(m_input);

		return Mathf.Clamp(m_score, 0.0f, 1.0f);
	}


	public override string ToString()
	{
		return string.Format("Consideration Score:; {0}; damageReductionInPercent: {1}; Input: {2}; ResponseCurve: {3}",
			Mathf.Clamp(m_score, 0.0f, 1.0f), m_damageReductionInPercent, m_input, m_score);
	}
}
