using System.Collections;
using System.Collections.Generic;
using UnityEngine;
	

public enum StatType { Strength, Intelligence, Agility, Defense, Stamina, Speed, All }
public enum ResourceType { SkillPoints, MagicPoints}
public enum TargetType { Enemy, Ally, Self, Environment }


[System.Serializable]
public abstract class CAbility 
{
	// Member variables
	[SerializeField] protected string m_abilityName;
	[SerializeField] protected string m_description;
	[SerializeField] protected TargetType m_targetType;
	[SerializeField] protected float m_preparationTime;
	[SerializeField] protected float m_range;


	// Methods
	public abstract IEnumerator Execute(CCombatParticipant executor);

	public override string ToString()
	{
		return "Ability Name: " + m_abilityName + "; Description: " + m_description + "; TargetType: " + m_targetType.ToString() + "; PreparationTime: " + m_preparationTime;
	}


	// Getter/Setter
	public string GetAbilityName()
	{
		return m_abilityName;
	}

	public string GetDescription()
	{
		return m_description;
	}

	public TargetType GetTargetType()
	{
		return m_targetType;
	}

	public float GetPreparationTime()
	{
		return m_preparationTime;
	}

	public float GetRange()
	{
		return m_range;
	}
}