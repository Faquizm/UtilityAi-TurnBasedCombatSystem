using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CTeamHealthAverage : CConsideration 
{
	// Member variables
	private float m_teamHealthAverage;
	
	// MonoBehaviour-Methods	
	sealed protected override void Start() 
	{
		base.Start();

		m_teamHealthAverage = 0.0f;
	}


	// Methods
	public override float CalculateConsiderationScore()
	{
		float healthPercentageSum = 0.0f;
		CAbility abilityToExecute = CUtilityAiSystem.GetInstance().GetContext().GetAbilityToExecute();
		List<CCombatParticipant> team = new List<CCombatParticipant>();
		CEntity target = CUtilityAiSystem.GetInstance().GetContext().GetTargetAsEntity();

		if (abilityToExecute.GetTargetType() == TargetType.Enemy)
		{
			if (target.CompareTag("Player"))
			{
				team.AddRange(CCombatSystem.GetInstance().GetEnemyTeam());
			}
			else if (target.CompareTag("Enemy"))
			{
				team.AddRange(CCombatSystem.GetInstance().GetPlayerTeam());
			}
			else
			{
				Debug.LogError("No team found.");
			}

		}
		else if (abilityToExecute.GetTargetType() == TargetType.Ally)
		{
			if (target.CompareTag("Player"))
			{
				team.AddRange(CCombatSystem.GetInstance().GetPlayerTeam());
			}
			else if (target.CompareTag("Enemy"))
			{
				team.AddRange(CCombatSystem.GetInstance().GetEnemyTeam());
			}
			else
			{
				Debug.LogError("No team found.");
			}
		}

		foreach (CCombatParticipant participant in team)
		{
			healthPercentageSum += participant.GetEntity().GetHealthPercentage();
		}

		m_teamHealthAverage = healthPercentageSum / team.Count;
		m_input = m_teamHealthAverage;
		m_score = m_responseCurve.CalculateResponseCurveValue(m_input);

		return Mathf.Clamp(m_score, 0.0f, 1.0f);
	}


	public override string ToString()
	{
		return string.Format("Consideration Score:; {0}; TeamHealthAverage (%): {1}; Input: {2}; ResponseCurve: {3}",
			Mathf.Clamp(m_score, 0.0f, 1.0f), m_teamHealthAverage, m_input, m_score);
	}
}
