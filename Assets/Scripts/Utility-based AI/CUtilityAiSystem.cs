using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CUtilityAiSystem : MonoBehaviour
{
	// Classes
	public class CActionOption
	{
		// Member
		private float m_actionScore;
		private float m_manipulation;
		private CAbility m_scoredByAbility;
		private GameObject m_onTarget;


		// Constructor
		public CActionOption(float actionScore, CAbility scoredByAbility, GameObject onTarget)
		{
			m_actionScore = actionScore;
			m_manipulation = 0.0f;
			m_scoredByAbility = scoredByAbility;
			m_onTarget = onTarget;
		}


		// Methods
		public void AddManipulation(float manipulation)
		{
			m_manipulation = manipulation;
		}

		public static int SortByActionScore(CActionOption first, CActionOption second)
		{
			return first.m_actionScore.CompareTo(second.m_actionScore);
		}

		public static int SortByManipulatedActionScore(CActionOption first, CActionOption second)
		{
			return first.GetFinalActionScore().CompareTo(second.GetFinalActionScore());
		}

		public override string ToString()
		{
			return string.Format("| {0:0.000} | {1,-20} | {2,-20} |", m_actionScore, m_scoredByAbility.GetAbilityName(), m_onTarget.name);
		}

		// Getter
		public float GetActionScore()
		{
			return m_actionScore;
		}

		public float GetManipulation()
		{
			return m_manipulation;
		}

		public float GetFinalActionScore()
		{
			float finalActionScore = m_actionScore + m_manipulation;
			finalActionScore = Mathf.Clamp(finalActionScore, 0.0f, 1.0f);
			return finalActionScore;
		}

		public CAbility GetAbility()
		{
			return m_scoredByAbility;
		}

		public GameObject GetTarget()
		{
			return m_onTarget;
		}
	}


	// Enum 
	public enum ActionScoreCompensation { None, MarkAndSizer, Average, ActionScoreDamping }
	public enum DecisionHeuristic { Best, ActionScoreManipulation, Median, Arbitrary, RandomFromBest, RandomAroundMedian }


	// Member variables
	private static CUtilityAiSystem m_utilityAiSystem;
	[SerializeField] private CAIContext m_context;

	[Header("AI Settings")]
	[SerializeField] private ActionScoreCompensation m_actionScoreCompensation;
	[SerializeField, Range(0.01f, 2.0f)] private float m_actionScoreDampingEpsilon = 0.5f;
	[SerializeField] private DecisionHeuristic m_decisionHeuristic;
	[SerializeField, Range(0, 20)] private int m_heuristicRange = 1;
	[SerializeField, Range(-0.2f, 0.2f)] private float m_manipulationRange = 0.0f;


	// MonoBehaviour-Methods
	void Awake()
	{
		if (m_utilityAiSystem == null)
		{
			m_utilityAiSystem = GameObject.FindGameObjectWithTag("AI").GetComponent<CUtilityAiSystem>();
			m_context = new CAIContext();

			m_actionScoreCompensation = CSettings.GetInstance().GetActionScoreCompensation();
			m_actionScoreDampingEpsilon = CSettings.GetInstance().GetEpsilon();
			m_decisionHeuristic = CSettings.GetInstance().GetDecisionHeuristic();
			m_heuristicRange = CSettings.GetInstance().GetHeuristicRange();
			m_manipulationRange = CSettings.GetInstance().GetManipulationRange();
		}
	}


	// Methods
	public CDecision EvaluatePossibleDecisionsFor(CCombatParticipant participant)	
	{
		List<CAction> participantActions = participant.GetEntity().GetEntityActions();
		
		List<CAbility> allAbilities = new List<CAbility>();
		List<CDamageAbility> damageAbilities = new List<CDamageAbility>();
		List<CHealAbility> healAbilities = new List<CHealAbility>();
		List<CStatAbility> statAbilities = new List<CStatAbility>();
		List<CDefenseAbility> defenseAbilities = participant.GetEntity().GetDefenseAbilities();
		CDefenseAbility hideAbility = participant.GetEntity().GetHideAbility();
		CDefenseAbility moveAbility = participant.GetEntity().GetMoveAbility();

		List<CActionOption> optionsToDecideFrom = new List<CActionOption>();

		allAbilities.Add(participant.GetEntity().GetComboAttack());
		allAbilities.Add(participant.GetEntity().GetCriticalAttack());
		allAbilities.AddRange(participant.GetEntity().GetSkillAbilities());
		allAbilities.AddRange(participant.GetEntity().GetMagicAbilities());

		foreach (CAbility ability in allAbilities)
		{
			if (ability.GetType() == typeof(CDamageAbility))
			{
				//if (HasEnoughResources(participant, ability))
				if (participant.HasEnoughResourcesFor(ability))
				{
					damageAbilities.Add((CDamageAbility)ability);
				}
			}
			else if (ability.GetType() == typeof(CHealAbility))
			{
				if (participant.HasEnoughResourcesFor(ability))
				{
					healAbilities.Add((CHealAbility)ability);
				}
			}
			else if (ability.GetType() == typeof(CStatAbility))
			{
				if (participant.HasEnoughResourcesFor(ability))
				{
					statAbilities.Add((CStatAbility)ability);
				}
			}
			else if (ability.GetType() == typeof(CDefenseAbility))
			{
				defenseAbilities.Add((CDefenseAbility)ability);
			}
		}
		Debug.Log(participant.GetEntity().name + " has " + damageAbilities.Count + " Attacks, " + healAbilities.Count + " Heals, " + statAbilities.Count + " Buffs/Debuffs and " + defenseAbilities.Count + " defense abilities to make a decision from.\n");


		// Iterate through all possibilities of actions (for each action per ability per valid target) and save all results			// Start logging
		CDecisionLogger.LogDecisionStart(participant);
		m_context.SetExecutor(participant.GetEntity().gameObject);
		foreach (CAction action in participantActions)
		{
			List<CCombatParticipant> validTargets = new List<CCombatParticipant>();

			switch (action.GetActionType())
			{
				case ActionType.Damage:
				case ActionType.Interrupt:
					foreach (CDamageAbility dmgAbility in damageAbilities)
					{
						// Skip all abilities which doesn't hit requirements
						if (!dmgAbility.GetCanInterrupt() && action.GetActionType() == ActionType.Interrupt)
						{
							continue;
						}

						if (dmgAbility.GetHasAreaOfEffect() != action.GetIsAreaOfEffectAction())
						{
							continue;
						}

						// Set the ability to the AI context to be used by the considerations
						m_context.SetAbilityToExecute(dmgAbility);


						validTargets = new List<CCombatParticipant>();

						// Get valid targets for the current ability
						if (participant.GetEntity().tag.Equals("Enemy"))
						{
							validTargets.AddRange(CCombatSystem.GetInstance().GetAlivePlayerParticipants());
						}
						else
						{
							validTargets.AddRange(CCombatSystem.GetInstance().GetAliveEnemyParticipants());
						}

						// Calculate the action score of the current action for each valid target
						foreach (CCombatParticipant target in validTargets)
						{
							// Set target to the AI context to be used by the considerations
							m_context.SetTarget(target.GetEntity().gameObject);							
							float actionScore = action.CalculateActionScore();

							// Log the action
							CDecisionLogger.LogAction(action, actionScore, m_context, participant);

							// Add the current option to the option list to decide from it later
							optionsToDecideFrom.Add(new CActionOption(actionScore, dmgAbility, target.GetEntity().gameObject));
						}
					}
					break;

				case ActionType.Heal:
					foreach (CHealAbility healAbility in healAbilities)
					{
						// Skip all abilities which doesn't hit requirements
						if (healAbility.GetHasAreaOfEffect() != action.GetIsAreaOfEffectAction())
						{
							continue;
						}

						validTargets = new List<CCombatParticipant>();
						m_context.SetAbilityToExecute(healAbility);

						TargetType healTargetType = healAbility.GetTargetType();
						switch (healTargetType)
						{
							case TargetType.Enemy:
							case TargetType.Environment:
								Debug.LogError("Invalid target type for \"" + healAbility.GetAbilityName() + "\" of " + participant.GetEntity().name);
								break;

							case TargetType.Ally:
								if (participant.GetEntity().tag.Equals("Enemy"))
								{
									validTargets.AddRange(CCombatSystem.GetInstance().GetAliveEnemyParticipants());
								}
								else
								{
									validTargets.AddRange(CCombatSystem.GetInstance().GetAlivePlayerParticipants());
								}
								break;

							case TargetType.Self:
								validTargets.Add(participant);
								break;

							default:
								break;
						}

						foreach (CCombatParticipant target in validTargets)
						{
							m_context.SetTarget(target.GetEntity().gameObject);
							float actionScore = action.CalculateActionScore();

							// Log the action
							CDecisionLogger.LogAction(action, actionScore, m_context, participant);

							optionsToDecideFrom.Add(new CActionOption(actionScore, healAbility, target.GetEntity().gameObject));
						}
					}
					break;

				case ActionType.Buff:
					foreach (CStatAbility statAbility in statAbilities)
					{
						// Skip all abilities which doesn't hit requirements
						if (statAbility.GetHasAreaOfEffect() != action.GetIsAreaOfEffectAction())
						{
							continue;
						}

						validTargets = new List<CCombatParticipant>();
						m_context.SetAbilityToExecute(statAbility);

						TargetType statTargetType = statAbility.GetTargetType();
						switch (statTargetType)
						{
							case TargetType.Enemy:
								break;

							case TargetType.Ally:
								if (participant.GetEntity().CompareTag("Player"))
								{
									validTargets.AddRange(CCombatSystem.GetInstance().GetAlivePlayerParticipants());
								}
								else
								{
									validTargets.AddRange(CCombatSystem.GetInstance().GetAliveEnemyParticipants());
								}
								break;

							case TargetType.Self:
								validTargets.Add(participant);
								break;

							case TargetType.Environment:
								Debug.LogError("Invalid target type for \"" + statAbility.GetAbilityName() + "\" of " + participant.GetEntity().name);
								break;
							default:
								break;
						}

						foreach (CCombatParticipant target in validTargets)
						{
							m_context.SetTarget(target.GetEntity().gameObject);
							float actionScore = action.CalculateActionScore();

							// Log the action
							CDecisionLogger.LogAction(action, actionScore, m_context, participant);

							optionsToDecideFrom.Add(new CActionOption(actionScore, statAbility, target.GetEntity().gameObject));
						}
					}
					break;

				case ActionType.Debuff:
					foreach (CStatAbility statAbility in statAbilities)
					{
						// Skip all abilities which doesn't hit requirements
						if (statAbility.GetHasAreaOfEffect() != action.GetIsAreaOfEffectAction())
						{
							continue;
						}

						validTargets = new List<CCombatParticipant>();
						m_context.SetAbilityToExecute(statAbility);

						TargetType statTargetType = statAbility.GetTargetType();
						switch (statTargetType)
						{
							case TargetType.Enemy:
								if (participant.GetEntity().CompareTag("Enemy"))
								{
									validTargets.AddRange(CCombatSystem.GetInstance().GetAlivePlayerParticipants());
								}
								else
								{
									validTargets.AddRange(CCombatSystem.GetInstance().GetAliveEnemyParticipants());
								}
								break;

							case TargetType.Ally:
							case TargetType.Self:
								break;

							case TargetType.Environment:
								Debug.LogError("Invalid target type for \"" + statAbility.GetAbilityName() + "\" of " + participant.GetEntity().name);
								break;
							default:
								break;
						}

						foreach (CCombatParticipant target in validTargets)
						{
							m_context.SetTarget(target.GetEntity().gameObject);
							float actionScore = action.CalculateActionScore();

							// Log the action
							CDecisionLogger.LogAction(action, actionScore, m_context, participant);

							optionsToDecideFrom.Add(new CActionOption(actionScore, statAbility, target.GetEntity().gameObject));
						}
					}
					break;


				case ActionType.Defense:
					foreach(CDefenseAbility defenseAbility in defenseAbilities)
					{
						validTargets = new List<CCombatParticipant>();
						m_context.SetAbilityToExecute(defenseAbility);

						TargetType defenseTargetType = defenseAbility.GetTargetType();
						switch (defenseTargetType)
						{
							case TargetType.Ally:
								if (participant.GetEntity().tag.Equals("Enemy"))
								{
									foreach (CCombatParticipant ally in CCombatSystem.GetInstance().GetAliveEnemyParticipants())
									{
										if (participant.GetEntity() != ally.GetEntity())
										{
											validTargets.Add(ally);
										}
									}
								}
								else
								{
									foreach (CCombatParticipant ally in CCombatSystem.GetInstance().GetAlivePlayerParticipants())
									{
										if (participant.GetEntity() != ally.GetEntity())
										{
											validTargets.Add(ally);
										}
									}
								}
								break;

							case TargetType.Self:
								validTargets.Add(participant);
								break;

							case TargetType.Enemy:
							case TargetType.Environment:
							default:
								break;
						}

						if (defenseAbility.GetTargetType() != TargetType.Environment)
						{
							foreach (CCombatParticipant target in validTargets)
							{
								// Set traget to the AI context to be used by the considerations
								m_context.SetTarget(target.GetEntity().gameObject);
								float actionScore = action.CalculateActionScore();

								// Log the action
								CDecisionLogger.LogAction(action, actionScore, m_context, participant);

								// Add the current option to the option list to decide from it later
								optionsToDecideFrom.Add(new CActionOption(actionScore, defenseAbility, target.GetEntity().gameObject));
							}
						}
					}
					break;

				case ActionType.Hide:
					validTargets = new List<CCombatParticipant>();

					m_context.SetAbilityToExecute(hideAbility);


					if (participant.GetEntity().tag.Equals("Enemy"))
					{
						validTargets.AddRange(CCombatSystem.GetInstance().GetAlivePlayerParticipants());
					}
					else
					{
						validTargets.AddRange(CCombatSystem.GetInstance().GetAliveEnemyParticipants());
					}


					foreach (CCombatParticipant target in validTargets)
					{
						m_context.SetTarget(target.GetEntity().gameObject);
						float actionScore = action.CalculateActionScore();

						// Log the action
						CDecisionLogger.LogAction(action, actionScore, m_context, participant);

						optionsToDecideFrom.Add(new CActionOption(actionScore, hideAbility, target.GetEntity().gameObject));
					}
					break;

				case ActionType.Move:
					List<Transform> availablePoints = new List<Transform>();
					availablePoints = CCombatInitializer.GetInstance().GetAllAvailablePoints();
					
					m_context.SetAbilityToExecute(moveAbility);

					foreach (Transform availablePoint in availablePoints)
					{
						m_context.SetTarget(availablePoint.gameObject);
						float actionScore = action.CalculateActionScore();

						// Log the action
						CDecisionLogger.LogAction(action, actionScore, m_context, participant);

						// Add the current option to the option list to decide from it later
						optionsToDecideFrom.Add(new CActionOption(actionScore, moveAbility, availablePoint.gameObject));
					}
					break;

				default:
					break;
			}
		}

		// Sort the results 
		optionsToDecideFrom.Sort(CActionOption.SortByActionScore);
		optionsToDecideFrom.Reverse();      // Descending
		
		// Choose an option and log the overview and the chosen option
		CActionOption chosenOption = ChooseOption(optionsToDecideFrom);

		// Log the overview of all possible options
		CDecisionLogger.LogActionOptions(optionsToDecideFrom, participant);
		CDecisionLogger.LogChosenOption(chosenOption, m_decisionHeuristic, participant);

		// Make the chosen option the returned decision
		CDecision decision;
		if (chosenOption.GetTarget() != null)
		{
			decision = new CDecision(chosenOption.GetAbility(), chosenOption.GetTarget());
		}
		else
		{
			Debug.LogError("No action option chosen. No abilities or valid targets found for " + participant.GetEntity().name);
			decision = null;
		}

		CDecisionLogger.LogDecisionEnd(participant);
		return decision;
	}


	/// <summary>
	/// Choses a suitable action for a given heuristic from a list of available options. 
	/// </summary>
	/// <param name="optionsToDecideFrom"><b>Sorted (descending)</b> list of action options to make a decision from.</param>
	/// <returns></returns>
	private CActionOption ChooseOption(List<CActionOption> optionsToDecideFrom)
    {
		if (optionsToDecideFrom == null || optionsToDecideFrom.Count == 0)
		{
			Debug.Log("No action option to decide from, because list is empty or null.");
			return null;
		}

		CActionOption chosenActionOption = null;


		switch (m_decisionHeuristic)
		{
			case DecisionHeuristic.Best:
				chosenActionOption = optionsToDecideFrom[0];
				break;

			case DecisionHeuristic.ActionScoreManipulation:
				foreach (CActionOption actionOption in optionsToDecideFrom)
				{
					ManipulateActionOption(actionOption);
				}

				// Sort options again considering the manipulation
				optionsToDecideFrom.Sort(CActionOption.SortByManipulatedActionScore);
				optionsToDecideFrom.Reverse();      // Descending

				chosenActionOption = optionsToDecideFrom[0];
				break;

			case DecisionHeuristic.Median:
				int medianIndex;
				if (optionsToDecideFrom.Count % 2 == 0)
				{
					medianIndex = (optionsToDecideFrom.Count / 2) - 1;
				}
				else
				{
					medianIndex = optionsToDecideFrom.Count / 2;
				}
				chosenActionOption = optionsToDecideFrom[medianIndex];
				break;

			case DecisionHeuristic.Arbitrary:
				int arbitraryIndex = Random.Range(0, optionsToDecideFrom.Count);
				chosenActionOption = optionsToDecideFrom[arbitraryIndex];
				break;

			case DecisionHeuristic.RandomFromBest:
				if (m_heuristicRange > optionsToDecideFrom.Count)
				{
					m_heuristicRange = optionsToDecideFrom.Count;
				}

				int randomFromBestIndex = Random.Range(0, m_heuristicRange);
				chosenActionOption = optionsToDecideFrom[randomFromBestIndex];
				break;

			case DecisionHeuristic.RandomAroundMedian:
				if (m_heuristicRange > optionsToDecideFrom.Count)
				{
					m_heuristicRange = optionsToDecideFrom.Count;
				}

				int randomAroundMedianIndex;
				int medianAroundIndex;
				int minIndex;
				int maxIndex;
				if (m_heuristicRange % 2 == 0)
				{
					if (optionsToDecideFrom.Count % 2 == 0)
					{
						medianAroundIndex = (optionsToDecideFrom.Count / 2) - 1;
						minIndex = medianAroundIndex - (m_heuristicRange / 2);
						maxIndex = medianAroundIndex + (m_heuristicRange / 2);
					}
					else
					{
						medianAroundIndex = optionsToDecideFrom.Count / 2;
						minIndex = medianAroundIndex - (m_heuristicRange / 2);
						maxIndex = medianAroundIndex + (m_heuristicRange / 2);
					}
				}
				else
				{
					if (optionsToDecideFrom.Count % 2 == 0)
					{
						medianAroundIndex = (optionsToDecideFrom.Count / 2) - 1;
						minIndex = medianAroundIndex - (m_heuristicRange / 2);
						maxIndex = medianAroundIndex + (m_heuristicRange / 2) + 1;
					}
					else
					{
						medianAroundIndex = optionsToDecideFrom.Count / 2;
						minIndex = medianAroundIndex - (m_heuristicRange / 2);
						maxIndex = medianAroundIndex + (m_heuristicRange / 2) + 1;
					}

				}

				randomAroundMedianIndex = Random.Range(minIndex, maxIndex);
				chosenActionOption = optionsToDecideFrom[randomAroundMedianIndex];
				break;

			default:
				Debug.LogError("No decision heurisic. ChosenOption is null");
				break;
		}

		return chosenActionOption;
	}


	public void ManipulateActionOption(CActionOption actionOption)
	{
		float manipulation = Random.Range(0.0f, m_manipulationRange) - (m_manipulationRange * 0.5f);
		actionOption.AddManipulation(manipulation);
	}


	public void UpdateEntitiesInRadius(float radius)
	{
		int entitiesInRadius = CCombatSystem.GetInstance().DetermineParticipantsInRadius(radius, m_context.GetTarget()).Count;
		m_context.SetEntitiesInRadius(entitiesInRadius);
	}


	// Getter/Setter
	public static CUtilityAiSystem GetInstance()
	{
		return m_utilityAiSystem;
	}

	public CAIContext GetContext()
	{
		return m_context;
	}

	public ActionScoreCompensation GetActionScoreCompensation()
	{
		return m_actionScoreCompensation;
	}

	public string PrintActionScoreCompensation()
	{
		if (m_actionScoreCompensation == ActionScoreCompensation.ActionScoreDamping)
		{
			return "ASD_" + m_actionScoreDampingEpsilon.ToString("0.0");
		}
		else
		{
			return m_actionScoreCompensation.ToString();
		}
	}

	public float GetEpsilon()
	{
		return m_actionScoreDampingEpsilon;
	}

	public float GetManipulationRange()
	{
		return m_manipulationRange;
	}
}