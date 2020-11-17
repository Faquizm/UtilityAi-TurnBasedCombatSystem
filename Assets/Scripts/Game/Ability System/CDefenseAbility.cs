using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class CDefenseAbility : CAbility
{
	// Member variables
	[SerializeField] protected float m_duration;
	[SerializeField] protected float m_damageReductionBy;


	// Constructors
	public CDefenseAbility()
	{

	}

	public CDefenseAbility(string entityFolderName, string jsonFileName)
	{
		TextAsset jsonFile = Resources.Load<TextAsset>("Common Abilities/" + jsonFileName);
		JsonUtility.FromJsonOverwrite(jsonFile.text, this);
	}


	// Methods
	public override IEnumerator Execute(CCombatParticipant executor)
	{
		switch (m_targetType)
		{
			case TargetType.Enemy:
				// Hide from
				// Determine all possible locations for hiding
				List<CObstacle> obstacles = CCombatSystem.GetInstance().GetObstacles();
				List<Transform> hideSpots = new List<Transform>();
				foreach (CObstacle obstacle in obstacles)
				{
					hideSpots.AddRange(obstacle.DetermineHideSpots(executor.GetDecision().GetTarget()));
				}

				// Hide at the closest hide spot
				Transform closestHideSpot = null;
				foreach (Transform hideSpot in hideSpots)
				{
					if (closestHideSpot == null)
					{
						closestHideSpot = hideSpot;
						continue;
					}

					float distanceToCurrentHideSpot = (executor.GetEntity().transform.position - hideSpot.position).magnitude;
					float distanceToClosestHideSpot = (executor.GetEntity().transform.position - closestHideSpot.position).magnitude;
					if (distanceToCurrentHideSpot < distanceToClosestHideSpot)
					{
						closestHideSpot = hideSpot;
					}
				}

				// Move entity towards the chosen target
				Vector3 velocity = Vector3.zero;
				Vector3 rotation = executor.GetEntity().transform.rotation.eulerAngles;
				Vector3 targetPosition = closestHideSpot.position;
				executor.GetEntity().GetComponent<NavMeshAgent>().SetDestination(targetPosition);

				bool hasReachedTarget = false;
				while (!hasReachedTarget)
				{
					float currentDistance = (targetPosition - executor.GetEntity().transform.position).magnitude;

					hasReachedTarget = currentDistance <= 0.1f;

					yield return new WaitForEndOfFrame();
				}

				Vector3 lookAtRotation = CCombatSystem.GetInstance().CalculateOppositeTeamCenter(executor);
				executor.GetEntity().transform.LookAt(lookAtRotation);

				executor.SetIsExecutingAction(false);
				//executor.SetIsDefending(false);
				break;


			case TargetType.Ally:
				lookAtRotation = CCombatSystem.GetInstance().CalculateOppositeTeamCenter(executor);
				executor.GetEntity().transform.LookAt(lookAtRotation);

				// Spawn effect
				string abilityName = m_abilityName.Replace(" ", "");
				GameObject effect = Object.Instantiate(Resources.Load("Entities/Common Ability Effects/" + abilityName, typeof(GameObject)), executor.GetEntity().transform.parent) as GameObject;
				effect.GetComponent<CEffect>().Init(executor, false);

				// Apply the ability
				CCombatParticipant target = CCombatSystem.GetInstance().GetParticipantByEntity(executor.GetDecision().GetTarget().GetComponent<CEntity>());
				target.SetActiveDamageReduction(m_damageReductionBy);

				// Stop duration counter if the combat system is paused as well.
				float durationCounter = 0.0f;
				float targetCurrentHealth = target.GetEntity().GetCurrentHealth();
				while (durationCounter < m_duration)
				{
					yield return new WaitWhile(() => CCombatSystem.GetInstance().GetIsPaused());
					durationCounter += Time.deltaTime;

					float healthAfterPause = target.GetEntity().GetCurrentHealth();
					if (targetCurrentHealth > healthAfterPause)
					{
						float healthDifference = targetCurrentHealth - healthAfterPause;

						if (!executor.GetEntity().IsDefeated())
						{
							target.GetEntity().TakeHeal(healthDifference);
							executor.GetEntity().TakeDamage(healthDifference);
						}
					}

					yield return new WaitForEndOfFrame();
				}

				// Undo the ability effect
				target.SetActiveDamageReduction(0.0f);
				executor.SetIsDefending(false);
				break;

			case TargetType.Self:
				// Protect self
				abilityName = m_abilityName.Replace(" ", "");
				effect = Object.Instantiate(Resources.Load("Entities/Common Ability Effects/" + abilityName, typeof(GameObject)), executor.GetEntity().transform.parent) as GameObject;
				effect.GetComponent<CEffect>().Init(executor, false);

				// Apply the ability
				executor.SetActiveDamageReduction(m_damageReductionBy);

				// Stop duration counter if the combat system is paused as well.
				durationCounter = 0.0f;
				while (durationCounter < m_duration)
				{
					yield return new WaitWhile(() => CCombatSystem.GetInstance().GetIsPaused());
					durationCounter += Time.deltaTime;
					yield return new WaitForEndOfFrame();
				}

				// Undo the ability effect
				executor.SetActiveDamageReduction(0.0f);
				executor.SetIsDefending(false);
				break;

			case TargetType.Environment:
				// Reserve the point 
				executor.GetDecision().GetTarget().GetComponent<CSpawnpoint>().Reserve(executor.GetEntity());

				// Move entity towards the chosen target
				velocity = Vector3.zero;
				rotation = executor.GetEntity().transform.rotation.eulerAngles;
				targetPosition = executor.GetDecision().GetTarget().transform.position;
				executor.GetEntity().GetComponent<NavMeshAgent>().SetDestination(targetPosition);

				hasReachedTarget = false;
				while (!hasReachedTarget)
				{
					targetPosition = executor.GetDecision().GetTarget().transform.position;
					executor.GetEntity().GetComponent<NavMeshAgent>().SetDestination(targetPosition);

					float currentDistance = (targetPosition - executor.GetEntity().transform.position).magnitude;

					hasReachedTarget = currentDistance <= 0.1f;

					yield return new WaitForEndOfFrame();
				}

				lookAtRotation = CCombatSystem.GetInstance().CalculateOppositeTeamCenter(executor);
				executor.GetEntity().transform.LookAt(lookAtRotation);

				// Continue the combat system
				executor.SetIsExecutingAction(false);
				break;

			default:
				break;
		}

		yield return null;
	}

	public override string ToString()
	{
		return base.ToString();
	}


	// Getter/Setter
	public float GetDuration()
	{
		return m_duration;
	}
}