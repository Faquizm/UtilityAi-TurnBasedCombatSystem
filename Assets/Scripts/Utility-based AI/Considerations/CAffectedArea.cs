using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CAffectedArea : CConsideration 
{
	// Member variables
	private bool m_isInAffectedArea;
	

	// MonoBehaviour-Methods
	sealed protected override void Start() 
	{
		base.Start();
	}


	// Methods
	public override float CalculateConsiderationScore()
	{
		CCombatParticipant executor = CUtilityAiSystem.GetInstance().GetContext().GetExecutorAsParticipant();
		CCombatParticipant target = CUtilityAiSystem.GetInstance().GetContext().GetTargetAsParticipant();

		CAbility targetDecisionAbility;
		if (target.GetDecision() != null && target.GetDecision().GetAbility() != null && executor != target)
		{
			targetDecisionAbility = target.GetDecision().GetAbility();

			// Check if ability has an area of effect
			bool hasAoeProperty = targetDecisionAbility.GetType().GetMethod("GetHasAreaOfEffect") != null;
			if (hasAoeProperty)
			{
				// Check if the entity is within the radius
				GameObject abilityTarget = target.GetDecision().GetTarget();
				float abilityRadius = (float)targetDecisionAbility.GetType().GetMethod("GetRadius").Invoke(targetDecisionAbility, null);
				float distanceToAbilityTarget = (executor.GetEntity().transform.position - abilityTarget.transform.position).magnitude;
				if (abilityRadius > distanceToAbilityTarget)
				{
					m_isInAffectedArea = true;
				}
				else
				{
					m_isInAffectedArea = false;
				}
			}
			else
			{
				Debug.Log("No area of effect ability chosen by the target.");
				m_isInAffectedArea = false;
			}
		}
		else
		{
			m_isInAffectedArea = false;
		}
		
		
		if (m_isInAffectedArea)
		{
			m_input = 1.0f;
		}
		else
		{
			m_input = 0.0f;
		}

		m_score = m_responseCurve.CalculateResponseCurveValue(m_input);

		return Mathf.Clamp(m_score, 0.0f, 1.0f);
	}


	public override string ToString()
	{
		return string.Format("Consideration Score:; {0}; isInAffectedArea: {1}; Input: {2}; ResponseCurve: {3}",
			Mathf.Clamp(m_score, 0.0f, 1.0f), m_isInAffectedArea, m_input, m_score);
	}
}
