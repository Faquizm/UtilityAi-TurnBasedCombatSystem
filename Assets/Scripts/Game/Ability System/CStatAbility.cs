using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CStatAbility : CAbility 
{
	// Member variables
	[SerializeField] protected StatType m_stat;
	[SerializeField, Range(-1.0f, 1.0f)] protected float m_percentage;
	[SerializeField] protected float m_duration;
	[SerializeField] protected ResourceType m_resourceType;
	[SerializeField] protected int m_costs;
	[SerializeField] protected bool m_hasAreaOfEffect;
	[SerializeField] protected float m_radius;


	// Constructor
	public CStatAbility()
	{

	}

	public CStatAbility(string entityFolderName, string jsonFileName)
	{
		TextAsset jsonFile = Resources.Load<TextAsset>("Entities/" + entityFolderName + "/Abilities/" + jsonFileName);
		JsonUtility.FromJsonOverwrite(jsonFile.text, this);
	}


	// Methods
	public override IEnumerator Execute(CCombatParticipant executor)
	{
		// Spawn the effect
		string abilityName = m_abilityName.Replace(" ", "");
		GameObject effect = Object.Instantiate(Resources.Load("Entities/" + executor.GetEntity().GetFolderName() + "/Effects/" + abilityName, typeof(GameObject)), executor.GetEntity().transform.parent) as GameObject;
		effect.GetComponent<CEffect>().Init(executor, m_hasAreaOfEffect);
		
		// As soon as the effect is destroyed, the ability is done being executed.
		yield return new WaitUntil(() => effect == null);

		// Apply the ability
		// Stat change
		if (m_hasAreaOfEffect)
		{
			List<CCombatParticipant> participantsInRadius = CCombatSystem.GetInstance().DetermineParticipantsInRadius(m_radius, executor.GetDecision().GetTarget());
			foreach (CCombatParticipant participantInRadius in participantsInRadius)
			{
				participantInRadius.GetEntity().StartCoroutine(ApplyStatChange(participantInRadius));
			}
		}
		else
		{
			CCombatParticipant target = CCombatSystem.GetInstance().GetParticipantByEntity(executor.GetDecision().GetTarget().GetComponent<CEntity>());
			executor.GetDecision().GetTarget().GetComponent<CEntity>().StartCoroutine(ApplyStatChange(target));
		}

		// Costs
		executor.GetEntity().SpendResource(m_resourceType, m_costs);

		// Continue the combat system
		executor.SetIsExecutingAction(false);
	}

	private IEnumerator ApplyStatChange(CCombatParticipant targetParticipant)
	{
		CEntityStats targetParticipantStats = targetParticipant.GetCombatStatChanges();
		targetParticipantStats.ChangeStatBy(m_percentage, m_stat);

		float changeDuration = 0.0f;
		while(changeDuration < m_duration)
		{
			yield return new WaitWhile(() => CCombatSystem.GetInstance().GetIsPaused());
			changeDuration += Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}

		targetParticipantStats.ChangeStatBy(-m_percentage, m_stat);
	}

	public override string ToString()
	{
		return base.ToString() + "; Stat: " + m_stat.ToString() + "; Percentage: " + m_percentage + "; Duration: " + m_duration + 
			" ; Resource: " + m_resourceType.ToString() + "; Costs: " + m_costs + "; Range: " + m_range + "; Effect on all: " + m_hasAreaOfEffect;
	}

	// Getter/Setter
	public StatType GetStatType()
	{
		return m_stat;
	}

	public float GetPercentage()
	{
		return m_percentage;
	}

	public float GetDuration()
	{
		return m_duration;
	}

	public ResourceType GetResourceType()
	{
		return m_resourceType;
	}

	public int GetCosts()
	{
		return m_costs;
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
