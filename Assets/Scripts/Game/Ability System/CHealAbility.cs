using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CHealAbility : CAbility 
{
	// Member variables
	[SerializeField] protected float m_baseHeal;
	[SerializeField] protected ResourceType m_resourceType;
	[SerializeField] protected int m_costs;
	[SerializeField] protected bool m_hasAreaOfEffect;
	[SerializeField] protected float m_radius;


	// Constructor
	public CHealAbility()
	{

	}

	public CHealAbility(string entityFolderName, string jsonFileName)
	{
		TextAsset jsonFile = Resources.Load<TextAsset>("Entities/" + entityFolderName + "/Abilities/" + jsonFileName);
		JsonUtility.FromJsonOverwrite(jsonFile.text, this);
	}



	// Methods
	public override IEnumerator Execute(CCombatParticipant executor)
	{
		string abilityName = m_abilityName.Replace(" ", "");
		GameObject effect = Object.Instantiate(Resources.Load("Entities/" + executor.GetEntity().GetFolderName() + "/Effects/" + abilityName, typeof(GameObject)), executor.GetEntity().transform.parent) as GameObject;
		effect.GetComponent<CEffect>().Init(executor, m_hasAreaOfEffect);

		// As soon as the effect is destroyed, the ability is done being executed.
		yield return new WaitUntil(() => effect == null);

		// Apply the ability
		// Heal
		if (m_hasAreaOfEffect)
		{
			List<CCombatParticipant> participantsInRadius = CCombatSystem.GetInstance().DetermineParticipantsInRadius(m_radius, executor.GetDecision().GetTarget());
			foreach (CCombatParticipant participantInRadius in participantsInRadius)
			{
				participantInRadius.GetEntity().TakeHeal(executor.CalculateHeal(this));
			}
		}
		else
		{
			executor.GetDecision().GetTarget().GetComponent<CEntity>().TakeHeal(executor.CalculateHeal(this));
		}

		// Costs
		executor.GetEntity().SpendResource(m_resourceType, m_costs);

		// Continue the combat system
		executor.SetIsExecutingAction(false);
	}

	public override string ToString()
	{
		return base.ToString() + "; Heal: " + m_baseHeal + "; Resource: " + m_resourceType.ToString() + "; Costs: " + m_costs + "; Range: " + m_range + "; Heals all: " + m_hasAreaOfEffect;
	}


	// Getter/Setter
	public float GetBaseHeal()
	{
		return m_baseHeal;
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
