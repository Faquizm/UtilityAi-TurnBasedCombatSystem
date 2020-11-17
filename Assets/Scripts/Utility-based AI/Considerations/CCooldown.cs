using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CCooldown : CConsideration 
{
	// Member variables
	private bool m_hasUsedAbilityRecently;
	

	// MonoBehaviour-Methods
	sealed protected override void Start() 
	{
		base.Start();
	}


	// Methods
	public override float CalculateConsiderationScore()
	{
		CCombatParticipant executor = CUtilityAiSystem.GetInstance().GetContext().GetExecutorAsParticipant();
		CDecision lastDecision = executor.GetOldDecision();
		CAbility abilityToExecute = CUtilityAiSystem.GetInstance().GetContext().GetAbilityToExecute();

		if (lastDecision != null && lastDecision.GetAbility() != null)
		{
			if (abilityToExecute.GetAbilityName().Equals(lastDecision.GetAbility().GetAbilityName()))
			{
				m_hasUsedAbilityRecently = true;
			}
			else
			{
				m_hasUsedAbilityRecently = false;
			}
		}
		else
		{
			m_hasUsedAbilityRecently = false;
		}


		if (m_hasUsedAbilityRecently)
		{
			m_input = 0.0f;
		}
		else
		{
			m_input = 1.0f;
		}

		m_score = m_responseCurve.CalculateResponseCurveValue(m_input);

		return Mathf.Clamp(m_score, 0.0f, 1.0f);
	}


	public override string ToString()
	{
		return string.Format("Consideration Score:; {0}; hasUsedAbilityRecently: {1}; Input: {2}; ResponseCurve: {3}",
			Mathf.Clamp(m_score, 0.0f, 1.0f), m_hasUsedAbilityRecently, m_input, m_score);
	}
}
