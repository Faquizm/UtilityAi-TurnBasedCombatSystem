using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CEntitiesInRadius : CConsideration 
{
	// Member variables
	private int m_entitiesInRadius;


	// MonoBehaviour-Methods
	sealed protected override void Start() 
	{
		base.Start();

		m_entitiesInRadius = 0;
	}
	
	
	// Methods
	public override float CalculateConsiderationScore()
	{
		GameObject target = CUtilityAiSystem.GetInstance().GetContext().GetTarget();
		CAbility abilityToExecute = CUtilityAiSystem.GetInstance().GetContext().GetAbilityToExecute();
		float abilityRadius;

		bool hasRadiusProperty = abilityToExecute.GetType().GetMethod("GetRadius") != null;
		if (hasRadiusProperty)
		{
			abilityRadius = (float)abilityToExecute.GetType().GetMethod("GetRadius").Invoke(abilityToExecute, null);
		}
		else
		{
			//Debug.LogWarning("Ability \"" + abilityToExecute.GetAbilityName() + "\" doesn't have a method called \"GetRadius\". Radius set to 10.0");
			abilityRadius = 10.0f;
		}

		CUtilityAiSystem.GetInstance().UpdateEntitiesInRadius(abilityRadius);
		m_entitiesInRadius = CUtilityAiSystem.GetInstance().GetContext().GetEntitiesInRadius();
		m_input = MapToBookends(m_entitiesInRadius);

		m_score = m_responseCurve.CalculateResponseCurveValue(m_input);

		return Mathf.Clamp(m_score, 0.0f, 1.0f);
	}


	public override string ToString()
	{
		return string.Format("Consideration Score:; {0}; EntitiesInRadius: {1}; Input: {2}; ResponseCurve: {3}",
			Mathf.Clamp(m_score, 0.0f, 1.0f), m_entitiesInRadius, m_input, m_score);
	}
}
