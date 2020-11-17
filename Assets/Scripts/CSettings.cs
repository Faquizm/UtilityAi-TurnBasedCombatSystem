using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CSettings : MonoBehaviour 
{
	// Member variables
	private static CSettings m_settings;

	[Header("Settings - AI")]
	[SerializeField] private CUtilityAiSystem.DecisionHeuristic m_decisionHeuristic;
	[SerializeField, Range(0, 20)] private int m_heuristicRange = 1;
	[SerializeField, Range(0.1f, 0.4f)] private float m_manipulationRange = 0.25f; 
	[Space(7)]
	[SerializeField] private CUtilityAiSystem.ActionScoreCompensation m_actionScoreCompensation;
	[SerializeField, Range(0.01f, 2.0f)] private float m_actionScoreDampingEpsilon = 0.1f;

	// MonoBehaviour-Methods
	void Awake()
	{
		if (m_settings == null)
		{
			m_settings = GameObject.FindGameObjectWithTag("Settings").GetComponent<CSettings>();		

			// Settings should exist in all scenes
			DontDestroyOnLoad(gameObject);
		}
		else
		{
			Destroy(gameObject);
		}
	}
	
	
	// Methods
	
	
	// Getter/Setter
	public static CSettings GetInstance()
	{
		return m_settings;
	}

	public CUtilityAiSystem.DecisionHeuristic GetDecisionHeuristic()
	{
		return m_decisionHeuristic;
	}

	public void SetDecisionHeuristic (CUtilityAiSystem.DecisionHeuristic heuristic)
	{
		m_decisionHeuristic = heuristic;
	}

	public CUtilityAiSystem.ActionScoreCompensation GetActionScoreCompensation()
	{
		return m_actionScoreCompensation;
	}

	public void SetActionScoreCompensation(CUtilityAiSystem.ActionScoreCompensation compensation)
	{
		m_actionScoreCompensation = compensation;
	}

	public int GetHeuristicRange()
	{
		return m_heuristicRange;
	}

	public void SetHeuristicRange(int range)
	{
		m_heuristicRange = range;
	}

	public float GetManipulationRange()
	{
		return m_manipulationRange;
	}

	public void SetManipulationRange(float range)
	{
		m_manipulationRange = range;
	}

	public float GetEpsilon()
	{
		return m_actionScoreDampingEpsilon;
	}

	public void SetEpsilon(float epsilon)
	{
		m_actionScoreDampingEpsilon = epsilon;
	}
}
