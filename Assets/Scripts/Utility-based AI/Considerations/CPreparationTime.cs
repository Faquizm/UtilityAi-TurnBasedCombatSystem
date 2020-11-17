using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CPreparationTime : CConsideration
{
	// Member variables
	private float m_preparationTime;
	
	// MonoBehaviour-Methods
	sealed protected override void Start() 
	{
		base.Start();

		m_preparationTime = 0.0f;
	}


	// Methods
	public override float CalculateConsiderationScore()
	{
		m_preparationTime = CUtilityAiSystem.GetInstance().GetContext().GetAbilityToExecute().GetPreparationTime();

		m_input = m_preparationTime;
		m_score = m_responseCurve.CalculateResponseCurveValue(m_input);

		return Mathf.Clamp(m_score, 0.0f, 1.0f);
	}

	public override string ToString()
	{
		return string.Format("Consideration Score:; {0}; PreparationTime: {1}; Input: {2}; ResponseCurve: {3}",
			Mathf.Clamp(m_score, 0.0f, 1.0f), m_preparationTime, m_input, m_score);
	}

}
