using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class COwnHealth : CConsideration
{
	// Member variables
	private float m_ownHealthPercentage;
	

	// MonoBehaviour-Methods
	sealed protected override void Start()
	{
		base.Start();

		m_ownHealthPercentage = 0.0f;
	}


	// Methods
	public override float CalculateConsiderationScore()
	{
		m_ownHealthPercentage = CUtilityAiSystem.GetInstance().GetContext().GetExecutorAsEntity().GetHealthPercentage();
		
		m_input = m_ownHealthPercentage;
		m_score = m_responseCurve.CalculateResponseCurveValue(m_input);

		return Mathf.Clamp(m_score, 0.0f, 1.0f);
	}


	public override string ToString()
	{
		return string.Format("Consideration Score:; {0}; ownHealth (%): {1}; Input: {2}; ResponseCurve: {3}",
			Mathf.Clamp(m_score, 0.0f, 1.0f), m_ownHealthPercentage, m_input, m_score);
	}
}
