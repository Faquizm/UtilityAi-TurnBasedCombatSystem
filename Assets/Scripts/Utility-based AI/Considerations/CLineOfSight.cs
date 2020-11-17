using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CLineOfSight : CConsideration 
{
	// Member variables
	private bool m_isInLineOfSight;

		
	// MonoBehaviour-Methods
	sealed protected override void Start() 
	{
		base.Start();

		m_isInLineOfSight = false;
	}


	// Methods
	public override float CalculateConsiderationScore()
	{
		GameObject executor = CUtilityAiSystem.GetInstance().GetContext().GetExecutor();
		GameObject target = CUtilityAiSystem.GetInstance().GetContext().GetTarget();

		Vector3 raycastDirection = (target.transform.position - executor.transform.position).normalized;
		
		LayerMask layermask = LayerMask.GetMask("Entities");
		RaycastHit[] raycastHits = Physics.RaycastAll(executor.transform.position, raycastDirection, Mathf.Infinity, layermask);
		System.Array.Sort(raycastHits, (first, second) => first.distance.CompareTo(second.distance));

		foreach (RaycastHit hit in raycastHits)
		{
			if (hit.transform.GetComponent<CEntity>() != null)
			{
				if (hit.transform.GetComponent<CEntity>() == target.GetComponent<CEntity>())
				{
					m_isInLineOfSight = true;
					break;
				}
			}
			else
			{
				m_isInLineOfSight = false;
				break;
			}
		}

		if (m_isInLineOfSight)
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
		return string.Format("Consideration Score:; {0}; isInLineOfSight: {1}; Input: {2}; ResponseCurve: {3}",
			Mathf.Clamp(m_score, 0.0f, 1.0f), m_isInLineOfSight, m_input, m_score);
	}
}
