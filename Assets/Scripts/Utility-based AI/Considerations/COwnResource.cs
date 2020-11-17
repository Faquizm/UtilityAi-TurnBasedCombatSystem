using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class COwnResource : CConsideration 
{
	// Member variables
	private float m_ownResourcePercentage;
	

	// MonoBehaviour-Methods
	sealed protected override void Start() 
	{
		base.Start();

		m_ownResourcePercentage = 0.0f;
	}


	// Methods
	public override float CalculateConsiderationScore()
	{
		CEntity executor = CUtilityAiSystem.GetInstance().GetContext().GetExecutorAsEntity();
		CAbility abilityToExecute = CUtilityAiSystem.GetInstance().GetContext().GetAbilityToExecute();
		ResourceType abilityResourceType;

		bool hasResourceTypeProperty = abilityToExecute.GetType().GetMethod("GetResourceType") != null;
		if (hasResourceTypeProperty)
		{
			abilityResourceType = (ResourceType)abilityToExecute.GetType().GetMethod("GetResourceType").Invoke(abilityToExecute, null);
		}
		else
		{
			Debug.LogError("Ability doesn't have a method called \"GetResourceType\".");
			return 0.0f;
		}

		if (abilityResourceType == ResourceType.SkillPoints)
		{
			m_ownResourcePercentage = executor.GetSkillPointsPercentage();
		}
		else if (abilityResourceType == ResourceType.MagicPoints)
		{
			m_ownResourcePercentage = executor.GetMagicPointsPercentage();
		}
		else
		{
			Debug.LogError("Unknown resource type.");
			return 0.0f;
		}

		// If the ability doesn't cost anything, the consideration is fulfilled
		int costs = (int)abilityToExecute.GetType().GetMethod("GetCosts").Invoke(abilityToExecute, null);
		if (costs == 0)
		{
			m_input = 1.0f;
		}
		else
		{
			m_input = m_ownResourcePercentage;
		}

		m_score = m_responseCurve.CalculateResponseCurveValue(m_input);

		return Mathf.Clamp(m_score, 0.0f, 1.0f);
	}

	public override string ToString()
	{
		return string.Format("Consideration Score:; {0}; ownResource (%): {1}; Input: {2}; ResponseCurve: {3}",
			Mathf.Clamp(m_score, 0.0f, 1.0f), m_ownResourcePercentage, m_input, m_score);
	}
}
