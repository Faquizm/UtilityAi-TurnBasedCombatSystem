using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class CCombatParticipant 
{
	// Member variables
	[Header("Entity")]
	[SerializeField] private CEntity m_entity;
	[SerializeField] private CDecision m_oldDecision;
	[SerializeField] private CDecision m_decision;

	[Header("Combat Stats")]
	[SerializeField] private CEntityStats m_combatStatChanges;
	[SerializeField] private float m_activeDamageReduction;
	[SerializeField] private float m_storedEntitySpeed;

	[Header("State - Gauge")]
	[SerializeField] private float m_gaugePosition;
	[SerializeField] private float m_nextGaugePosition;
	[SerializeField] private bool m_isLocked;
	[SerializeField] private float m_unlockPosition;

	[Header("State - Combat")]
	[SerializeField] private bool m_hasDecisionChanged;
	[SerializeField] private bool m_hasChosenAnAction;
	[SerializeField] private bool m_isPreparingAction;
	[SerializeField] private bool m_hasReachedExecutionPoint;
	[SerializeField] private bool m_isExecutingAction;

	[SerializeField] private bool m_isDefending;

	private Coroutine m_currentExecuteCoroutine;

	// Constructors
	public CCombatParticipant(CEntity entity)
	{
		// Entity
		m_entity = entity;
		m_oldDecision = null;
		m_decision = null;

		// Combat Stats
		m_combatStatChanges = new CEntityStats(true);
		m_activeDamageReduction = 0.0f;
		m_storedEntitySpeed = 0.0f;

		// State - Gauge
		m_gaugePosition = 0.0f;
		m_isLocked = false;
		m_unlockPosition = 0.0f;

		// State - Combat
		m_hasDecisionChanged = false;
		m_hasChosenAnAction = false;
		m_isPreparingAction = false;
		m_hasReachedExecutionPoint = false;
		m_isExecutingAction = false;

		m_isDefending = false;

		m_currentExecuteCoroutine = null;
	}

	
	// Methods
	public void PrepareAction()
	{
		if (m_decision != null)
		{
			if (!IsPreparingInstantAbility())
			{
				// Store the normal speed of the entity
				if (m_entity.GetStats().GetSpeed() > 0)
				{
					m_storedEntitySpeed = m_entity.GetStats().GetSpeed();
				}
				else
				{
					m_storedEntitySpeed = 10.0f;
				}

				// Change the entity's speed to suit the preparation time of the chosen ability
				float preparationTime = m_decision.GetAbility().GetPreparationTime();
				float statWeight = m_entity.GetStatWeights().GetStatWeight(StatType.Speed);
				float combatChange = 1 + m_combatStatChanges.GetStatChange(StatType.Speed);
				float unscaledPreparationSpeed = CCombatSystem.GetInstance().GetCombatGauge().CalculateUnscaledPreparationSpeed(preparationTime, statWeight, combatChange);
				m_entity.GetStats().SetSpeed(unscaledPreparationSpeed);
			}

			// Change state
			m_isPreparingAction = true;
		}
		else
		{
			Debug.LogError(string.Format("{0} tried to prepare an action but it's decision is null.", m_entity.GetName()));
		}
	}


	public void StartExecution()
	{
		m_currentExecuteCoroutine = m_entity.StartCoroutine(ExecuteAction());
	}


	public IEnumerator ExecuteAction()
	{
		// Participant isn't preparing anymore
		SetIsPreparingAction(false);

		bool isDamageAbility = m_decision.GetAbility().GetType() == typeof(CDamageAbility);
		bool isHealAbility = m_decision.GetAbility().GetType() == typeof(CHealAbility);
		bool isStatAbility = m_decision.GetAbility().GetType() == typeof(CStatAbility);
		bool isDefenseAbility = m_decision.GetAbility().GetType() == typeof(CDefenseAbility);

		if (isDamageAbility || isHealAbility || isStatAbility)
		{
			// Move entity towards the chosen target
			float passedTime = 0.0f;
			float timeoutDuration = CCombatSystem.GetInstance().GetTimeoutDuration();

			Vector3 velocity = Vector3.zero;
			Vector3 rotation = m_entity.transform.rotation.eulerAngles;
			Vector3 targetPosition = m_decision.GetTarget().transform.position;
			m_entity.GetComponent<NavMeshAgent>().SetDestination(targetPosition);

			bool hasReachedTarget = false;
			bool isInLineOfSight = false;
			while (passedTime < timeoutDuration && !(hasReachedTarget && isInLineOfSight))
			{
				targetPosition = m_decision.GetTarget().transform.position;
				m_entity.GetComponent<NavMeshAgent>().SetDestination(targetPosition);

				float currentDistance = (targetPosition - m_entity.transform.position).magnitude;
				float abilityRange = m_decision.GetAbility().GetRange();
				float meleeRange = m_entity.gameObject.GetComponent<Collider>().bounds.extents.magnitude + m_decision.GetTarget().GetComponent<Collider>().bounds.extents.magnitude;

				hasReachedTarget = currentDistance <= abilityRange || currentDistance <= meleeRange;

				// Pause the movement if the combat system is paused
				if (CCombatSystem.GetInstance().GetIsPaused())
				{
					velocity = m_entity.GetComponent<NavMeshAgent>().velocity;
					StopMovement();
					yield return new WaitWhile(CCombatSystem.GetInstance().GetIsPaused);
					ResumeMovement(velocity);
				}

				// Choose a new target if the current target is killed while moving towards it
				if (m_decision.GetTarget().GetComponent<CEntity>().IsDefeated())
				{
					SearchNewTarget();
				}

				// Check the line of sight
				if (m_decision.GetAbility().GetTargetType() == TargetType.Self || m_decision.GetTarget().GetComponent<CEntity>() == m_entity)
				{
					isInLineOfSight = true;
				}
				else
				{
					Vector3 raycastDirection = (m_decision.GetTarget().GetComponent<CEntity>().transform.position - m_entity.transform.position).normalized;
					LayerMask layerMask = LayerMask.GetMask("Entities");
					RaycastHit hit;
					bool isTarget = false;
					bool hasHitSomething = Physics.Raycast(m_entity.transform.position, raycastDirection, out hit, m_decision.GetAbility().GetRange() * 1.5f, layerMask);
					if (hasHitSomething)
					{
						if (hit.transform.GetComponent<CEntity>() != null)
						{
							isTarget = hit.transform.GetComponent<CEntity>() == m_decision.GetTarget().GetComponent<CEntity>();
						}
					}

					isInLineOfSight = hasHitSomething && isTarget;
				}

				yield return new WaitForEndOfFrame();
				passedTime += Time.deltaTime;
			}

			// Stop movement after reaching the target or timeout
			StopMovement();

			// Check if entity was timed out. If not, execute the ability.
			bool hasTimedOut = passedTime >= timeoutDuration;
			if (hasTimedOut)
			{
				Debug.Log(string.Format("{0} hasn't reached {1}.", m_entity.GetName(), m_decision.GetTarget().GetComponent<CEntity>().GetName()));

				FinishAction(false);
			}
			else
			{
				if (!m_decision.GetTarget().GetComponent<CEntity>().IsDefeated())
				{
					// Look at the target if already in range
					m_entity.transform.LookAt(m_decision.GetTarget().transform);

					// Participant reached the target starts executing its action now
					SetIsExecutingAction(true);
					m_entity.StartCoroutine(m_decision.GetAbility().Execute(this));

					// Wait while the participant is executing its action (wait until it is done executing its action)
					yield return new WaitWhile(() => m_isExecutingAction);
				}

				// Move to random position
				if (m_entity.gameObject.activeSelf)
				{
					m_entity.StartCoroutine(MoveToRandomSpawnpoint());
				}

				// Reset the participant's state
				FinishAction(true);
			}
		}
		else
		{
			// Executing a defense ability has a slightly different and kind of specific process
			switch (m_decision.GetAbility().GetTargetType())
			{
				case TargetType.Enemy:      // Hide
					// Start hiding from the target. 
					SetIsExecutingAction(true);
					//SetIsDefending(true);
					m_entity.StartCoroutine(m_decision.GetAbility().Execute(this));
					//yield return new WaitWhile(() => m_isDefending);
					yield return new WaitWhile(() => m_isExecutingAction);

					// Reset the participant's state
					FinishAction(true);
					break;

				case TargetType.Ally:		// Protect
				case TargetType.Self:
					// Move entity towards the chosen target
					float passedTime = 0.0f;
					float timeoutDuration = CCombatSystem.GetInstance().GetTimeoutDuration();

					Vector3 velocity = Vector3.zero;
					Vector3 rotation = m_entity.transform.rotation.eulerAngles;
					Vector3 targetPosition = m_decision.GetTarget().transform.position;
					m_entity.GetComponent<NavMeshAgent>().SetDestination(targetPosition);

					bool hasReachedTarget = false;
					bool isInLineOfSight = false;
					while (passedTime < timeoutDuration && !(hasReachedTarget && isInLineOfSight))
					{
						targetPosition = m_decision.GetTarget().transform.position;
						m_entity.GetComponent<NavMeshAgent>().SetDestination(targetPosition);

						float currentDistance = (targetPosition - m_entity.transform.position).magnitude;
						float abilityRange = m_decision.GetAbility().GetRange();
						float meleeRange = m_entity.gameObject.GetComponent<CapsuleCollider>().radius + m_decision.GetTarget().GetComponent<CapsuleCollider>().radius;

						hasReachedTarget = currentDistance <= abilityRange || currentDistance <= meleeRange;

						// Pause the movement if the combat system is paused
						if (CCombatSystem.GetInstance().GetIsPaused())
						{
							velocity = m_entity.GetComponent<NavMeshAgent>().velocity;
							StopMovement();
							yield return new WaitWhile(CCombatSystem.GetInstance().GetIsPaused);

							if (m_decision.GetTarget().GetComponent<CEntity>().IsDefeated())
							{
								break;
							}

							ResumeMovement(velocity);
						}

						// Check the line of sight
						if (m_decision.GetAbility().GetTargetType() == TargetType.Self)
						{
							isInLineOfSight = true;
						}
						else
						{
							Vector3 raycastDirection = (m_decision.GetTarget().GetComponent<CEntity>().transform.position - m_entity.transform.position).normalized;
							LayerMask layerMask = LayerMask.GetMask("Entities");
							RaycastHit hit;
							bool isTarget = false;
							bool hasHitSomething = Physics.Raycast(m_entity.transform.position, raycastDirection, out hit, m_decision.GetAbility().GetRange() * 1.5f, layerMask);
							if (hasHitSomething)
							{
								if (hit.transform.GetComponent<CEntity>() != null)
								{
									isTarget = hit.transform.GetComponent<CEntity>() == m_decision.GetTarget().GetComponent<CEntity>();
								}
							}

							isInLineOfSight = hasHitSomething && isTarget;
						}

						yield return new WaitForEndOfFrame();
						passedTime += Time.deltaTime;
					}

					// Stop movement after reaching the target or timeout
					StopMovement();

					// Check if entity was timed out. If not, execute the ability.
					bool hasTimedOut = passedTime >= timeoutDuration;
					if (hasTimedOut)
					{
						CCombatSystem.GetInstance().GetCombatGauge().ResetParticipantGaugePosition(this, false);

						Debug.Log(string.Format("{0} hasn't reached {1}.", m_entity.name, m_decision.GetTarget().GetComponent<CEntity>().name));
					}
					else if (m_decision.GetTarget().GetComponent<CEntity>().IsDefeated())
					{
						Debug.Log(string.Format("{0} hasn't reached {1} before {1} died.", m_entity.name, m_decision.GetTarget().GetComponent<CEntity>().name));
						FinishAction(false);
					}
					else // Neither timed out nor defeated:
					{
						// Look at the target if already in range
						m_entity.transform.LookAt(m_decision.GetTarget().transform);

						// IMPORTANT: CombatSystem doesn't stop (isExecuting still false internally), so that the target can be attacked (and therefore being defended)
						SetIsDefending(true);

						// Start defending the target. 
						m_entity.StartCoroutine(m_decision.GetAbility().Execute(this));

						// Wait while the participant is executing its action (wait until it is done executing its action)
						yield return new WaitWhile(() => m_isDefending);

						// Move to random position
						if (m_decision.GetAbility().GetTargetType() == TargetType.Ally)
						{
							m_entity.StartCoroutine(MoveToRandomSpawnpoint());
						}

						// Reset the participant's state
						FinishAction(true);
					}

					break;

				case TargetType.Environment:	// Move
					SetIsExecutingAction(true);
					m_entity.StartCoroutine(m_decision.GetAbility().Execute(this));

					yield return new WaitWhile(() => m_isExecutingAction);

					// Reset the participant's state
					FinishAction(true);
					break;

				default:
					break;
			}
		}
	}


	private IEnumerator MoveToRandomSpawnpoint()
	{
		// Move to a random position after being done executing
		// Choose an available spawnpoint
		List<Transform> returnPositions = CCombatInitializer.GetInstance().DetermineSpawnpoints(m_entity.transform.position, CCombatSystem.GetInstance().GetReturnPositionRadius());
		while (true)
		{
			int randomPositionIndex = Random.Range(0, returnPositions.Count);
			if (returnPositions[randomPositionIndex].GetComponent<CSpawnpoint>().IsAvailable())
			{
				returnPositions[randomPositionIndex].GetComponent<CSpawnpoint>().Reserve(m_entity);
				m_entity.GetComponent<NavMeshAgent>().SetDestination(returnPositions[randomPositionIndex].position);
				ResumeMovement(Vector3.zero);
				break;
			}
		}

		// Move to available spawnpoint
		float distanceToReturnPosition = (m_entity.GetComponent<NavMeshAgent>().destination - m_entity.transform.position).magnitude;
		while (distanceToReturnPosition > 0.1f)
		{
			if (CCombatSystem.GetInstance().GetIsPaused())
			{
				Vector3 velocity = m_entity.GetComponent<NavMeshAgent>().velocity;
				StopMovement();
				yield return new WaitWhile(CCombatSystem.GetInstance().GetIsPaused);
				ResumeMovement(velocity);
			}

			distanceToReturnPosition = (m_entity.GetComponent<NavMeshAgent>().destination - m_entity.transform.position).magnitude;

			yield return null;
		}

		Vector3 lookAtRotation = CCombatSystem.GetInstance().CalculateOppositeTeamCenter(this);
		m_entity.transform.LookAt(lookAtRotation);
	}


	public void FinishAction(bool hasExecutedAction)
	{
		if (m_decision.GetAbility().GetType() == typeof(CDefenseAbility))
		{
			CCombatSystem.GetInstance().GetCombatGauge().ResetParticipantGaugePosition(this);
		}
		else
		{
			CCombatSystem.GetInstance().GetCombatGauge().ResetParticipantGaugePosition(this, hasExecutedAction);
		}

		if (m_storedEntitySpeed > 0)
		{
			m_entity.GetStats().SetSpeed(m_storedEntitySpeed);
		}

		SetHasChosenAnAction(false);
		SetIsPreparingAction(false);
		SetHasReachedExecutionPoint(false);
		SetIsExecutingAction(false);
		SetIsDefending(false);

		// Remember the current decision
		m_oldDecision = new CDecision(m_decision);
		m_decision = null;

		Debug.Log("Action of " + m_entity.name + " finished.");
	}


	public void SearchNewTarget()
	{
		string targetTag = m_decision.GetTarget().tag;
		List<CCombatParticipant> potentialTargets = new List<CCombatParticipant>();

		if (targetTag.Equals("Player"))
		{
			potentialTargets = CCombatSystem.GetInstance().GetAlivePlayerParticipants();
		}
		else if (targetTag.Equals("Enemy"))
		{
			potentialTargets = CCombatSystem.GetInstance().GetAliveEnemyParticipants();
		}
		else
		{
			Debug.LogError("Unknown Entity tag");
		}

		int randomIndex = Random.Range(0, potentialTargets.Count);
		m_decision.SetTarget(potentialTargets[randomIndex].m_entity.gameObject);
	}


	public void Interrupt(CCombatParticipant target)
	{
		if (target.m_currentExecuteCoroutine != null)
		{
			m_entity.StopCoroutine(target.m_currentExecuteCoroutine);
		}

		target.StopMovement();

		CCombatSystem.GetInstance().GetCombatGauge().UnlockParticipant(target);
		target.FinishAction(false);
	}


	public int CalculateDamage(CDamageAbility ability)
	{
		int min;
		int max;
		return CalculateDamage(ability, out min, out max);
	}


	public int CalculateDamage(CDamageAbility ability, out int minDamage, out int maxDamage)
	{
		int actualDamage;
		int averageDamage;
		float baseDamage = ability.GetBaseDamage();

		CEntityStats entityStats = m_entity.GetStats();
		CEntityStats entityStatWeights = m_entity.GetStatWeights();
		float damageRangePercentage = CCombatSystem.GetInstance().GetAbilityValueRange();

		if (ability.GetResourceType() == ResourceType.SkillPoints)
		{
			int damageAdditionStrength = (int)(entityStats.GetStrength() * 0.5f);
			float statChangeFactorStrength = 1 + m_combatStatChanges.GetStatChange(StatType.Strength);
			float statWeightStrength = entityStatWeights.GetStatWeight(StatType.Strength);

			int damageAdditionAgility = (int)(entityStats.GetAgility() * 0.25f);
			float statChangeFactorAgility = 1 + m_combatStatChanges.GetStatChange(StatType.Agility);
			float statWeightAgility = entityStatWeights.GetStatWeight(StatType.Agility);

			averageDamage = (int)(baseDamage 
				+ damageAdditionStrength * statChangeFactorStrength * statWeightStrength
				+ damageAdditionAgility * statChangeFactorAgility * statWeightAgility);
		}
		else if (ability.GetResourceType() == ResourceType.MagicPoints)
		{
			int damageAdditionIntelligence = (int)(entityStats.GetIntelligence() * 0.5f);
			float statChangeFactorIntelligence = 1 + m_combatStatChanges.GetStatChange(StatType.Intelligence);
			float statWeightIntelligence = entityStatWeights.GetStatWeight(StatType.Intelligence);

			int damageAdditionAgility = (int)(entityStats.GetAgility() * 0.25f);
			float statChangeFactorAgility = 1 + m_combatStatChanges.GetStatChange(StatType.Agility);
			float statWeightAgility = entityStatWeights.GetStatWeight(StatType.Agility);

			averageDamage = (int)(baseDamage
				+ damageAdditionIntelligence * statChangeFactorIntelligence * statWeightIntelligence
				+ damageAdditionAgility + statChangeFactorAgility * statWeightAgility);
		}
		else
		{
			Debug.LogError("Unknown resource type when calculating actual damage.");
			minDamage = 0;
			maxDamage = 0;
			return 0;
		}

		// Reduce damage based on defense
		if (m_hasChosenAnAction)
		{
			CCombatParticipant targetParticipant = CCombatSystem.GetInstance().GetParticipantByEntity(m_decision.GetTarget().GetComponent<CEntity>());
			float damageReduction = targetParticipant.GetEntity().GetStats().GetDefense() * 0.5f;
			float statChangeFactorDefense = 1 + targetParticipant.m_combatStatChanges.GetStatChange(StatType.Defense);
			float statWeightDefense = targetParticipant.GetEntity().GetStatWeights().GetStatWeight(StatType.Defense);
			averageDamage -= (int)((damageReduction * statChangeFactorDefense * statWeightDefense) / ability.GetHits());

			averageDamage = Mathf.Clamp(averageDamage, 2, int.MaxValue);
		}
		
		minDamage = (int)(averageDamage * (1.0 - damageRangePercentage * 0.5f));
		maxDamage = (int)(averageDamage * (1.0 + damageRangePercentage * 0.5f));
		actualDamage = Random.Range(minDamage, maxDamage + 1);

		Debug.Log(string.Format("Damage of {0} calculated. Range: {1} - {2}; actualDamage: {3}", ability.GetAbilityName(), minDamage, maxDamage, actualDamage));

		return actualDamage;
	}


	public int CalculateHeal(CHealAbility ability)
	{
		int min;
		int max;
		return CalculateHeal(ability, out min, out max);
	}


	public int CalculateHeal(CHealAbility ability, out int minHeal, out int maxHeal)
	{
		int actualHeal;
		int averageHeal;
		float baseHeal = ability.GetBaseHeal();

		CEntityStats entityStats = m_entity.GetStats();
		CEntityStats entityStatWeights = m_entity.GetStatWeights();
		float healRangePercentage = CCombatSystem.GetInstance().GetAbilityValueRange();

		if (ability.GetResourceType() == ResourceType.MagicPoints)
		{
			int healAdditionIntelligence = (int)(entityStats.GetIntelligence() * 0.5f);
			float statChangeFactorIntelligence = 1 + m_combatStatChanges.GetStatChange(StatType.Intelligence);
			float statWeightIntelligence = entityStatWeights.GetStatWeight(StatType.Intelligence);

			averageHeal = (int)(baseHeal + healAdditionIntelligence * statChangeFactorIntelligence * statWeightIntelligence);
		}
		else
		{
			Debug.LogError("Heal ability with other resource type then " + ResourceType.MagicPoints.ToString() + ".");
			minHeal = 0;
			maxHeal = 0;
			return 0;
		}

		minHeal = (int)(averageHeal * (1.0 - healRangePercentage * 0.5f));
		maxHeal = (int)(averageHeal * (1.0 + healRangePercentage * 0.5f));
		actualHeal = Random.Range(minHeal, maxHeal + 1);

		Debug.Log(string.Format("Heal of {0} calculated. Range: {1} - {2}; actualHeal: {3}", ability.GetAbilityName(), minHeal, maxHeal, actualHeal));

		return actualHeal;
	}

	public bool HasEnoughResourcesFor(CAbility ability)
	{
		bool hasEnoughResources = false;

		bool hasCostsProperty = ability.GetType().GetMethod("GetCosts") != null;
		bool hasResourceTypeProperty = ability.GetType().GetMethod("GetResourceType") != null;
		if (hasCostsProperty && hasResourceTypeProperty)
		{
			ResourceType abilityResource = (ResourceType)ability.GetType().GetMethod("GetResourceType").Invoke(ability, null);
			int abilityCosts = (int)ability.GetType().GetMethod("GetCosts").Invoke(ability, null);


			if (abilityResource == ResourceType.SkillPoints)
			{
				hasEnoughResources = m_entity.GetCurrentSkillPoints() >= abilityCosts;
			}
			else if (abilityResource == ResourceType.MagicPoints)
			{
				hasEnoughResources = m_entity.GetCurrentMagicPoints() >= abilityCosts;
			}
			else
			{
				Debug.LogError("Unknown resource type when checking usability of an ability.");
			}
		}

		return hasEnoughResources;
	}


	public void ResumeMovement(Vector3 oldVelocity)
	{
		m_entity.GetComponent<NavMeshAgent>().isStopped = false;
		m_entity.GetComponent<NavMeshAgent>().velocity = oldVelocity;
	}


	public void StopMovement()
	{
		m_entity.GetComponent<NavMeshAgent>().isStopped = true;
		m_entity.GetComponent<NavMeshAgent>().velocity = Vector3.zero;
	}


	public void CreateNewDecision()
	{
		m_decision = new CDecision();
	}


	public void CreateDecision(CAbility ability, GameObject target)
	{
		m_decision = new CDecision(ability, target);
	}


	private bool HasDecisionChanged()
	{
		bool hasDecisionChanged;

		if (m_oldDecision == null)
		{
			hasDecisionChanged = true;
		}
		else
		{
			bool hasAbilityChanged = !m_oldDecision.GetAbility().GetAbilityName().Equals(m_decision.GetAbility().GetAbilityName());
			bool hasTargetChanged = !m_oldDecision.GetTarget().GetComponent<CEntity>() == m_decision.GetTarget().GetComponent<CEntity>();

			hasDecisionChanged = hasAbilityChanged || hasTargetChanged;
		}

		return hasDecisionChanged;
	}


	public void MakeDecision()
	{
		// Find a new decision
		m_decision = CUtilityAiSystem.GetInstance().EvaluatePossibleDecisionsFor(this);

		if (m_decision != null)
		{
			SetHasChosenAnAction(true);
			m_hasDecisionChanged = HasDecisionChanged();
		}
		else
		{
			Debug.LogError("No decision found for " + m_entity.GetName() + ".");
		}
	}


	public bool IsPreparingInstantAbility()
	{
		if (m_decision.GetAbility().GetPreparationTime() < 0.001f)
		{
			return true;
		}
		else
		{
			return false;
		}
	}


	public void ResetDecision()
	{
		SetHasChosenAnAction(false);
		m_decision = null;
	}


	public void ResetDecisionHasChanged()
	{
		m_hasDecisionChanged = false;
	}


	// Getter/Setter
	public CEntity GetEntity()
	{
		return m_entity;
	}

	public CDecision GetOldDecision()
	{
		return m_oldDecision;
	}

	public CDecision GetDecision()
	{
		return m_decision;
	}

	public CEntityStats GetCombatStatChanges()
	{
		return m_combatStatChanges;
	}

	public float GetActiveDamageReduction()
	{
		return m_activeDamageReduction;
	}

	public void SetActiveDamageReduction(float damageReduction)
	{
		m_activeDamageReduction = damageReduction;
	}

	public float GetStoredEntitySpeed()
	{
		return m_storedEntitySpeed;
	}

	public float GetGaugePosition()
	{
		return m_gaugePosition;
	}

	public void SetGaugePosition(float gaugePosition)
	{
		m_gaugePosition = gaugePosition;
	}

	public float GetNextGaugePosition()
	{
		return m_nextGaugePosition;
	}

	public void SetNextGaugePosition(float nextGaugePosition)
	{
		m_nextGaugePosition = nextGaugePosition;
	}

	public bool GetIsLocked()
	{
		return m_isLocked;
	}

	public void SetIsLocked(bool isLocked)
	{
		m_isLocked = isLocked;
	}

	public float GetUnlockPosition()
	{
		return m_unlockPosition;
	}

	public void SetUnlockPosition(float unlockPosition)
	{
		m_unlockPosition = unlockPosition;
	}

	public bool GetHasDecisionChanged()
	{
		return m_hasDecisionChanged;
	}

	public void SetHasDecisionChanged(bool hasDecisionChanged)
	{
		m_hasDecisionChanged = hasDecisionChanged;
	}

	public bool GetHasChosenAnAction()
	{
		return m_hasChosenAnAction;
	}

	public void SetHasChosenAnAction(bool hasChosenAction)
	{
		m_hasChosenAnAction = hasChosenAction;
	}

	public bool GetIsPreparingAction()
	{
		return m_isPreparingAction;
	}

	public void SetIsPreparingAction(bool isPreparingAction)
	{
		m_isPreparingAction = isPreparingAction;
	}

	public bool GetHasReachedExecutionPoint()
	{
		return m_hasReachedExecutionPoint;
	}

	public void SetHasReachedExecutionPoint(bool hasReachedExectionPoint)
	{
		m_hasReachedExecutionPoint = hasReachedExectionPoint;
	}

	public bool GetIsExecutingAction()
	{
		return m_isExecutingAction;
	}

	public void SetIsExecutingAction(bool isExecutingAction)
	{
		m_isExecutingAction = isExecutingAction;
	}

	public bool GetIsDefending()
	{
		return m_isDefending;
	}

	public void SetIsDefending(bool isDefending)
	{
		m_isDefending = isDefending;
	}
}
