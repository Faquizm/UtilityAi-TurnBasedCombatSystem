using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CStatChangeValue : CConsideration
{
	// Member variables
	private float m_currentStatValue;
	private float m_abilityPercentage;
	private float m_statChangeValue; 


	// MonoBehaviour-Methods
	sealed protected override void Start()
	{
		base.Start();

		m_currentStatValue = 0.0f;
		m_abilityPercentage = 0.0f;
		m_statChangeValue = 0.0f;
	}


	// Methods
	public override float CalculateConsiderationScore()
	{
		CCombatParticipant target = CUtilityAiSystem.GetInstance().GetContext().GetTargetAsParticipant();
		CAbility abilityToExecute = CUtilityAiSystem.GetInstance().GetContext().GetAbilityToExecute();
		StatType statType;
		float statWeight;
		float statCombatChange;
		
		bool hasStatTypeProperty = abilityToExecute.GetType().GetMethod("GetStatType") != null;
		bool hasPercentageProperty = abilityToExecute.GetType().GetMethod("GetPercentage") != null;
		if (hasStatTypeProperty && hasPercentageProperty)
		{
			statType = (StatType)abilityToExecute.GetType().GetMethod("GetStatType").Invoke(abilityToExecute, null);
			if (statType == StatType.All)
			{
				Debug.LogError("StatType \"All\" not supported in StatChangeValue yet.");
				return 0.0f;
			}
			else
			{ 
				statWeight = target.GetEntity().GetStatWeights().GetStatWeight(statType);
				statCombatChange = target.GetCombatStatChanges().GetStatChange(statType);
				m_currentStatValue = target.GetEntity().GetStats().GetStatByType(statType);
				m_abilityPercentage = (float)abilityToExecute.GetType().GetMethod("GetPercentage").Invoke(abilityToExecute, null);

				m_currentStatValue *= statWeight * (1.0f + statCombatChange);
				m_statChangeValue = m_currentStatValue * Mathf.Abs(m_abilityPercentage);
			}
		}
		else
		{
			Debug.LogError("Ability is missing either \"GetStatType\" or \"GetPercentage\"");
			return 0.0f;
		}

		m_input = MapToBookends(m_statChangeValue);

		m_score = m_responseCurve.CalculateResponseCurveValue(m_input);

		return Mathf.Clamp(m_score, 0.0f, 1.0f);
	}


	public override string ToString()
	{
		return string.Format("Consideration Score:; {0}; statChangeValue: {1}; Input: {2}; ResponseCurve: {3}", 
			Mathf.Clamp(m_score, 0.0f, 1.0f), m_statChangeValue, m_input, m_score);
	}
}
