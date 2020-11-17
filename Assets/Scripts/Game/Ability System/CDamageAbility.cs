using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class CDamageAbility : CAbility
{
	// Member variables
	[SerializeField] protected float m_baseDamage;
	[SerializeField] protected int m_hits;
	[SerializeField] protected ResourceType m_resourceType;
	[SerializeField] protected int m_costs;
	[SerializeField] protected bool m_canInterrupt;
	[SerializeField] protected bool m_hasAreaOfEffect;
	[SerializeField] protected float m_radius;


	// Constructors
	public CDamageAbility()
	{

	}

	public CDamageAbility(string entityFolderName, string jsonFileName)
	{
		TextAsset jsonFile = Resources.Load<TextAsset>("Entities/" + entityFolderName + "/Abilities/" + jsonFileName);
		JsonUtility.FromJsonOverwrite(jsonFile.text, this);
	}


	// Methods
	public override IEnumerator Execute(CCombatParticipant executor)
	{
		// Activate the visual effect
		string abilityName = m_abilityName.Replace(" ", "");
		GameObject effect = Object.Instantiate(Resources.Load("Entities/" + executor.GetEntity().GetFolderName() + "/Effects/" + abilityName, typeof(GameObject)), executor.GetEntity().transform.parent) as GameObject;
		effect.GetComponent<CEffect>().Init(executor, m_hasAreaOfEffect);

		// As soon as the effect is destroyed, the ability is done being executed.
		yield return new WaitUntil(() => effect == null);

		// Apply the ability
		// Deal damage
		CCombatParticipant target = CCombatSystem.GetInstance().GetParticipantByEntity(executor.GetDecision().GetTarget().GetComponent<CEntity>());
		List<CCombatParticipant> damagedParticipants = new List<CCombatParticipant>();
		if (m_hasAreaOfEffect)
		{
			List<CCombatParticipant> participantsInRadius = CCombatSystem.GetInstance().DetermineParticipantsInRadius(m_radius, executor.GetDecision().GetTarget());
			foreach (CCombatParticipant participantInRadius in participantsInRadius)
			{
				// Check line of sight between participants in radius and the target.
				Vector3 raycastDirection = (participantInRadius.GetEntity().transform.position - target.GetEntity().transform.position).normalized;
				LayerMask layerMask = LayerMask.GetMask("Entities");
				RaycastHit[] raycastHits = Physics.RaycastAll(target.GetEntity().transform.position, raycastDirection, m_radius, layerMask);
				System.Array.Sort(raycastHits, (first, second) => first.distance.CompareTo(second.distance));

				bool isInLineOfSight = false;
				foreach (RaycastHit hit in raycastHits)
				{
					if (hit.transform.GetComponent<CEntity>() == null)		// Obstacle hit
					{
						isInLineOfSight = false;
						break;
					}
					else
					{
						if (hit.transform.GetComponent<CEntity>() == participantInRadius.GetEntity())
						{
							isInLineOfSight = true;
							break;
						}
						else
						{
							isInLineOfSight = false;
						}
					}
				}

				// Only deal damage if participants are in line of sight or is the target itself
				if (isInLineOfSight || participantInRadius.GetEntity() == target.GetEntity())
				{
					for (int i = 0; i < m_hits; i++)
					{
						float damageFactor = 1.0f - participantInRadius.GetActiveDamageReduction();     // 0.75f damage reduction leads to 0.25f percent incoming damage
						participantInRadius.GetEntity().TakeDamage(damageFactor * executor.CalculateDamage(this));
						damagedParticipants.Add(participantInRadius);
					}
				}	
			}
		}
		else
		{
			for (int i = 0; i < m_hits; i++)
			{
				float damageFactor = 1.0f - target.GetActiveDamageReduction();
				executor.GetDecision().GetTarget().GetComponent<CEntity>().TakeDamage(damageFactor * executor.CalculateDamage(this));
				damagedParticipants.Add(CCombatSystem.GetInstance().GetParticipantByEntity(executor.GetDecision().GetTarget().GetComponent<CEntity>()));
			}
		}

		// Costs
		executor.GetEntity().SpendResource(m_resourceType, m_costs);

		// Regenerate skill and magic points if the ability is a normal combo/critical attack
		if (m_abilityName.Equals("Combo Attack"))
		{
			executor.GetEntity().SpendResource(ResourceType.SkillPoints, -4);
			executor.GetEntity().SpendResource(ResourceType.MagicPoints, -4);
		}
		else if (m_abilityName.Equals("Critical Attack"))
		{
			executor.GetEntity().SpendResource(ResourceType.SkillPoints, -2);
			executor.GetEntity().SpendResource(ResourceType.MagicPoints, -2);
		}

		// Interrupt
		if (m_canInterrupt)
		{
			foreach (CCombatParticipant damagedParticipant in damagedParticipants)
			{
				if (!damagedParticipant.GetEntity().IsDefeated() && !damagedParticipant.GetIsDefending() && (damagedParticipant.GetIsPreparingAction() || damagedParticipant.GetHasReachedExecutionPoint()))
				{
					executor.Interrupt(damagedParticipant);
					Debug.Log("\"" + executor.GetEntity().name + "\" interrupted \"" + damagedParticipant.GetEntity().name + "\"");
				}
			}
		}

		// Continue the combat system
		executor.SetIsExecutingAction(false);

		// Pause all damaged entities on gauge
		foreach (CCombatParticipant damagedParticipant in damagedParticipants)
		{
			if (!damagedParticipant.GetEntity().IsDefeated())
			{
				damagedParticipant.GetEntity().StartCoroutine(PauseDamagedParticipant(damagedParticipant));
			}
		}
	}


	private IEnumerator PauseDamagedParticipant(CCombatParticipant damagedParticipant)
	{
		if (damagedParticipant.GetEntity().GetStats().GetSpeed() > 0)
		{
			float currentSpeed = damagedParticipant.GetEntity().GetStats().GetSpeed();
			float waitDuration = 0.0f;
			damagedParticipant.GetEntity().GetStats().SetSpeed(0.0f);

			while (waitDuration < CCombatSystem.GetInstance().GetWaitDuration())
			{
				yield return new WaitWhile(() => CCombatSystem.GetInstance().GetIsPaused());
				waitDuration += Time.deltaTime;
				yield return new WaitForEndOfFrame();
			}

			damagedParticipant.GetEntity().GetStats().SetSpeed(currentSpeed);
		}
	}


	public override string ToString()
	{
		return base.ToString() + "; Damage: " + m_baseDamage + "; Resource: " + m_resourceType.ToString() + "; Costs: " + m_costs + "; Range: " + m_range + "; canInterrupt: " + m_canInterrupt + "; Damages all: " + m_hasAreaOfEffect;
	}



	// Getter/Setter
	public float GetBaseDamage()
	{
		return m_baseDamage;
	}

	public float GetTotalBaseDamage()
	{
		return m_baseDamage * m_hits;
	}

	public int GetHits()
	{
		return m_hits;
	}

	public ResourceType GetResourceType()
	{
		return m_resourceType;
	}

	public int GetCosts()
	{
		return m_costs;
	}

	public bool GetCanInterrupt()
	{
		return m_canInterrupt;
	}

	public bool GetHasAreaOfEffect()
	{
		return m_hasAreaOfEffect;
	}

	public float GetRadius()
	{
		return m_radius;
	}
}