using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CAbilityScore : CConsideration 
{
	// Member variables
	private int m_abilityScore;


	// MonoBehaviour-Methods
	sealed protected override void Start()
	{
		base.Start();

		m_abilityScore = 0;
	}


	// Methods
	public override float CalculateConsiderationScore()
	{
		CAbility abilityToExecute = CUtilityAiSystem.GetInstance().GetContext().GetAbilityToExecute();

		bool isDamageAbility = abilityToExecute.GetType() == typeof(CDamageAbility);
		bool isHealAbility = abilityToExecute.GetType() == typeof(CHealAbility);
		if (isDamageAbility)
		{
			CDamageAbility damageAbilityToExecute = (CDamageAbility)abilityToExecute;

			float totalBaseDamage = damageAbilityToExecute.GetTotalBaseDamage();
			m_abilityScore = Mathf.Clamp((int)(totalBaseDamage / 2), 0, 8);

			bool canInterrupt = damageAbilityToExecute.GetCanInterrupt();
			if (canInterrupt)
			{
				m_abilityScore++;
			}

			bool hasAreaOfEffect = damageAbilityToExecute.GetHasAreaOfEffect();
			if (hasAreaOfEffect)
			{
				m_abilityScore++;
			}
		}
		else if (isHealAbility)
		{
			CHealAbility healAbilityToExecute = (CHealAbility)abilityToExecute;

			float baseHeal = healAbilityToExecute.GetBaseHeal();
			m_abilityScore = Mathf.Clamp((int)(baseHeal / 4), 0, 8);

			bool hasAreaOfEffect = healAbilityToExecute.GetHasAreaOfEffect();
			if (hasAreaOfEffect)
			{
				m_abilityScore += 2;
			}
		}
		else
		{
			m_abilityScore = 0;
		}

		m_input = MapToBookends(m_abilityScore);
		m_score = m_responseCurve.CalculateResponseCurveValue(m_input);

		return Mathf.Clamp(m_score, 0.0f, 1.0f);
	}


	public override string ToString()
	{
		return string.Format("Consideration Score:; {0}; AbilityScore: {1}; Input: {2}; ResponseCurve: {3}",
			Mathf.Clamp(m_score, 0.0f, 1.0f), m_abilityScore, m_input, m_score);
	}
}
