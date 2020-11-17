using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CTargetHealth : CConsideration 
{
	// Member variables
	private float m_targetHealthPercentage;
	

	// MonoBehaviour-Methods
	sealed protected override void Start() 
	{
		base.Start();

		m_targetHealthPercentage = 0.0f;
	}


	// Methods
	public override float CalculateConsiderationScore()
	{
		m_targetHealthPercentage = CUtilityAiSystem.GetInstance().GetContext().GetTarget().GetComponent<CEntity>().GetHealthPercentage();
		
		m_input = m_targetHealthPercentage;
		m_score = m_responseCurve.CalculateResponseCurveValue(m_input);

		return Mathf.Clamp(m_score, 0.0f, 1.0f);
	}


	public override string ToString()
	{
		return string.Format("Consideration Score:; {0}; TargetHealth (%): {1}; Input: {2}; ResponseCurve: {3}",
			Mathf.Clamp(m_score, 0.0f, 1.0f), m_targetHealthPercentage, m_input, m_score);
	}
}
