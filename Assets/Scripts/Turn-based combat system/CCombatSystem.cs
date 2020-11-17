using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CCombatSystem : MonoBehaviour
{
	// Member variables
	private static CCombatSystem m_combatSystem;
	private CCombatGauge m_combatGauge;
	private List<CObstacle> m_obstacles;

	[Header("Settings - Damage")]
	[Tooltip("Time a participant will stop on the gauge when being damaged.")]
	[SerializeField, Range(1.0f, 5.0f)] private float m_waitDuration;
	[Space(7), Tooltip("Percentage around the average damage to choose the actual damage from.")]
	[SerializeField, Range(0.1f, 0.4f)] private float m_abilityValueRange = 0.2f;

	[Header("Settings - Ability execution")]
	[SerializeField, Range(5.0f, 20.0f)] private float m_timeoutDuration;
	[SerializeField, Range(5.0f, 20.0f)] private float m_returnPositionRadius = 10.0f;

	[Header("Settings - Interrupt")]
	[SerializeField, Range(0.1f, 0.45f)] private float m_gaugeLengthResetPercentage;
	[SerializeField, DisabledVariable] private float m_gaugeResetPosition;

	[Header("Teams")]
	[SerializeField] private List<CCombatParticipant> m_enemyTeam;
	[SerializeField] private List<CCombatParticipant> m_playerTeam;
	private List<CCombatParticipant> m_allParticipants;

	[Header("State")]
	[SerializeField] private bool m_isRunning;
	[SerializeField] private bool m_isPaused;
	[SerializeField] private bool m_isPlayerDefeated;
	[SerializeField] private bool m_isEnemyDefeated;



	// MonoBehaviour-Methods
	void Awake()
	{
		if (m_combatSystem == null)
		{
			m_combatSystem = GameObject.FindGameObjectWithTag("CombatSystem").GetComponent<CCombatSystem>();
			m_combatGauge = GetComponent<CCombatGauge>();
			m_obstacles = new List<CObstacle>(FindObjectsOfType<CObstacle>());

			m_allParticipants = new List<CCombatParticipant>();
			m_enemyTeam = new List<CCombatParticipant>();
			m_playerTeam = new List<CCombatParticipant>();

			m_isPlayerDefeated = false;
			m_isEnemyDefeated = false;
		}
	}
	

	void Update()
	{
		if (!m_isPaused)
		{
			// Activate observer mode
			if (Input.GetKeyDown(KeyCode.O) || Input.GetButtonDown("ObserverMode"))
			{
				CCombatSystemUi.GetInstance().ToggleObserverUI(true);

				foreach (CCombatParticipant player in m_playerTeam)
				{
					player.GetEntity().SetIsControlledByAI(true);

					// Make a decision if necessary
					if (player.GetGaugePosition() > m_combatGauge.GetAiLastDecisionThreshold() && !player.GetHasChosenAnAction())
					{
						player.MakeDecision();
					}
				}
			}

			// Activate player mode
			if (Input.GetKeyDown(KeyCode.P) || Input.GetButtonDown("PlayerMode"))
			{
				CCombatSystemUi.GetInstance().ToggleObserverUI(false);

				foreach (CCombatParticipant player in m_playerTeam)
				{
					player.GetEntity().SetIsControlledByAI(false);

					// Delete current AI decision
					if (player.GetGaugePosition() < m_combatGauge.GetPlayerDecisionThreshold())
					{
						player.ResetDecision();
					}
				}
			}
		}


		if (Input.GetKeyDown(KeyCode.Space) || Input.GetButtonDown("PauseCombatSystem"))
		{
			bool isAnyoneExecuting = false;
			foreach (CCombatParticipant participant in m_allParticipants)
			{
				isAnyoneExecuting |= participant.GetIsExecutingAction();
			}

			if (!isAnyoneExecuting)
			{
				bool isAtDecisionPoint = false;

				foreach (CCombatParticipant participant in m_playerTeam)
				{
					if (participant.GetGaugePosition() > 800 && participant.GetGaugePosition() < 805)
					{
						isAtDecisionPoint = true;
						break;
					}
				}

				if (!isAtDecisionPoint)
				{
					m_isPaused = !m_isPaused;
				}
			}
		}
	}


	void OnValidate()
	{
		m_gaugeResetPosition = 1000 * m_gaugeLengthResetPercentage;
	}


	// Methods
	public void Init()
	{
		CDecisionLogger.Init();

		Vector3 enemyTeamCenter = CalculateOppositeTeamCenter(m_playerTeam[0]);
		Vector3 playerTeamCenter = CalculateOppositeTeamCenter(m_enemyTeam[0]);

		foreach (CCombatParticipant participant in m_allParticipants)
		{
			if (participant.GetEntity().CompareTag("Player"))
			{
				participant.GetEntity().transform.LookAt(enemyTeamCenter);
			}
			else if (participant.GetEntity().CompareTag("Enemy"))
			{
				participant.GetEntity().transform.LookAt(playerTeamCenter);
			}
		}

		m_combatGauge.SeedParticipantsOnGauge();
		m_allParticipants.Sort(SortByTag);
	}

	public void AddCombatParticipant(CCombatParticipant participant)
	{
		if (participant.GetEntity().tag.Equals("Enemy"))
		{
			m_enemyTeam.Add(participant);
		}
		else if (participant.GetEntity().tag.Equals("Player"))
		{
			m_playerTeam.Add(participant);
		}
		else
		{
			Debug.LogError(string.Format("Unknown tag on {0}.", participant.GetEntity().name));
		}

		m_allParticipants.Add(participant);
	}


	public void StartCombatSystem()
	{
		Debug.Log("Starting Combat System...");

		StartCoroutine(RunCombatCore());
		StartCoroutine(RunCombatGauge());
		StartCoroutine(CCombatSystemUi.GetInstance().RunCombatUi());

		Debug.Log("Starting Combat System done.");
	}


	private IEnumerator RunCombatGauge()
	{
		Debug.Log("Combat gauge running...");

		while (!(m_isPlayerDefeated || m_isEnemyDefeated))
		{
			yield return new WaitWhile(() => m_isPaused);

			m_combatGauge.UpdateParticipantsGaugePosition(Time.fixedDeltaTime);			// If thresholds are skipped, this maybe needs to be changed to delta time because it depends on precise timing

			yield return new WaitForEndOfFrame();
		}

		Debug.Log("Combat gauge stopped.");
	}


	private IEnumerator RunCombatCore()
	{
		m_isRunning = true;
		Debug.Log("Combat Core running...");


		// As long as no team was defeated...
		while (!(m_isPlayerDefeated || m_isEnemyDefeated))
		{
			yield return new WaitWhile(() => m_isPaused);

			// Until a participant reaches a threshold, move the entities
			foreach (CCombatParticipant participant in m_allParticipants)
			{
				if (participant.GetEntity().IsDefeated())
				{
					continue;
				}

				bool isParticipantLocked = participant.GetIsLocked();
				bool isControlledByAI = participant.GetEntity().GetIsControlledByAI();
				if (isControlledByAI)
				{
					// AI decision making
					bool isAtDecisionPoint = (int)participant.GetGaugePosition() % (int)m_combatGauge.GetAiDecisionInterval() > (int)participant.GetNextGaugePosition() % (int)m_combatGauge.GetAiDecisionInterval()
						&& (int)participant.GetGaugePosition() <= (int)m_combatGauge.GetAiLastDecisionThreshold()/* && participant.GetDecision() == null*/;
					if (!isParticipantLocked && isAtDecisionPoint)
					{
						participant.MakeDecision();
						m_combatGauge.LockParticipant(participant);
					}

					// Prepare the chosen action if preparation point is reached
					bool isAtPreparationPoint = (int)participant.GetGaugePosition() >= (int)m_combatGauge.GetPreparationPoint();
					if (!isParticipantLocked && isAtPreparationPoint && !participant.GetIsPreparingAction())
					{
						participant.PrepareAction();

						if (participant.IsPreparingInstantAbility())
						{
							m_combatGauge.MoveToExecutionPoint(participant);
							m_combatGauge.UnlockParticipant(participant);
						}
					}
				}
				else
				{
					// Player decision making
					bool isAtDecisionPoint = (int)participant.GetGaugePosition() > m_combatGauge.GetPlayerDecisionThreshold() && participant.GetDecision() == null;

					if (!isParticipantLocked && isAtDecisionPoint)
					{
						participant.CreateNewDecision();

						// Decision UI
						m_isPaused = true;
						CCombatSystemUi.GetInstance().ToggleParticipantUI(participant, true);

						yield return new WaitUntil(participant.GetHasChosenAnAction);

						CCombatSystemUi.GetInstance().ToggleParticipantUI(participant, false);
						m_combatGauge.LockParticipant(participant);
						m_isPaused = false;

						// Prepare the chosen action
						participant.PrepareAction();

						if (participant.IsPreparingInstantAbility())
						{
							m_combatGauge.MoveToExecutionPoint(participant);
							m_combatGauge.UnlockParticipant(participant);
						}
					}
				}

				// Execute action
				bool isAtExecutionPoint = (int)participant.GetGaugePosition() >= 1000;
				if (!isParticipantLocked && isAtExecutionPoint)
				{
					participant.SetHasReachedExecutionPoint(true);
					m_combatGauge.LockParticipant(participant);

					participant.StartExecution();
				}

				// Pause CombatSystem if an action is being executed
				if (participant.GetIsExecutingAction())
				{
					m_isPaused = true;

					if (participant.GetDecision().GetTarget().GetComponent<CEntity>() != null)
					{
						CCombatParticipant target = GetParticipantByEntity(participant.GetDecision().GetTarget().GetComponent<CEntity>());

						yield return new WaitWhile(() => participant.GetIsExecutingAction());

						// Stop iterating through participants if a team is already defeated
						if (target.GetEntity().IsDefeated())
						{
							if (IsTeamDefeated(m_playerTeam) || IsTeamDefeated(m_enemyTeam))
							{
								break;
							}
						}
					}
					else
					{
						yield return new WaitWhile(() => participant.GetIsExecutingAction());
					}

					m_isPaused = false;
				}
			}

			m_isPlayerDefeated = IsTeamDefeated(m_playerTeam);
			m_isEnemyDefeated = IsTeamDefeated(m_enemyTeam);
			yield return null;
		}

		Debug.Log("Combat Core stopped.");

		// End combat
		EndCombatSystem();
	}

	private void EndCombatSystem()
	{
		// Mark the defeated group
		if (m_isPlayerDefeated)
		{
			m_playerTeam[0].GetEntity().gameObject.transform.parent.GetComponent<CGroup>().SetIsGroupDefeated(true);
		}
		else if (m_isEnemyDefeated)
		{
			m_enemyTeam[0].GetEntity().gameObject.transform.parent.GetComponent<CGroup>().SetIsGroupDefeated(true);
		}
		else
		{
			Debug.LogError("No team defeated, but combat has ended. This may be unintended.");
		}

		foreach (CCombatParticipant participant in m_allParticipants)
		{
			participant.GetEntity().StopAllCoroutines();

			if (!(participant.GetEntity().GetStats().GetSpeed() > 0))
			{
				participant.GetEntity().GetStats().SetSpeed(participant.GetStoredEntitySpeed());
			}

			if (participant.GetEntity().tag.Equals("Player"))
			{
				if (participant.GetEntity().GetIsControlledByAI())
				{
					participant.GetEntity().SetIsControlledByAI(false);
				}

				if (participant.GetEntity().IsDefeated())
				{
					participant.GetEntity().gameObject.SetActive(true);
				}
			}
		}

		CCombatInitializer.GetInstance().AddCombatParticipantsToSceneManager();
		CSceneManager.GetInstance().LeaveCombatScene();

		m_isRunning = false;
		Debug.Log("Combat System stopped.");
	}


	public List<CCombatParticipant> DetermineParticipantsInRadius(float radius, GameObject target)
	{
		List<CCombatParticipant> participantsInRadius = new List<CCombatParticipant>();
		Vector3 targetPosition = target.transform.localPosition;
		string targetTag = target.tag;

		if (targetTag.Equals("Player"))
		{
			foreach (CCombatParticipant participant in m_playerTeam)
			{
				Vector3 participantPosition = participant.GetEntity().transform.localPosition;
				if ((participantPosition - targetPosition).magnitude < radius && !participant.GetEntity().IsDefeated())
				{
					participantsInRadius.Add(participant);
				}
			}

		}
		else if (targetTag.Equals("Enemy"))
		{
			foreach (CCombatParticipant participant in m_enemyTeam)
			{
				Vector3 participantPosition = participant.GetEntity().transform.localPosition;
				if ((participantPosition - targetPosition).magnitude < radius && !participant.GetEntity().IsDefeated())
				{
					participantsInRadius.Add(participant);
				}
			}
		}
		else
		{
			Debug.LogWarning("Unknown tag on target when determining participants in radius.");
		}

		return participantsInRadius;
	}


	public bool IsTeamDefeated(List<CCombatParticipant> team)
	{
		bool areAllParticipantsDefeated = true;
		foreach (CCombatParticipant participant in team)
		{
			areAllParticipantsDefeated &= participant.GetEntity().IsDefeated();
		}

		return areAllParticipantsDefeated;
	}


	public void ResetParticipantHasDecisionChanged(CCombatParticipant participant)
	{
		participant.ResetDecisionHasChanged();
	}

	public Vector3 CalculateOppositeTeamCenter(CCombatParticipant ofParticipant)
	{
		Vector3 center = Vector3.zero;
		int countedParticipants = 0;
		if (ofParticipant.GetEntity().CompareTag("Player"))
		{
			// Look at enemy team center
			foreach (CCombatParticipant teamMember in m_enemyTeam)
			{
				if (!teamMember.GetEntity().IsDefeated())
				{
					center += teamMember.GetEntity().transform.position;
					countedParticipants++;
				}
			}
		}
		else
		{
			// Look at player team center
			foreach (CCombatParticipant teamMember in m_playerTeam)
			{
				if (!teamMember.GetEntity().IsDefeated())
				{
					center += teamMember.GetEntity().transform.position;
					countedParticipants++;
				}
			}
		}

		if (countedParticipants > 0)
		{
			center /= countedParticipants;
		}
		else
		{
			center = Vector3.zero;
		}

		return center;
	}


	// Static methods
	public static int SortByTag(CCombatParticipant participant_01, CCombatParticipant participant_02)
	{
		if (participant_01.GetEntity().tag.Equals("Player") && participant_02.GetEntity().tag.Equals("Player")
			|| participant_01.GetEntity().tag.Equals("Enemy") && participant_02.GetEntity().tag.Equals("Enemy"))
		{
			return 0;
		}
		else if (participant_01.GetEntity().tag.Equals("Player") && participant_02.GetEntity().tag.Equals("Enemy"))
		{
			return 1;
		}
		else if (participant_01.GetEntity().tag.Equals("Enemy") && participant_02.GetEntity().tag.Equals("Player"))
		{
			return -1;
		}
		else
		{
			Debug.LogError("Unknown entity tags.");
			return 0;
		}
	}



	// Getter/Setter
	public static CCombatSystem GetInstance()
	{
		return m_combatSystem;
	}

	public CCombatGauge GetCombatGauge()
	{
		return m_combatGauge;
	}

	public List<CObstacle> GetObstacles()
	{
		return m_obstacles;
	}

	public float GetTimeoutDuration()
	{
		return m_timeoutDuration;
	}

	public float GetGaugeResetPosition()
	{
		return m_gaugeResetPosition;
	}

	public float GetWaitDuration()
	{
		return m_waitDuration;
	}

	public float GetAbilityValueRange()
	{
		return m_abilityValueRange;
	}

	public float GetReturnPositionRadius()
	{
		return m_returnPositionRadius;
	}

	public List<CCombatParticipant> GetEnemyTeam()
	{
		return m_enemyTeam;
	}

	public List<CCombatParticipant> GetAliveEnemyParticipants()
	{
		List<CCombatParticipant> aliveParticipants = new List<CCombatParticipant>();

		foreach (CCombatParticipant participant in m_enemyTeam)
		{
			if (!participant.GetEntity().IsDefeated())
			{
				aliveParticipants.Add(participant);
			}
		}

		if (aliveParticipants.Count == 0)
		{
			Debug.LogWarning("No alive enemy participants found.");
		}

		return aliveParticipants;
	}

	public List<CCombatParticipant> GetPlayerTeam()
	{
		return m_playerTeam;
	}

	public List<CCombatParticipant> GetAlivePlayerParticipants()
	{
		List<CCombatParticipant> aliveParticipants = new List<CCombatParticipant>();

		foreach (CCombatParticipant participant in m_playerTeam)
		{
			if (!participant.GetEntity().IsDefeated())
			{
				aliveParticipants.Add(participant);
			}
		}

		if (aliveParticipants.Count == 0)
		{
			Debug.LogWarning("No alive player participants found.");
		}

		return aliveParticipants;
	}

	public List<CCombatParticipant> GetBothTeamsAsOneList()
	{
		return m_allParticipants;
	}

	public CCombatParticipant GetParticipantByEntity(CEntity entity)
	{
		foreach (CCombatParticipant participant in m_allParticipants)
		{
			if (participant.GetEntity() == entity)
			{
				return participant;
			}
		}

		Debug.LogWarning("No participant with name \"" + entity.GetName() + "\" found.");
		return null;
	}

	public bool GetIsRunning()
	{
		return m_isRunning;
	}

	public bool GetIsPaused()
	{
		return m_isPaused;
	}

	public void SetIsPaused(bool isPaused)
	{
		m_isPaused = isPaused;
	}
}
