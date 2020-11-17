using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Enums
public enum ActionType { Damage, Heal, Buff, Debuff, Interrupt, Defense, Hide, Move }

public class CAction : MonoBehaviour
{
	// Member variables
	[Header("General")]
	[SerializeField] protected ActionType m_actionType;
	[SerializeField] protected bool m_isAreaOfEffectAction;
	protected string m_name;
	protected float m_actionScore;
	
	[Space(7)]
	[Header("Considerations")]
	protected List<CConsideration> m_considerations;
	protected List<float> m_considerationScores;


	// MonoBehaviour-Methods
	private void Awake()
	{
		m_name = gameObject.name;
		m_considerations = new List<CConsideration>();
	}

	protected virtual void Start()
	{
		if (GetComponentInParent<CEntity>() != null)
		{
			GetComponentInParent<CEntity>().AddActionToEntity(this);
		}
		else
		{
			Debug.LogWarning("No component found to add action (" + (m_name + "; " + GetType().ToString()) + ").");
		}
	}


	// Methods
	public virtual float CalculateActionScore()
	{
		// Iterate through considerations
		m_considerationScores = new List<float>();
		m_actionScore = 1.0f;

		// Calculate all consideration values
		foreach (CConsideration consideration in m_considerations)
		{
			m_considerationScores.Add(consideration.CalculateConsiderationScore());
		}

		// Compensate
		CUtilityAiSystem.ActionScoreCompensation actionScoreCompensation = CUtilityAiSystem.GetInstance().GetActionScoreCompensation();
		switch (actionScoreCompensation)
		{
			case CUtilityAiSystem.ActionScoreCompensation.None:
				// Use no compensation
				foreach (float considerationScore in m_considerationScores)
				{
					m_actionScore *= considerationScore;
				}
				break;

			case CUtilityAiSystem.ActionScoreCompensation.MarkAndSizer:
				// Use the compensation factor to compensate each consideration score made by Dave Mark and Ben Sizer
				float modificationFactor = 1.0f - (1.0f / m_considerationScores.Count);

				foreach (float considerationScore in m_considerationScores)
				{
					float makeUpValue = (1.0f - considerationScore) * modificationFactor;
					float finalConsiderationScore = considerationScore + (makeUpValue * considerationScore);

					m_actionScore *= finalConsiderationScore;
				}
				break;

			case CUtilityAiSystem.ActionScoreCompensation.Average:
				// Use the average as action score
				float considerationScoresSum = 0.0f;

				foreach (float considerationScore in m_considerationScores)
				{
					considerationScoresSum += considerationScore;
				}

				m_actionScore = considerationScoresSum / m_considerationScores.Count;
				break;

			case CUtilityAiSystem.ActionScoreCompensation.ActionScoreDamping:
				// Use own method of compensation 
				float epsilon = CUtilityAiSystem.GetInstance().GetEpsilon();

				if (m_considerationScores.Count > 1)
				{
					foreach (float considerationScore in m_considerationScores)
					{
						m_actionScore *= considerationScore + (considerationScore * (1.0f - considerationScore));
					}
				}
				else
				{
					m_actionScore *= m_considerationScores[0];
				}
				
				m_actionScore = (m_considerationScores.Count / (epsilon + m_considerationScores.Count)) * m_actionScore;
				break;

			default:
				break;
		}

		return m_actionScore;
	}


	public void AddConsideration(CConsideration consideration)
	{
		m_considerations.Add(consideration);
	}


	// Getter/Setter
	public string GetName()
	{
		return m_name;
	}

	public ActionType GetActionType()
	{
		return m_actionType;
	}

	public bool GetIsAreaOfEffectAction()
	{
		return m_isAreaOfEffectAction;
	}

	public CConsideration GetConsiderationAt(int index)
	{
		if (index < m_considerations.Count)
		{
			return m_considerations[index];
		}
		else
		{
			return m_considerations[0];
		}
	}

	public int GetConsiderationCount()
	{
		return m_considerations.Count;
	}
}