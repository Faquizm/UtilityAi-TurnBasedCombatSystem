using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CGaugeProgress : CConsideration
{
	// Member variables
	private float m_targetGaugePosition;

	// MonoBehaviour-Methods
	sealed protected override void Start() 
	{
		base.Start();

		m_targetGaugePosition = 0.0f;
	}


	// Methods
	public override float CalculateConsiderationScore()
	{
		m_targetGaugePosition = CUtilityAiSystem.GetInstance().GetContext().GetTargetAsParticipant().GetGaugePosition();

		m_input = MapToBookends(m_targetGaugePosition);

		m_score = m_responseCurve.CalculateResponseCurveValue(m_input);

		return Mathf.Clamp(m_score, 0.0f, 1.0f);
	}


	public override string ToString()
	{
		return string.Format("Consideration Score:; {0}; targetGaugePosition: {1}; Input: {2}; ResponseCurve: {3}",
			Mathf.Clamp(m_score, 0.0f, 1.0f), m_targetGaugePosition, m_input, m_score);
	}
}
