using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CTargetedBy : CConsideration
{
	// Member variables
	private bool m_isTargetOfEnemy;


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
		CEntity enemyTarget;
		if (target.GetDecision() != null)
		{
			targetDecisionAbility = target.GetDecision().GetAbility();

			if (targetDecisionAbility != null)
			{
				if (targetDecisionAbility.GetTargetType() != TargetType.Environment)
				{
					enemyTarget = target.GetDecision().GetTarget().GetComponent<CEntity>();
					if (executor.GetEntity() == enemyTarget)
					{
						m_isTargetOfEnemy = true;
					}
					else
					{
						m_isTargetOfEnemy = false;
					}
				}
				else
				{
					//Debug.LogError("\"TargetedBy\"-Consideration used for an environment target.");
					m_isTargetOfEnemy = false;
				}
			}
			else
			{
				m_isTargetOfEnemy = false;
			}
		}
		else
		{
			//Debug.Log("Target hasn't made a decision yet when considering the enemy's target.");
			m_isTargetOfEnemy = false;
		}


		if (m_isTargetOfEnemy)
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
		return string.Format("Consideration Score:; {0}; isTargetOfEnemy: {1}; Input: {2}; ResponseCurve: {3}",
			Mathf.Clamp(m_score, 0.0f, 1.0f), m_isTargetOfEnemy, m_input, m_score);
	}
}