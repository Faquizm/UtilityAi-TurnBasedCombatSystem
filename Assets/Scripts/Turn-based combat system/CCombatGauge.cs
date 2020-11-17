using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CCombatGauge : MonoBehaviour
{
	// Constant variables
	const int INTERVAL_COUNT = 5;

	// Member variables
	[Header("Settings - Gauge")]
	[SerializeField] private bool m_hasAiMultipleChoice = true;
	[SerializeField, DisabledVariable] private float m_gaugeLength = 1000.0f;
	[SerializeField, DisabledVariable] private float m_intervalLength = 200.0f;
	[SerializeField, DisabledVariable] private float m_playerDecisionThreshold = 800.0f;
	[SerializeField, DisabledVariable] private float m_aiLastDecisionThreshold = 600.0f;
	[SerializeField, DisabledVariable] private float m_aiDecisionInterval = 200.0f;

	[Header("Settings - Seeding")]
	[SerializeField] private bool m_isRandomizedAroundSeedPoint;
	[SerializeField] private float m_maxRandomOffset = 100.0f;
	
	[Header("Settings - Runtime")]
	[SerializeField, Range(3.0f, 10.0f)] private float m_entitySpeedScale = 5.0f;


	// MonoBehaviour-Methods
	void Awake()
	{
		UpdateSettings();
	}

	void OnValidate()
	{
		UpdateSettings();
	}


	// Methods
	public void SeedParticipantsOnGauge()
	{
		int playerParticipantsCount = CCombatSystem.GetInstance().GetPlayerTeam().Count;
		int enemyParticipantsCount = CCombatSystem.GetInstance().GetEnemyTeam().Count;

		if (playerParticipantsCount > 0 && enemyParticipantsCount > 0)
		{
			bool isPlayerGroupSurprised = CCombatSystem.GetInstance().GetPlayerTeam()[0].GetEntity().transform.parent.GetComponent<CGroup>().GetIsSurprised();
			bool isEnemyGroupSurprised = CCombatSystem.GetInstance().GetEnemyTeam()[0].GetEntity().transform.parent.GetComponent<CGroup>().GetIsSurprised();
			if (!isPlayerGroupSurprised && !isEnemyGroupSurprised)
			{
				// Position all units in a fair way
				Seed(CCombatSystem.GetInstance().GetBothTeamsAsOneList(), false);
			}
			else
			{
				// Position player and enemy units based on whether they are surprised or not
				Seed(CCombatSystem.GetInstance().GetPlayerTeam(), isPlayerGroupSurprised);
				Seed(CCombatSystem.GetInstance().GetEnemyTeam(), isEnemyGroupSurprised);
			}

			// Check if a participant was seeded beyond the last decision point
			CheckExceedingLastDecisionPoint();
		}
		else
		{
			Debug.LogError("Player team or enemy team isn't initialized yet. Couldn't seed participants on gauge.");
		}
	}


	private void Seed(List<CCombatParticipant> participants, bool isSurprised)
	{
		List<CCombatParticipant> copiedParticipants = new List<CCombatParticipant>(participants);
		int participantsCount = participants.Count;

		// Randomize order of the gauge units positioning
		List<CCombatParticipant> randomOrderedParticipants = new List<CCombatParticipant>();
		for (int i = 0; i < participantsCount; i++)
		{
			int rndParticipantIndex = Random.Range(0, copiedParticipants.Count);
			randomOrderedParticipants.Add(copiedParticipants[rndParticipantIndex]);
			copiedParticipants.RemoveAt(rndParticipantIndex);
		}

		int minGaugeSeedPosition = (int)m_intervalLength;
		int maxGaugeSeedPosition = (int)(m_playerDecisionThreshold - m_intervalLength);
		int gaugeSeedPositionInterval = maxGaugeSeedPosition - minGaugeSeedPosition;

		float gaugeSeedPositionStep;
		if (participantsCount == 1)
		{
			gaugeSeedPositionStep = gaugeSeedPositionInterval / 2.0f;
		}
		else
		{
			gaugeSeedPositionStep = gaugeSeedPositionInterval / (participantsCount - 1);
		}

		for (int i = 0; i < participantsCount; i++)
		{
			float randomizeOffset = 0.0f;
			if (m_isRandomizedAroundSeedPoint)
			{
				randomizeOffset = Random.Range(0.0f, m_maxRandomOffset) - m_maxRandomOffset / 2.0f;
			}

			float surpriseDisadvantage = 0.0f;
			if (isSurprised)
			{
				surpriseDisadvantage = m_intervalLength;
			}

			float startGaugePosition = minGaugeSeedPosition + gaugeSeedPositionStep * i - randomizeOffset - surpriseDisadvantage;
			startGaugePosition = Mathf.Clamp(startGaugePosition, 0.0f, maxGaugeSeedPosition);

			randomOrderedParticipants[i].SetGaugePosition(startGaugePosition-1.0f);
			randomOrderedParticipants[i].SetNextGaugePosition(startGaugePosition+1.0f);
		}
	}


	public void CheckExceedingLastDecisionPoint()
	{
		foreach (CCombatParticipant participant in CCombatSystem.GetInstance().GetBothTeamsAsOneList())
		{
			bool hasExceededLastDecisionPoint = participant.GetGaugePosition() >= m_aiLastDecisionThreshold;
			if (participant.GetEntity().GetIsControlledByAI() && hasExceededLastDecisionPoint)
			{
				participant.MakeDecision();
			}
		}
	}


	public void MoveToExecutionPoint(CCombatParticipant participant)
	{
		participant.SetGaugePosition(1000.0f);
		participant.SetNextGaugePosition(1000.0f);
	}


	public void UpdateParticipantsGaugePosition(float timeDelta)
	{
		foreach (CCombatParticipant participant in CCombatSystem.GetInstance().GetBothTeamsAsOneList())
		{
			if (participant.GetEntity().IsDefeated())
			{
				participant.SetGaugePosition(0.0f);
				continue;
			}	

			if (!participant.GetHasReachedExecutionPoint())
			{
				// Current position
				float currentGaugePosition = participant.GetGaugePosition();

				// Step forward
				float speedScaleFactor = m_entitySpeedScale * participant.GetEntity().GetStatWeights().GetStatWeight(StatType.Speed) * (1 + participant.GetCombatStatChanges().GetStatChange(StatType.Speed));
				float scaledEntitySpeed = participant.GetEntity().GetStats().GetSpeed() * speedScaleFactor;
				scaledEntitySpeed = Mathf.Clamp(scaledEntitySpeed, 0.0f, 120.0f);
				float gaugePositionDelta = scaledEntitySpeed * timeDelta;

				// Future position
				float newGaugePosition = currentGaugePosition + gaugePositionDelta;

				participant.SetGaugePosition(newGaugePosition);
				participant.SetNextGaugePosition(newGaugePosition + (2.0f * gaugePositionDelta));

				// Unlock participant if he passed it's unlock position
				if (participant.GetGaugePosition() > participant.GetUnlockPosition())
				{
					UnlockParticipant(participant);
				}
			}
		}
	}


	public void LockParticipant(CCombatParticipant participant)
	{
		participant.SetIsLocked(true);

		float unlockPosition = ((int)participant.GetGaugePosition() + m_intervalLength / 10) % 1000;
		participant.SetUnlockPosition(unlockPosition);
	}


	public void UnlockParticipant(CCombatParticipant participant)
	{
		participant.SetIsLocked(false);
		participant.SetUnlockPosition(0.0f);
	}


	public float CalculateUnscaledPreparationSpeed(float preparationTime, float statWeight, float statCombatChange)
	{
		float speedScaleFactor = m_entitySpeedScale * statWeight * statCombatChange;
		return (m_intervalLength / preparationTime) / speedScaleFactor;
	}


	public void ResetParticipantGaugePosition(CCombatParticipant participant, bool hasExecutedAction)
	{
		if (hasExecutedAction)
		{
			participant.SetGaugePosition(0.0f);
			return;
		}
		else
		{
			participant.SetGaugePosition(CCombatSystem.GetInstance().GetGaugeResetPosition());
			return;
		}
	}


	public void ResetParticipantGaugePosition(CCombatParticipant participant)
	{
		ResetParticipantGaugePosition(participant, false);
	}


	private void UpdateSettings()
	{
		m_gaugeLength = 1000;
		m_intervalLength = m_gaugeLength / INTERVAL_COUNT;
		m_playerDecisionThreshold = m_intervalLength * (INTERVAL_COUNT * 0.8f);

		if (m_hasAiMultipleChoice)
		{
			m_aiDecisionInterval = m_intervalLength;
			m_aiLastDecisionThreshold = m_intervalLength * (INTERVAL_COUNT * 0.6f);
		}
		else
		{
			m_aiLastDecisionThreshold = m_intervalLength * (INTERVAL_COUNT * 0.4f);
			m_aiDecisionInterval = m_aiLastDecisionThreshold;
		}
	}


	// Getter/Setter
	public float GetGaugeLength()
	{
		return m_gaugeLength;
	}

	public float GetPlayerDecisionThreshold()
	{
		return m_playerDecisionThreshold;
	}

	public float GetPreparationPoint()
	{
		return GetPlayerDecisionThreshold();
	}

	public float GetAiLastDecisionThreshold()
	{
		return m_aiLastDecisionThreshold;
	}

	public float GetAiDecisionInterval()
	{
		return m_aiDecisionInterval;
	}
}