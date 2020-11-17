using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CDistanceTo : CConsideration
{
	// Member variables
	private float m_distance;


	// MonoBehaviour-Methods
	void Awake()
	{
		
	}

	sealed protected override void Start()
	{
		base.Start();

		m_distance = 0.0f;
	}



	// Methods
	public override float CalculateConsiderationScore()
	{
		Vector3 ownPosition = CUtilityAiSystem.GetInstance().GetContext().GetExecutor().transform.position;
		Vector3 targetPosition = CUtilityAiSystem.GetInstance().GetContext().GetTarget().transform.position;

		m_distance = (ownPosition - targetPosition).magnitude;

		m_input = MapToBookends(m_distance);
		m_score = m_responseCurve.CalculateResponseCurveValue(m_input);

		return Mathf.Clamp(m_score, 0.0f, 1.0f);
	}


	public override string ToString()
	{
		return string.Format("Consideration Score:; {0}; distance: {1}; Input: {2}; ResponseCurve: {3}",
			Mathf.Clamp(m_score, 0.0f, 1.0f), m_distance, m_input, m_score);
	}
}
